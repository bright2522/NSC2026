using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    public class FollowParent : MonoBehaviour
    {
        public bool useLateUpdate;
        public bool useLerp;
        public GameObject parentObject; // Reference to the Parent object
        public float smoothSpeed = 0.125f; // Smoothing speed
        
        private Vector3 _velocity = Vector3.zero; // velocity for smoothdamp

        private void LateUpdate()
        {
            if(useLateUpdate) Follow(); 
        }

        private void Update()
        {
            if(!useLateUpdate) Follow(); 
        }

        private void Follow()
        {
            // Use Lerp
            var desiredPosition = parentObject.transform.position;
            
            if(useLerp)
            {
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
                transform.position = smoothedPosition;
            }
            else
            {
                // Use SmoothDamp
                transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, smoothSpeed);
            }
        }
    }
}