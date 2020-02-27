using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameField : MonoBehaviour
{
    public enum CellState
    {
        Empty, Misdelivered, Occupied, Hit
    }


    public GameObject cellPrefab;
    public float bottomLeftX;
    public float bottomLeftY;
    public bool toAdjustOrigin = false;
    public static float cellToCamHeightProportion = 1 / 11f;

    protected static CellState[,] body = new CellState[10, 8];
    protected static Bounds[,] boundsOfCells;
    protected static float cellSize;
    protected static string originObjName = "GameFieldOrigin";

    GameObject origin;
    static Vector2 bottomLeftCorner;


    // Start is called before the first frame update
    protected virtual void Start()
    {
        origin = GameObject.Find(originObjName);
        origin.transform.position = new Vector3(bottomLeftX, bottomLeftY);

        var sprRenderer = cellPrefab.GetComponent<SpriteRenderer>();
        Settings.ScaleSpriteByY(sprRenderer, cellToCamHeightProportion, out cellSize);
        GenerateField();
        bottomLeftCorner = boundsOfCells[0, 0].min;

    }

    // Update is called once per frame
    void Update()
    {
        if (!toAdjustOrigin) return;
        var originPosition = new Vector2(bottomLeftX, bottomLeftY);
        origin.transform.position = originPosition;
    }

    void GenerateField()
    {
        boundsOfCells = new Bounds[Width(), Height()];
        for (int i = 0; i < Width(); i++)
        {
            GenerateFieldColumn(i);
        }
    }

    void GenerateFieldColumn(int i)
    {
        for (int j = 0; j < Height(); j++)
        {
            var cellPos = new Vector2(bottomLeftX + i * cellSize, bottomLeftY + j * cellSize);
            var cell = Instantiate(cellPrefab, cellPos, Quaternion.identity);
            OnCellGenerated(i, j, cell);
        }
    }

    protected virtual void OnCellGenerated(int i, int j, GameObject cell)
    {
        cell.transform.SetParent(origin.transform);
        boundsOfCells[i, j] = cell.GetComponent<SpriteRenderer>().bounds;
        body[i, j] = (int)CellState.Empty;
    }

    protected static int Width()
    {
        return body.GetLength(0);
    }

    protected static int Height()
    {
        return body.GetLength(1);
    }

    public static void CheckLocationOverField(Vector3 mousePos, Ship ship)
    {
        var upperRightBounds = boundsOfCells[Width() - 1, Height() - 1].max;
        var isShipOverField = mousePos.x > bottomLeftCorner.x && 
            mousePos.x < upperRightBounds.x &&
            mousePos.y > bottomLeftCorner.y && mousePos.y < upperRightBounds.y;
        if (!isShipOverField)
        {
            ship.isPositionCorrect = false;
            ship.isWithinCell = false;
            return;
        }

        
        var cellMatrixPos = GetCellMatrixPos(mousePos);
        int x = (int)cellMatrixPos.x, y = (int)cellMatrixPos.y;
        ship.isWithinCell = true;
        ship.cellCenterPosition = boundsOfCells[x, y].center;
        ship.isPositionCorrect = IsLocationAppropriate(ship, x, y);

        //if (ship.isPositionCorrect) Debug.Log("correct!");
    }

    static bool IsLocationAppropriate(Ship ship, int x, int y)
    {
        for (int i = 0; i < ship.floorsNum; i++)
        {
            if (!AreSurroundingCellsEmpty(ship, x, y))
                return false;
            if (ship.orientation == Ship.Orientation.Horizontal) x++;
            else y--;
        }
        return true;
    }

    static bool AreSurroundingCellsEmpty(Ship ship, int x, int y)
    {
        if (!IsPointWithinMatrix(x, y)) return false;
        var dx = new int[] { 1, 1, 0, -1, -1, -1, 0, 1, 0 }; // to check surrounding cells
        var dy = new int[] { 0, -1, -1, -1, 0, 1, 1, 1, 0 };
        for (int j = 0; j < 9; j++)
        {
            int shiftX = x + dx[j], shiftY = y + dy[j];
            var isPosAppropr = !IsPointWithinMatrix(shiftX, shiftY) ||
                body[shiftX, shiftY] != CellState.Occupied;
            if (!isPosAppropr) return false;
        }
        return true;
    }

    protected static bool IsPointWithinMatrix(int x, int y)
    {
        return x >= 0 && x < Width() && y >= 0 && y < Height();
    }

    public static void MarkShipCellsAsOccupied(Ship ship)
    {
        SetCellsStateUnderneathShip(ship, CellState.Occupied);        
        //Debug.Log("SET");
        //Debug.Log(GetCellMatrixPos(ship.cellCenterPosition));
    }

    public static void TakeShipOff(Ship ship)
    {
        SetCellsStateUnderneathShip(ship, CellState.Empty);
        //Debug.Log("TAKEN off");
    }

    static void SetCellsStateUnderneathShip(Ship ship, CellState cellState)
    {
        var cellNormalPos = GetCellMatrixPos(ship.cellCenterPosition);
        int x = (int)cellNormalPos.x, y = (int)cellNormalPos.y;
        if (!IsPointWithinMatrix(x, y)) throw new System.Exception("Cell center incorrect");



        Debug.Log(x + " :: " + y);
        Debug.Log("MARKING SHIP CELL for " + ship.name + " , floors " + ship.floorsNum);


        for (int i = 0; i < ship.floorsNum; i++)
        {
            body[x, y] = cellState;
            if (ship.orientation == Ship.Orientation.Horizontal) x++;
            else y--;
        }



        //for (int i = Height() - 1; i >= 0; i--) // Displaying field matrix
        //{
        //    var line = "";
        //    for (int j = 0; j < Width(); j++)
        //    {
        //        line += body[j, i] + "  ";
        //    }
        //    Debug.Log(line);
        //}
    }

    protected static Vector2 GetCellMatrixPos(Vector2 pointInField)
    {
        var dx = pointInField.x - bottomLeftCorner.x;
        var dy = pointInField.y - bottomLeftCorner.y;
        int nx = (int)(dx / cellSize), ny = (int)(dy / cellSize);
        return new Vector2(nx, ny);
    }
}
