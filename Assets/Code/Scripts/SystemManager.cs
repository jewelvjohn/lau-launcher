using UnityEngine;
using UnityEngine.Events;

public enum AppState
{
    Home,
    AppDrawer,
    InApp
}

public enum GestureType
{
    None,
    SwipeUp,
    SwipeDown,
    SwipeLeft,
    SwipeRight
}

public class SystemManager : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private UnityEvent OnAppDrawerEnter;
    [SerializeField] private UnityEvent OnAppDrawerExit;

    [Header("Graphics")]
    [SerializeField] private int targetFrameRate;
    [SerializeField] private int vSyncCount;

    [Header("Gesture")]
    [SerializeField] private RectTransform appDrawerSwipeArea;
    [SerializeField] private float gestureThresold = 20f;

    private AppState appState;
    private GestureType gestureType;

    private bool activeGesture = false;
    private bool isAppDrawerSwipe = false;

    private void Awake()
    {
        InitializeApplication();
    }

    private void InitializeApplication()
    {
        QualitySettings.vSyncCount = vSyncCount;
        Application.targetFrameRate = targetFrameRate;

        appState = AppState.Home;
        gestureType = GestureType.None;
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

    public void UserGestureStart(Vector2 position, float time)
    {
        activeGesture = true;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            appDrawerSwipeArea,
            position,
            null,
            out localPoint
        );

        if (appDrawerSwipeArea.rect.Contains(localPoint))
            isAppDrawerSwipe = true;
        else
            isAppDrawerSwipe = false;
    }
    
    public void UserGestureEnd(Vector2 position, float time)
    {
        activeGesture = false;
        isAppDrawerSwipe = false;
        gestureType = GestureType.None;
    }

    public void UserGesture(Vector2 delta)
    {
        if (activeGesture)
        {
            ProcessGesture(delta);

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

    private void HomeGesture(Vector2 delta)
    {
        switch (gestureType)
        {
            case GestureType.SwipeUp:
                if (isAppDrawerSwipe)
                    AppDrawerEnter();
                break;
        }
    }

    private void AppDrawerGesture(Vector2 delta)
    {
        switch (gestureType)
        {
            case GestureType.SwipeDown:
                AppDrawerExit();
                break;
        }
    }
}
