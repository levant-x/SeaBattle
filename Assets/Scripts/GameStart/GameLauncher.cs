using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLauncher : AutoAllocator
{
    GameObject errorMessagePanel;

    protected override void Start()
    {
        base.Start();
        errorMessagePanel = transform.Find("ErrorMessagePanel").gameObject;
    }


    public void OnGameStartButtonClick()
    {
        if (Dispatcher.AreAllShipsAllocated()) PrepareForGameStart();
        else errorMessagePanel.SetActive(true);
    }

    void PrepareForGameStart()
    {
        Settings.PlayerField = body;
        OnAutoLocateClick();
        Settings.EnemyField = body;
    }
}
