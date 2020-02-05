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
    public bool isWithinCell { get; set; } = false;
    public bool isPositionCorrect { get; set; } = false;
    public Vector3 cellCenterPosition { get; set; }

    Vector3 lastPos;
    // first - to set initial angle
    Orientation firstOrientation, lastOrientation;
    Animator[] animators;
    bool toMove = false, wasLocatedOnce = false;
    int floorsNum;
    float rotAngle;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        if (orientation == Orientation.Horizontal) rotAngle = 90f;
        else rotAngle = -90f;

        
        firstOrientation = lastOrientation = orientation;
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
        Debug.Log($"{name} created START");
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
                transform.position = cellCenterPosition = lastPos;
                if (orientation != lastOrientation) Rotate();
                GameField.MarkShipCellsAsOccupied(this);
                isPositionCorrect = isWithinCell = true;
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
        else if (toMove)
        {
            if (!isPositionCorrect) return;
            RememberPositionAndRotation();
            GameField.MarkShipCellsAsOccupied(this);
        }
        else if (wasLocatedOnce) GameField.TakeShipOff(this);
        OnShipClick();
        wasLocatedOnce = true;
    }

    void RememberPositionAndRotation()
    {
        lastPos = transform.position;
        lastOrientation = orientation;
    }

    void Rotate()
    {
        if (orientation == Orientation.Horizontal) orientation = Orientation.Vertical;
        else orientation = Orientation.Horizontal;
        rotAngle = -rotAngle;
        transform.Rotate(new Vector3(0, 0, rotAngle), Space.Self);
    }

    public void AutoLocate()
    {
        transform.position = cellCenterPosition;
        if (lastOrientation != orientation)
        {
            orientation = lastOrientation;
            Rotate();
            Debug.Log($"ship {name} rotated to {orientation} by {rotAngle}");
        }
        RememberPositionAndRotation();
        isPositionCorrect = wasLocatedOnce = true;
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