using UnityEngine;
using System.Collections.Generic;

namespace EchoOfSilence.Systems
{
    /// <summary>
    /// Система зарядов исследования для ограничения количества забросов локатора
    /// Research charge system for limiting locator casts
    /// </summary>
    public class ResearchChargeSystem : MonoBehaviour
    {
        [Header("Research Charges Settings")]
        [SerializeField] private int maxCharges = 5;
        [SerializeField] private int currentCharges = 5;
        
        [Header("Location Limits")]
        [SerializeField] private Dictionary<string, int> locationCastLimits = new Dictionary<string, int>();
        [SerializeField] private Dictionary<string, int> locationCurrentCasts = new Dictionary<string, int>();
        
        public static ResearchChargeSystem Instance { get; private set; }
        
        // Events
        public System.Action<int> OnChargesChanged;
        public System.Action<string> OnLocationLimitReached;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeLocationLimits();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeLocationLimits()
        {
            // Инициализация лимитов для различных локаций
            // Initialize limits for different locations
            locationCastLimits["Pond_Main"] = 3;
            locationCastLimits["Pond_Forest"] = 2;
            locationCastLimits["Lake_Deep"] = 5;
            
            // Инициализация текущих забросов
            foreach (var location in locationCastLimits.Keys)
            {
                locationCurrentCasts[location] = 0;
            }
        }
        
        /// <summary>
        /// Проверяет, можно ли сделать заброс в указанной локации
        /// Checks if a cast can be made at the specified location
        /// </summary>
        public bool CanCastAtLocation(string locationId)
        {
            if (currentCharges <= 0)
            {
                Debug.Log("Нет зарядов исследования / No research charges available");
                return false;
            }
            
            if (!locationCastLimits.ContainsKey(locationId))
            {
                Debug.LogWarning($"Неизвестная локация: {locationId} / Unknown location: {locationId}");
                return false;
            }
            
            if (locationCurrentCasts[locationId] >= locationCastLimits[locationId])
            {
                Debug.Log($"Достигнут лимит забросов для локации {locationId} / Cast limit reached for location {locationId}");
                OnLocationLimitReached?.Invoke(locationId);
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Использует заряд для заброса в указанной локации
        /// Uses a charge for casting at the specified location
        /// </summary>
        public bool UseCastCharge(string locationId)
        {
            if (!CanCastAtLocation(locationId))
                return false;
            
            currentCharges--;
            locationCurrentCasts[locationId]++;
            
            OnChargesChanged?.Invoke(currentCharges);
            
            Debug.Log($"Заряд использован. Осталось: {currentCharges} / Charge used. Remaining: {currentCharges}");
            Debug.Log($"Забросы в {locationId}: {locationCurrentCasts[locationId]}/{locationCastLimits[locationId]}");
            
            return true;
        }
        
        /// <summary>
        /// Добавляет заряды исследования
        /// Adds research charges
        /// </summary>
        public void AddCharges(int amount)
        {
            currentCharges = Mathf.Min(currentCharges + amount, maxCharges);
            OnChargesChanged?.Invoke(currentCharges);
            Debug.Log($"Добавлено зарядов: {amount}. Всего: {currentCharges} / Added charges: {amount}. Total: {currentCharges}");
        }
        
        /// <summary>
        /// Сбрасывает лимиты локации (например, при смене дня)
        /// Resets location limits (e.g., when day changes)
        /// </summary>
        public void ResetLocationLimits()
        {
            foreach (var location in locationCastLimits.Keys)
            {
                locationCurrentCasts[location] = 0;
            }
            Debug.Log("Лимиты локаций сброшены / Location limits reset");
        }
        
        // Getters
        public int CurrentCharges => currentCharges;
        public int MaxCharges => maxCharges;
        public int GetLocationCasts(string locationId) => locationCurrentCasts.GetValueOrDefault(locationId, 0);
        public int GetLocationLimit(string locationId) => locationCastLimits.GetValueOrDefault(locationId, 0);
    }
}