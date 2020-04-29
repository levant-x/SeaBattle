using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAllocator : GameField
{
    public event System.Action onAutoAllocationCompleted;

    List<Bounds> spawnAreas = new List<Bounds>();
    Bounds selectedArea;
    bool allignAtField;
    Dispatcher[] playDispatchers;


    public void OnAutoLocateClick(bool allignAtField = false)
    {
        this.allignAtField = allignAtField;
        ClearGameField();
        var firstArea = new Bounds(new Vector3((float)Width() / 2, (float)Height() / 2),
            new Vector3(Width(), Height()));
        spawnAreas.Add(firstArea);
        playDispatchers = Dispatcher.CreateAllShips();
        c = playDispatchers.Length - 1;
    }

    int c;

    void Update()
    {
        if (!AreAllShipsInitialized()) return;



        //foreach (Ship ship in playDispatchers)
        //{

        //}

        Ship ship = playDispatchers[c] as Ship;
        c--;
        AutoLocateShip(ship);
        if (allignAtField) ship.SetAutolocated();

        if (c == 0)
        {
            spawnAreas.Clear();
            playDispatchers = null;
            onAutoAllocationCompleted?.Invoke();
        }
    }

    void ClearGameField()
    {
        for (int i = 0; i < body.Length; i++) ClearFieldCell(i);
    }

    void ClearFieldCell(int i)
    {
        var decartCoords = Settings.ConvertLinearCoordinateToDecart(i,
            Width(), Height());
        body[(int)decartCoords.x, (int)decartCoords.y] = CellState.Empty;
    }

    bool AreAllShipsInitialized()
    {
        if (playDispatchers == null || playDispatchers.Length == 0) return false;
        foreach (Ship ship in playDispatchers)
            if (ship.floorsNum == 0) return false;
        return true;
    }

    void AutoLocateShip(Ship ship)
    {
        SelectArea(ship);
        var x = Random.Range((int)selectedArea.min.x, (int)selectedArea.max.x);
        var y = Random.Range((int)selectedArea.min.y, (int)selectedArea.max.y);
        ship.cellCenterPosition = boundsOfCells[x, y].center;
        //Debug.Log(x + " :: " + y);

        MarkupSpawnAreas(ship);
        MarkShipCellsAsOccupied(ship);
    }

    void SelectArea(Ship ship)
    {
        var areasWorkingList = CopyList(spawnAreas);
        for (int i = 0; i < body.Length; i++)
        {
            var areaNum = Random.Range(0, areasWorkingList.Count);
            var randomArea = areasWorkingList[areaNum];

            Debug.Log("Rnd area " + randomArea);
            Debug.Break();

            selectedArea = new Bounds(randomArea.center, randomArea.size);
            var isAreaAppropriate = CheckAndAdjustSpawnAreaAndOrient(ship,
                ref selectedArea);
            if (!isAreaAppropriate) areasWorkingList.Remove(randomArea);
            else break;
        }
    }

    List<T> CopyList<T>(List<T> list)
    {
        return new List<T>(list);
    }

    bool CheckAndAdjustSpawnAreaAndOrient(Ship ship, ref Bounds area)
    {
        var canStandHorizontally = area.size.x >= ship.floorsNum;
        var canStandVertically = area.size.y >= ship.floorsNum;

        //Debug.Log("vert " + canStandVertically + ", hor " + canStandHorizontally);

        float adjSize = ship.floorsNum - 1;
        //Debug.Log(adjSize + " adj size");

        if (!canStandHorizontally && !canStandVertically)
        {
            return false;
        }
        else if (canStandHorizontally && canStandVertically)
        {
            ship.orientation = (Ship.Orientation)Random.Range(0, 2);
            //Debug.Log("random orient " + ship);
        }
        else if (canStandHorizontally) ship.orientation = Ship.Orientation.Horizontal;
        else ship.orientation = Ship.Orientation.Vertical;

        if (ship.orientation == Ship.Orientation.Horizontal)
        {
            area.center = new Vector3(area.center.x - adjSize / 2, area.center.y);
            area.Expand(new Vector3(-adjSize, 0));
            //Debug.Log("expanded hor " + area + $" min {area.min}, max {area.max}");
        }
        else
        {
            area.center = new Vector3(area.center.x, area.center.y + adjSize / 2);
            area.Expand(new Vector3(0, -adjSize));
            //Debug.Log("expanded vert " + area + $" min {area.min}, max {area.max}");
        }
        return true;
    }


    void MarkupSpawnAreas(Ship ship)
    {

        Debug.Log("marking up the rest for ship " + ship);
        //System.Threading.Thread.Sleep(500);
        Debug.Break();

        var normCellPosition = GetCellMatrixPos(ship.cellCenterPosition);
        float x = normCellPosition.x, y = normCellPosition.y;

        float shipExtention = (float)ship.floorsNum / 2;
        float centerX = x + shipExtention, centerY = y + 0.5f;
        float areaWidth = ship.floorsNum + 2, areaHeight = 3;

        if (ship.orientation == Ship.Orientation.Vertical)
        {
            centerY = y + 1 - shipExtention;
            centerX = x + 0.5f;
            areaHeight = areaWidth;
            areaWidth = 3;
        }
        var occupiedArea = new Bounds(new Vector3(centerX, centerY),
            new Vector3(areaWidth, areaHeight));

        //Debug.Log($"ship has taken {shipExtention} {areaWidth} {occupiedArea.min} {occupiedArea.max}");

        var spawnAreasCopy = CopyList(spawnAreas);
        int c = 0;
        for (int i = 0; i < spawnAreasCopy.Count; i++)
        {
            var area = spawnAreasCopy[i];
           
            if (!AreBoundsOverlap(area, occupiedArea)) continue;

            //Debug.Log($"intersected area {area.min} {area.max}");

            spawnAreas.Remove(area);
            SplitSpawnArea(area, occupiedArea);

            c++; if (c > 100) break;
        }
    }

    bool AreBoundsOverlap(Bounds initial, Bounds occup)
    {
        if (initial == selectedArea) return true;
        var minMaxX = Mathf.Min(initial.max.x, occup.max.x);
        var maxMinX = Mathf.Max(initial.min.x, occup.min.x);
        var minMaxY = Mathf.Min(initial.max.y, occup.max.y);
        var maxMinY = Mathf.Max(initial.min.y, occup.min.y);

        var overlap = minMaxX > maxMinX && minMaxY > maxMinY;

        //Debug.Log($"old {initial.min} {initial.max}, new {occup.min} {occup.max},  {overlap}");
        return overlap;
    }

    void SplitSpawnArea(Bounds initialArea, Bounds occupiedArea)
    {
        CreateSubarea(initialArea, initialArea.max.y, occupiedArea.max.y, false);
        CreateSubarea(initialArea, initialArea.max.x, occupiedArea.max.x, true);
        CreateSubarea(initialArea, occupiedArea.min.y, initialArea.min.y, false);
        CreateSubarea(initialArea, occupiedArea.min.x, initialArea.min.x, true);
    }

    void CreateSubarea(Bounds initArea, float max, float min, bool isVertical)
    {
        if (max - min < 1) return;
        var subArea = new Bounds();
        if (isVertical)
        {
            subArea.center = new Vector3((max + min) / 2, initArea.center.y);
            subArea.size = new Vector3(max - min, initArea.size.y);
        }
        else
        {
            subArea.center = new Vector3(initArea.center.x, (max + min) / 2);
            subArea.size = new Vector3(initArea.size.x, max - min);
        }
        spawnAreas.Add(subArea);
        //Debug.Log($"area created {subArea.min} {subArea.max}");
    }

    void OnDrawGizmos()
    {
        var colors = new Color[]
        {
            Color.blue, Color.cyan, Color.green, Color.magenta, Color.red, Color.yellow, Color.black
        };
        int c = 0;
        foreach (var area in spawnAreas)
        {
            Gizmos.color = colors[c];
            var center = area.center * cellSize + new Vector3(bottomLeftCorner.x,
                bottomLeftCorner.y) - Vector3.one * Random.Range(-0.2f, 0.2f);
            Gizmos.DrawWireCube(center, area.size * cellSize);
            c++;
            if (c == colors.Length) c = 0;
        }
    }
}
