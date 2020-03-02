using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipBattleInfo 
{
    Dictionary<Vector2, bool> hitFloors = new Dictionary<Vector2, bool>();
    int totalFloorsCount = 1;

    public Vector2 clearAreaStart { get; protected set; }
    public Vector2 clearAreaEnd { get; protected set; }
    public Vector2 position { get; protected set; }
    public int floorsCount { get; protected set; } = 1;


    public ShipBattleInfo(int x, int y, bool isDamaged = false)
    {
        var newFloorPos = new Vector2(x, y);
        clearAreaStart = clearAreaEnd = position = newFloorPos;
        clearAreaEnd += Vector2.up + Vector2.right;
        clearAreaStart += Vector2.down + Vector2.left;

        Debug.Log($"new info created {this}");
        hitFloors.Add(newFloorPos, isDamaged);
    }

    public void MergeAnotherShipInfo(ShipBattleInfo another)
    {
        Debug.Log($"merging {another} by {this}");


        foreach (var floor in another.hitFloors)
            hitFloors.Add(floor.Key, floor.Value);
        floorsCount += another.floorsCount;
        totalFloorsCount += another.totalFloorsCount;
        if (Is1stLessThan2nd(another.clearAreaStart, clearAreaStart))
            clearAreaStart = another.clearAreaStart;
        else clearAreaEnd = another.clearAreaEnd;
    }

    bool Is1stLessThan2nd(Vector2 a, Vector2 b)
    {
        return a.x < b.x || a.y < b.y;
    }

    public void HitFloor(int x, int y)
    {
        var floorNormalPos = new Vector2(x, y);
        hitFloors[floorNormalPos] = true;
        floorsCount--;
    }

    public override string ToString()
    {
        var result = $"{base.ToString()} {clearAreaStart} ";
        result = $"{result} {clearAreaEnd} : {totalFloorsCount} floor(s)";
        return result;
    }
}
