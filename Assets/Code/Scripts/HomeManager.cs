using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class AppInfo
{
    public string packageName;
    public string appName;
    public Sprite appIcon;
}

public class HomeManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject homeButtonPrefab;
    
    [Header("Properties")]
    [SerializeField] private bool sortAlphabetically = true;
    [SerializeField] private bool includeSystemApps = false;
    
    [Header("Frame Rate Debug")]
    [SerializeField] private bool showFPS = false;
    [SerializeField] private float updateInterval = 0.5f;
    [SerializeField] private TMP_Text frameRateCounter;

    private float accum = 0f;
    private int frames = 0;
    private float timeLeft;
    private float fps;

    #region Debug
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
    #endregion

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 90;

        Initialize();
    }

    private void Update()
    {
        FrameRateCalculate();
    }

    private void Initialize()
    {
        RefreshButtons();
        // Additional initialization logic can be added here

        List<AppInfo> installedApps = GetInstalledApps(includeSystemApps);
        CreateAppButtons(installedApps);
    }

    private void RefreshButtons()
    {
        for (int i = 0; i < homePanel.transform.childCount; i++)
        {
            Destroy(homePanel.transform.GetChild(i).gameObject);
        }
    }

    public List<AppInfo> GetInstalledApps(bool includeSystemApps = false)
    {
        List<AppInfo> appList = new List<AppInfo>();
#if UNITY_ANDROID && !UNITY_EDITOR
    try
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
        
        // Create an intent for launcher apps
        AndroidJavaObject launcherIntent = new AndroidJavaObject("android.content.Intent", "android.intent.action.MAIN");
        launcherIntent.Call<AndroidJavaObject>("addCategory", "android.intent.category.LAUNCHER");
        
        // Query all apps with launcher intent
        AndroidJavaClass packageManagerClass = new AndroidJavaClass("android.content.pm.PackageManager");
        int GET_META_DATA = packageManagerClass.GetStatic<int>("GET_META_DATA");
        
        AndroidJavaObject resolveInfoList = packageManager.Call<AndroidJavaObject>("queryIntentActivities", launcherIntent, GET_META_DATA);
        int packageCount = resolveInfoList.Call<int>("size");
        
        for (int i = 0; i < packageCount; i++)
        {
            AndroidJavaObject resolveInfo = resolveInfoList.Call<AndroidJavaObject>("get", i);
            AndroidJavaObject activityInfo = resolveInfo.Get<AndroidJavaObject>("activityInfo");
            AndroidJavaObject applicationInfo = activityInfo.Get<AndroidJavaObject>("applicationInfo");
            
            string packageName = activityInfo.Get<string>("packageName");
            
            // Filter system apps if needed
            if (!includeSystemApps)
            {
                int appFlags = applicationInfo.Get<int>("flags");
                int FLAG_SYSTEM = 1;
                bool isSystemApp = (appFlags & FLAG_SYSTEM) != 0;
                
                if (isSystemApp)
                {
                    continue;
                }
            }
            
            // Get app name
            AndroidJavaObject appNameObj = applicationInfo.Call<AndroidJavaObject>("loadLabel", packageManager);
            string appName = appNameObj.Call<string>("toString");
            
            // Get app icon
            Sprite appIconSprite = GetAppIconSprite(applicationInfo, packageManager);
            
            AppInfo app = new AppInfo
            {
                packageName = packageName,
                appName = appName,
                appIcon = appIconSprite
            };
            
            appList.Add(app);
        }
        
        Debug.Log($"Found {appList.Count} launchable applications");
    }
    catch (System.Exception e)
    {
        Debug.LogError($"Error getting installed apps: {e.Message}");
    }
#else
        Debug.LogWarning("This function only works on Android devices");
        // Test data for Unity Editor
        appList.Add(new AppInfo { packageName = "com.example.app1", appName = "Test App 1" });
        appList.Add(new AppInfo { packageName = "com.example.app2", appName = "Test App 2" });
