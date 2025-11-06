using UnityEngine;

public class SystemManager : MonoBehaviour
{
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
}
