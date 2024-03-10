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
    public Vector2 bottomLeftCorner;
    public bool toAdjustOrigin = false;
    public static float cellToCamHeightProportion = 1 / 11f;
    //public static GameField singleton;

    protected CellState[,] body = new CellState[10, 8];
    protected Bounds[,] boundsOfCells;
    protected float cellSize;
    protected string originObjName = "GameFieldOrigin";

    GameObject origin;
    static CellState cellStateToSet;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        Ship.gameField = this;
        origin = GameObject.Find(originObjName);
        origin.transform.position = bottomLeftCorner;

        var sprRenderer = cellPrefab.GetComponent<SpriteRenderer>();
        Settings.ScaleSpriteByY(sprRenderer, cellToCamHeightProportion,
            out cellSize);
        GenerateField();
        bottomLeftCorner = boundsOfCells[0, 0].min;
    }
    
    void FixedUpdate()
    {
        if (!toAdjustOrigin) return;
        var originPosition = bottomLeftCorner;
        origin.transform.position = originPosition;
    }

    void GenerateField()
    {
        boundsOfCells = new Bounds[Width(), Height()];
        for (int x = 0; x < Width(); x++) GenerateFieldColumn(x);
    }

    void GenerateFieldColumn(int x)
    {
        for (int y = 0; y < Height(); y++) GenerateCell(x, y);
    }

    void GenerateCell(int x, int y)
    {
        var cellPos = new Vector2(bottomLeftCorner.x + x * cellSize,
            bottomLeftCorner.y + y * cellSize);
        var cell = Instantiate(cellPrefab, cellPos, Quaternion.identity);
        OnCellGenerated(x, y, cell);
    }

    protected virtual void OnCellGenerated(int i, int j, GameObject cell)
    {
        cell.transform.SetParent(origin.transform);
        boundsOfCells[i, j] = cell.GetComponent<SpriteRenderer>().bounds;
        body[i, j] = (int)CellState.Empty;
    }

    public int Width()
    {
        return body.GetLength(0);
    }

    public int Height()
    {
        return body.GetLength(1);
    }

    public void CheckLocationOverField(Vector3 mousePos, Ship ship)
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
        ship.isPositionCorrect = IsShipPositionAppropriate(ship, x, y);
    }

    bool IsShipPositionAppropriate(Ship ship, int x, int y)
    {
        for (int i = 0; i < ship.floorsNum; i++)
            if (!IsCellLocationAppropriate(ship, ref x, ref y))
                return false;
        return true;
    }

    bool IsCellLocationAppropriate(Ship ship, ref int x, ref int y)
    {
        if (!AreSurroundingCellsEmpty(ship, x, y))
            return false;
        ShiftCoordinate(ship, ref x, ref y);
        return true;
    }

    void ShiftCoordinate(Ship ship, ref int x, ref int y)
    {
        if (ship.orientation == Ship.Orientation.Horizontal) x++;
        else y--;
    }

    bool AreSurroundingCellsEmpty(Ship ship, int x, int y)
    {
        if (!IsPointWithinMatrix(x, y)) return false;

        // to check surrounding cells
        var dx = new int[] { 1, 1, 0, -1, -1, -1, 0, 1, 0 }; 
        var dy = new int[] { 0, -1, -1, -1, 0, 1, 1, 1, 0 };
        for (int j = 0; j < 9; j++)
            if (!IsSurroundingCellEmpty(x + dx[j], y + dy[j]))
                return false;
        return true;
    }

    bool IsSurroundingCellEmpty(int shiftX, int shiftY)
    {
        var isPosAppropr = !IsPointWithinMatrix(shiftX, shiftY) ||
            body[shiftX, shiftY] != CellState.Occupied;
        if (!isPosAppropr) return false;
        else return true;
    }

    public bool IsPointWithinMatrix(int x, int y)
    {
        return Settings.IsPointWithinMatrix(x, y, body);
    }

    public void MarkShipCellsAsOccupied(Ship ship)
    {
        SetCellsStateUnderneathShip(ship, CellState.Occupied);
    }

    public void TakeShipOff(Ship ship)
    {
        SetCellsStateUnderneathShip(ship, CellState.Empty);
    }

    void SetCellsStateUnderneathShip(Ship ship, CellState cellState)
    {
        cellStateToSet = cellState;
        var cellNormalPos = GetCellMatrixPos(ship.cellCenterPosition);
        int x = (int)cellNormalPos.x, y = (int)cellNormalPos.y;
        for (int i = 0; i < ship.floorsNum; i++) SetCellState(ship, ref x, ref y);
    }

    void SetCellState(Ship ship, ref int x, ref int y)
    {
        body[x, y] = cellStateToSet;
        ShiftCoordinate(ship, ref x, ref y);
    }

    protected Vector2 GetCellMatrixPos(Vector2 pointInField)
    {
        var dx = pointInField.x - bottomLeftCorner.x;
        var dy = pointInField.y - bottomLeftCorner.y;
        int nx = (int)(dx / cellSize), ny = (int)(dy / cellSize);
        return new Vector2(nx, ny);
    }
}