using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PagesSwitch : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene("GameStart");
    }
}
