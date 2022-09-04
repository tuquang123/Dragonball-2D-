using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance;
    //delegate   ()
    public delegate void RewardedAdResult(bool isSuccess, int rewarded);

    //event  
    public static event RewardedAdResult AdResult;

    public enum AD_NETWORK { Unity, Admob}

    [Header("REWARDED VIDEO AD")]
    public AD_NETWORK rewardedUnit;
    public int getRewarded = 5;
    public float timePerWatch = 90;
float lastTimeWatch = -999;

    [Header("SHOW AD VICTORY/GAMEOVER")]
    public AD_NETWORK adGameOverUnit;
    public int showAdGameOverCounter = 2;
     int counter_gameOver = 0;
    public int showAdVictoryCounter = 1;
    int counter_victory = 0;

    private void Awake()
    {
        if (AdsManager.Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void ShowAdmobBanner(bool show)
    {
        if (GlobalValue.RemoveAds)
        {
            Debug.LogWarning("Ads Remove");
            return;
        }

        AdmobController.Instance.ShowBanner(show);
    }

    #region NORMAL AD

    public void ShowNormalAd(GameManager.GameState state)
    {
        if (GlobalValue.RemoveAds)
        {
            Debug.LogWarning("Ads Remove");
            return;
        }

        Debug.Log("SHOW NORMAL AD " + state);

        if (state == GameManager.GameState.Dead)
            StartCoroutine(ShowNormalAdCo(state, 0.8f));
        else
            StartCoroutine(ShowNormalAdCo(state, 0));
    }

    IEnumerator ShowNormalAdCo(GameManager.GameState state, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (state == GameManager.GameState.Dead)
        {
            counter_gameOver++;
            if (counter_gameOver >= showAdGameOverCounter)
            {
                if (adGameOverUnit == AD_NETWORK.Unity)
                {
                   
                }
                else if (adGameOverUnit == AD_NETWORK.Admob)
                {
                    if (AdmobController.Instance.ForceShowInterstitialAd())
                    {
                        counter_gameOver = 0;
                    }
                }
            }
        }else if(state == GameManager.GameState.Finish)
        {
            counter_victory++;
            if (counter_victory >= showAdVictoryCounter)
            {
                if (adGameOverUnit == AD_NETWORK.Unity)
                {
                    
                }
                else if (adGameOverUnit == AD_NETWORK.Admob)
                {
                    if (AdmobController.Instance.ForceShowInterstitialAd())
                    {
                        counter_victory = 0;
                    }
                }
            }
        }
        //}
    }

    public void ResetCounter()
    {
        counter_gameOver = 0;
        //counter_gameFinish = 0;
    }

    #endregion

    #region REWARDED VIDEO AD

    public bool isRewardedAdReady()
    {
        if ((rewardedUnit == AD_NETWORK.Unity) )
            return true;

        if ((rewardedUnit == AD_NETWORK.Admob) && AdmobController.Instance.isRewardedVideoAdReady())
            return true;

        return false;

    }

    public float TimeWaitingNextWatch()
    {
        return timePerWatch - (Time.realtimeSinceStartup - lastTimeWatch);
    }

    public void ShowRewardedAds()
    {
        lastTimeWatch = Time.realtimeSinceStartup;

        if (rewardedUnit == AD_NETWORK.Unity)
        {
           
        }
        else
        {
            AdmobController.AdResult += AdmobController_AdResult;
            AdmobController.Instance.WatchRewardedVideoAd();
        }

        
    }

    private void AdmobController_AdResult(bool isWatched)
    {
        AdmobController.AdResult -= AdmobController_AdResult;
        AdResult(true, getRewarded);
    }
    #endregion
}
