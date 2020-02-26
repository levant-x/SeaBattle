using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel : MonoBehaviour
{
    public void OnCloseButtonClick()
    {
        gameObject.SetActive(false);
    }
}
