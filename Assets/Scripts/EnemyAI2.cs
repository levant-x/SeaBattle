using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerGameField;

public class EnemyAI2 : EnemyAI1
{
    List<List<Vector2>> directions;
    List<Vector2> vertical, horizontal;
    AttackResult lastResult;

    protected override AttackResult GetResult(PlayerGameField gameField)
    {
        return base.GetResult(gameField);
    }

    protected override void InitThis(int width, int height)
    {
        base.InitThis(width, height);
        ResetDirections();
    }

    void ResetDirections()
    {
        directions = new List<List<Vector2>>() { horizontal, vertical };
        horizontal = new List<Vector2>() { Vector2.left, Vector2.right };
        vertical = new List<Vector2>() { Vector2.up, Vector2.down };
    }
}
