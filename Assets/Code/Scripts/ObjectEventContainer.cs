using System;
using UnityEngine.Events;

[Serializable]
public class ObjectEventContainer
{
    public ObjectEvent objectEvent;
    public UnityEvent response;

    public void SubscribeObjectEvent()
    {
        objectEvent.RegisterListener(this);
    }

    public void UnsubscribeObjectEvent()
    {
        objectEvent.UnregisterListener(this);
    }

    public void OnEventRaised()
    {
        response.Invoke();
    }
}