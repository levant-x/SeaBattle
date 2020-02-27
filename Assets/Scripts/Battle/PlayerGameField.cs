using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGameField : GameField
{
    protected enum AttackResult
    {
        Misdelivered, Hit, Sunk, Error
    }

    protected Animator[,] cellsAnimators;
    protected bool[,] hitCells = new bool[Width(), Height()];
    protected static PlayerGameField currentPlayer;
    protected Dictionary<Vector2, ShipBattleInfo> navy = new Dictionary<Vector2, ShipBattleInfo>();
    bool makeFloorCellVisible = true;

    static PlayerGameField thisInstance;

    CellState[,] r = body.Clone() as CellState[,];

    protected override void Start()
    {
        thisInstance = this;

        for (int i = 0; i < 20; i++)
        {
            int x = Random.Range(0, Width()), y = Random.Range(0, Height());
            r[x, y] = CellState.Occupied;
            
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
        if (body[i, j] == CellState.Occupied)
        {
            RegisterShip(i, j);
            if (makeFloorCellVisible) cellAnimator.SetTrigger($"{CellState.Occupied}");
        }
    }

    private void RegisterShip(int i, int j)
    {
        var floorPos = new Vector2(i, j);
        ShipBattleInfo shipBattleInfo = TryGetShip(i - 1, j);
        if (shipBattleInfo == null) shipBattleInfo = TryGetShip(i, j - 1); 

        if (shipBattleInfo != null) shipBattleInfo.AddFloor(floorPos);
        else shipBattleInfo = new ShipBattleInfo(i, j);
        navy.Add(floorPos, shipBattleInfo);        
    }

    ShipBattleInfo TryGetShip(int x, int y)
    {
        navy.TryGetValue(new Vector2(x, y), out ShipBattleInfo result);
        return result;
    }

    protected AttackResult Attack(int x, int y)
    {
        var result = AttackResult.Error;
        if (currentPlayer == this || hitCells[x, y]) return result;
        if (body[x, y] == (int)CellState.Empty)
        {
            body[x, y] = CellState.Misdelivered;
            result = AttackResult.Misdelivered;
        }
        else
        {
            body[x, y] = CellState.Hit;
            result = AttackResult.Hit;
        }

        if (result == AttackResult.Hit) DamageShip(x, y, result);
        cellsAnimators[x, y].SetTrigger(result.ToString());
        return result;
    }

    void DamageShip(int x, int y, AttackResult attackResult)
    {
        var damagedShip = navy[new Vector2(x, y)];
        damagedShip.HitFloor(x, y);
        if (damagedShip.floorsCount == 0)
        {
            attackResult = AttackResult.Sunk;
            DelineateShip(damagedShip.clearAreaStart, damagedShip.clearAreaEnd);
        }
    }

    void DelineateShip(Vector2 start, Vector2 end)
    {
        // посмотреть ечейки выше, ниже, левее, правее

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
        //for (int y = startY; y <= endY; y++)
        //{
        //    if (toSkipMiddle && y == startY + 1) continue;
        //    else if (body[y, y] != (int)CellState.Empty) continue;
        //    body[y, y] = (int)CellState.Misdelivered;
        //    cellsAnimators[y, y].SetTrigger(CellState.Misdelivered.ToString());
        //}
    }



    public static void OnCellClick(GameObject cell)
    {
        var normalPos = GetCellMatrixPos(cell.transform.position);
        Debug.Log(normalPos);
        var r = thisInstance.Attack((int)normalPos.x, (int)normalPos.y);
        Debug.Log(r);
    }
}
