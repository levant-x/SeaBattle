using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLauncher : AutoAllocator
{
    GameObject errorMessagePanel;

    protected override void Start()
    {
        base.Start();
        autoAllocationCompleted += OnAutoAllocationCompleted;
        errorMessagePanel = transform.Find("ErrorMessagePanel").gameObject;
    }


    public void OnGameStartButtonClick()
    {
        OnAutoLocateClick();

        //if (Dispatcher.AreAllShipsAllocated()) PrepareForGameStart();
        //else errorMessagePanel.SetActive(true);
    }

    int a = 0;

    private void OnAutoAllocationCompleted()
    {
        if (a == 0)
        {
            Settings.playerField = (CellState[,])body.Clone();
            a++;
            OnAutoLocateClick();
            return;
        }
        Settings.enemyField = (CellState[,])body.Clone();
        Settings.ChangeScene("Battle");
    }

    void PrepareForGameStart()
    {

        Settings.playerField = CloneField();
        OnAutoLocateClick();
        Settings.enemyField = CloneField();
        PrintField(body);
        Settings.ChangeScene("Battle");
    }

    CellState[,] CloneField()
    {
        return (CellState[,])body.Clone();
    }
}
