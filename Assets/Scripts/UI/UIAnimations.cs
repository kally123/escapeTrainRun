using UnityEngine;
using UnityEngine.UI;

namespace EscapeTrainRun.UI
{
    /// <summary>
    /// Reusable UI animation utilities.
    /// </summary>
    public static class UIAnimations
    {
        /// <summary>
        /// Animates a UI element scaling in.
        /// </summary>
        public static System.Collections.IEnumerator ScaleIn(Transform target, float duration, float startScale = 0f, float endScale = 1f)
        {
            float elapsed = 0f;
            Vector3 start = Vector3.one * startScale;
            Vector3 end = Vector3.one * endScale;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = EaseOutBack(elapsed / duration);
                target.localScale = Vector3.Lerp(start, end, t);
                yield return null;
            }

            target.localScale = end;
        }

        /// <summary>
        /// Animates a UI element scaling out.
        /// </summary>
        public static System.Collections.IEnumerator ScaleOut(Transform target, float duration)
        {
            float elapsed = 0f;
            Vector3 start = target.localScale;
            Vector3 end = Vector3.zero;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                target.localScale = Vector3.Lerp(start, end, t);
                yield return null;
            }

            target.localScale = end;
        }

        /// <summary>
        /// Animates a canvas group fading in.
        /// </summary>
        public static System.Collections.IEnumerator FadeIn(CanvasGroup canvasGroup, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = elapsed / duration;
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        /// <summary>
        /// Animates a canvas group fading out.
        /// </summary>
        public static System.Collections.IEnumerator FadeOut(CanvasGroup canvasGroup, float duration)
        {
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }

        /// <summary>
        /// Animates a UI element sliding in from a direction.
        /// </summary>
        public static System.Collections.IEnumerator SlideIn(RectTransform target, SlideDirection direction, float duration, float distance = 500f)
        {
            Vector2 startOffset = GetDirectionOffset(direction) * distance;
            Vector2 endPos = target.anchoredPosition;
            Vector2 startPos = endPos + startOffset;

            target.anchoredPosition = startPos;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = EaseOutCubic(elapsed / duration);
                target.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            target.anchoredPosition = endPos;
        }

        /// <summary>
        /// Animates a UI element sliding out to a direction.
        /// </summary>
        public static System.Collections.IEnumerator SlideOut(RectTransform target, SlideDirection direction, float duration, float distance = 500f)
        {
            Vector2 startPos = target.anchoredPosition;
            Vector2 endOffset = GetDirectionOffset(direction) * distance;
            Vector2 endPos = startPos + endOffset;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                target.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            target.anchoredPosition = endPos;
        }

        /// <summary>
        /// Animates a pulsing effect.
        /// </summary>
        public static System.Collections.IEnumerator Pulse(Transform target, float scale, float duration, int count = 1)
        {
            Vector3 originalScale = target.localScale;
            Vector3 pulseScale = originalScale * scale;

            for (int i = 0; i < count; i++)
            {
                // Scale up
                float elapsed = 0f;
                while (elapsed < duration / 2f)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = elapsed / (duration / 2f);
                    target.localScale = Vector3.Lerp(originalScale, pulseScale, t);
                    yield return null;
                }

                // Scale down
                elapsed = 0f;
                while (elapsed < duration / 2f)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = elapsed / (duration / 2f);
                    target.localScale = Vector3.Lerp(pulseScale, originalScale, t);
                    yield return null;
                }
            }

            target.localScale = originalScale;
        }

        /// <summary>
        /// Animates a shake effect.
        /// </summary>
        public static System.Collections.IEnumerator Shake(Transform target, float intensity, float duration)
        {
            Vector3 originalPos = target.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float damper = 1f - (elapsed / duration);
                float x = Random.Range(-1f, 1f) * intensity * damper;
                float y = Random.Range(-1f, 1f) * intensity * damper;
                target.localPosition = originalPos + new Vector3(x, y, 0f);
                yield return null;
            }

            target.localPosition = originalPos;
        }

        /// <summary>
        /// Animates counting up a number.
        /// </summary>
        public static System.Collections.IEnumerator CountUp(Text textComponent, int targetValue, float duration, string format = "{0}")
        {
            float elapsed = 0f;
            int currentValue = 0;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = EaseOutCubic(elapsed / duration);
                currentValue = Mathf.RoundToInt(Mathf.Lerp(0, targetValue, t));
                textComponent.text = string.Format(format, currentValue);
                yield return null;
            }

            textComponent.text = string.Format(format, targetValue);
        }

        #region Easing Functions

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        private static float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        private static float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }

        #endregion

        #region Helpers

        private static Vector2 GetDirectionOffset(SlideDirection direction)
        {
            return direction switch
            {
                SlideDirection.Left => Vector2.left,
                SlideDirection.Right => Vector2.right,
                SlideDirection.Up => Vector2.up,
                SlideDirection.Down => Vector2.down,
                _ => Vector2.zero
            };
        }

        #endregion
    }

    /// <summary>
    /// Directions for slide animations.
    /// </summary>
    public enum SlideDirection
    {
        Left,
        Right,
        Up,
        Down
    }
}
