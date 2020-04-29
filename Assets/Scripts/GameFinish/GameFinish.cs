using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFinish : MonoBehaviour
{
    static GameObject finalScreenObj;
    static Animation bgrAnimation, deepWaterAnimation;
    static bool hasBeenWon = false, toBeContinued = true;



    void Start()
    {
        finalScreenObj = transform.Find("FinalScreenObject").gameObject;
        var bgrObj = finalScreenObj.transform.Find("Background").gameObject;
        bgrAnimation = bgrObj.GetComponent<Animation>();
        var deepWaterObj = finalScreenObj.transform.Find("DeepWater").gameObject;
        deepWaterAnimation = deepWaterObj.GetComponent<Animation>();
    }

    public static void FinishRound(bool hasBeenWon)
    {
        GameFinish.hasBeenWon = hasBeenWon;
        toBeContinued = true;
        finalScreenObj.SetActive(true);

    }

    static void FinishWonGame()
    {
        
    }

    static void FinishLostGame()
    {
        bgrAnimation.Play("Sinking");
        deepWaterAnimation.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (!toBeContinued || bgrAnimation.IsPlaying("SlowAppearing")) return;
        if (hasBeenWon) FinishWonGame();
        else FinishLostGame();
        toBeContinued = false;
    }
}
