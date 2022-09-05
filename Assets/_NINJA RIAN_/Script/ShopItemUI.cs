using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    public enum ITEM_TYPE { buyDart, iap1, iap2, iap3, watchVideo, buyLive}

    public ITEM_TYPE itemType;
    public int rewarded = 100;
    public float price = 100;
    public GameObject watchVideocontainer;

    public AudioClip soundRewarded;

    public Text priceTxt, rewardedTxt, rewardTimeCountDownTxt;

    private void Update()
    {
        UpdateStatus();
    }

    void UpdateStatus()
    {
        if (itemType == ITEM_TYPE.buyLive)
        {
            priceTxt.text = price + "";
            rewardedTxt.text = GlobalValue.SaveLives + "";
        }
        else if (itemType == ITEM_TYPE.buyDart)
        {
            priceTxt.text = price + "";
            rewardedTxt.text = GlobalValue.Bullets + "/" + GlobalValue.getDartLimited();
        }
        else if (itemType == ITEM_TYPE.watchVideo)
        {
            priceTxt.text = "FREE";
            rewardedTxt.text = "+" + rewarded;

         
            if (watchVideocontainer != null)
            {
                
            }
        }
        else
        {
            priceTxt.text = "$" + price;
            rewardedTxt.text = "+" + rewarded;
        }
    }

    public void Buy()
    {
        #if UNITY_PURCHASING
        switch (itemType)
        {
            case ITEM_TYPE.buyLive:
                if (GlobalValue.SavedCoins >= price && GlobalValue.SaveLives <100)
                {
                    GlobalValue.SavedCoins -= (int)price;
                    GlobalValue.SaveLives += rewarded;
                    SoundManager.PlaySfx(soundRewarded);
                }
                else
                {
                    SoundManager.PlaySfx(SoundManager.Instance.soundNotEnoughCoin);
                }
                break;
            case ITEM_TYPE.buyDart:
                if(GlobalValue.SavedCoins >= price && GlobalValue.Bullets < GlobalValue.getDartLimited())
                {
                    GlobalValue.SavedCoins -= (int)price;
                    GlobalValue.Bullets += rewarded;
                    SoundManager.PlaySfx(soundRewarded);
                }
                else
                {
                    SoundManager.PlaySfx(SoundManager.Instance.soundNotEnoughCoin);
                }
                break;
            case ITEM_TYPE.watchVideo:
                if (AdsManager.Instance && AdsManager.Instance.isRewardedAdReady())
                {
                    AdsManager.AdResult += AdsManager_AdResult;
                    AdsManager.Instance.ShowRewardedAds();
                }
                break;
            case ITEM_TYPE.iap1:
                Purchaser.iAPResult += Purchaser_iAPResult;
                Purchaser.Instance.BuyItem1();
                break;
            case ITEM_TYPE.iap2:
                Purchaser.iAPResult += Purchaser_iAPResult;
                Purchaser.Instance.BuyItem2();
                break;
            case ITEM_TYPE.iap3:
                Purchaser.iAPResult += Purchaser_iAPResult;
                Purchaser.Instance.BuyItem3();
                break;
        }
#endif
                }

    private void AdsManager_AdResult(bool isSuccess, int rewarded)
    {
        //AdsManager.AdResult -= AdsManager_AdResult;
        if (isSuccess)
        {
            GlobalValue.SavedCoins += rewarded;
            SoundManager.PlaySfx(soundRewarded);
            UpdateStatus();
        }
    }

    private void Purchaser_iAPResult(int id)
    {
        #if UNITY_PURCHASING
        Purchaser.iAPResult -= Purchaser_iAPResult;
        GlobalValue.SavedCoins += rewarded;
        SoundManager.PlaySfx(soundRewarded);
        UpdateStatus();
#endif
    }
}
