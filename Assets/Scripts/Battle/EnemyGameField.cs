using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGameField : PlayerGameField
{
    bool isAttackInProcess = false;

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
        //Debug.Log("this is " + GetType().Name);
        //Debug.Log("cell belongs to " + cell.transform.parent.name);


        if (cell.transform.parent.name != originObjName ||
            hasBeenInput) return;
        var cellNormalPos = GetCellMatrixPos(cell.transform.position);
        targetX = (int)cellNormalPos.x;
        targetY = (int)cellNormalPos.y;

        if (body[targetX, targetY] == CellState.Hit || 
            body[targetX, targetY] == CellState.Misdelivered) return;
        hasBeenInput = true;
        if (Settings.isMultiplayerMode) 
        {
            // attack the net
            // attack self 
            hasBeenInput = false;
            Debug.Log("input switched");
        }
        // otherwise the player detects by itself it's time to attack 



    }

    void FixedUpdate()
    {
        //if (!Equals(currentPlayer) || isAttackInProcess || 
        //    Settings.isMultiplayerMode) return;
        /*if (isAttackInProcess)*/ return;
        InvokeAttack();
    }

    void InvokeAttack()
    {
        isAttackInProcess = true;
        Invoke("AttackEnemyAfterPause", Random.Range(1f, 1f));
    }

    void AttackEnemyAfterPause()
    {
        var attResult = Settings.attacker.AttackGameField(enemy);
        if (attResult != AttackResult.Misdelivered) InvokeAttack();
        else isAttackInProcess = false;
    }
}
