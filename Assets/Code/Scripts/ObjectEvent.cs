using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu()]
public class ObjectEvent : ScriptableObject
{
    private List<ObjectEventContainer> listeners = new List<ObjectEventContainer>();

    public void RegisterListener(ObjectEventContainer listener)
    {
        listeners.Add(listener);
    }

    public void UnregisterListener(ObjectEventContainer listener)
    {
        listeners.Remove(listener);
    }

    public void Raise()
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnEventRaised();
        }
    }
}
