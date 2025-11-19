using UnityEngine;
using UnityEngine.Events;

public enum AppState
{
    Home,
    AppDrawer,
    InApp
}

public class SystemManager : MonoBehaviour
{
    public static SystemManager Instance { get; private set; }

    [Header("Events")]
    [SerializeField] private UnityEvent OnAppDrawerEnter;
    [SerializeField] private UnityEvent OnAppDrawerExit;

    [Header("Graphics")]
    [SerializeField] private int targetFrameRate;
    [SerializeField] private int vSyncCount;

    private AppState appState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        InitializeApplication();
    }

    private void InitializeApplication()
    {
        QualitySettings.vSyncCount = vSyncCount;
        Application.targetFrameRate = targetFrameRate;

        appState = AppState.Home;
    }

    public void AppDrawerEnter()
    {
        OnAppDrawerEnter?.Invoke();
        appState = AppState.AppDrawer;
    }

    public void AppDrawerExit()
    {
        OnAppDrawerExit?.Invoke();
        appState = AppState.Home;
    }

    public void SetAppState(AppState state)
    {
        appState = state;
    }

    public AppState GetAppState()
    {
        return appState;
    }

    public void OnBackAction()
    {
        switch (appState)
        {
            case AppState.Home:
                break;

            case AppState.AppDrawer:
                AppDrawerExit();
                break;
        }
    }
}
