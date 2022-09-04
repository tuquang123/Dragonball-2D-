using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Menu_GUI : MonoBehaviour {
    public static Menu_GUI Instance;
    public Text scoreText;
	public Text bulletText;
	public Text coinText;
    public Text liveTxt;
    public GameObject scrollGroup;
    public Animator scroll1Anim, scroll2Anim, scroll3Anim;
    bool isScroll1Collected, isScroll2Colltected, isScroll3Collected;
    private void Awake()
    {
        Instance = this;
    }

    bool firstPlay = true;

    private void Start()
    {
        if (DefaultValue.Instance)
            bulletText.enabled = !DefaultValue.Instance.defaultBulletMax;

        if (LevelMapType.Instance.levelType == LEVELTYPE.BossFight)
        {
            scrollGroup.SetActive(false);
        }

        firstPlay = false;
    }

    private void OnEnable()
    {
        if (firstPlay)
            return;

        if (isScroll1Collected) {
            scroll1Anim.SetTrigger("isCollected");
        }
        if (isScroll2Colltected)
        {
            scroll2Anim.SetTrigger("isCollected");
        }
        if (isScroll3Collected)
        {
            scroll3Anim.SetTrigger("isCollected");
        }
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = GameManager.Instance.Point.ToString("0000000");
        coinText.text = GlobalValue.SavedCoins.ToString("0");
        bulletText.text = GlobalValue.Bullets + "/" + GlobalValue.getDartLimited();
        bulletText.color = GlobalValue.Bullets == GlobalValue.getDartLimited() ? Color.red : Color.white;
        liveTxt.text = GlobalValue.SaveLives + "";
    }

    public void ScrollCollectAnim(int ID, bool noAnimation = false)
    {
        switch (ID)
        {
            case 1:
                scroll1Anim.SetTrigger(noAnimation ? "isCollected" : "collect");
                isScroll1Collected = true;
                break;
            case 2:
                scroll2Anim.SetTrigger(noAnimation ? "isCollected" : "collect");
                isScroll2Colltected = true;
                break;
            case 3:
                scroll3Anim.SetTrigger(noAnimation ? "isCollected" : "collect");
                isScroll3Collected = true;
                break;
            default:
                break;
        }
    }
}
