using System;
using System.Collections;
using UnityEngine;

namespace Coffee.UISoftMask.Demos
{
    public class SoftMask_Demo_Animation : MonoBehaviour
    {
        [SerializeField] private AnimationType m_AnimationType;

        public float timeScale { get; set; } = 1;

        private void OnEnable()
        {
            switch (m_AnimationType)
            {
                case AnimationType.BlinkHitArea:
                    StartCoroutine(Co_Flash(GetComponent<CanvasGroup>()));
                    break;
                case AnimationType.IrisOut:
                    StartCoroutine(Co_IrisOut(transform));
                    StartCoroutine(Co_Rotate(transform));
                    break;
                case AnimationType.Move:
                    StartCoroutine(Co_Move(transform));
                    break;
            }
        }

        private IEnumerator Co_IrisOut(Transform t)
        {
            while (true)
            {
                yield return Co_Tween(10, 1, 1, v => t.localScale = Vector3.one * v);
                yield return new WaitForSeconds(1);
                yield return Co_Tween(1, 0, 0.5f, v => t.localScale = Vector3.one * v);
                yield return new WaitForSeconds(1);
                yield return Co_Tween(0, 10, 1, v => t.localScale = Vector3.one * v);
                yield return new WaitForSeconds(1);
            }
        }

        private IEnumerator Co_Rotate(Transform t)
        {
            while (true)
            {
                yield return Co_Tween(0, 360, 2, v => t.localRotation = Quaternion.Euler(0, 0, v));
            }
        }

        private IEnumerator Co_Flash(CanvasGroup cg)
        {
            while (true)
            {
                yield return Co_Tween(0, 1, 0.5f, v => cg.alpha = v);
                yield return new WaitForSeconds(1);
                yield return Co_Tween(1, 0, 0.5f, v => cg.alpha = v);
                yield return new WaitForSeconds(1);
            }
        }

        private IEnumerator Co_Move(Transform t)
        {
            yield return null;
            while (true)
            {
                t.localRotation = Quaternion.Euler(0, 0, 30 * (Mathf.PingPong(Time.realtimeSinceStartup, 2) - 1));
                t.localScale = Vector3.one * (Mathf.PingPong(Time.realtimeSinceStartup, 1) + 1);
                yield return null;
            }
        }

        private IEnumerator Co_Tween(float from, float to, float duration, Action<float> callback)
        {
            var value = from;
            var diff = (to - from) / duration;

            callback(value);

            while (0 < diff ? value < to : to < value)
            {
                yield return null;

                value += diff * Time.deltaTime * timeScale;
                callback(0 < diff ? Mathf.Clamp(value, from, to) : Mathf.Clamp(value, to, from));
            }
        }

        private enum AnimationType
        {
            Move,
            BlinkHitArea,
            IrisOut
        }
    }
}
