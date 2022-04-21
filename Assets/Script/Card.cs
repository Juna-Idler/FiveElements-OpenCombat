using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{

    public CardData.FiveElements Element;
    public int Power;


    public static GameClient Client;

    private static Sprite[] NumberSprites;
    private static readonly Color[] ElementColors =
    {
        new Color(0, 0.5f, 0.6f),
        new Color(0.9f, 0, 0),
        new Color(0.8f, 0.8f, 0),
        new Color(0.9f,0.9f,0.9f),
        new Color(0.1f, 0.1f, 0.3f)
    };

    public void Initialize(CardData data)
    {
        Element = data.Element;
        Power = data.Power;

        if (NumberSprites == null)
        {
            NumberSprites = Resources.LoadAll<Sprite>("Number");
        }
        Sprite image = NumberSprites[Power];

        GameObject face = transform.GetChild(0).gameObject;
        SpriteRenderer renderer = face.GetComponent<SpriteRenderer>();
        renderer.color = ElementColors[(int)Element];
        GameObject number = transform.GetChild(0).GetChild(0).gameObject;
        SpriteRenderer sprite = number.GetComponent<SpriteRenderer>();
        sprite.sprite = image;
    }

}
