using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipsInfoStorage 
{
    protected Dictionary<Vector2, ShipBattleInfo> navy = new Dictionary<Vector2, ShipBattleInfo>();
    Vector2[] searchDirections = new Vector2[]
    { Vector2.left, Vector2.up, Vector2.right, Vector2.down };
    ShipBattleInfo newFloorInfo;


    public void RegisterShipFloor(int x, int y, bool isDamaged = false)
    {
        newFloorInfo = new ShipBattleInfo(x, y, isDamaged);
        var newFloorPos = new Vector2(x, y);
        //foreach (var existingFloorPos in navy.Keys)
            SearchForNearbyFloors(newFloorPos); 

        navy.Add(newFloorPos, newFloorInfo);
    }

    void SearchForNearbyFloors(Vector2 newFloorPos)
    {
        foreach (var dir in searchDirections)
            CheckNearbyFloor(new Vector2(newFloorPos.x, newFloorPos.y)
                + dir, newFloorPos);
    }

    void CheckNearbyFloor(Vector2 nearbyFloorPos, Vector2 newFloorPos)
    {
        var isNeighbour = navy.ContainsKey(nearbyFloorPos);
            //&&             !navy[nearbyFloorPos].Equals(newFloorInfo);

        Debug.Log($"floor by {nearbyFloorPos} isNeighbour= {isNeighbour}");
        if (isNeighbour) MergeFloors(nearbyFloorPos);
    }

    void MergeFloors(Vector2 existingFloorPos)
    {
        newFloorInfo.MergeAnotherShipInfo(navy[existingFloorPos]);
        navy[existingFloorPos] = newFloorInfo;

        Debug.Log($"new ship after merging {newFloorInfo}");
    }

    ShipBattleInfo TryGetShip(Vector2 key)
    {
        navy.TryGetValue(key, out ShipBattleInfo result);
        return result;
    }

    public ShipBattleInfo GetShipInfo(int x, int y)
    {
        return TryGetShip(new Vector2(x, y));
    }

    public bool AreAllShipsSunk()
    {
        foreach (var shipInfo in navy.Values)
            if (shipInfo.floorsCount > 0) return false;
        return true;
    }

    public static void DelineateArea(ShipBattleInfo ship, Action<int, int> target)
    {
        Vector2 start = ship.clearAreaStart, end = ship.clearAreaEnd;
        Vector2 direction = Vector2.up, cursor = start;
        Debug.Log(ship);
        do
        {      
            target((int)cursor.x, (int)cursor.y);
            cursor += direction;
            if (IsCornerPosition(cursor, start, end)) TurnAroundCorner(ref direction);
        } while (cursor != start);
    }

    static bool IsCornerPosition(Vector2 cursor, Vector2 start, Vector2 end)
    {
        var result = (cursor.x == start.x || cursor.x == end.x) &&
            (cursor.y == start.y || cursor.y == end.y);
        return result;
    }

    static void TurnAroundCorner(ref Vector2 direction)
    {
        var oldValueX = direction.x;
        direction.x = direction.y;
        direction.y = -oldValueX;
    }
}
