using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipsDispatcher : Ship
{
    static Dictionary<string, int> shipsLeftToAllocate = new Dictionary<string, int>();
    static Dictionary<string, Text> shipsLabels = new Dictionary<string, Text>();
    static List<ShipsDispatcher> allShips = new List<ShipsDispatcher>();

    public GameObject shipPrefab;
    public static Ship currentShip;
    string dictKey;

        
    // Start is called before the first frame update
    void Start()
    {
        FillLabelsDict();        
        if (gameObject.name.Contains("(Clone)")) allShips.Add(this);
        else GameField.OnAutoLocate += OnAutoLocateClick;
        dictKey = gameObject.name.Replace("(Clone)", null);

        var shipsToAllocate = 5 - int.Parse(dictKey.Replace("Ship-", null));
        if (!shipsLeftToAllocate.ContainsKey(gameObject.name))
        {
            shipsLeftToAllocate.Add(gameObject.name, shipsToAllocate);
            RefreshLabel();
        }
    }

    void OnDestroy()
    {
        allShips.Remove(this);
    }

    void OnAutoLocateClick()
    {
        while (shipsLeftToAllocate[dictKey] > 0)
        {
            Instantiate(shipPrefab, transform.parent.transform);
            shipsLeftToAllocate[dictKey]--;
        }
    }

    void FillLabelsDict()
    {
        if (shipsLabels.Count > 0) return;
        var textBlocks = transform.parent.GetComponentsInChildren<Text>();
        foreach (var textBlock in textBlocks)
        {
            if (!textBlock.name.Contains("label")) continue;
            shipsLabels.Add(textBlock.name.Replace(" label", null), textBlock);
        }
    }

    public void OnShipClick()
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

    public static ShipsDispatcher[] GetAllShips()
    {
        var result = new ShipsDispatcher[allShips.Count];
        allShips.CopyTo(result);
        return result;
    }
}
