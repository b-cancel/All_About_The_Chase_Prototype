using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class restartBtn : MonoBehaviour {

    public void onReStartClick()
    {
        SceneManager.LoadScene("Start", LoadSceneMode.Single);
    }
}
