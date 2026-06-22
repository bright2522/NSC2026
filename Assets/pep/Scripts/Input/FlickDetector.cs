using System;
using UnityEngine;
using UnityInput = UnityEngine.Input;

namespace Pep.Input
{
    public class FlickDetector : MonoBehaviour
    {
        [SerializeField] private AccelerometerGestureDetector sensor;
        [SerializeField] private float flickThreshold = 8.5f;
        [SerializeField] private float cooldownSeconds = 0.4f;
        [SerializeField] private bool allowEditorSpaceFallback = true;

        public event Action<Vector3, float> OnFlickDetected;

        public float LastFlickIntensity { get; private set; }
        public Vector3 LastFlickDirection { get; private set; }

        private Vector3 previousRelative;
        private float cooldownRemaining;
        private bool hasPendingFlick;

        private void Awake()
        {
            if (sensor == null)
            {
                sensor = GetComponent<AccelerometerGestureDetector>();
            }
        }

        private void Update()
        {
            float dt = Mathf.Max(0.0001f, Time.deltaTime);
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - dt);

            Vector3 currentRelative = sensor != null ? sensor.RelativeAcceleration : Vector3.zero;
            Vector3 jerk = (currentRelative - previousRelative) / dt;
            previousRelative = currentRelative;

            float intensity = jerk.magnitude;
            if (cooldownRemaining <= 0f && intensity >= flickThreshold)
            {
                RegisterFlick(jerk.normalized, intensity);
                return;
            }

#if UNITY_EDITOR
            if (allowEditorSpaceFallback && UnityInput.GetKeyDown(KeyCode.Space) && cooldownRemaining <= 0f)
            {
                RegisterFlick(Vector3.up, flickThreshold);
            }
#endif
        }

        public void Calibrate()
        {
            if (sensor != null)
            {
                sensor.CalibrateNeutral();
                previousRelative = sensor.RelativeAcceleration;
            }
        }

        public bool ConsumeFlick(out Vector3 direction, out float intensity)
        {
            if (!hasPendingFlick)
            {
                direction = Vector3.zero;
                intensity = 0f;
                return false;
            }

            hasPendingFlick = false;
            direction = LastFlickDirection;
            intensity = LastFlickIntensity;
            return true;
        }

        private void RegisterFlick(Vector3 direction, float intensity)
        {
            LastFlickDirection = direction;
            LastFlickIntensity = intensity;
            hasPendingFlick = true;
            cooldownRemaining = cooldownSeconds;
            OnFlickDetected?.Invoke(direction, intensity);
        }
    }
}
