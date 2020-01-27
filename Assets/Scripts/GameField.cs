using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameField : MonoBehaviour
{
    public enum CellState
    {
        Empty, Misdelivered, Occupied, Misplaced, Hit
    }


    public GameObject cellPrefab;
    public float bottomLeftX, bottomLeftY;
    public bool toAdjustOrigin = false;

    static int[,] body = new int[10, 8];
    static Bounds[,] boundsOfCells;
    static float cellSize;

    List<GameObject> cells = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        var sprRenderer = cellPrefab.GetComponent<SpriteRenderer>();
        cellSize = sprRenderer.bounds.size.x;
        GenerateField();
    }

    // Update is called once per frame
    void Update()
    {
        if (toAdjustOrigin)
        {
            foreach (var item in cells) Destroy(item);
            GenerateField();
        }
    }

    void GenerateField()
    {
        boundsOfCells = new Bounds[Width(), Height()];
        for (int i = 0; i < Width(); i++)
        {
            for (int j = 0; j < Height(); j++)
            {
                var cellPos = new Vector2(bottomLeftX + i * cellSize, bottomLeftY + j * cellSize);
                var cell = Instantiate(cellPrefab, cellPos, Quaternion.identity);
                boundsOfCells[i, j] = new Bounds(cell.transform.position, 
                    new Vector3(cellSize, cellSize));
                cells.Add(cell);
            }
        }
    }

    static int Width()
    {
        return body.GetLength(0);
    }

    static int Height()
    {
        return body.GetLength(1);
    }

    public static void CheckLocationOverField(Vector3 mousePos, Ship ship)
    {
        var bottomLeftBounds = boundsOfCells[0, 0].min;
        var upperRightBounds = boundsOfCells[Width() - 1, Height() - 1].max;
        var isShipOverField = mousePos.x > bottomLeftBounds.x && mousePos.x < upperRightBounds.x &&
            mousePos.y > bottomLeftBounds.y && mousePos.y < upperRightBounds.y;

        if (!isShipOverField)
        {
            ship.isPositionCorrect = false;
            ship.isWithinCell = false;
            return;
        }

        var dx = mousePos.x - bottomLeftBounds.x;
        var dy = mousePos.y - bottomLeftBounds.y;

        int x = (int)(dx / cellSize), y = (int)(dy / cellSize);
        ship.cellIndexX = x;
        ship.cellIndexY = y;
        ship.isWithinCell = true;
        ship.cellCenterPosition = boundsOfCells[x, y].center;
        ship.isPositionCorrect = IsLocationAppropriate(ship, x, y);
    }

    static bool IsLocationAppropriate(Ship ship, int x, int y)
    {
        if (ship.orientation == Ship.Orientation.Horizontal) x += ship.FloorsNum() - 1;
        else y -= ship.FloorsNum() - 1;
        
        if (!IsPointWithinMatrix(x, y) || ship.IsCollided()) return false;
        return true;
    }

    static bool IsPointWithinMatrix(int x, int y)
    {
        return x >= 0 && x < Width() && y >= 0 && y < Height();
    }

    public static void MarkShipCellsAsOccupied(Ship ship)
    {
        SetCellsStateUnderneathShip(ship, CellState.Occupied);
    }

    public static void TakeShipOff(Ship ship)
    {
        SetCellsStateUnderneathShip(ship, CellState.Empty);
    }

    static void SetCellsStateUnderneathShip(Ship ship, CellState cellState)
    {
        int x = ship.cellIndexX, y = ship.cellIndexY;
        for (int i = 0; i < ship.FloorsNum(); i++)
        {
            body[x, y] = (int)cellState;
            if (ship.orientation == Ship.Orientation.Horizontal) x += 1;
            else y -= 1;
        }
    }
}
