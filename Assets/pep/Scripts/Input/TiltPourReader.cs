using UnityEngine;

namespace Pep.Input
{
    public class TiltPourReader : MonoBehaviour
    {
        [SerializeField] private AccelerometerGestureDetector sensor;
        [SerializeField] private float minTilt = 0.08f;
        [SerializeField] private float maxTilt = 0.65f;
        [SerializeField] private float smoothing = 14f;

        public float Tilt { get; private set; }
        public float TiltVelocity { get; private set; }
        public float PourRate { get; private set; }

        private float previousTilt;

        private void Awake()
        {
            if (sensor == null)
            {
                sensor = GetComponent<AccelerometerGestureDetector>();
            }
            CalibrateNeutral();
        }

        private void Update()
        {
            float dt = Mathf.Max(0.0001f, Time.deltaTime);
            float axisValue = sensor != null ? sensor.RelativeAcceleration.x : 0f;
            float absTilt = Mathf.Abs(axisValue);
            float targetTilt = Mathf.InverseLerp(minTilt, maxTilt, absTilt);
            float lerpT = Mathf.Clamp01(dt * smoothing);

            Tilt = Mathf.Lerp(Tilt, targetTilt, lerpT);
            TiltVelocity = (Tilt - previousTilt) / dt;
            previousTilt = Tilt;
            PourRate = Mathf.Clamp01(Tilt);
        }

        public void CalibrateNeutral()
        {
            if (sensor != null)
            {
                sensor.CalibrateNeutral();
            }

            Tilt = 0f;
            previousTilt = 0f;
            TiltVelocity = 0f;
            PourRate = 0f;
        }
    }
}
