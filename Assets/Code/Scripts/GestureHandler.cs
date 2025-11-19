using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class GestureEvent
{
    public GestureType type;
    public bool absolute = false;
    public bool normalize = false;
    public float normalizeMax = 100f;
    public float normalizeMin = 0f;
    public UnityEvent<float> OnMove;
}

public class GestureHandler : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private List<GestureEvent> gestureEvents;

    [Header("Reference")]
    [SerializeField] private RectTransform gestureArea;

    private bool isGestureActive = false;

    public void OnGestureStart(Vector2 position)
    {
        if (gestureArea)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                gestureArea,
                position,
                null,
                out localPoint
            );

            if (gestureArea.rect.Contains(localPoint))
                isGestureActive = true;
            else
                isGestureActive = false;
        }
        else
        {
            isGestureActive = true;
        }
    }

    public void OnGestureEnd(Vector2 position)
    {
        isGestureActive = false;
    }

    public void OnGesture(GestureType gestureType, Vector2 delta)
    {
        if (isGestureActive && gestureEvents != null)
        {
            foreach (GestureEvent gestureEvent in gestureEvents)
            {
                if (gestureEvent.type == gestureType)
                {
                    float value = (gestureEvent.type == GestureType.SwipeLeft || gestureEvent.type == GestureType.SwipeRight) ? delta.x : delta.y;

                    if (gestureEvent.absolute)
                    {
                        value = Mathf.Abs(value);
                    }
                    
                    if (gestureEvent.normalize)
                    {
                        value += gestureEvent.normalizeMin;
                        value /= (gestureEvent.normalizeMax + gestureEvent.normalizeMin);
                    }

                    gestureEvent.OnMove?.Invoke(value);
                }
            }
        }
    }
}
