using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ship : MonoBehaviour
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

    ShipsDispatcher dispatcher;
    Vector3 finalPos;
    Camera cam;
    Animator[] animators;
    bool toMove = false, isWorkingCopy = false, isCollided = false, wasLocatedOnce = false;
    int floorsNum;

    // Start is called before the first frame update
    void Start()
    {        
        dispatcher = GetComponentInChildren<ShipsDispatcher>();
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

        //var boxCollider = gameObject.AddComponent<BoxCollider2D>();
        //boxCollider.isTrigger = true;
        //boxCollider.size = new Vector2((floorsNum + 0.5f) * floorSize, floorSize * 1.5f);
        //boxCollider.offset = new Vector2(floorsNum * floorSize / 2 - floorSize / 2, 0);
    }

    void OnTriggerEnter2D(Collider2D collider2D)
    {
        isCollided = true;
    }

    void OnTriggerExit2D(Collider2D collider2D)
    {
        isCollided = false;
    }

    // Update is called once per frame
    void Update()
    {
        toMove = Equals(ShipsDispatcher.currentShip);
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
                transform.position = finalPos;
                isPositionCorrect = true;
            }
            else Destroy(gameObject);
            ShipsDispatcher.currentShip = null;
        }
        else if (Input.GetKeyUp(KeyCode.Space)) Rotate();
        SwitchPlacementAnimation();
    }

    void OnFloorClick()
    {
        if (!Input.GetMouseButtonUp(0)) return;
        else if (toMove && isPositionCorrect)
        {
            finalPos = transform.position;
            GameField.MarkShipCellsAsOccupied(this);
        }
        else if (wasLocatedOnce && isWorkingCopy) GameField.TakeShipOff(this);
        dispatcher.OnShipClick();
        if (!wasLocatedOnce) wasLocatedOnce = true;
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

    public int FloorsNum()
    {
        return floorsNum;
    }

    public bool IsCollided()
    {
        return isCollided;
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
