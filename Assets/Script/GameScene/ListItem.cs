using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ListItem : MonoBehaviour , IPointerClickHandler
{
    public static CardListView List;
    public int Index;

    public GameObject Card;
    public Vector2 OriginalPosition;
    public bool OriginalActive;


    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
    }
}
