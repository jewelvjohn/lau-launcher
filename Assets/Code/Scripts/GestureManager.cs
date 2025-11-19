using UnityEngine;
using System.Collections.Generic;

public enum GestureType
{
    None,
    SwipeUp,
    SwipeDown,
    SwipeLeft,
    SwipeRight
}

public class GestureManager : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private float gestureThresold = 20f;

    [Header("References")]
    [SerializeField] private List<GestureHandler> listeners;

    private GestureType gestureType;

    private void Awake()
    {
        gestureType = GestureType.None;
    }

    public void UserGestureStart(Vector2 position, float time)
    {
        foreach (GestureHandler listener in listeners)
        {
            listener.OnGestureStart(position);
        }
    }

    public void UserGestureEnd(Vector2 position, float time)
    {
        foreach (GestureHandler listener in listeners)
        {
            listener.OnGestureEnd(position);
        }

        gestureType = GestureType.None;
    }

    public void UserGesture(Vector2 delta)
    {
        ProcessGesture(delta);

        if (gestureType != GestureType.None)
        {
            foreach (GestureHandler listener in listeners)
            {
                listener.OnGesture(gestureType, delta);
            }
        }
    }

    private void ProcessGesture(Vector2 delta)
    {
        if (gestureType != GestureType.None)
            return;

        if (delta.magnitude < gestureThresold)
            return;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            gestureType = delta.x < 0 ? GestureType.SwipeRight : GestureType.SwipeLeft;
        else
            gestureType = delta.y < 0 ? GestureType.SwipeUp : GestureType.SwipeDown;
    }
}
