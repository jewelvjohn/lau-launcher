using TMPro;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    [Header("Frame Rate Debug")]
    [SerializeField] private bool showFPS = false;
    [SerializeField] private float updateInterval = 0.5f;
    [SerializeField] private TMP_Text frameRateCounter;

    private float accum = 0f;
    private int frames = 0;
    private float timeLeft;
    private float fps;

    private void Start()
    {
        if (!showFPS)
        {
            frameRateCounter.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            frameRateCounter.transform.parent.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        FrameRateCalculate();
    }

    private void FrameRateCalculate()
    {
        if (!showFPS || frameRateCounter == null) return;

        timeLeft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        frames++;

        // Update FPS at specified interval
        if (timeLeft <= 0f)
        {
            fps = accum / frames;
            timeLeft = updateInterval;
            accum = 0f;
            frames = 0;

            // Update UI
            if (fps >= 60f)
                frameRateCounter.color = Color.green;
            else if (fps >= 30f)
                frameRateCounter.color = Color.yellow;
            else
                frameRateCounter.color = Color.red;

            frameRateCounter.text = $"{fps:F0}";
        }
    }
}
