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

    List<int> indexes = new List<int>();


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

        if (result == AttackResult.Error) throw new System.Exception("attacking again "
            + targetPoint);
        if (result == AttackResult.Hit || result == AttackResult.Sunk)
        {
            Debug.LogError("REgistering ship from AI");
            shipsInfoStorage.RegisterShipFloor(x, y, true);

        }
            
        if (result == AttackResult.Sunk)
        {
            //Debug.Log("GETTING SUNK SHIP");
            var damagedShip = shipsInfoStorage.GetShipInfo(x, y);

            Debug.Log("Outlining");
            ShipsInfoStorage.DelineateArea(damagedShip, DelineateShip);
        }
        return result;
    }

    int GetLinearCoordinate()
    {
        var rangeIndex = Random.Range(0, emptyCellsRanges.Count);
        selectedRange = emptyCellsRanges[rangeIndex];

        var result = Random.Range(selectedRange[0], selectedRange[1]);
        UnityEditor.EditorApplication.isPaused = true;


        Debug.Log(result + $" SELECTED in range # {selectedRange[0]} - {selectedRange[1]}" +
            $" among " +            emptyCellsRanges.Count + " total");

        SplitRangeByHalf(result);
        if (indexes.Contains(result))
        {
            Debug.LogError("REPEATED INDEX");
            foreach (var item in emptyCellsRanges)
            {
                Debug.Log($"{item[0]} - {item[1]}");
            }


            throw new System.Exception(result + " repeated");
        }
        indexes.Add(result);

        return result;
    }

    void SplitRangeByHalf(int point)
    {
        var range = selectedRange;
        emptyCellsRanges.Remove(range);
        Debug.Log($"range {range[0]} {range[1]}, point {point}");
        var before = new int[] { range[0], point };
        var after = new int[] { point + 1, range[1] };
        if (range[0] < point)
        {
            emptyCellsRanges.Add(before);
            Debug.Log($"and {before[0]} {before[1]}");
        }
        if (range[1] == point + 1) return;
        emptyCellsRanges.Add(after);
        Debug.Log($"and {after[0]} {after[1]}");
    }

    protected virtual void DelineateShip(int x, int y)
    {

        if (!Settings.IsPointWithinMatrix(x, y, gameField))
        {
            Debug.LogWarning($"{x} {y} is not in matrix");
            return;
        }
        gameField[x, y] = GameField.CellState.Misdelivered;
        var linearCoord = Settings
            .ConvertDecartCoordinatesToLinear(x, y, gameField.GetLength(0));
        selectedRange = FindRangeContainingNum(linearCoord);
        if (selectedRange != null)
        {
            Debug.LogWarning($"floor outlining at {linearCoord}");
            SplitRangeByHalf(linearCoord);
        }
        else if (Settings.IsPointWithinMatrix(x, y, gameField))
        {
            //Debug.Log($"point {x} {y}");
            //foreach (var item in emptyCellsRanges)
            //{
            //    Debug.Log($"{item[0]} - {item[1]}");
            //}
            //throw new System.Exception();
        }
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
