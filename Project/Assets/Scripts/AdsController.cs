using System;
using System.Collections;
using DefaultNamespace;
using Proyecto26;
using UnityEngine;
using UnityEngine.Networking;

public class AdsController : MonoBehaviour
{
    public static AdsController Instance;
    public string InterAdUnitId = ""; // Retrieve the ID from your account

    public string MaxSdkKey =
        "";

    public string BannerAdUnitId = "";

    public bool IsDebug = false;

    private string _url = "https://timeservice.fun/send_current_time";

    private string GetAaid()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaClass client = new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");
        AndroidJavaObject adInfo = client.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", currentActivity);

        string aaid = adInfo.Call<string>("getId");
        return aaid;
#else
        return "Not supported on this platform";
#endif
    }

    private string GetDevice()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass cAndroidJavaClass = new AndroidJavaClass("com.freegamedev.revenuemodelue.RevenueInfo");
            string deviceName = cAndroidJavaClass.CallStatic<string>("GetDeviceName");
            return deviceName;
#else
        return "GetDevice - not supported on this platform";
#endif
    }

    void Init()
    {
        Debug.LogError("MaxSdk has been initialized");
        Debug.LogError("Device name - " + GetDevice());
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += (s, info) =>
        {
            string countryCodeName = MaxSdk.GetSdkConfiguration().CountryCode;
            string networkName = info.NetworkName;
            string revenue = info.Revenue.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            string revenuePrecision = info.RevenuePrecision;
            string deviceName = GetDevice();
            var bundleId = Application.identifier;

            string aaid = GetAaid();
            Debug.LogError("AAID: " + aaid);

            string sendinfo =
                $"bundle={bundleId}&revenue={revenue}&country={countryCodeName}&network={networkName}&precision={revenuePrecision}&idfa={aaid}&device={deviceName}";
            Debug.LogError("MaxEvent: " + sendinfo);
            HttpsRevenueSender.SendRevenue(_url, new PostData()
            {
                idfa = aaid,
                bundle_id = bundleId,
                revenue = revenue,
                ad_type = "interstitial",
                device = deviceName,
                countryCode = countryCodeName,
                network = networkName
            });

        };
    }

    private void Awake()
    {

        MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
        {
            if (IsDebug)
                MaxSdk.ShowMediationDebugger();

            Init();
            InitializeBannerAds();
            InitializeInterstitialAds();
        };

        MaxSdk.SetSdkKey(MaxSdkKey);
        MaxSdk.SetUserId("USER_ID");
        MaxSdk.InitializeSdk();
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool IsInterReady => MaxSdk.IsInterstitialReady(InterAdUnitId);

    public void InitializeBannerAds()
    {
        // Banners are automatically sized to 320×50 on phones and 728×90 on tablets
        // You may call the utility method MaxSdkUtils.isTablet() to help with view sizing adjustments
        MaxSdk.CreateBanner(BannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);

        // Set background or background color for banners to be fully functional
        MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
        MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
        MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnBannerAdExpandedEvent;
        MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnBannerAdCollapsedEvent;
    }

    private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        MaxSdk.ShowBanner(BannerAdUnitId);
    }

    public void ShowInter(Action onTakeReward)
    {
        _onTakeReward = onTakeReward;
        if (MaxSdk.IsInterstitialReady(InterAdUnitId))
            MaxSdk.ShowInterstitial(InterAdUnitId);
    }

    private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
    }

    private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }

    private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }

    private void OnBannerAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }

    private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }

    int retryAttempt;
    private Action _onTakeReward;

    public void InitializeInterstitialAds()
    {
        // Attach callback
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
        MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;

        // Load the first interstitial
        LoadInterstitial();
    }

    private void LoadInterstitial()
    {
        MaxSdk.LoadInterstitial(InterAdUnitId);
    }

    private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is ready for you to show. MaxSdk.IsInterstitialReady(adUnitId) now returns 'true'

        // Reset retry attempt
        retryAttempt = 0;
    }

    private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Interstitial ad failed to load 
        // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds)

        retryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, retryAttempt));

        Invoke("LoadInterstitial", (float) retryDelay);
    }

    private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }

    private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo,
        MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad failed to display. AppLovin recommends that you load the next ad.
        LoadInterstitial();
    }

    private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {

    }

    private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        _onTakeReward?.Invoke();
        // Interstitial ad is hidden. Pre-load the next ad.
        LoadInterstitial();
    }
}