#endif
        return appList;
    }

    private Sprite GetAppIconSprite(AndroidJavaObject applicationInfo, AndroidJavaObject packageManager)
    {
        try
        {
            // Get the app icon drawable
            AndroidJavaObject iconDrawable = applicationInfo.Call<AndroidJavaObject>("loadIcon", packageManager);

            // Convert drawable to bitmap
            AndroidJavaObject bitmap = DrawableToBitmap(iconDrawable);

            // Convert bitmap to Texture2D
            Texture2D texture = BitmapToTexture2D(bitmap);

            // Convert Texture2D to Sprite
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            return sprite;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error getting app icon: {e.Message}");
            return null;
        }
    }

    private AndroidJavaObject DrawableToBitmap(AndroidJavaObject drawable)
    {
        // Get drawable bounds
        int width = drawable.Call<int>("getIntrinsicWidth");
        int height = drawable.Call<int>("getIntrinsicHeight");

        // Create bitmap
        AndroidJavaObject bitmapConfig = new AndroidJavaClass("android.graphics.Bitmap$Config").GetStatic<AndroidJavaObject>("ARGB_8888");
        AndroidJavaObject bitmap = new AndroidJavaClass("android.graphics.Bitmap").CallStatic<AndroidJavaObject>("createBitmap", width, height, bitmapConfig);

        // Draw drawable to canvas
        AndroidJavaObject canvas = new AndroidJavaObject("android.graphics.Canvas", bitmap);
        drawable.Call("setBounds", 0, 0, width, height);
        drawable.Call("draw", canvas);

        return bitmap;
    }

    private Texture2D BitmapToTexture2D(AndroidJavaObject bitmap)
    {
        int width = bitmap.Call<int>("getWidth");
        int height = bitmap.Call<int>("getHeight");

        // Create byte array output stream
        AndroidJavaObject stream = new AndroidJavaObject("java.io.ByteArrayOutputStream");

        // Compress bitmap to PNG format
        AndroidJavaObject compressFormat = new AndroidJavaClass("android.graphics.Bitmap$CompressFormat").GetStatic<AndroidJavaObject>("PNG");
        bitmap.Call<bool>("compress", compressFormat, 100, stream);

        // Get byte array
        byte[] imageBytes = stream.Call<byte[]>("toByteArray");

        // Create Texture2D from bytes
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.LoadImage(imageBytes);

        return texture;
    }

    /// <summary>
    /// Creates button instances for each app
    /// </summary>
    void CreateAppButtons(List<AppInfo> apps)
    {
        if (homeButtonPrefab == null)
        {
            Debug.LogError("homeButtonPrefab is not assigned!");
            return;
        }

        if (homePanel == null)
        {
            Debug.LogError("parentContainer is not assigned!");
            return;
        }

        foreach (AppInfo app in apps)
        {
            GameObject buttonObj = Instantiate(homeButtonPrefab, homePanel.transform);
            HomeButton homeButton = buttonObj.GetComponent<HomeButton>();

            // Find TMP component in children
            if (homeButton != null)
            {
                if (app.appName != "") homeButton.SetName(app.appName);
                if (app.appIcon != null) homeButton.SetIcon(app.appIcon);
            }
            else
            {
                Debug.LogWarning($"No HomeButton found in of {buttonObj.name}");
            }

            // Add onClick listener
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                string packageToOpen = app.packageName; // Capture in local variable for closure
                button.onClick.AddListener(() => LaunchApp(packageToOpen));
            }
            else
            {
                Debug.LogWarning($"No Button component found on {buttonObj.name}");
            }
        }
    }

    /// <summary>
    /// Launches an Android app by package name
    /// </summary>
    public void LaunchApp(string packageName)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");

            AndroidJavaObject launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName);
            
            if (launchIntent != null)
            {
                currentActivity.Call("startActivity", launchIntent);
                Debug.Log($"Launched app: {packageName}");
            }
            else
            {
                Debug.LogWarning($"No launch intent found for package: {packageName}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error launching app {packageName}: {e.Message}");
        }
#else
        Debug.Log($"Would launch app: {packageName} (only works on Android device)");
#endif
    }
}