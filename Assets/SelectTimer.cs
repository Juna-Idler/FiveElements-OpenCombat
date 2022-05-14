using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectTimer : MonoBehaviour
{
    public Image Circle;
    public Text Text;


    private float Limit = 1;

    public void SetTime(float time)
    {
        Circle.fillAmount = (Limit - time) / Limit;
        Text.text = time.ToString("F1");
    }

    public void SetActive(float limit)
    {
        gameObject.SetActive(true);
        Limit = limit;
        SetTime(limit);
    }

}
