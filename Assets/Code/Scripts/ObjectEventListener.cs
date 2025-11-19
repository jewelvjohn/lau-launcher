using UnityEngine;
using System.Collections.Generic;

public class ObjectEventListener : MonoBehaviour
{
    public List<ObjectEventContainer> objectEventListeners;

    private void OnEnable()
    {
        foreach (ObjectEventContainer gameEventContainer in objectEventListeners)
        {
            gameEventContainer.SubscribeObjectEvent();
        }
    }

    private void OnDisable()
    {
        foreach (ObjectEventContainer gameEventContainer in objectEventListeners)
        {
            gameEventContainer.UnsubscribeObjectEvent();
        }
    }
}
