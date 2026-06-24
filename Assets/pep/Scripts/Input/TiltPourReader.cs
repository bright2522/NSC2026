using UnityEngine;
using UnityInput = UnityEngine.Input;

namespace Pep.Input
{
    public class TiltPourReader : MonoBehaviour
    {
        [SerializeField] private AccelerometerGestureDetector sensor;
        [SerializeField] private float minTilt = 0.08f;
        [SerializeField] private float maxTilt = 0.65f;
        [SerializeField] private float smoothing = 14f;

        [Header("PC Debug Fallback")]
        [Tooltip("Hold left mouse button — mouse X position controls pour rate (center=0%, right edge=100%)")]
        [SerializeField] private bool useMousePositionFallback = true;

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

            if (useMousePositionFallback && UnityInput.GetMouseButton(0))
            {
                // Mouse X: left half → no pour, right half → increasing pour up to max
                float mouseNorm = (UnityInput.mousePosition.x / Mathf.Max(1f, Screen.width)) * 2f - 1f;
                float mouseAxis = mouseNorm * maxTilt;
                if (Mathf.Abs(mouseAxis) > Mathf.Abs(axisValue))
                    axisValue = mouseAxis;
            }

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

        public void Configure(AccelerometerGestureDetector detector)
        {
            sensor = detector;
            CalibrateNeutral();
        }
    }
}
