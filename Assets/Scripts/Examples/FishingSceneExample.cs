using UnityEngine;
using EchoOfSilence.Managers;
using EchoOfSilence.Systems;
using EchoOfSilence.UI;

namespace EchoOfSilence.Examples
{
    /// <summary>
    /// Пример настройки сцены с системой рыбалки/сканирования
    /// Example scene setup for fishing/scanning system
    /// </summary>
    public class FishingSceneExample : MonoBehaviour
    {
        [Header("Example Setup")]
        [SerializeField] private bool autoSetupScene = true;
        [SerializeField] private GameObject pondPrefab;
        [SerializeField] private GameObject locatorPrefab;
        [SerializeField] private GameObject playerPrefab;
        
        [Header("Scene Configuration")]
        [SerializeField] private Vector3 pondPosition = Vector3.zero;
        [SerializeField] private Vector3 playerStartPosition = new Vector3(-5f, 0f, 0f);
        [SerializeField] private string pondLocationId = "Pond_Main";
        
        private void Start()
        {
            if (autoSetupScene)
            {
                SetupExampleScene();
            }
        }
        
        /// <summary>
        /// Автоматически настраивает пример сцены
        /// Automatically sets up example scene
        /// </summary>
        [ContextMenu("Setup Example Scene")]
        public void SetupExampleScene()
        {
            Debug.Log("Настройка примера сцены / Setting up example scene");
            
            // 1. Создаём GameManager если его нет
            if (GameManager.Instance == null)
            {
                CreateGameManager();
            }
            
            // 2. Создаём систему зарядов если её нет
            if (ResearchChargeSystem.Instance == null)
            {
                CreateResearchChargeSystem();
            }
            
            // 3. Создаём пруд с системой взаимодействия
            CreatePondWithInteraction();
            
            // 4. Создаём игрока (опционально)
            CreatePlayer();
            
            // 5. Создаём простой UI
            CreateSimpleUI();
            
            Debug.Log("Пример сцены настроен! / Example scene setup complete!");
            Debug.Log("Кликните ЛКМ по пруду для заброса локатора / Left-click on pond to cast locator");
        }
        
        private void CreateGameManager()
        {
            GameObject gameManagerGO = new GameObject("GameManager");
            GameManager gameManager = gameManagerGO.AddComponent<GameManager>();
            
            Debug.Log("GameManager создан / GameManager created");
        }
        
        private void CreateResearchChargeSystem()
        {
            GameObject chargeSystemGO = new GameObject("ResearchChargeSystem");
            ResearchChargeSystem chargeSystem = chargeSystemGO.AddComponent<ResearchChargeSystem>();
            
            Debug.Log("ResearchChargeSystem создан / ResearchChargeSystem created");
        }
        
        private void CreatePondWithInteraction()
        {
            // Создаём объект пруда
            GameObject pondGO = new GameObject("Pond");
            pondGO.transform.position = pondPosition;
            
            // Добавляем коллайдер для определения области пруда
            CircleCollider2D pondCollider = pondGO.AddComponent<CircleCollider2D>();
            pondCollider.radius = 3f;
            pondCollider.isTrigger = true;
            
            // Устанавливаем слой пруда (создаём если не существует)
            int pondLayer = LayerMask.NameToLayer("Pond");
            if (pondLayer == -1)
            {
                Debug.LogWarning("Слой 'Pond' не найден. Создайте его в Layer Manager / 'Pond' layer not found. Create it in Layer Manager");
                pondLayer = 0; // Default layer
            }
            pondGO.layer = pondLayer;
            
            // Добавляем визуальное представление пруда
            SpriteRenderer pondSprite = pondGO.AddComponent<SpriteRenderer>();
            pondSprite.color = new Color(0.2f, 0.5f, 1f, 0.7f); // Голубой полупрозрачный
            
            // Создаём простой спрайт для пруда
            Texture2D pondTexture = CreateCircleTexture(128, Color.cyan);
            Sprite pondSpriteAsset = Sprite.Create(pondTexture, new Rect(0, 0, 128, 128), Vector2.one * 0.5f, 100f);
            pondSprite.sprite = pondSpriteAsset;
            
            // Добавляем систему взаимодействия с прудом
            PondInteractionSystem pondInteraction = pondGO.AddComponent<PondInteractionSystem>();
            
            // Настройка через рефлексию (так как поля private)
            var locationIdField = typeof(PondInteractionSystem).GetField("locationId", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            locationIdField?.SetValue(pondInteraction, pondLocationId);
            
            Debug.Log($"Пруд создан в позиции {pondPosition} / Pond created at position {pondPosition}");
        }
        
        private void CreatePlayer()
        {
            GameObject playerGO = new GameObject("Player");
            playerGO.transform.position = playerStartPosition;
            
            // Добавляем визуальное представление игрока
            SpriteRenderer playerSprite = playerGO.AddComponent<SpriteRenderer>();
            playerSprite.color = Color.green;
            
            // Создаём простой спрайт для игрока
            Texture2D playerTexture = CreateCircleTexture(64, Color.green);
            Sprite playerSpriteAsset = Sprite.Create(playerTexture, new Rect(0, 0, 64, 64), Vector2.one * 0.5f, 100f);
            playerSprite.sprite = playerSpriteAsset;
            
            Debug.Log($"Игрок создан в позиции {playerStartPosition} / Player created at position {playerStartPosition}");
        }
        
        private void CreateSimpleUI()
        {
            // Создаём Canvas
            GameObject canvasGO = new GameObject("UI Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // Создаём простой текст для отображения зарядов
            GameObject textGO = new GameObject("ChargeText");
            textGO.transform.SetParent(canvasGO.transform);
            
            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 1f);
            textRect.anchorMax = new Vector2(0f, 1f);
            textRect.anchoredPosition = new Vector2(100f, -50f);
            textRect.sizeDelta = new Vector2(200f, 50f);
            
            var text = textGO.AddComponent<TMPro.TextMeshProUGUI>();
            text.text = "Заряды: 5/5\nCharges: 5/5";
            text.fontSize = 18;
            text.color = Color.white;
            
            Debug.Log("Простой UI создан / Simple UI created");
        }
        
        /// <summary>
        /// Создаёт простую круглую текстуру
        /// Creates a simple circle texture
        /// </summary>
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
        
        /// <summary>
        /// Тестовый метод для добавления зарядов
        /// Test method for adding charges
        /// </summary>
        [ContextMenu("Add 3 Charges")]
        public void TestAddCharges()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddResearchCharges(3);
            }
        }
        
        /// <summary>
        /// Тестовый метод для сброса лимитов
        /// Test method for resetting limits
        /// </summary>
        [ContextMenu("Reset Daily Limits")]
        public void TestResetLimits()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetDailyLimits();
            }
        }
    }
}