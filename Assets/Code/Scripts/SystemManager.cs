using UnityEngine;
using UnityEngine.Events;

public class SystemManager : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private UnityEvent OnAppDrawerButton;

    [Header("Graphics")]
    [SerializeField] private int targetFrameRate;
    [SerializeField] private int vSyncCount;

    private void Awake()
    {
        InitializeApplication();
    }

    private void InitializeApplication()
    {
        QualitySettings.vSyncCount = vSyncCount;
        Application.targetFrameRate = targetFrameRate;
    }

    public void AppDrawerButtonClick()
    {
        OnAppDrawerButton?.Invoke();
    }
}
