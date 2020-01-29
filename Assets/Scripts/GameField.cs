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
    public float bottomLeftX, bottomLeftY;
    public bool toAdjustOrigin = false;

    static int[,] body = new int[10, 8];
    static Bounds[,] boundsOfCells;
    static Vector2 bottomLeftCorner;
    static float cellSize;

    List<GameObject> cells = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        var sprRenderer = cellPrefab.GetComponent<SpriteRenderer>();
        cellSize = sprRenderer.bounds.size.x;
        GenerateField();
        bottomLeftCorner = boundsOfCells[0, 0].min;
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
                body[i, j] = (int)CellState.Empty;
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
        var upperRightBounds = boundsOfCells[Width() - 1, Height() - 1].max;
        var isShipOverField = mousePos.x > bottomLeftCorner.x && mousePos.x < upperRightBounds.x &&
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
    }

    static bool IsLocationAppropriate(Ship ship, int x, int y)
    {
        var dx = new int[] { 1, 1, 0, -1, -1, -1, 0, 1 }; // to check surrounding cells
        var dy = new int[] { 0, -1, -1, -1, 0, 1, 1, 1 };
        for (int i = 0; i < ship.FloorsNum(); i++)
        {
            for (int j = 0; j < 8; j++)
            {
                int shiftX = x + dx[j], shiftY = y + dy[j];
                var isPosAppropr = !IsPointWithinMatrix(shiftX, shiftY) ||
                    body[shiftX, shiftY] != (int)CellState.Occupied;
                if (!isPosAppropr) return false;
            }
            if (!IsPointWithinMatrix(x, y)) return false;
            if (ship.orientation == Ship.Orientation.Horizontal) x++;
            else y--;
        }     
        return true;
    }

    static bool IsPointWithinMatrix(int x, int y)
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

        for (int i = 0; i < ship.FloorsNum(); i++)
        {
            body[x, y] = (int)cellState;
            if (ship.orientation == Ship.Orientation.Horizontal) x += 1;
            else y -= 1;
        }



        //for (int i = 0; i < Width(); i++)
        //{
        //    var line = "";
        //    for (int j = 0; j < Height(); j++)
        //    {
        //        line += body[i, j] + "  ";
        //    }
        //    Debug.Log(line);
        //}
    }

    static Vector2 GetCellMatrixPos(Vector2 pointInField)
    {
        var dx = pointInField.x - bottomLeftCorner.x;
        var dy = pointInField.y - bottomLeftCorner.y;
        int nx = (int)(dx / cellSize), ny = (int)(dy / cellSize);
        return new Vector2(nx, ny);
    }

    public void OnAutoLocateClick()
    {
        var templateShips = ShipsDispatcher.GetAllShips(true);
        foreach (var tplShip in templateShips)
        {
            tplShip.GenerateAllAmount();
        }
        var playShips = ShipsDispatcher.GetAllShips(false);
        Debug.Log(playShips.Length);
        ClearGameField();


        Debug.Log("Field clear");

        StartCoroutine(WaitForAllShips(playShips));

        //foreach (var ship in playShips)
        //{
        //    AutoLocateShip(ship);
        //}
    }

    IEnumerator WaitForAllShips(ShipsDispatcher[] ships)
    {
        foreach (var ship in ships)
        {
            AutoLocateShip(ship);


            while (!Input.GetKeyDown(KeyCode.Space))
            {
                yield return new WaitForSeconds(waitTime);
            }
        }
    }

    public float waitTime = 1;

    void ClearGameField()
    {
        for (int i = 0; i < Width(); i++)
        {
            for (int j = 0; j < Height(); j++)
            {
                body[i, j] = (int)CellState.Empty;
            }
        }
    }

    void AutoLocateShip(Ship ship)
    {
        int fieldSqr = body.Length, initPointIndex = 0;
        var step = Random.Range(fieldSqr / 4, fieldSqr / 3);
        int point = Random.Range(0, fieldSqr), x, y;

        Debug.Log(ship.name +  " at point " + point);

        int c = 0;
        while (true)
        {
            x = point % Width();
            y = point / Width();

            Debug.Log(x + " :: " + y);
            //while (!Input.GetKeyUp(KeyCode.Space))
            //{
            //}

            if (IsLocationAppropriate(ship, x, y)) break;
            Debug.Log("Rotating ");

            ship.Rotate();
            if (IsLocationAppropriate(ship, x, y)) break;
            Debug.Log("Moving across");

            point += step;
            initPointIndex += step;
            if (point >= fieldSqr) point -= fieldSqr;
            if (initPointIndex >= fieldSqr)
            {
                initPointIndex -= fieldSqr;
                step--;
            }
            Debug.Log("point " + point);
            Debug.Log("init point " + initPointIndex);



             c++;
            if (c > 1000)
            {
                Debug.Log("OVERLOOPED");
                break;
            }
        }
        Debug.Log("LOCATED");
        ship.gameObject.transform.position = boundsOfCells[x, y].center;
        MarkShipCellsAsOccupied(ship);
    }
}
