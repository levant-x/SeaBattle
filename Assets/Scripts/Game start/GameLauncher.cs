using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLauncher : AutoAllocator
{
    GameObject errorMessagePanel;
    bool arePlayerShipsLocated, areEnemyShipsLocated, toLaunchGame;

    protected override void Start()
    {
        base.Start();
        onAutoAllocationCompleted += OnAutoAllocationCompleted;
        errorMessagePanel = transform.Find("ErrorMessagePanel").gameObject;
        areEnemyShipsLocated = arePlayerShipsLocated = toLaunchGame = false;
    }

    public void OnGameStartButtonClick()
    {
        if (Dispatcher.AreAllShipsAllocated()) PrepareForGameStart();
        else OnAutoLocateClick();  // errorMessagePanel.SetActive(true);
    }

    void PrepareForGameStart()
    {
        if (toLaunchGame) return;
        toLaunchGame = true;
        Settings.playerField = CloneField();
        OnAutoLocateClick();
    }
    
    private void OnAutoAllocationCompleted()
    {
        if (!toLaunchGame) return;
        Settings.enemyField = CloneField();
        Settings.ChangeScene("Battle");
    }

    CellState[,] CloneField()
    {
        return (CellState[,])body.Clone();
    }
}
