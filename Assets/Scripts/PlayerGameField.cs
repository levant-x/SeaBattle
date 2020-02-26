using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGameField : GameField
{
    protected Animator[,] cellsAnimators;
    protected bool[,] hitCells = new bool[Width(), Height()];
    protected static PlayerGameField currentPlayer;
    protected Dictionary<Vector2, ShipBattleInfo> navy = new Dictionary<Vector2, ShipBattleInfo>();
    bool makeFloorCellVisible = true;

    int[,] r = body.Clone() as int[,];

    protected override void Start()
    {  

        for (int i = 0; i < 20; i++)
        {
            int x = Random.Range(0, Width()), y = Random.Range(0, Height());
            r[x, y] = (int)CellState.Occupied;
            Debug.Log($"{x} {y}");
            
        }    
        

        cellsAnimators = new Animator[Width(), Height()];
        originObjName = "PlayerGameField";
        if (Camera.main.aspect < 2)
            cellToCamHeightProportion = Camera.main.aspect / 22f;
        base.Start(); 
    }

    protected override void OnCellGenerated(int i, int j, GameObject cell)
    {
        base.OnCellGenerated(i, j, cell);
        body[i, j] = r[i, j];
        var cellAnimator = cell.GetComponent<Animator>();
        cellsAnimators[i, j] = cellAnimator;
        if (body[i, j] == (int)CellState.Occupied) RegisterShip(i, j, cellAnimator);
    }

    private void RegisterShip(int i, int j, Animator cellAnimator)
    {
        var floorPos = new Vector2(i, j);
        navy.TryGetValue(floorPos, out ShipBattleInfo shipBattleInfo);
        if (shipBattleInfo == null) shipBattleInfo = new ShipBattleInfo(i, j);
        else shipBattleInfo.AddFloor(floorPos);
        navy.Add(floorPos, shipBattleInfo);
        if (makeFloorCellVisible) cellAnimator.SetTrigger(CellState.Occupied.ToString());
    }

    public AttackResult Attack(int x, int y)
    {
        var result = new AttackResult();
        if (currentPlayer == this || hitCells[x, y]) return result;

        if (body[x, y] == (int)CellState.Empty) body[x, y] = (int)CellState.Misdelivered;
        else body[x, y] = (int)CellState.Hit;

        result.status = (AttackResult.Status)body[x, y];
        if (result.status == AttackResult.Status.Hit) DamageShip(x, y, result);
        return result;
    }

    void DamageShip(int x, int y, AttackResult attackResult)
    {
        var damagedShip = navy[new Vector2(x, y)];
        damagedShip.HitFloor(x, y);
        if (damagedShip.floorsCount == 0)
        {
            attackResult.status = AttackResult.Status.Sunk;
            attackResult.clearAreaStart = damagedShip.clearAreaStart;
            attackResult.clearAreaEnd = damagedShip.clearAreaEnd;
            DelineateShip(damagedShip.clearAreaStart, damagedShip.clearAreaEnd);
        }
    }

    void DelineateShip(Vector2 start, Vector2 end)
    {
        int startY = (int)start.y, endY = (int)end.y, startX = (int)start.x;
        bool toSkipMiddleHere = end.x - start.x == 3;
        for (int x = startX; x <= end.x; x++)
        {
            if (toSkipMiddleHere && x == (int)start.x + 1) continue;
            DelineateShipHorizontally(x, startY, endY, !toSkipMiddleHere);
        }
    }

    void DelineateShipHorizontally(int x, int startY, int endY, bool toSkipMiddle)
    {
        for (int y = startY; y <= endY; y++)
        {
            if (toSkipMiddle && y == startY + 1) continue;
            else if (body[y, y] != (int)CellState.Empty) continue;
            body[y, y] = (int)CellState.Misdelivered;
            cellsAnimators[y, y].SetTrigger(CellState.Misdelivered.ToString());
        }
    }



    public static void OnCellClick(GameObject cell)
    {
        Debug.Log(cell.name);
    }
}
