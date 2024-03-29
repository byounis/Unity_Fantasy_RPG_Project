using System;
using RPG.Attributes;
using RPG.Core;
using RPG.Helpers;
using GameDevTV.Saving;
using UnityEngine;
using UnityEngine.AI;

namespace RPG.Movement
{
    public class Mover : MonoBehaviour, IAction, ISaveable
    {
        [SerializeField] private ActionScheduler _actionScheduler;
        [SerializeField] private float _maxSpeed = 6f;
        [SerializeField] private float _maxPathLength = 40f;

        private static readonly int ForwardSpeedAnimatorParameterID = Animator.StringToHash("ForwardSpeed");
        private Animator _animator;
        private Health _health;
        private NavMeshAgent _navMeshAgent;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _health = GetComponent<Health>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }
        
        private void Update()
        {
            _navMeshAgent.enabled = !_health.HasDied;

            UpdateAnimator();
        }

        public void StartMoveAction(Vector3 destination, float speedFraction = 1)
        {
            _actionScheduler.StartAction(this);
            MoveTo(destination, speedFraction);
        }

        public bool CanMoveTo(Vector3 destination)
        {
            var path = new NavMeshPath();
            var hasPath = NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path);

            if (!hasPath || path.status != NavMeshPathStatus.PathComplete)
            {
                return false;
            }

            if (path.GetPathLength() > _maxPathLength)
            {
                return false;
            }

            return true;
        }

        public void MoveTo(Vector3 destination, float speedFraction = 1)
        {
            _navMeshAgent.isStopped = false;
            _navMeshAgent.speed = Mathf.Clamp01(speedFraction) * _maxSpeed;
            _navMeshAgent.SetDestination(destination);
        }

        public void Cancel()
        {
            _navMeshAgent.isStopped = true;
        }

        private void UpdateAnimator()
        {
            var velocity = _navMeshAgent.velocity;
            var localVelocity = transform.InverseTransformDirection(velocity);
            var forwardSpeed = localVelocity.z;

            _animator.SetFloat(ForwardSpeedAnimatorParameterID, forwardSpeed);
        }

        #region Saving

        [Serializable]
        private struct MoverSaveData
        {
            private SerializableVector3 _position;
            private SerializableVector3 _rotation;

            public Vector3 Position => _position.ToVector();
            public Vector3 Rotation => _rotation.ToVector();

            public MoverSaveData(Vector3 position, Vector3 rotation)
            {
                _position = new SerializableVector3(position);
                _rotation = new SerializableVector3(rotation);
            }
        }
        
        public object CaptureState()
        {
            var moverSaveData = new MoverSaveData(transform.position, transform.rotation.eulerAngles);
            return moverSaveData;
        }

        public void RestoreState(object state)
        {
            var moverSaveData = (MoverSaveData)state;
            _navMeshAgent.Warp(moverSaveData.Position);
            transform.eulerAngles = moverSaveData.Rotation;
        }

        #endregion
    }
}