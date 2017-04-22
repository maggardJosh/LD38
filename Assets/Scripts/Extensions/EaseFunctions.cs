using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Extensions
{

    public class EaseFunctions
    {
        public enum Type
        {
            Linear,
            CircInOut,
            CircOut,
            CircIn,
            BackIn,
            BackOut,
            BackInOut,
            BounceIn,
            BounceOut,
            BounceInOut
        }

        public static float Ease(AnimationCurve curve, float currentTime, float startValue, float valueDelta, float time)
        {
            return startValue + (valueDelta * curve.Evaluate(currentTime / time));
        }

        public static Vector3 Ease(AnimationCurve curve, float currentTime, Vector3 startValue, Vector3 valueDelta, float time)
        {
            return startValue + (curve.Evaluate(currentTime / time) * valueDelta);
        }

        public static float Ease(Type type, float currentTime, float startValue, float valueDelta, float time)
        {
            return GetEaseFunc(type).Invoke(Mathf.Min(currentTime, time), startValue, valueDelta, time);
        }

        public static Vector3 Ease(Type type, float currentTime, Vector3 startValue, Vector3 valueDelta, float time)
        {
            return new Vector3(Ease(type, currentTime, startValue.x, valueDelta.x, time),
                Ease(type, currentTime, startValue.y, valueDelta.y, time),
                Ease(type, currentTime, startValue.z, valueDelta.z, time));
        }

        private static Func<float, float, float, float, float> GetEaseFunc(Type t)
        {
            switch (t)
            {
                case Type.Linear:
                    return LinearEasing;
                case Type.CircInOut:
                    return CircInOutEasing;
                case Type.CircOut:
                    return CircOutEasing;
                case Type.CircIn:
                    return CircInEasing;
                case Type.BackIn:
                    return BackInEasing;
                case Type.BackOut:
                    return BackOutEasing;
                case Type.BackInOut:
                    return BackInOutEasing;
                case Type.BounceIn:
                    return BounceInEasing;
                case Type.BounceOut:
                    return BounceOutEasing;
                case Type.BounceInOut:
                    return BounceInOutEasing;
                default:
                    throw new Exception("Unhandled ease function type: " + t);
            }

        }

        private static float LinearEasing(float t, float b, float c, float d)
        {
            return c * t / d + b;
        }
        private static float CircInEasing(float t, float b, float c, float d)
        {
            t /= -d;
            return -c * (Mathf.Sqrt(1 - t * t) - 1) + b;
        }
        private static float CircOutEasing(float t, float b, float c, float d)
        {
            t /= d;
            t--;
            return c * Mathf.Sqrt(1 - t * t) + b;
        }
        private static float CircInOutEasing(float t, float b, float c, float d)
        {
            t /= d / 2;
            if (t < 1) return -c / 2f * (Mathf.Sqrt(1 - t * t) - 1) + b;
            t -= 2;
            return c / 2 * (Mathf.Sqrt(1 - t * t) + 1) + b;
        }
        private static float BackInEasing(float t, float b, float c, float d)
        {

            return c * (t /= d) * t * ((1.70158f + 1) * t - 1.70158f) + b;
        }

        private static float BackOutEasing(float t, float b, float c, float d)
        {
            return c * ((t = t / d - 1) * t * ((1.70158f + 1) * t + 1.70158f) + 1) + b;
        }


        private static float BackInOutEasing(float t, float b, float c, float d)
        {
            float s = 1.70158f;
            if ((t /= d / 2) < 1)
            {
                return c / 2 * (t * t * (((s *= (1.525f)) + 1) * t - s)) + b;
            }
            return c / 2 * ((t -= 2) * t * (((s *= (1.525f)) + 1) * t + s) + 2) + b;
        }

        private static float BounceOutEasing(float t, float b, float c, float d)
        {
            if ((t /= d) < (1 / 2.75))
            {
                return c * (7.5625f * t * t) + b;
            }
            else if (t < (2 / 2.75))
            {
                return c * (7.5625f * (t -= (1.5f / 2.75f)) * t + .75f) + b;
            }
            else if (t < (2.5 / 2.75))
            {
                return c * (7.5625f * (t -= (2.25f / 2.75f)) * t + .9375f) + b;
            }
            else
            {
                return c * (7.5625f * (t -= (2.625f / 2.75f)) * t + .984375f) + b;
            }
        }

        private static float BounceInEasing(float t, float b, float c, float d)
        {
            return c - BounceOutEasing(d - t, 0, c, d) + b;
        }

        private static float BounceInOutEasing(float t, float b, float c, float d)
        {
            if (t < d / 2)
                return BounceInEasing(t * 2, 0, c, d) * 0.5f + b;
            else
                return BounceOutEasing(t * 2 - d, 0, c, d) * .5f + c * 0.5f + b;
        }

        public static IEnumerator DelayAction(float time, Action action)
        {
            if (time > 0)
                yield return new WaitForSeconds(time);
            if (action != null)
                action.Invoke();
        }
        public static IEnumerator GenericTween(Type easeType, float time, Action<float> updateAction, Action beginAction = null, Action endAction = null)
        {
            updateAction(0);
            if (beginAction != null)
                beginAction.Invoke();
            float count = 0;
            while (count < time)
            {

                updateAction(Ease(easeType, count, 0, 1, time));
                count += Time.deltaTime;
                yield return null;
            }
            updateAction(1);
            if (endAction != null)
                endAction.Invoke();
        }
        public static IEnumerator TweenRot(Type easeType, Quaternion beginRot, Quaternion endRot, float time, Action<Quaternion> updateAction)
        {
            updateAction(beginRot);
            float count = 0;
            while (count < time)
            {
                updateAction.Invoke(EaseRot(easeType, beginRot, endRot, count, time));
                count += Time.deltaTime;
                yield return null;
            }
            updateAction.Invoke(endRot);

        }

        private static Quaternion EaseRot(Type easeType, Quaternion orig, Quaternion dest, float count, float time)
        {
            return Quaternion.Lerp(orig, dest, Ease(easeType, count, 0, 1, time));
        }
        public static IEnumerator TweenPos(GameObject obj, Type easeType, Vector3 startPos, Vector3 endPos, float delay, float moveTime, Action beginAction = null, Action endAction = null)
        {
            return TweenPos(obj, easeType, easeType, easeType, startPos, endPos, delay, moveTime, beginAction, endAction);
        }
        public static IEnumerator TweenPos(Type easeType, Vector3 startPos, Vector3 endPos, float delay, float moveTime, Action<Vector3> updateAction, Action beginAction = null, Action endAction = null)
        {
            return TweenPos(easeType, easeType, easeType, startPos, endPos, delay, moveTime, updateAction, beginAction, endAction);
        }
        public static IEnumerator TweenPos(GameObject obj, Type xEaseType, Type yEaseType, Type zEaseType, Vector3 startPos, Vector3 endPos, float delay, float moveTime, Action beginAction = null, Action endAction = null)
        {
            return TweenPos(xEaseType, yEaseType, zEaseType, startPos, endPos, delay, moveTime, (newPos) => { if (obj != null) obj.transform.position = newPos; }, beginAction, endAction);
        }

        public static IEnumerator TweenPos(Type xEaseType, Type yEaseType, Type zEaseType, Vector3 startPos, Vector3 endPos, float delay, float moveTime, Action<Vector3> updateAction, Action beginAction = null, Action endAction = null)
        {
            if (delay > 0)
                yield return new WaitForSeconds(delay);
            if (beginAction != null)
                beginAction.Invoke();
            updateAction.Invoke(startPos);
            Vector3 diff = endPos - startPos;
            float count = 0;
            while (count < moveTime)
            {
                updateAction.Invoke(new Vector3(Ease(xEaseType, count, startPos.x, diff.x, moveTime),
                    Ease(yEaseType, count, startPos.y, diff.y, moveTime),
                    Ease(zEaseType, count, startPos.z, diff.z, moveTime)));
                count += Time.deltaTime;
                yield return null;
            }
            updateAction.Invoke(endPos);
            if (endAction != null)
                endAction.Invoke();

        }
    }
}
