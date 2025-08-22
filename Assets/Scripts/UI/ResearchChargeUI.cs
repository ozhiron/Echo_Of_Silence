using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EchoOfSilence.Systems;

namespace EchoOfSilence.UI
{
    /// <summary>
    /// UI для отображения зарядов исследования
    /// UI for displaying research charges
    /// </summary>
    public class ResearchChargeUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI chargeCountText;
        [SerializeField] private Slider chargeSlider;
        [SerializeField] private Image[] chargeIcons;
        [SerializeField] private GameObject notificationPanel;
        [SerializeField] private TextMeshProUGUI notificationText;
        
        [Header("Visual Settings")]
        [SerializeField] private Color chargeAvailableColor = Color.white;
        [SerializeField] private Color chargeUsedColor = Color.gray;
        [SerializeField] private float notificationDuration = 3f;
        
        private ResearchChargeSystem researchChargeSystem;
        private Coroutine notificationCoroutine;
        
        /// <summary>
        /// Инициализирует UI с системой зарядов
        /// Initializes UI with charge system
        /// </summary>
        public void Initialize(ResearchChargeSystem chargeSystem)
        {
            researchChargeSystem = chargeSystem;
            
            if (researchChargeSystem != null)
            {
                // Подписываемся на события
                researchChargeSystem.OnChargesChanged += UpdateChargeDisplay;
                researchChargeSystem.OnLocationLimitReached += ShowLocationLimitNotification;
                
                // Инициализируем отображение
                UpdateChargeDisplay(researchChargeSystem.CurrentCharges);
                UpdateSlider();
            }
            
            // Скрываем панель уведомлений
            if (notificationPanel != null)
                notificationPanel.SetActive(false);
        }
        
        /// <summary>
        /// Обновляет отображение количества зарядов
        /// Updates charge count display
        /// </summary>
        public void UpdateChargeDisplay(int currentCharges)
        {
            // Обновляем текст
            if (chargeCountText != null)
            {
                chargeCountText.text = $"{currentCharges}/{researchChargeSystem.MaxCharges}";
            }
            
            // Обновляем слайдер
            UpdateSlider();
            
            // Обновляем иконки зарядов
            UpdateChargeIcons(currentCharges);
        }
        
        private void UpdateSlider()
        {
            if (chargeSlider != null && researchChargeSystem != null)
            {
                chargeSlider.maxValue = researchChargeSystem.MaxCharges;
                chargeSlider.value = researchChargeSystem.CurrentCharges;
            }
        }
        
        private void UpdateChargeIcons(int currentCharges)
        {
            if (chargeIcons == null) return;
            
            for (int i = 0; i < chargeIcons.Length; i++)
            {
                if (chargeIcons[i] != null)
                {
                    // Активные заряды - белый цвет, использованные - серый
                    chargeIcons[i].color = i < currentCharges ? chargeAvailableColor : chargeUsedColor;
                }
            }
        }
        
        /// <summary>
        /// Показывает уведомление о достижении лимита локации
        /// Shows location limit notification
        /// </summary>
        public void ShowLocationLimitNotification(string locationId)
        {
            string message = $"Лимит забросов достигнут для {locationId}!\nCast limit reached for {locationId}!";
            ShowNotification(message);
        }
        
        /// <summary>
        /// Показывает уведомление о нехватке зарядов
        /// Shows insufficient charges notification
        /// </summary>
        public void ShowInsufficientChargesNotification()
        {
            string message = "Нет зарядов исследования!\nNo research charges available!";
            ShowNotification(message);
        }
        
        /// <summary>
        /// Показывает общее уведомление
        /// Shows general notification
        /// </summary>
        public void ShowNotification(string message)
        {
            if (notificationPanel == null || notificationText == null) return;
            
            // Останавливаем предыдущее уведомление
            if (notificationCoroutine != null)
            {
                StopCoroutine(notificationCoroutine);
            }
            
            // Показываем новое уведомление
            notificationText.text = message;
            notificationPanel.SetActive(true);
            
            notificationCoroutine = StartCoroutine(HideNotificationAfterDelay());
        }
        
        private System.Collections.IEnumerator HideNotificationAfterDelay()
        {
            yield return new WaitForSeconds(notificationDuration);
            
            if (notificationPanel != null)
                notificationPanel.SetActive(false);
        }
        
        /// <summary>
        /// Показывает информацию о текущих лимитах локации
        /// Shows current location limits info
        /// </summary>
        public void ShowLocationInfo(string locationId)
        {
            if (researchChargeSystem == null) return;
            
            int currentCasts = researchChargeSystem.GetLocationCasts(locationId);
            int maxCasts = researchChargeSystem.GetLocationLimit(locationId);
            
            string message = $"Локация {locationId}:\n{currentCasts}/{maxCasts} забросов\nLocation {locationId}:\n{currentCasts}/{maxCasts} casts";
            ShowNotification(message);
        }
        
        private void OnDestroy()
        {
            // Отписываемся от событий
            if (researchChargeSystem != null)
            {
                researchChargeSystem.OnChargesChanged -= UpdateChargeDisplay;
                researchChargeSystem.OnLocationLimitReached -= ShowLocationLimitNotification;
            }
        }
    }
}