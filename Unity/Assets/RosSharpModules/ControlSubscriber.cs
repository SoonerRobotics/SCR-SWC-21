using System.Threading;
using UnityEngine;

namespace RosSharp.RosBridgeClient.MessageTypes.swc_msgs
{
    [RequireComponent(typeof(DifferentialControl))]
    public class ControlSubscriber : UnitySubscriber<WheelVels>
    {
        private DifferentialControl car;

        private WheelVels lastMessage;

        private float linearVelStdDev = 0.1f;

        private bool firstMessage = true;
        private bool begingame = false;
        private bool newMessage = false;
        private float startTime = 100000;

        protected override void Start()
        {
            base.Start();
            car = GetComponent<DifferentialControl>();
            lastMessage = new WheelVels();
            startTime = Time.realtimeSinceStartup;

            if (ConfigLoader.simulator.ManualControl) {
                this.enabled = false;
            }

            switch (ConfigLoader.competition.NoiseLevel) {
                case ConfigLoader.CompetitionConfig.NoiseLevels.none:
                    linearVelStdDev *= 0;
                    break;
                case ConfigLoader.CompetitionConfig.NoiseLevels.reduced:
                    linearVelStdDev *= 0.5f;
                    break;
            }

            if (!ConfigLoader.simulator.CompetitionMode) {
                // Don't care about the stopsim stuff
                begingame = false;
                firstMessage = false;
                GameManager.instance.StartSim();
            }
        }

        private void FixedUpdate() {
            if (newMessage) {
                car.SetControl(lastMessage.left + SimUtils.getRandNormal(0, linearVelStdDev), lastMessage.right + SimUtils.getRandNormal(0, linearVelStdDev));
                newMessage = false;
            }

            if (begingame)
            {
                begingame = false;
                GameManager.instance.StartSim();
            }

            if (firstMessage && !begingame && Time.realtimeSinceStartup - startTime >= 30) {
                GameManager.instance.StopSim("Did not start in 30 seconds!");
            }
        }

        protected override void ReceiveMessage(WheelVels control)
        {
            lastMessage = control;
            newMessage = true;

            if (firstMessage)
            {
                begingame = true;
                firstMessage = false;
            }
        }
    }
}