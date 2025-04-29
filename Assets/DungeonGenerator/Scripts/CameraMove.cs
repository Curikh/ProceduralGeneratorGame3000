/*
using UnityEngine;
using UnityEngine.EventSystems;

namespace DungeonGenerator
{
    public class CameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float followSpeed = 5f;
        [SerializeField] private float targetOrthoSize = 5f;
        
        private Vector3 dragOrigin;
        private Transform playerTarget;
        private bool shouldFollowPlayer = false;
        
        private static bool canMove = true;
        public static bool CanMove { get => canMove; set => canMove = value; }

        void Update()
        {
            ZoomCamera();
            
            if (shouldFollowPlayer && playerTarget != null)
            {
                FollowPlayer();
            }
            else
            {
                MoveCamera();
            }
        }

        // Вызывается из MapGenerator после создания игрока
        public void InitializeCamera(Transform playerTransform)
        {
            playerTarget = playerTransform;
            shouldFollowPlayer = true;
            canMove = true;
            
            if (playerTarget != null)
            {
                // Мгновенное перемещение к игроку без анимации
                Vector3 targetPosition = playerTarget.position;
                targetPosition.z = transform.position.z;
                transform.position = targetPosition;
                
                Camera.main.orthographicSize = targetOrthoSize;
            }
        }

        void ZoomCamera()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                Camera.main.orthographicSize = Mathf.Clamp(
                    Camera.main.orthographicSize - scroll * zoomSpeed,
                    2f,
                    20f
                );
            }
        }

        void FollowPlayer()
        {
            if (!canMove) return;
            
            Vector3 targetPosition = playerTarget.position;
            targetPosition.z = transform.position.z;
            
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                followSpeed * Time.deltaTime
            );
        }

        void MoveCamera()
        {
            if (!canMove) return;
            if (EventSystem.current.IsPointerOverGameObject()) return;

            if (Input.GetMouseButtonDown(0))
            {
                dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 difference = dragOrigin - Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Camera.main.transform.position += difference;
            }
        }

        // Добавляем метод для сброса камеры при новой генерации
        public void ResetCamera()
        {
            shouldFollowPlayer = false;
            playerTarget = null;
            canMove = true;
        }
    }
}
*/