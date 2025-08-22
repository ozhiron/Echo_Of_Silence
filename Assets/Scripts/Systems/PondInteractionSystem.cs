using UnityEngine;
using EchoOfSilence.Core;

namespace EchoOfSilence.Systems
{
    /// <summary>
    /// Система взаимодействия с прудом для заброса локатора
    /// Pond interaction system for locator casting
    /// </summary>
    public class PondInteractionSystem : MonoBehaviour
    {
        [Header("Pond Settings")]
        [SerializeField] private string locationId = "Pond_Main";
        [SerializeField] private LayerMask pondLayerMask = 1;
        [SerializeField] private Camera playerCamera;
        
        [Header("Locator Prefab")]
        [SerializeField] private GameObject locatorPrefab;
        [SerializeField] private Transform playerPosition; // Позиция игрока для заброса
        
        [Header("Casting Settings")]
        [SerializeField] private float maxCastDistance = 10f;
        [SerializeField] private float minCastDistance = 2f;
        
        private Locator currentLocator;
        private bool canInteract = true;
        
        // Events
        public System.Action<Vector3> OnLocatorCast;
        public System.Action OnCastFailed;
        
        private void Awake()
        {
            if (playerCamera == null)
                playerCamera = Camera.main;
        }
        
        private void Start()
        {
            // Попытка найти игрока если не назначен
            if (playerPosition == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    playerPosition = player.transform;
            }
        }
        
        private void Update()
        {
            if (canInteract && Input.GetMouseButtonDown(0))
            {
                HandleMouseClick();
            }
        }
        
