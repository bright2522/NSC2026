using UnityEngine;
using UnityInput = UnityEngine.Input;

namespace Pep.Input
{
    public class AccelerometerGestureDetector : MonoBehaviour
    {
        [Header("Sensor")]
        [SerializeField] private bool compensateSensors = true;
        [SerializeField] private bool useLowPass = true;
        [SerializeField] private float lowPassStrength = 12f;

        [Header("Editor Fallback")]
        [SerializeField] private bool useEditorFallback = true;
        [SerializeField] private float fallbackScale = 0.65f;
        [SerializeField] private float mouseScale = 0.02f;

        public Vector3 RawAcceleration { get; private set; }
        public Vector3 FilteredAcceleration { get; private set; }
        public Vector3 RelativeAcceleration => FilteredAcceleration - neutralAcceleration;

        private Vector3 neutralAcceleration;
        private bool hasCalibration;

        private void Awake()
        {
            if (compensateSensors) UnityInput.compensateSensors = true;
            CalibrateNeutral();
        }

        private void Update()
        {
            RawAcceleration = ReadAcceleration();
            if (useLowPass)
            {
                float t = Mathf.Clamp01(Time.deltaTime * lowPassStrength);
                FilteredAcceleration = Vector3.Lerp(FilteredAcceleration, RawAcceleration, t);
            }
            else
            {
                FilteredAcceleration = RawAcceleration;
            }
        }

        public void CalibrateNeutral()
        {
            neutralAcceleration = ReadAcceleration();
            FilteredAcceleration = neutralAcceleration;
            hasCalibration = true;
        }

        private Vector3 ReadAcceleration()
        {
            Vector3 acceleration = UnityInput.acceleration;
#if UNITY_EDITOR
            if (useEditorFallback)
            {
                float x = UnityInput.GetAxisRaw("Horizontal");
                float y = UnityInput.GetAxisRaw("Vertical");
                Vector3 mouseDelta = Vector3.zero;
                if (UnityInput.GetMouseButton(0))
                {
                    mouseDelta = new Vector3(UnityInput.GetAxis("Mouse X"), UnityInput.GetAxis("Mouse Y"), 0f) * mouseScale;
                }

                acceleration += new Vector3(x, y, 0f) * fallbackScale + mouseDelta;
            }
#endif
            if (!hasCalibration)
            {
                neutralAcceleration = acceleration;
            }
            return acceleration;
        }
    }
}
