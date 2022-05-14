using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeBar : MonoBehaviour
{
    public Slider Slider;

    private float Limit = 1;

    public void SetTime(float time)
    {
        Slider.value = (Limit - time) / Limit;
    }

    public void SetActive(float limit)
    {
        gameObject.SetActive(true);
        Limit = limit;
        Slider.value = 0;
    }
}
