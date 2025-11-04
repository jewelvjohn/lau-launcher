using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

[System.Serializable]
public class AppInfo
{
    public string appName;
    public string packageName;
    public Sprite appIcon;
}

public class HomeManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIDocument document;
    [SerializeField] private VisualTreeAsset appButtonTemplate;
    
    [Header("Properties")]
    [SerializeField] private bool sortAlphabetically = true;
    [SerializeField] private bool includeSystemApps = false;
    [SerializeField] private bool includeRedundentPackages = false;
    
    [Header("Sample Data")]
    [SerializeField] private List<AppInfo> sampleApps;

    [Header("Frame Rate Debug")]
    [SerializeField] private bool showFPS = false;
    [SerializeField] private float updateInterval = 0.5f;

    private float accum = 0f;
    private int frames = 0;
    private float timeLeft;
    private float fps;

    // UXML Elements
    private VisualElement rootElement;
    private ScrollView appScrollView;

    #region Debug
    private void FrameRateCalculate()
    {
        if (!showFPS) return;

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

            // Update UI or log FPS
            Debug.Log($"FPS: {fps:F2}");
        }
    }
    #endregion

    private void OnEnable()
    {
        rootElement = document.rootVisualElement;
        appScrollView = rootElement.Q<ScrollView>("AppScroll");
    }

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
        // Additional initialization logic can be added here

        List<AppInfo> installedApps = GetInstalledApps(includeSystemApps);
        CreateAppButtons(installedApps);
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

            if (!includeRedundentPackages)
            {
                // Check for duplicate package names
                bool alreadyExists = appList.Exists(app => app.packageName == packageName);
                if (alreadyExists)
                {
                    continue;
                }
            }
            
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
        if (sampleApps != null && sampleApps.Count > 0)
        {
            appList.AddRange(sampleApps);
        }
        else
        {
            Debug.LogWarning("No sample apps assigned for testing in Unity Editor");
            // Add some dummy apps for testing

            appList.Add(new AppInfo { packageName = "com.example.app1", appName = "Test App 1" });
            appList.Add(new AppInfo { packageName = "com.example.app2", appName = "Test App 2" });
        }
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
    private void CreateAppButtons(List<AppInfo> apps)
    {
        if (apps.Count == 0)
        {
            Debug.LogWarning("No apps to create buttons for.");
            return;
        }

        if (sortAlphabetically)
        {
            apps.Sort((a, b) => a.appName.CompareTo(b.appName));
        }

        appScrollView.Clear();
        foreach (AppInfo app in apps)
        {
            // Instantiate buttons for apps
            VisualElement appButton = appButtonTemplate.Instantiate();
            if (appButton == null)
            {
                Debug.LogError("App button template instantiation failed.");
                continue;
            }

            Label appLabel = appButton.Q<Label>("AppNameTemplate");
            VisualElement appIconElement = appButton.Q<VisualElement>("AppIconTemplate");

            if (appLabel == null || appIconElement == null)
            {
                Debug.LogError($"App button template is missing required elements. {appLabel}, {appIconElement}");
                continue;
            }

            if (app.appName != "")
                appLabel.text = app.appName;
            else
                continue;

            if (app.appIcon != null)
                appIconElement.style.backgroundImage = new StyleBackground(app.appIcon);

            Button buttonComponent = appButton.Q<Button>("AppButtonTemplate");
            if (buttonComponent == null)
            {
                Debug.LogError("App button template is missing Button component.");
                continue;
            }
            buttonComponent.clicked += () => LaunchApp(app.packageName);
            
            appScrollView.Add(appButton);
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