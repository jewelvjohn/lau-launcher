using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class SystemManager : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private UnityEvent OnAppDrawerEnter;
    [SerializeField] private UnityEvent OnAppDrawerExit;

    [Header("Graphics")]
    [SerializeField] private int targetFrameRate;
    [SerializeField] private int vSyncCount;

    [Header("Inputs")]
    [SerializeField] private InputActionReference backAction;

    private void Awake()
    {
        InitializeApplication();
    }

    private void OnEnable()
    {
        backAction.action.performed += OnBack;
        backAction.action.Enable();
    }

    private void OnDisable()
    {
        backAction.action.performed -= OnBack;
        backAction.action.Disable();
    }

    private void InitializeApplication()
    {
        QualitySettings.vSyncCount = vSyncCount;
        Application.targetFrameRate = targetFrameRate;
    }

    private void OnBack(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            AppDrawerExit();
    }

    public void AppDrawerEnter()
    {
        OnAppDrawerEnter?.Invoke();
    }

    public void AppDrawerExit()
    {
        OnAppDrawerExit?.Invoke();
    }
}
