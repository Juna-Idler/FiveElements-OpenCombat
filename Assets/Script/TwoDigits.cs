using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoDigits : MonoBehaviour
{

    public SpriteRenderer First;
    public SpriteRenderer Second;

    public void Set(int number)
    {
        if (number < 0)
            return;
        if (number > 9)
        {
            First.transform.localPosition = new Vector2(45, 0);
            int first = number % 10;
            int second = number / 10 % 10;

            First.sprite = Card.NumberSprite(first);
            Second.sprite = Card.NumberSprite(second);
            Second.enabled = true;
        }
        else
        {
            First.transform.localPosition = new Vector2(0, 0);
            First.sprite = Card.NumberSprite(number);
            Second.enabled = false;
        }
    }
}
