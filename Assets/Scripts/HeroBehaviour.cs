using UnityEngine;

public class HeroBehaviour : MonoBehaviour
{
    private GameObject _target;

    private void Update()
    {
        var currentPosition = transform.position;
        if (_target == null)
        {
            var targets = FindObjectsOfType<TargetBehaviour>();
            var numTargets = targets.Length;
            var closestTarget = (GameObject) null;
            var closestTargetDistance = float.MaxValue;

            for (var i = 0; i < numTargets; ++i)
            {
                var target = targets[i].gameObject;
                var targetPosition = target.transform.position;

                var distance2Target = Vector3.Distance(currentPosition, targetPosition);

                if (distance2Target < closestTargetDistance)
                {
                    closestTargetDistance = distance2Target;
                    closestTarget = target;
                }
            }

            _target = closestTarget;
        }

        if (_target != null)
        {
            var targetPosition = _target.transform.position;
            var distance = Vector3.Distance(currentPosition, targetPosition);

            if (distance > .01f)
            {
                var dir = (targetPosition - currentPosition);
                dir.Normalize();

                transform.position = currentPosition + dir * Time.deltaTime;
            }
            else
            {
                Destroy(_target);
                _target = null;
            }
        }
    }
}