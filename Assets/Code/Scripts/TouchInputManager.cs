using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class TouchInputManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputActionReference backAction;
    [SerializeField] private InputActionReference touchContact;
    [SerializeField] private InputActionReference touchPosition;

    [Header("Events")]
    [SerializeField] private UnityEvent OnBackGesture;
    [SerializeField] private UnityEvent<Vector2, float> OnTouchStart;
    [SerializeField] private UnityEvent<Vector2, float> OnTouchEnd;
    [SerializeField] private UnityEvent<Vector2> OnTouchMove;

    private bool active = false;

    private Vector2 initialPosition;

    private void OnEnable()
    {
        backAction.action.Enable();
        touchContact.action.Enable();

        backAction.action.performed += OnBack;

        touchContact.action.started += StartTouchContact;
        touchContact.action.canceled += EndTouchContact;
    }

    private void OnDisable()
    {
        backAction.action.Disable();
        touchContact.action.Disable();
        
        backAction.action.performed -= OnBack;

        touchContact.action.performed -= StartTouchContact;
        touchContact.action.canceled -= EndTouchContact;
    }

    private void Update()
    {
        if (active)
        {
            Vector2 currentPosition = touchPosition.action.ReadValue<Vector2>();
            OnTouchMove?.Invoke(initialPosition - currentPosition);
        }
    }

    private void OnBack(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            OnBackGesture?.Invoke();
    }

    private void StartTouchContact(InputAction.CallbackContext ctx)
    {
        Vector2 touchPositionValue = touchPosition.action.ReadValue<Vector2>();
        float touchTime = (float)ctx.time;

        active = true;
        initialPosition = touchPositionValue;

        OnTouchStart?.Invoke(touchPositionValue, touchTime);
    }

    private void EndTouchContact(InputAction.CallbackContext ctx)
    {
        Vector2 touchPositionValue = touchPosition.action.ReadValue<Vector2>();
        float touchTime = (float)ctx.time;

        active = false;

        OnTouchEnd?.Invoke(touchPositionValue, touchTime);
    }
}
