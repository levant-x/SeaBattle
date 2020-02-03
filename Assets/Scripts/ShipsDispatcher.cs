using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipsDispatcher : MonoBehaviour
{
    static Dictionary<string, int> shipsLeftToAllocate = new Dictionary<string, int>();
    static Dictionary<string, Text> shipsLabels = new Dictionary<string, Text>();
    static List<ShipsDispatcher> allShips = new List<ShipsDispatcher>();

    public GameObject shipPrefab;
    public static Ship currentShip;
    string dictKey;

        
    // Start is called before the first frame update
    protected virtual void Start()
    {
        allShips.Add(this);
        dictKey = gameObject.name.Replace("(Clone)", null);
        if (!gameObject.name.Contains("Clone")) FillLabelsDict();

        var shipsOfKindToAllocate = 5 - int.Parse(dictKey.Replace("Ship-", null));
        if (!shipsLeftToAllocate.ContainsKey(dictKey))
        {
            shipsLeftToAllocate.Add(gameObject.name, shipsOfKindToAllocate);
        }
        RefreshLabel();
    }

    void OnDestroy()
    {
        allShips.Remove(this);
    }

    public static ShipsDispatcher[] GetAllShips(bool templateOnes)
    {
        var res = new List<ShipsDispatcher>();
        foreach (var disp in allShips)
        {         
            if (disp.gameObject.name.Contains("Clone") ^ templateOnes)
            {
                res.Add(disp);
            }
        }
        return res.ToArray();
    }

    public void GenerateAllAmount()
    {
        for (int i = 0; i < shipsLeftToAllocate[dictKey]; i++)
        {
            var ship = Instantiate(shipPrefab, transform.parent.transform);
            ship.SendMessage("Start");
            //allShips.Add(ship.GetComponentInChildren<ShipsDispatcher>());
        }
        shipsLeftToAllocate[dictKey] = 0;
    }

    void FillLabelsDict()
    {
        var textBlock = GameObject.Find(dictKey + " label").GetComponent<Text>();        
        shipsLabels.Add(textBlock.name.Replace(" label", null), textBlock);
    }

    protected void OnShipClick()
    {        
        if (gameObject.name.Contains("Clone")) // ship on the field
        {
            if (currentShip == null) currentShip = GetComponentInChildren<Ship>();
            else if (currentShip.isPositionCorrect)
            {
                if (!currentShip.WasLocatedOnce()) shipsLeftToAllocate[dictKey]--;   
                RefreshLabel();   
                currentShip = null;          
            }
        }
        else if (currentShip == null) // sample template
        {
            if (shipsLeftToAllocate[dictKey] == 0) return;
            var shipObjToPlay = Instantiate(shipPrefab, transform.parent.transform);
            currentShip = shipObjToPlay.GetComponentInChildren<Ship>();
        }
                
        Debug.Log("OnShipClick");
    }

    void RefreshLabel()
    {
        shipsLabels[dictKey].text = shipsLeftToAllocate[dictKey] + "x";
    }
}
