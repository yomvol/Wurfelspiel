using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple patrolling behavior, NPC stops and waits a bit between two far off waypoints
/// </summary>
public class NPCWalkerBrain : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float _pauseTime;
    [SerializeField] private float _speed;
    [Tooltip("Squared magnitude definition of long distance between two waypoints")]
    [SerializeField] private float _longDistanceCoeff;
    [SerializeField] private Transform _player;

    // List: B, C  |--adding start point--> A, B, C, A
    // points must be in World space
    [SerializeField] private List<Vector3> _waypoints;

    private int _currentWaypoint = 0;
    private int _walkingAnimationParameter;

    private void Start()
    {
        _walkingAnimationParameter = Animator.StringToHash("Walking");

        _waypoints.Insert(0, transform.position);
        _waypoints.Add(transform.position); // make a loop with waypoints
        StartCoroutine(DoPatrolling());
    }

    private IEnumerator DoPatrolling()
    {
        while (true)
        {
            // get next waypoint
            if (_currentWaypoint < _waypoints.Count - 1)
            {
                _currentWaypoint++;
            }
            else if (_waypoints.Count > 1)
            {
                _currentWaypoint = 1;
            }
            Vector3 dest = _waypoints[_currentWaypoint];

            // make sure start and end difference is not negligible
            Vector3 diff = dest - transform.position;
            if (diff.sqrMagnitude < 1.7f)
            {
                _animator.SetBool(_walkingAnimationParameter, false);
                Debug.Log("Two consequtive waypoints are too close to each other.");
                break;
            }

            // walking to the destination
            _animator.SetBool(_walkingAnimationParameter, true);
            if (Mathf.Abs(diff.y - 0) < 0.01f)
            {
                transform.LookAt(dest);
            }
            else
            {
                transform.LookAt(new Vector3(dest.x, transform.position.y, dest.z));
            }

            while (!(Vector3.Distance(transform.position, dest) < 0.001f))
            {
                transform.position = Vector3.MoveTowards(transform.position, dest, _speed * Time.deltaTime);
                yield return null;
            }

            // we stop and wait only after long distances travelled
            if (diff.sqrMagnitude > _longDistanceCoeff)
            {
                _animator.SetBool(_walkingAnimationParameter, false);
                transform.LookAt(new Vector3(_player.position.x, transform.position.y, _player.position.z));
                yield return new WaitForSeconds(_pauseTime);
            }
        }
    }
}
