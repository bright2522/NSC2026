using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Pep.GameplayEvents
{
    public enum CockroachOutcome
    {
        Hit,
        Escaped
    }

    public class CockroachRunner : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private float moveSpeed = 2.2f;
        [SerializeField] private float stopDistance = 0.05f;
        [SerializeField] private List<Transform> waypoints = new List<Transform>();

        public event Action<CockroachRunner, CockroachOutcome> OnFinished;

        private int waypointIndex;
        private bool finished;

        private void Update()
        {
            if (finished || waypoints.Count == 0) return;

            Transform target = waypoints[waypointIndex];
            if (target == null)
            {
                Finish(CockroachOutcome.Escaped);
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance > stopDistance) return;

            waypointIndex++;
            if (waypointIndex >= waypoints.Count)
            {
                Finish(CockroachOutcome.Escaped);
            }
        }

        public void Setup(List<Transform> path, float speed)
        {
            waypoints.Clear();
            if (path != null)
            {
                waypoints.AddRange(path);
            }

            moveSpeed = Mathf.Max(0.1f, speed);
            waypointIndex = 0;
            finished = false;
            if (waypoints.Count > 0 && waypoints[0] != null)
            {
                transform.position = waypoints[0].position;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Hit();
        }

        private void OnMouseDown()
        {
            Hit();
        }

        private void Hit()
        {
            if (finished) return;
            Finish(CockroachOutcome.Hit);
        }

        private void Finish(CockroachOutcome outcome)
        {
            if (finished) return;
            finished = true;
            OnFinished?.Invoke(this, outcome);
            Destroy(gameObject);
        }
    }
}
