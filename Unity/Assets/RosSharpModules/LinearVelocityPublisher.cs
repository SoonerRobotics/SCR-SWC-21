using UnityEngine;

namespace RosSharp.RosBridgeClient.MessageTypes.swc_msgs
{
    [RequireComponent(typeof(DifferentialControl))]
    public class LinearVelocityPublisher : UnityPublisher<WheelVels>
    {
        private WheelVels message;

        private float previousScanTime = 5;

        private float velocityNoiseStdDev = 0.05f;

        private float updatePeriod = 0.05f;
        private DifferentialControl c;

        public bool noNoiseOverride = false;

        protected override void Start()
        {
            base.Start();
            c = GetComponent<DifferentialControl>();
            message = new WheelVels();

            switch (ConfigLoader.competition.NoiseLevel) {
                case ConfigLoader.CompetitionConfig.NoiseLevels.none:
                    velocityNoiseStdDev *= 0;
                    break;
                case ConfigLoader.CompetitionConfig.NoiseLevels.reduced:
                    velocityNoiseStdDev *= 0.5f;
                    break;
            }

            if (noNoiseOverride) {
                velocityNoiseStdDev = 0;
            }
        }

        private void FixedUpdate()
        {
            if (UnityEngine.Time.time >= previousScanTime + updatePeriod)
            {
                WriteMessage();
                previousScanTime = UnityEngine.Time.time;
            }
        }

        private void WriteMessage() {
            message.left = Mathf.Round((c.Left + SimUtils.getRandNormal(0, velocityNoiseStdDev)) * 1000f) / 1000f;
            message.right = Mathf.Round((c.Right + SimUtils.getRandNormal(0, velocityNoiseStdDev)) * 1000f) / 1000f;
            Publish(message);
        }
    }
}