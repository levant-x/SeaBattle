using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipBattleInfo 
{
    Dictionary<Vector2, bool> hitFloors = new Dictionary<Vector2, bool>();


    public Vector2 clearAreaStart { get; protected set; }
    public Vector2 clearAreaEnd { get; protected set; }
    public int floorsCount { get; protected set; } = 0;


    public ShipBattleInfo(int x, int y)
    {
        var newFloorPos = new Vector2(x, y);
        clearAreaStart = new Vector2(x - 1, y - 1);
        clearAreaEnd = new Vector2(x + 1, y + 1);
        AddFloor(clearAreaStart);
    }

    public void AddFloor(Vector2 newFloorPos)
    {
        hitFloors.Add(newFloorPos, false);
        if (newFloorPos.x == clearAreaEnd.x)
            clearAreaEnd = new Vector2(newFloorPos.x + 1, newFloorPos.y);
        else clearAreaEnd = new Vector2(newFloorPos.x, newFloorPos.y);
        floorsCount++;
    }

    public void HitFloor(int x, int y)
    {
        var floorNormalPos = new Vector2(x, y);
        hitFloors[floorNormalPos] = true;
        floorsCount--;
    }
}
