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
    [Header("Events")]
    [SerializeField] private UnityEvent OnAppDrawerEnter;
    [SerializeField] private UnityEvent OnAppDrawerExit;

    [Header("Graphics")]
    [SerializeField] private int targetFrameRate;
    [SerializeField] private int vSyncCount;

    private AppState appState;

    private bool activeGesture = false;

    private void Awake()
    {
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

    public void UserBack()
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

    public void UserSwipeStart(Vector2 position, float time)
    {
        activeGesture = true;
    }
    
    public void UserSwipeEnd(Vector2 position, float time)
    {
        activeGesture = false;
    }

    public void UserSwipe(Vector2 delta)
    {
        if (activeGesture)
        {
            switch (appState)
            {
                case AppState.Home:
                    HomeGesture(delta);
                    break;

                case AppState.AppDrawer:
                    AppDrawerGesture(delta);
                    break;
            }
        }
    }

    public void HomeGesture(Vector2 delta)
    {
        Debug.Log("Home swipe: " + delta);
    }

    public void AppDrawerGesture(Vector2 delta)
    {
        Debug.Log("App Drawer delta: " + delta);
    }
}
