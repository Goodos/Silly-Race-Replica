//using Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdsButton : MonoBehaviour
{
    [SerializeField] private Button _skipLvl;
    // Start is called before the first frame update
    void Start()
    {
        _skipLvl.onClick.AddListener(ShowAds);
        _skipLvl.gameObject.SetActive(AdsController.Instance.IsRewardedReady);
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += UpdateBtn;
    }

    private void UpdateBtn(string arg1, MaxSdkBase.AdInfo arg2)
    {
        _skipLvl.gameObject.SetActive(AdsController.Instance.IsRewardedReady);
    }

    private void ShowAds()
    {
        AdsController.Instance.ShowRewardedAd(() =>
        {
            //GameController.Instance.CollectSoftCurrency((GameModel.Instance.SoftCurrency+1)*2);
        });
    }

    private void OnDestroy()
    {
        _skipLvl.onClick.RemoveListener(ShowAds);  
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= UpdateBtn;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
