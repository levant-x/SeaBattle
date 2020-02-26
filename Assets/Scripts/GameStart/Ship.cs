using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ship : Dispatcher
{
    public enum Orientation
    {
        Horizontal, Vertical
    }

    public Orientation orientation = Orientation.Horizontal;
    public GameObject floorButtonPref;

    public Vector3 cellCenterPosition { get; set; }
    public bool isWithinCell { get; set; } = false;
    public bool isPositionCorrect { get; set; } = false;

    Vector3 lastPos, floorPos;
    // first - to set initial angle
    Orientation firstOrientation, lastOrientation;
    Animator[] animators;
    Transform floor;
    bool toMove = false;
    float rotAngle, floorSize;

    public bool wasAllocatedOnce { get; protected set; } = false;
    public int floorsNum { get; protected set; }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        if (orientation == Orientation.Horizontal) rotAngle = 90f;
        else rotAngle = -90f;
        
        firstOrientation = lastOrientation = orientation;
        floorsNum = transform.childCount;
        animators = new Animator[floorsNum];

        for (int i = 0; i < floorsNum; i++)
        {
            floor = transform.GetChild(i);
            floorPos = transform.position;
            var floorSprRenderer = floor.GetComponent<SpriteRenderer>();
            Settings.ScaleSpriteByY(floorSprRenderer, GameField.cellToCamHeightProportion,
                out floorSize);
            ShiftFloor(i);
            CreateFloorButton();
        }
    }

    void ShiftFloor(int floorIndex)
    {
        if (orientation == Orientation.Horizontal) floorPos.x += floorIndex * floorSize;
        else if (orientation == Orientation.Vertical) floorPos.y -= floorIndex * floorSize;
        floor.transform.position = floorPos; // placing floor where need to
        animators[floorIndex] = floor.GetComponent<Animator>(); // saving animator
    }

    void CreateFloorButton()
    {
        if (floor.GetComponentInChildren<Button>() is Button buttonScript)
        {
            HookButtonClick(buttonScript);
            return;
        }

        var floorButtonObj = Instantiate(floorButtonPref, floor.transform);
        floorButtonObj.transform.position = floorPos; // allocating button
        var buttonRectTransf = floorButtonObj.GetComponent<RectTransform>();
        buttonRectTransf.sizeDelta = new Vector2(floorSize, floorSize); // sizing button
        HookButtonClick(floorButtonObj.GetComponentInChildren<Button>());
    }

    void HookButtonClick(Button buttonScript)
    {
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
            if (wasAllocatedOnce) ResetTransform();
            else Destroy(gameObject);
            currentShip = null;
        }
        else if (Input.GetKeyUp(KeyCode.Space)) Rotate();
        SwitchPlacementAnimation();
    }

    void ResetTransform()
    {
        transform.position = cellCenterPosition = lastPos;
        if (orientation != lastOrientation) Rotate();
        GameField.MarkShipCellsAsOccupied(this);
        isPositionCorrect = isWithinCell = true;
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
        else if (wasAllocatedOnce) GameField.TakeShipOff(this);
        OnShipClick();
        wasAllocatedOnce = true;
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
        isPositionCorrect = wasAllocatedOnce = true;
    }

    void SwitchPlacementAnimation()
    {
        foreach (var animator in animators)
        {
            animator.SetBool("IsMisplaced", !isPositionCorrect);
        }
    }
}