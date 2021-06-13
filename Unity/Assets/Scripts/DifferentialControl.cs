using System.Collections;
using System.Collections.Generic;
using System.Threading;
using RosSharp.RosBridgeClient;
using UnityEngine;


/*
 * Uses equations from
 * http://datagenetics.com/blog/december12016/index.html
 * and
 * https://www.xarg.org/book/kinematics/ackerman-steering/
 */
public class DifferentialControl : MonoBehaviour
{

    public GameObject frontAxleTf;

    private float L = 0.3f; // Axle Length

    [Range(0.0F, 10.0F)]
    public float ManualTopSpeed = 1.0f;

    public float Left { get; private set; }
    public float Right { get; private set; }

    public float CntrlLeft { get; private set; } = 0;
    public float CntrlRight { get; private set; } = 0;

    public Vector3 linear_vel { get; private set; } = new Vector3();
    public Vector3 angular_vel { get; private set; } = new Vector3();
    public Vector3 accel { get; private set; } = new Vector3();

    private float Radius;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        ManualTopSpeed = ConfigLoader.simulator.ManualTopSpeed;

        GameManager.instance.robotTf = frontAxleTf.transform;

        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0.2f,0,0);
        rb.inertiaTensor = new Vector3(100,100,100);
        rb.inertiaTensorRotation = Quaternion.identity;
        rb.maxAngularVelocity = 100f;
    }

    // Update is called once per frame
    void Update()
    {
        if (ConfigLoader.simulator.ManualControl) {
            CntrlLeft = Input.GetAxis("Speed") * ManualTopSpeed - Input.GetAxis("Angle");
            CntrlRight = Input.GetAxis("Speed") * ManualTopSpeed + Input.GetAxis("Angle");
        }
    }

    public void SetControl(float leftLinear, float rightLinear) {
        CntrlLeft = leftLinear;
        CntrlRight = rightLinear;
    }

    private void FixedUpdate()
    {
        Left = Mathf.Clamp(CntrlLeft, -2.2f, 2.2f);
        Right = Mathf.Clamp(CntrlRight, -2.2f, 2.2f);

        // TODO: Wanky non-linear acceleration code to make it hard to use

        float AvgForwardSpeed = (Left + Right) / 2.0f;
        float heading = -this.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

        Vector3 new_linear_vel = new Vector3(AvgForwardSpeed * Mathf.Cos(heading), 0, AvgForwardSpeed * Mathf.Sin(heading));
        accel = (new_linear_vel - linear_vel) / Time.fixedDeltaTime;

        linear_vel = new_linear_vel;
        rb.velocity = linear_vel;

        angular_vel = new Vector3(0, (Left - Right) / L, 0);
        rb.angularVelocity = angular_vel;
    }
}
