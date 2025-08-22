using UnityEngine;
using EchoOfSilence.Systems;
using EchoOfSilence.UI;

namespace EchoOfSilence.Managers
{
    /// <summary>
    /// Главный менеджер игры, координирующий все системы
    /// Main game manager coordinating all systems
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private ResearchChargeSystem researchChargeSystem;
        [SerializeField] private PondInteractionSystem[] pondSystems;
        [SerializeField] private ResearchChargeUI researchChargeUI;
        
        [Header("Game Settings")]
        [SerializeField] private bool initializeOnStart = true;
        
        public static GameManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void Start()
        {
            if (initializeOnStart)
            {
                InitializeGame();
            }
        }
        
        private void InitializeGame()
        {
            // Инициализация системы зарядов
            if (researchChargeSystem == null)
            {
                GameObject chargeSystemGO = new GameObject("ResearchChargeSystem");
                researchChargeSystem = chargeSystemGO.AddComponent<ResearchChargeSystem>();
            }
            
            // Подписка на события системы зарядов
            if (researchChargeSystem != null)
            {
                researchChargeSystem.OnChargesChanged += OnChargesChanged;
                researchChargeSystem.OnLocationLimitReached += OnLocationLimitReached;
            }
            
            // Инициализация систем прудов
            InitializePondSystems();
            
            // Инициализация UI
            if (researchChargeUI != null)
            {
                researchChargeUI.Initialize(researchChargeSystem);
            }
            
            Debug.Log("Игра инициализирована / Game initialized");
        }
        
        private void InitializePondSystems()
        {
            if (pondSystems == null) return;
            
            foreach (var pondSystem in pondSystems)
            {
                if (pondSystem != null)
                {
                    // Подписываемся на события прудов
                    pondSystem.OnLocatorCast += OnLocatorCast;
                    pondSystem.OnCastFailed += OnCastFailed;
                }
            }
        }
        
        private void OnChargesChanged(int newChargeCount)
        {
            Debug.Log($"Заряды изменились: {newChargeCount} / Charges changed: {newChargeCount}");
            
            // Обновляем UI
            if (researchChargeUI != null)
            {
                researchChargeUI.UpdateChargeDisplay(newChargeCount);
            }
        }
        
        private void OnLocationLimitReached(string locationId)
        {
            Debug.Log($"Достигнут лимит для локации: {locationId} / Location limit reached: {locationId}");
            
            // Показываем уведомление игроку
            ShowLocationLimitNotification(locationId);
        }
        
        private void OnLocatorCast(Vector3 position)
        {
            Debug.Log($"Локатор заброшен в позицию: {position} / Locator cast to position: {position}");
            
            // Здесь можно добавить звуковые эффекты, уведомления и т.д.
        }
        
        private void OnCastFailed()
        {
            Debug.Log("Заброс не удался / Cast failed");
            
            // Показываем уведомление о неудачном забросе
            ShowCastFailedNotification();
        }
        
        private void ShowLocationLimitNotification(string locationId)
        {
            // TODO: Показать UI уведомление о достижении лимита локации
            Debug.Log($"UI: Лимит забросов достигнут для {locationId}");
        }
        
        private void ShowCastFailedNotification()
        {
            // TODO: Показать UI уведомление о неудачном забросе
            Debug.Log("UI: Заброс не удался");
        }
        
        /// <summary>
        /// Добавляет заряды исследования (например, при покупке или находке)
        /// Adds research charges (e.g., when buying or finding)
        /// </summary>
        public void AddResearchCharges(int amount)
        {
            if (researchChargeSystem != null)
            {
                researchChargeSystem.AddCharges(amount);
            }
        }
        
        /// <summary>
        /// Сбрасывает лимиты локаций (например, при новом дне)
        /// Resets location limits (e.g., on new day)
        /// </summary>
        public void ResetDailyLimits()
        {
            if (researchChargeSystem != null)
            {
                researchChargeSystem.ResetLocationLimits();
            }
        }
        
        /// <summary>
        /// Принудительно убирает все локаторы
        /// Forcefully removes all locators
        /// </summary>
        public void RemoveAllLocators()
        {
            if (pondSystems != null)
            {
                foreach (var pondSystem in pondSystems)
                {
                    pondSystem?.ForceRemoveLocator();
                }
            }
        }
        
        private void OnDestroy()
        {
            // Отписываемся от событий
            if (researchChargeSystem != null)
            {
                researchChargeSystem.OnChargesChanged -= OnChargesChanged;
                researchChargeSystem.OnLocationLimitReached -= OnLocationLimitReached;
            }
            
            if (pondSystems != null)
            {
                foreach (var pondSystem in pondSystems)
                {
                    if (pondSystem != null)
                    {
                        pondSystem.OnLocatorCast -= OnLocatorCast;
                        pondSystem.OnCastFailed -= OnCastFailed;
                    }
                }
            }
        }
    }
}