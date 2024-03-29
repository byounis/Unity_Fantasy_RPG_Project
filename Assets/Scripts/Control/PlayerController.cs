using System;
using RPG.Attributes;
using RPG.Combat;
using RPG.Helpers;
using RPG.Movement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace RPG.Control
{
    public class PlayerController : MonoBehaviour
    {
        [Serializable]
        struct CursorMapping
        {
            public CursorType CursorType;
            public Texture2D Texture;
            public Vector2 HotSpot;
        }

        [SerializeField] private CursorMapping[] _cursorMappings;
        [SerializeField] private int _maxDistanceNavMeshProjection = 1;
        [SerializeField] private float _sphereCastRadius = 1;
        
        private Mover _mover;
        private Camera _mainCamera;
        private Health _health;
        private bool _isDraggingUI;

        private void Awake()
        {
            _mover = GetComponent<Mover>();
            _health = GetComponent<Health>();
        }

        private void Start()
        {
            _mainCamera = Camera.main;
        }
    
        private void Update()
        {
            if (InteractWithUI())
            {
                return;
            }
            
            if (_health.HasDied)
            {
                SetCursor(CursorType.None);
                return;
            }

            if (InteractWithComponent())
            {
                return;
            }

            if (InteractWithMovement())
            {
                return;
            }
            
            SetCursor(CursorType.None);
        }

        private bool InteractWithComponent()
        {
            var hits = RaycastAllSorted();
            
            foreach (var hit in hits)
            {
                var raycastables = hit.transform.GetComponents<IRaycastable>();

                foreach (var raycastable in raycastables)
                {
                    if (raycastable.HandleRaycast(this))
                    {
                        SetCursor(raycastable.GetCursorType());
                        return true;
                    }
                }
                
            }
            
            return false;
        }

        private RaycastHit[] RaycastAllSorted()
        {
            var hits = Physics.SphereCastAll(GetMouseRay(), _sphereCastRadius);
            var distances = new float[hits.Length];
            for (var index = 0; index < hits.Length; index++)
            {
                distances[index] = hits[index].distance;
            }

            Array.Sort(distances, hits);
            return hits;
        }

        private bool InteractWithUI()
        {
            if (Input.GetMouseButtonUp(0))
            {
                _isDraggingUI = false;
            }
            
            if (EventSystem.current.IsPointerOverGameObject())
            {
                if (Input.GetMouseButtonDown(0))
                {
                    _isDraggingUI = true;
                }
                
                SetCursor(CursorType.UI);
                return true;
            }

            return _isDraggingUI;
        }

        private bool InteractWithMovement()
        {
            var isHit = RaycastNavMesh(out var target);

            if (!isHit)
            {
                return false;
            }

            if (!GetComponent<Mover>().CanMoveTo(target))
            {
                return false;
            }
            
            if (Input.GetMouseButton(0))
            {
                _mover.StartMoveAction(target);
            }
            
            SetCursor(CursorType.Movement);

            return true;
        }

        private bool RaycastNavMesh(out Vector3 target)
        {
            target = Vector3.zero;
            var ray = GetMouseRay();
            var isRaycastHit = Physics.Raycast(ray, out var hitInfo);

            if (!isRaycastHit)
            {
                return false;
            }

            var isNavMeshHit = NavMesh.SamplePosition(hitInfo.point, out var navMeshHit, _maxDistanceNavMeshProjection,
                NavMesh.AllAreas);
            if(!isNavMeshHit)
            {
                return false;
            }
            
            target = navMeshHit.position;

            return true;
        }

        private void SetCursor(CursorType cursorType)
        {
            var cursorMapping = GetCursorMapping(cursorType);
            Cursor.SetCursor(cursorMapping.Texture, cursorMapping.HotSpot, CursorMode.Auto);
        }

        private CursorMapping GetCursorMapping(CursorType cursorType)
        {
            foreach (var cursorMapping in _cursorMappings)
            {
                if (cursorMapping.CursorType == cursorType)
                {
                    return cursorMapping;
                }
            }

            return default;
        }
        
        private Ray GetMouseRay()
        {
            return _mainCamera.ScreenPointToRay(Input.mousePosition);
        }
    }
}
