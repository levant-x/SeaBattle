using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ship : ShipsDispatcher
{
    public enum Orientation
    {
        Horizontal, Vertical
    }

    public enum RotationDirection
    {
        Deg270, Deg90
    }

    public Orientation orientation = Orientation.Horizontal;
    public RotationDirection rotateBy = RotationDirection.Deg270;
    public GameObject floorButtonPref;
    public bool isWithinCell = false, isPositionCorrect = false;
    public Vector3 cellCenterPosition;

    Vector3 lastPos;
    Orientation lastOrientation;
    Camera cam;
    Animator[] animators;
    bool toMove = false, isWorkingCopy = false, wasLocatedOnce = false;
    int floorsNum;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        Debug.Log("START SHIP " + name);

        //dispatcher = GetComponentInChildren<ShipsDispatcher>();
        isWorkingCopy = gameObject.name.Contains("(Clone)");
        cam = Camera.main;
        floorsNum = transform.childCount;
        animators = new Animator[floorsNum];
        float floorSize = 0;
        for (int i = 0; i < floorsNum; i++)
        {
            var floor = transform.GetChild(i);
            var floorPos = transform.position;
            floorSize = floor.GetComponent<SpriteRenderer>().bounds.size.x;
            if (orientation == Orientation.Horizontal) floorPos.x += i * floorSize;
            else if (orientation == Orientation.Vertical) floorPos.y += i * floorSize;
            floor.transform.position = floorPos; // placing floor where need to
            animators[i] = floor.GetComponent<Animator>(); // saving animator

            var floorButtonObj = Instantiate(floorButtonPref, floor.transform);
            floorButtonObj.transform.position = floorPos; // allocating button
            var buttonRectTransf = floorButtonObj.GetComponent<RectTransform>();
            buttonRectTransf.sizeDelta = new Vector2(floorSize, floorSize); // sizing button
            var buttonScript = floorButtonObj.GetComponent<Button>();
            buttonScript.onClick.AddListener(OnFloorClick); // making button clickable
        }

        Debug.Log("START " + floorsNum);
    }

    // Update is called once per frame
    void Update()
    {
        toMove = Equals(currentShip);
        if (!toMove) return;

        var mousePos = Input.mousePosition;
        mousePos = cam.ScreenToWorldPoint(mousePos);
        GameField.CheckLocationOverField(mousePos, this);        
        mousePos.z = transform.position.z;
        transform.position = mousePos;
        if (isWithinCell) transform.position = cellCenterPosition;

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (wasLocatedOnce)
            {
                transform.position = lastPos;
                if (orientation != lastOrientation) Rotate();
                isPositionCorrect = true;
            }
            else Destroy(gameObject);
            currentShip = null;
        }
        else if (Input.GetKeyUp(KeyCode.Space)) Rotate();
        SwitchPlacementAnimation();
    }

    void OnFloorClick()
    {
        if (!Input.GetMouseButtonUp(0)) return;
        else if (toMove && isPositionCorrect)
        {
            SetToPlace();
            GameField.MarkShipCellsAsOccupied(this);
        }
        else if (wasLocatedOnce && isWorkingCopy) GameField.TakeShipOff(this);
        /*dispatcher.*/OnShipClick();
        if (!wasLocatedOnce) wasLocatedOnce = true;
    }

    void SetToPlace()
    {
        lastPos = transform.position;
        lastOrientation = orientation;
    }

    public void Rotate()
    {
        var angleStr = rotateBy.ToString().Replace("Deg", null);
        var angleFloat = float.Parse(angleStr);
        if (orientation == Orientation.Horizontal) orientation = Orientation.Vertical;
        else
        {
            orientation = Orientation.Horizontal;
            angleFloat = -angleFloat;
        }
        transform.Rotate(new Vector3(0, 0, angleFloat), Space.Self);
    }

    public void AutoLocate()
    {
        SetToPlace();
        transform.position = cellCenterPosition;
        wasLocatedOnce = true;
    }

    public int FloorsNum()
    {
        Debug.Log("FLOORS NUM ASKED, is " + floorsNum);
        return floorsNum;
    }

    public bool WasLocatedOnce()
    {
        return wasLocatedOnce;
    }

    void SwitchPlacementAnimation()
    {
        foreach (var animator in animators)
        {
            animator.SetBool("IsMisplaced", !isPositionCorrect);
        }
    }
}
