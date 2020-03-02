using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipsInfoStorage 
{
    protected Dictionary<Vector2, ShipBattleInfo> navy = new Dictionary<Vector2, ShipBattleInfo>();
    ShipBattleInfo newFloorInfo;


    public void RegisterShipFloor(int x, int y, bool isDamaged = false)
    {
        newFloorInfo = new ShipBattleInfo(x, y, isDamaged);
        var newFloorPos = new Vector2(x, y);
        SearchForNearbyFloors(newFloorPos); 
        navy.Add(newFloorPos, newFloorInfo);
    }

    void SearchForNearbyFloors(Vector2 newFloorPos)
    {
        CheckNearbyFloor(newFloorPos + Vector2.left, newFloorPos);
        CheckNearbyFloor(newFloorPos + Vector2.up, newFloorPos);
        CheckNearbyFloor(newFloorPos + Vector2.right, newFloorPos);
        CheckNearbyFloor(newFloorPos + Vector2.down, newFloorPos);            
    }

    void CheckNearbyFloor(Vector2 nearbyFloorPos, Vector2 newFloorPos)
    {
        var isNeighbour = navy.ContainsKey(nearbyFloorPos);
        if (isNeighbour) MergeFloors(nearbyFloorPos);
    }

    void MergeFloors(Vector2 existingFloorPos)
    {
        newFloorInfo.MergeAnotherShipInfo(navy[existingFloorPos]);
        var floorsToRebind = navy[existingFloorPos].GetAllFloors();
        foreach (var floorPos in floorsToRebind) navy[floorPos] = newFloorInfo;
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
            if (shipInfo.leftFloorsCount > 0) return false;
        return true;
    }

    public static void DelineateArea(ShipBattleInfo ship, Action<int, int> target)
    {
        Vector2 start = ship.clearAreaStart, end = ship.clearAreaEnd;
        Vector2 direction = Vector2.up, cursor = start;
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
