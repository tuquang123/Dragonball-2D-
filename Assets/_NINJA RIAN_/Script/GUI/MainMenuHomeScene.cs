using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;

public class MainMenuHomeScene : MonoBehaviour {
	public static MainMenuHomeScene Instance;

	public GameObject StartMenu;
	public GameObject WorldsChoose;
	public GameObject LoadingScreen;
	public GameObject Shop;
    public GameObject Settings;
    public GameObject removeAdBut;
    public GameObject finishGame;
    public GameObject exitPanel;

	SoundManager soundManager;
    public GameObject AskResetPanel;
    public GameObject rateButton;

    public Text attemptTxt;

    [Header("Sound and Music")]
    public Image soundImage;
    public Image musicImage;
    public Sprite soundImageOn, soundImageOff, musicImageOn, musicImageOff;

    void Awake(){
		Instance = this;
		soundManager = FindObjectOfType<SoundManager> ();

        if(GlobalValue.LevelHighest >= 30 && PlayerPrefs.GetInt("isFinishGameShown", 0) == 0)
        {
            FinishGame(true);
            PlayerPrefs.SetInt("isFinishGameShown", 1);
        }else
            FinishGame(false);
    }
    
	IEnumerator Start () {
        

        if (!GlobalValue.isSetDefaultValue)
        {
            GlobalValue.isSetDefaultValue = true;
            if (DefaultValue.Instance)
            {
                GlobalValue.Bullets = DefaultValue.Instance.defaultBulletMax ? int.MaxValue : DefaultValue.Instance.defaultBullet;
                GlobalValue.powerBullet = DefaultValue.Instance.defaultPowerBullet;
                GlobalValue.storeGod = DefaultValue.Instance.defaultGodItem;
               GlobalValue.SaveLives = DefaultValue.Instance.defaultLives;
            }
        }

        StartMenu.SetActive(false);
        WorldsChoose.SetActive (false);
		LoadingScreen.SetActive (false);
        Shop.SetActive (false);
        AskResetPanel.SetActive(false);
        Settings.SetActive(false);
        exitPanel.SetActive(false);
        SoundManager.PlayMusic(SoundManager.Instance.musicsMenu);
        if (GlobalValue.isFirstOpenMainMenu)
        {
            GlobalValue.isFirstOpenMainMenu = false;
            SoundManager.Instance.PauseMusic(true);
            SoundManager.PlaySfx(SoundManager.Instance.beginSoundInMainMenu);
            yield return new WaitForSeconds(SoundManager.Instance.beginSoundInMainMenu.length);
            SoundManager.Instance.PauseMusic(false);
            
            SoundManager.ResetMusic();
        }

        //yield return new WaitForSeconds(1);
        SoundManager.PlayGameMusic();
        StartMenu.SetActive(true);

        soundManager = FindObjectOfType<SoundManager>();
        attemptTxt.text = "ATTEMPTS: " + GlobalValue.Attempt;

        soundImage.sprite = GlobalValue.isSound ? soundImageOn : soundImageOff;
        musicImage.sprite = GlobalValue.isMusic ? musicImageOn : musicImageOff;
        if (!GlobalValue.isSound)
            SoundManager.SoundVolume = 0;
        if (!GlobalValue.isMusic)
            SoundManager.MusicVolume = 0;
    }

    #region Music and Sound
    public void TurnSound()
    {
        GlobalValue.isSound = !GlobalValue.isSound;
        soundImage.sprite = GlobalValue.isSound ? soundImageOn : soundImageOff;

        SoundManager.SoundVolume = GlobalValue.isSound ? 1 : 0;
    }

    public void TurnMusic()
    {
        GlobalValue.isMusic = !GlobalValue.isMusic;
        musicImage.sprite = GlobalValue.isMusic ? musicImageOn : musicImageOff;

        SoundManager.MusicVolume = GlobalValue.isMusic ? SoundManager.Instance.musicsGameVolume : 0;
    }
    #endregion

    void Update()
    {
        removeAdBut.gameObject.SetActive(!GlobalValue.RemoveAds);
        rateButton.SetActive(!GlobalValue.isOpenRateButton);
    }

    public void TurnExitPanel(bool open)
    {
        SoundManager.Click();
        exitPanel.SetActive(open);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void OpenFacebook()
    {
#if !UNITY_WEBGL
        //Application.OpenURL(facebookLink);
        GameMode.Instance.OpenFacebook();
#else
        openPage(facebookLink);
#endif
        SoundManager.PlaySfx(soundManager.soundClick);
    }

    public void OpenStoreLink()
    {
        SoundManager.PlaySfx(soundManager.soundClick);
        GameMode.Instance.OpenStoreLink();
//#if UNITY_ANDROID
//        Application.OpenURL(androidLink);
//#elif UNITY_IOS
//        Application.OpenURL(iosLink);
//#endif

        finishGame.SetActive(false);
    }

    public void OpenGooglePlayLink()
    {
        SoundManager.PlaySfx(soundManager.soundClick);
        GameMode.Instance.OpenGooglePlayLink();
//#if !UNITY_WEBGL
//        Application.OpenURL(androidLink);
//#else
//        openPage(androidLink);
//#endif
    }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void openPage(string url);
#endif

    public void RemoveAds()
    {
#if UNITY_PURCHASING
        if (Purchaser.Instance)
        {
            Purchaser.Instance.BuyRemoveAds();
        }
#endif
    }

    public void FinishGame(bool open)
    {
        finishGame.SetActive(open);
    }

    public void AskReset(bool open)
    {
        SoundManager.Click();
        AskResetPanel.SetActive(open);
    }

    public void OpenSettings(bool open)
    {
        Settings.SetActive(open);
        StartMenu.SetActive(!open);
    }

  //  public void OpenWorld(int world){
		//WorldsChoose.SetActive (false);
		//LevelsChoose.SetActive (true);

		//for (int i = 0; i < WorldLevel.Length; i++) {
		//	if (i == (world - 1)) {
		//		WorldLevel [i].SetActive (true);
		//	} else
		//		WorldLevel [i].SetActive (false);
		//}

		//SoundManager.PlaySfx (soundManager.soundClick);
  //      SoundManager.PlayMusic(soundManager.musicLevelChoose);
  //  }

	public void OpenWorldChoose(){
        StartMenu.SetActive(false);
        WorldsChoose.SetActive (true);

		SoundManager.PlaySfx (soundManager.soundClick);
        SoundManager.PlayMusic(soundManager.musicWorldChoose);
    }

	public void OpenStartMenu(){
        StartMenu.SetActive(true);
        WorldsChoose.SetActive (false);

		SoundManager.PlaySfx (soundManager.soundClick);
        SoundManager.PlayMusic(soundManager.musicsMenu);
    }

	public void OpenShop(bool open) { 
        Shop.SetActive (open);
        StartMenu.SetActive(!open);
	}

    public void LoadScene(string name)
    {
        WorldsChoose.SetActive(false);
        //SceneManager.LoadSceneAsync(name);
        LoadingScreen.SetActive(true);
        StartCoroutine(LoadAsynchronously(name));
    }

    [Header("LOADING PROGRESS")]
    public Slider slider;
    public Text progressText;
    IEnumerator LoadAsynchronously(string name)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(name);
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            if (slider != null)
                slider.value = progress;
            if (progressText != null)
                progressText.text = (int) progress * 100f + "%";
            //			Debug.LogError (progress);
            yield return null;
        }
    }

    public void Exit(){
		Application.Quit ();
	}
}
