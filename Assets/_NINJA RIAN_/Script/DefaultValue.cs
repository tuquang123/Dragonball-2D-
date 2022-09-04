using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ContinueAdType { rewardedVideo, None}
public class DefaultValue : MonoBehaviour {
    public static DefaultValue Instance;

    public ContinueAdType continueAdType;
    [Header("DEFAULT VALUE")]
    public int defaultLives = 3;
    public int defaultCoin = 100;

 
    public enum NormalBulletType { SinglePress, HoldShooting}
    public NormalBulletType normalBulletType;

    public int normalBulletLimited = 6;
    public bool defaultBulletMax = false;
    public int defaultBullet = 0;

    public int defaultPowerBullet = 0;
    public int defaultGodItem = 0;
    [Space]
    [Header("SHOP")]
    public int livePrice = 10;
    public int bulletPrice = 20;
    public int powerBulletPrice = 30;
    public int godPrice = 50;
    public int char2Price = 200;
    public int char3Price = 300;

    [Header("AD BUTTONS")]
    public bool gotoCheckpointAd = false;
    public bool nextLevelAd = false;
    public bool restartLevelAd = false;

    [Header("WATCH VIDEO REWARD")]
    public int rewardedLives = 3;

    [Header("KEYBOARD CONTROL")]
    public KeyCode keyMoveLeft;
    public KeyCode keyMoveRight;
    public KeyCode keyMoveDown;
    public KeyCode keyJump;
    //public KeyCode keyMoveLeft2 = KeyCode.LeftArrow;
    //public KeyCode keyMoveRight2 = KeyCode.RightArrow;
    //public KeyCode keyMoveDown2 = KeyCode.DownArrow;
    //public KeyCode keyJump2 = KeyCode.UpArrow;

    public KeyCode keyMelee;
    public KeyCode keyNormalBullet;
    public KeyCode keyPowerBullet;
    public KeyCode keyGod;
    public KeyCode keyPause;
    public KeyCode keyDogge;

    // Use this for initialization
    private void Awake()
    {
        Instance = this;
    }
    void Start () {

        GlobalValue.normalBulletLimited = normalBulletLimited;
        DontDestroyOnLoad(gameObject);
	}

    private void OnDrawGizmos()
    {
        if (defaultBulletMax)
            defaultBullet = int.MaxValue;
    }
}
