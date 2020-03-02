using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGameField : GameField
{
    public enum AttackResult
    {
        Misdelivered, Hit, Sunk, Error
    }

    protected Animator[,] cellsAnimators;
    protected ShipsInfoStorage shipsInfoStorage = new ShipsInfoStorage();
    protected bool discloseFloorCells = true;

    protected static PlayerGameField currentPlayer;
    protected PlayerGameField enemy = null;
    protected static bool hasBeenInput = false;
    protected static int targetX, targetY;


    public PlayerGameField()
    {
        originObjName = GetType().Name + "Origin";
        Debug.Log(originObjName);
        cellsAnimators = new Animator[Width(), Height()];
    }
    
    protected override void Start()
    {
        if (Camera.main.aspect < 2)
            cellToCamHeightProportion = Camera.main.aspect / 22f;
        Settings.enemyInitialized += OnEnemyInitialized;
        Settings.RegisterGameField(this);
        base.Start();
    }

    protected void OnEnemyInitialized(PlayerGameField gameField)
    {
        if (gameField.GetType() != GetType()) RegisterEnemy(gameField);
    }

    void RegisterEnemy(PlayerGameField gameField)
    {
        enemy = gameField;
        if (this is EnemyGameField) currentPlayer = this;
    }

    protected override void OnCellGenerated(int x, int y, GameObject cell)
    {
        base.OnCellGenerated(x, y, cell);
        var cellAnimator = cell.GetComponent<Animator>();
        cellsAnimators[x, y] = cellAnimator;
        body[x, y] = GetCellValue(x, y);
        if (body[x, y] == CellState.Occupied) GenerateFloor(x, y, cellAnimator);
    }

    void GenerateFloor(int x, int y, Animator animator)
    {
        shipsInfoStorage.RegisterShipFloor(x, y);
        if (discloseFloorCells) animator.SetTrigger($"{CellState.Occupied}");
    }

    protected virtual CellState GetCellValue(int x, int y)
    {
        return Settings.playerField[x, y];
    }

    public AttackResult Attack(int x, int y)
    {
        //Debug.Log(GetType() + " attacked by enemy " + enemy.GetType().Name);

        var result = AttackResult.Error;
        if (!CanReceiveAttack(x, y)) return result; 
        if (body[x, y] == CellState.Empty) body[x, y] = CellState.Misdelivered;
        else body[x, y] = CellState.Hit;

        result = (AttackResult)Enum.Parse(typeof(AttackResult), $"{body[x, y]}");
        var animTrigger = result;
        if (result == AttackResult.Hit)
        {
            result = DamageShip(x, y);
            if (shipsInfoStorage.AreAllShipsSunk()) GameOver();
        }
        else
        {
            //Debug.Log(currentPlayer.GetType() + " was current ");
            //Debug.Log(enemy.GetType() + " is enemy");

            //currentPlayer = this;

            //Debug.Log(currentPlayer.GetType() + " plays now");
        }
        cellsAnimators[x, y].SetTrigger(animTrigger.ToString());
        return result;
    }
    
    protected bool CanReceiveAttack(int x, int y)
    {
        var result = !Equals(currentPlayer) &&
            body[x, y] != CellState.Hit && body[x, y] != CellState.Misdelivered;
        return result;
    }

    AttackResult DamageShip(int x, int y)
    {
        var damagedShip = shipsInfoStorage.GetShipInfo(x, y);
        damagedShip.HitFloor(x, y);
        if (damagedShip.floorsCount == 0)
        {
            ShipsInfoStorage.DelineateArea(damagedShip, MarkCell);
            ShipsInfoStorage.DelineateArea(damagedShip, AnimateCell);
            return AttackResult.Sunk;
        }
        else return AttackResult.Hit;
    }

    protected void MarkCell(int x, int y)
    {
        if (!IsPointWithinMatrix(x, y) || body[x, y] == CellState.Misdelivered) return;
        body[x, y] = CellState.Misdelivered;
    }

    protected void AnimateCell(int x, int y)
    {
        if (!IsPointWithinMatrix(x, y)) return;
        cellsAnimators[x, y].SetTrigger($"{CellState.Misdelivered}");
    } 
    
    protected void GameOver()
    {

    }

    void FixedUpdate()
    {
        if (Settings.isMultiplayerMode || !hasBeenInput) return;
        Debug.Log(enemy.Attack(targetX, targetY));
        hasBeenInput = false;
        Debug.Log("input switched");
    }
}