        private void HandleMouseClick()
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            
            if (IsValidCastPosition(mouseWorldPos))
            {
                AttemptCastLocator(mouseWorldPos);
            }
        }
        
        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = Mathf.Abs(playerCamera.transform.position.z);
            return playerCamera.ScreenToWorldPoint(mouseScreenPos);
        }
        
        private bool IsValidCastPosition(Vector3 worldPosition)
        {
            // Проверяем, что клик был по водоёму
            Collider2D hitCollider = Physics2D.OverlapPoint(worldPosition, pondLayerMask);
            if (hitCollider == null)
            {
                Debug.Log("Клик не по водоёму / Click not on pond");
                return false;
            }
            
            // Проверяем расстояние от игрока
            if (playerPosition != null)
            {
                float distance = Vector3.Distance(playerPosition.position, worldPosition);
                if (distance > maxCastDistance)
                {
                    Debug.Log($"Слишком далеко для заброса: {distance:F1}м / Too far to cast: {distance:F1}m");
                    return false;
                }
                
                if (distance < minCastDistance)
                {
                    Debug.Log($"Слишком близко для заброса: {distance:F1}м / Too close to cast: {distance:F1}m");
                    return false;
                }
            }
            
            return true;
        }
        
        private void AttemptCastLocator(Vector3 castPosition)
        {
            // Проверяем, что система зарядов доступна
            if (ResearchChargeSystem.Instance == null)
            {
                Debug.LogError("ResearchChargeSystem не найдена! / ResearchChargeSystem not found!");
                OnCastFailed?.Invoke();
                return;
            }
            
            // Проверяем заряды исследования
            if (!ResearchChargeSystem.Instance.CanCastAtLocation(locationId))
            {
                OnCastFailed?.Invoke();
                return;
            }
            
            // Убираем предыдущий локатор, если он есть
            if (currentLocator != null)
            {
                currentLocator.RemoveLocator();
                currentLocator = null;
            }
            
            // Используем заряд
            if (!ResearchChargeSystem.Instance.UseCastCharge(locationId))
            {
                OnCastFailed?.Invoke();
                return;
            }
            
            // Создаём и забрасываем новый локатор
            CastNewLocator(castPosition);
        }
        
        private void CastNewLocator(Vector3 targetPosition)
        {
            // Если префаб не назначен, создаём простой локатор
            if (locatorPrefab == null)
            {
                CreateSimpleLocatorPrefab();
            }
            
            if (locatorPrefab == null)
            {
                Debug.LogError("Не удалось создать locator prefab / Failed to create locator prefab");
                return;
            }
            
            // Создаём локатор
            GameObject locatorObject = Instantiate(locatorPrefab);
            currentLocator = locatorObject.GetComponent<Locator>();
            
            if (currentLocator == null)
            {
                Debug.LogError("Locator component не найден на prefab / Locator component not found on prefab");
                Destroy(locatorObject);
                return;
            }
            
            // Подписываемся на события
            currentLocator.OnLocatorLanded += OnLocatorLanded;
            currentLocator.OnLocatorDestroyed += OnLocatorDestroyed;
            
            // Определяем стартовую позицию
            Vector3 startPos = playerPosition != null ? playerPosition.position : transform.position;
            
            // Запускаем анимацию заброса
            currentLocator.ThrowToPosition(startPos, targetPosition);
            
            OnLocatorCast?.Invoke(targetPosition);
            
            Debug.Log($"Локатор заброшен в позицию: {targetPosition} / Locator cast to position: {targetPosition}");
        }
        
        private void CreateSimpleLocatorPrefab()
        {
            // Создаём простой префаб локатора во время выполнения
            GameObject locatorGO = new GameObject("Locator");
            
            // Добавляем визуальное представление
            SpriteRenderer spriteRenderer = locatorGO.AddComponent<SpriteRenderer>();
            spriteRenderer.color = Color.red;
            
            // Создаём простой спрайт
            Texture2D locatorTexture = CreateCircleTexture(32, Color.red);
            Sprite locatorSprite = Sprite.Create(locatorTexture, new Rect(0, 0, 32, 32), Vector2.one * 0.5f, 100f);
            spriteRenderer.sprite = locatorSprite;
            
            // Добавляем компонент локатора
            locatorGO.AddComponent<Locator>();
            
            // Сохраняем как временный префаб
            locatorPrefab = locatorGO;
            locatorGO.SetActive(false); // Деактивируем шаблон
            
            Debug.Log("Создан простой prefab локатора / Created simple locator prefab");
        }
        
        private Texture2D CreateCircleTexture(int size, Color color)
        {
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            
            Vector2 center = Vector2.one * (size / 2f);
            float radius = size / 2f - 2f;
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float distance = Vector2.Distance(pos, center);
                    
                    if (distance <= radius)
                    {
                        pixels[y * size + x] = color;
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            return texture;
        }
        
        private void OnLocatorLanded()
        {
            Debug.Log("Локатор приземлился и начал сканирование / Locator landed and started scanning");
            // Здесь можно запустить процесс сканирования рыб
        }
        
        private void OnLocatorDestroyed()
        {
            currentLocator = null;
            Debug.Log("Локатор убран / Locator removed");
        }
        
        /// <summary>
        /// Включает/выключает возможность взаимодействия с прудом
        /// Enables/disables pond interaction
        /// </summary>
        public void SetInteractionEnabled(bool enabled)
        {
            canInteract = enabled;
        }
        
        /// <summary>
        /// Принудительно убирает текущий локатор
        /// Forcefully removes current locator
        /// </summary>
        public void ForceRemoveLocator()
        {
            if (currentLocator != null)
            {
                currentLocator.RemoveLocator();
            }
        }
        
        // Getters
        public bool HasActiveLocator => currentLocator != null && currentLocator.IsIdle;
        public Vector3? CurrentLocatorPosition => currentLocator?.transform.position;
        public string LocationId => locationId;
        
        private void OnDrawGizmosSelected()
        {
            // Визуализация зоны заброса в редакторе
            if (playerPosition != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(playerPosition.position, maxCastDistance);
                
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(playerPosition.position, minCastDistance);
            }
        }
    }
}