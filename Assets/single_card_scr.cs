using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class single_card_scr : MonoBehaviour
{

    private int number;

    public void setNumber(int number)
    {
        this.number = number;
    }

    public void updateNumber()
    {
        GetComponentInChildren<TextMeshProUGUI>().text = number.ToString();
    }

}
