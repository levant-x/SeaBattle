using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerGameField;

public class EnemyAI1 : IAttacker
{
    protected GameField.CellState[,] gameField;
    protected ShipsInfoStorage shipsInfoStorage = new ShipsInfoStorage();
    protected List<int[]> emptyCellsRanges = new List<int[]>(1);
    int[] selectedRange;


    public bool isGameOver => shipsInfoStorage.AreAllShipsSunk();

    public AttackResult AttackGameField(PlayerGameField gameField)
    {
        if (this.gameField == null) InitThis(gameField.Width(), gameField.Height());
        return GetResult(gameField);
    }

    protected virtual AttackResult GetResult(PlayerGameField gameField)
    {
        var targetPoint = GetLinearCoordinate();
        var decartCoords = Settings.ConvertLinearCoordinateToDecart(targetPoint,
            gameField.Width(), gameField.Height());
        int x = (int)decartCoords.x, y = (int)decartCoords.y;
        var result = gameField.Attack(x, y);

        if (result == AttackResult.Hit || result == AttackResult.Sunk)
            shipsInfoStorage.RegisterShipFloor(x, y, true);            
        if (result == AttackResult.Sunk)
            ShipsInfoStorage.DelineateArea(shipsInfoStorage.GetShipInfo(x, y), 
                DelineateShip);
        return result;
    }

    int GetLinearCoordinate()
    {
        var rangeIndex = Random.Range(0, emptyCellsRanges.Count);
        selectedRange = emptyCellsRanges[rangeIndex];
        var result = Random.Range(selectedRange[0], selectedRange[1]);
        SplitRangeByHalf(result);
        return result;
    }

    void SplitRangeByHalf(int point)
    {
        var range = selectedRange;
        emptyCellsRanges.Remove(range);
        if (range[0] < point) emptyCellsRanges.Add(new int[] { range[0], point });
        if (range[1] == point + 1) return;
        emptyCellsRanges.Add(new int[] { point + 1, range[1] });
    }

    protected virtual void DelineateShip(int x, int y)
    {
        if (!Settings.IsPointWithinMatrix(x, y, gameField)) return;
        gameField[x, y] = GameField.CellState.Misdelivered;
        var linearCoord = Settings
            .ConvertDecartCoordinatesToLinear(x, y, gameField.GetLength(0));
        selectedRange = FindRangeContainingNum(linearCoord);
        if (selectedRange != null) SplitRangeByHalf(linearCoord);
    }

    int[] FindRangeContainingNum(int number)
    {
        foreach (var range in emptyCellsRanges)
            if (range[0] <= number && number < range[1]) return range;
        return null;
    }

    protected virtual void InitThis(int width, int height)
    {
        gameField = new GameField.CellState[width, height];
        emptyCellsRanges.Add(new int[] { 0, width * height });
    }
}
