using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public abstract class BaseAnimationProperty<T>
{
    public UnityEvent<T> OnValueChange;

    public abstract void Animate(float t);
}

[Serializable]
public class FloatAnimationProperty : BaseAnimationProperty<float>
{
    public float min;
    public float max;

    public override void Animate(float t)
    {
        t = Mathf.Clamp01(t);
        float value = Mathf.Lerp(min, max, t);
        OnValueChange.Invoke(value);
    }
}

[Serializable]
public class ColorAnimationProperty : BaseAnimationProperty<Color>
{
    public Color startColor = Color.white;
    public Color endColor = Color.black;

    public override void Animate(float t)
    {
        t = Mathf.Clamp01(t);
        Color color = Color.Lerp(startColor, endColor, t);
        OnValueChange.Invoke(color);
    }
}

public class PropertyAnimator : MonoBehaviour
{
    [Header("Float Properties")]
    [SerializeField] private FloatAnimationProperty[] floatProperties;

    [Header("Color Properties")]
    [SerializeField] private ColorAnimationProperty[] colorProperties;

    private float currentTimeline;
    private float currentDuration;

    private void LateUpdate()
    {
        AnimateOverTime();
    }

    private void AnimateOverTime()
    {
        if (currentDuration <= 0)
            return;

        if (currentTimeline < 1f)
        {
            currentTimeline += Time.deltaTime * (1 / currentDuration);
            AnimateProperties(currentTimeline);

            if (currentTimeline >= 1f)
                currentDuration = 0f;
        }
    }

    public void PlayAnimation(float duration)
    {
        currentTimeline = 0f;
        currentDuration = duration;
    }

    public void AnimateProperties(float timeline)
    {
        foreach (var p in floatProperties)
            p.Animate(timeline);

        foreach (var c in colorProperties)
            c.Animate(timeline);
    }
}