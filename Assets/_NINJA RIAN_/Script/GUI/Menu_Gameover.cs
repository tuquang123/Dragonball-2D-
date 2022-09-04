using UnityEngine;
using System.Collections;
using UnityEngine.UI;
//using UnityEngine.Advertisements;

public class Menu_Gameover : MonoBehaviour {
    public Text attemptTxt;
    void OnEnable() {
        attemptTxt.text = "Attempt: " + GlobalValue.Attempt;
	}

    public void TryAgain()
    {
        GameManager.Instance.ResetLevel();
    }
}
