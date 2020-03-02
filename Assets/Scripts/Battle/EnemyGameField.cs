using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGameField : PlayerGameField
{
    bool isAttackInProcess = false, isGameFinishing = false;


    public EnemyGameField()
    {
        discloseFloorCells = true;
        BattleCell.onCellClick += OnBattleCellClick;
    }

    protected override void Start()
    {
        Settings.enemyInitialized += OnEnemyInitialized;
        base.Start();
    }

    protected override CellState GetCellValue(int x, int y)
    {
        return Settings.enemyField[x, y];
    }
       
    private void OnBattleCellClick(GameObject cell)
    {
        if (cell.transform.parent.name != originObjName ||
            hasBeenInput) return;
        var cellNormalPos = GetCellMatrixPos(cell.transform.position);
        targetX = (int)cellNormalPos.x;
        targetY = (int)cellNormalPos.y;

        if (body[targetX, targetY] == CellState.Hit || 
            body[targetX, targetY] == CellState.Misdelivered) return;
        hasBeenInput = true;

        if (!Settings.isMultiplayerMode) return;
        // (the player detects by itself it's time to attack)

        // attack the net
        // attack self 
        hasBeenInput = false;
    }

    void FixedUpdate()
    {
        if (isGameOver) FinishGame();
        else if (!Equals(currentPlayer) || isAttackInProcess ||
            Settings.isMultiplayerMode || isGameFinishing) return;
        else InvokeAttack();
    }

    void InvokeAttack()
    {
        isAttackInProcess = true;
        Invoke("AttackEnemyAfterPause", Random.Range(1f, 2f));
    }

    void AttackEnemyAfterPause()
    {
        var attResult = Settings.attacker.AttackGameField(enemy);
        if (attResult != AttackResult.Misdelivered)
            if (Settings.attacker.isGameOver) isGameOver = true;
            else InvokeAttack();
        else isAttackInProcess = false;
    }

    void FinishGame()
    {
        isGameFinishing = true;
        Invoke("Back", 3);
    }

    void Back()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameStart");
    }
}
