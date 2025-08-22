using UnityEngine;
using System.Collections;

namespace EchoOfSilence.Core
{
    /// <summary>
    /// Локатор (поплавок) для сканирования рыб в водоёме
    /// Locator (float) for scanning fish in the pond
    /// </summary>
    public class Locator : MonoBehaviour
    {
        [Header("Locator Settings")]
        [SerializeField] private float idleAnimationSpeed = 1f;
        [SerializeField] private float bobAmount = 0.1f;
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        [Header("Animation")]
        [SerializeField] private AnimationCurve throwCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float throwDuration = 1.5f;
        [SerializeField] private float throwHeight = 2f;
        
        private Vector3 startPosition;
        private Vector3 targetPosition;
        private bool isIdle = false;
        private bool isThrowingAnimation = false;
        
        // Events
        public System.Action OnLocatorLanded;
        public System.Action OnLocatorDestroyed;
        
        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        /// <summary>
        /// Запускает анимацию заброса локатора
        /// Starts the locator throw animation
        /// </summary>
        public void ThrowToPosition(Vector3 startPos, Vector3 targetPos)
        {
            startPosition = startPos;
            targetPosition = targetPos;
            transform.position = startPosition;
            
            StartCoroutine(ThrowAnimation());
        }
        
        private IEnumerator ThrowAnimation()
        {
            isThrowingAnimation = true;
            float elapsedTime = 0f;
            
            while (elapsedTime < throwDuration)
            {
                float progress = elapsedTime / throwDuration;
                float curveValue = throwCurve.Evaluate(progress);
                
                // Параболическая траектория
                Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, curveValue);
                currentPos.y += Mathf.Sin(progress * Mathf.PI) * throwHeight;
                
                transform.position = currentPos;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Финальная позиция
            transform.position = targetPosition;
            isThrowingAnimation = false;
            
            // Эффект попадания в воду
            PlayWaterSplashEffect();
            
            // Начинаем idle анимацию
            StartIdleAnimation();
            
            OnLocatorLanded?.Invoke();
        }
        
        private void PlayWaterSplashEffect()
        {
            // TODO: Добавить эффект всплеска воды
            Debug.Log("Локатор попал в воду! / Locator hit the water!");
            
            // Здесь можно добавить партиклы, звук, и т.д.
            // Here you can add particles, sound, etc.
        }
        
        private void StartIdleAnimation()
        {
            isIdle = true;
            StartCoroutine(IdleAnimation());
        }
        
        private IEnumerator IdleAnimation()
        {
            Vector3 basePosition = transform.position;
            float time = 0f;
            
            while (isIdle && !isThrowingAnimation)
            {
                // Плавающая анимация
                float yOffset = Mathf.Sin(time * idleAnimationSpeed) * bobAmount;
                transform.position = basePosition + Vector3.up * yOffset;
                
                time += Time.deltaTime;
                yield return null;
            }
        }
        
        /// <summary>
        /// Останавливает idle анимацию и убирает локатор
        /// Stops idle animation and removes the locator
        /// </summary>
        public void RemoveLocator()
        {
            isIdle = false;
            OnLocatorDestroyed?.Invoke();
            
            // Анимация исчезновения (опционально)
            StartCoroutine(FadeOut());
        }
        
        private IEnumerator FadeOut()
        {
            float fadeTime = 0.5f;
            float elapsedTime = 0f;
            Color originalColor = spriteRenderer.color;
            
            while (elapsedTime < fadeTime)
            {
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            Destroy(gameObject);
        }
        
        /// <summary>
        /// Проверяет, находится ли локатор в состоянии idle
        /// Checks if the locator is in idle state
        /// </summary>
        public bool IsIdle => isIdle && !isThrowingAnimation;
        
        /// <summary>
        /// Проверяет, проигрывается ли анимация заброса
        /// Checks if throw animation is playing
        /// </summary>
        public bool IsThrowingAnimation => isThrowingAnimation;
    }
}