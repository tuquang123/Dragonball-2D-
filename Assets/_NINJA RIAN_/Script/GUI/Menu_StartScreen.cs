using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Menu_StartScreen : MonoBehaviour {
    public Text attempts;
    public Text worldTxt;

    void Start()
    {
        if (GlobalValue.levelPlaying == -1)
        {
            worldTxt.text = "TEST GAMEPLAY";
            attempts.text = "";
        }
        else
        {
            worldTxt.text = "LEVEL: " + GlobalValue.levelPlaying;
            attempts.text = "ATTEMPTS: " + GlobalValue.Attempt;
        }
    }
}
