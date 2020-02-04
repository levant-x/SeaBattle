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
    Orientation firstOrientation, lastOrientation;
    Animator[] animators;
    bool toMove = false, isWorkingCopy = false, wasLocatedOnce = false;
    int floorsNum;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        isWorkingCopy = gameObject.name.Contains("(Clone)");
        firstOrientation = orientation;
        floorsNum = transform.childCount;
        animators = new Animator[floorsNum];
        float floorSize = 0;

        for (int i = 0; i < floorsNum; i++)
        {
            var floor = transform.GetChild(i);
            var floorPos = transform.position;
            floorSize = floor.GetComponent<SpriteRenderer>().bounds.size.x;
            if (orientation == Orientation.Horizontal) floorPos.x += i * floorSize;
            else if (orientation == Orientation.Vertical) floorPos.y -= i * floorSize;
            floor.transform.position = floorPos; // placing floor where need to
            animators[i] = floor.GetComponent<Animator>(); // saving animator

            if (floor.GetComponentInChildren<Button>() != null)
            {
                HookButtonClick(floor);
                continue;
            }
            // replicates oneself with buttons have been created first time
            var floorButtonObj = Instantiate(floorButtonPref, floor.transform);
            floorButtonObj.transform.position = floorPos; // allocating button
            var buttonRectTransf = floorButtonObj.GetComponent<RectTransform>();
            buttonRectTransf.sizeDelta = new Vector2(floorSize, floorSize); // sizing button
            HookButtonClick(floor);
        }
    }

    void HookButtonClick(Transform floor)
    {
        var buttonScript = floor.GetComponentInChildren<Button>();
        buttonScript.onClick.AddListener(OnFloorClick); // making button clickable
    }

    // Update is called once per frame
    void Update()
    {
        toMove = Equals(currentShip);
        if (!toMove) return;

        var mousePos = Input.mousePosition;
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
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
                GameField.MarkShipCellsAsOccupied(this);
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
            RememberState();
            GameField.MarkShipCellsAsOccupied(this);
        }
        else if (wasLocatedOnce && isWorkingCopy) GameField.TakeShipOff(this);
        OnShipClick();
        if (!wasLocatedOnce) wasLocatedOnce = true;
    }

    void RememberState()
    {
        lastPos = transform.position;
        lastOrientation = orientation;
    }

    void Rotate()
    {
        //var angleStr = rotateBy.ToString().Replace("Deg", null);
        var angleFloat = -90f; //float.Parse(angleStr);
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
        transform.position = cellCenterPosition;
        RememberState();
        if (firstOrientation != orientation)
        {
            orientation = firstOrientation;
            Rotate();
        }
        wasLocatedOnce = true;
    }

    public int FloorsNum()
    {
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
