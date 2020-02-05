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
    protected string dictKey;
    bool isWorkingInstance = true;
        
    // Start is called before the first frame update
    protected virtual void Start()
    {
        isWorkingInstance = gameObject.name.Contains("(Clone)");
        dictKey = gameObject.name.Replace("(Clone)", null);
        if (!isWorkingInstance)
        {
            FillLabelsDict();
            allShips.Add(this);
        }

        var shipsOfKindToAllocate = 5 - int.Parse(dictKey.Replace("Ship-", null));
        if (!shipsLeftToAllocate.ContainsKey(dictKey))
        {
            shipsLeftToAllocate.Add(dictKey, shipsOfKindToAllocate);
        }
        RefreshLabel();
    }

    void OnDestroy()
    {
        allShips.Remove(this);
    }

    static ShipsDispatcher[] GetAllShips(bool templateOnes)
    {
        var result = new List<ShipsDispatcher>();
        foreach (var disp in allShips)
            if (disp.isWorkingInstance ^ templateOnes) result.Add(disp);
        return result.ToArray();
    }

    public static ShipsDispatcher[] CreateAllShips()
    {
        var templateShips = GetAllShips(true);
        foreach (var tmplShip in templateShips)
        {
            CreateShip(tmplShip);
        }
        return GetAllShips(false); 
    }

    static void CreateShip(ShipsDispatcher dispatcher)
    {
        for (int i = 0; i < shipsLeftToAllocate[dispatcher.dictKey]; i++)
        {
            var ship = Instantiate(dispatcher.shipPrefab,
                dispatcher.transform.parent.transform);
            allShips.Add(ship.GetComponent<Ship>());
        }
        shipsLeftToAllocate[dispatcher.dictKey] = 0;
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
            if (currentShip == null)
            {
                currentShip = GetComponentInChildren<Ship>();
            }
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
    }

    void RefreshLabel()
    {
        shipsLabels[dictKey].text = shipsLeftToAllocate[dictKey] + "x";
    }
}
