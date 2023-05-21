using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class stack_scr : MonoBehaviour
{
    private int smallNumber, bigNumber;

    public void setNumbers(int s, int b)
    {
        smallNumber = s; bigNumber = b;
    }

    public void updateNumbers()
    {
        GetComponentsInChildren<TextMeshProUGUI>()[0].text = smallNumber.ToString();
        GetComponentsInChildren<TextMeshProUGUI>()[1].text = bigNumber.ToString();
    }
}
