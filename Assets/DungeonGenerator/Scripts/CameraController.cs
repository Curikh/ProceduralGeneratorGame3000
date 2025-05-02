using UnityEngine;
using UnityEngine.EventSystems;

namespace DungeonGenerator
{
    public class CameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float followSpeed = 5f;
        [SerializeField] private float targetOrthoSize = 5f; // Желаемый размер камеры при приближении к игроку
        
        private Vector3 dragOrigin;
        private Transform playerTarget;
        private bool shouldFollowPlayer = false;
        
        private static bool canMove = true;
        public static bool CanMove { get => canMove; set => canMove = value; }

        // Вызывается из MapGenerator после создания игрока
        public void SetPlayerTarget(Transform playerTransform)
        {
            playerTarget = playerTransform;
            shouldFollowPlayer = true;
            
            // Мгновенное перемещение к игроку при первом обнаружении
            if (playerTarget != null)
            {
                Vector3 targetPosition = playerTarget.position;
                targetPosition.z = transform.position.z; // Сохраняем Z-позицию камеры
                transform.position = targetPosition;
                
                // Устанавливаем целевой размер камеры
                Camera.main.orthographicSize = targetOrthoSize;
            }
        }

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

        void ZoomCamera()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                Camera.main.orthographicSize = Mathf.Clamp(
                    Camera.main.orthographicSize - scroll * zoomSpeed,
                    2f,  // Минимальный zoom
                    20f  // Максимальный zoom
                );
            }
        }

        void FollowPlayer()
        {
            if (!canMove) return;
            
            Vector3 targetPosition = playerTarget.position;
            targetPosition.z = transform.position.z; // Сохраняем Z-позицию камеры
            
            // Плавное следование за игроком
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
    }
}