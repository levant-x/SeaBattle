using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipBattleInfo 
{
    List<Vector2> floors = new List<Vector2>();
    int initialFloorsCount = 1;

    public Vector2 clearAreaStart { get; protected set; }
    public Vector2 clearAreaEnd { get; protected set; }
    public Vector2 position { get; protected set; }
    public int leftFloorsCount { get; protected set; } = 1;


    public ShipBattleInfo(int x, int y, bool isDamaged = false)
    {
        var newFloorPos = new Vector2(x, y);
        clearAreaStart = clearAreaEnd = position = newFloorPos;
        clearAreaStart += Vector2.down + Vector2.left;
        clearAreaEnd += Vector2.up + Vector2.right;
        floors.Add(newFloorPos);
    }

    public void MergeAnotherShipInfo(ShipBattleInfo another)
    {
        floors.AddRange(another.floors);
        leftFloorsCount += another.leftFloorsCount;
        initialFloorsCount += another.initialFloorsCount;

        if (Is1stLessThan2nd(another.clearAreaStart, clearAreaStart))
            clearAreaStart = another.clearAreaStart;
        else clearAreaEnd = another.clearAreaEnd;
    }

    public Vector2[] GetAllFloors()
    {
        var result = new Vector2[floors.Count];
        floors.CopyTo(result);
        return result;
    }

    bool Is1stLessThan2nd(Vector2 a, Vector2 b)
    {
        return a.x < b.x ^ a.y < b.y;
    }

    public void HitFloor(int x, int y)
    {
        leftFloorsCount--;
    }

    public override string ToString()
    {
        var result = $"{base.ToString()} {clearAreaStart} ";
        result = $"{result} {clearAreaEnd} : {initialFloorsCount} floor(s), ";
        return result + leftFloorsCount + " left";
    }
}
