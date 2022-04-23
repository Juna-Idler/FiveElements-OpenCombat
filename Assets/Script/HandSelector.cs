using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandSelector : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler,
    IPointerClickHandler
{
    public static GameClient Client;
    public int Index;

    public GameObject Card;

    public GameObject Plus;
    public GameObject Minus;
    public GameObject Maru;
    public GameObject Batu;

    public void ResetAllOption()
    {
        Plus.SetActive(false);
        Minus.SetActive(false);
        Maru.SetActive(false);
        Batu.SetActive(false);
    }
    public void SetPlusMinus(int n)
    {
        if (n > 0)
        {
            Plus.SetActive(true);
            Minus.SetActive(false);
        }
        else if (n < 0)
        {
            Plus.SetActive(false);
            Minus.SetActive(true);
        }
        else
        {
            Plus.SetActive(false);
            Minus.SetActive(false);
        }
    }
    public void SetMaruBatu(int n)
    {
        if (n > 0)
        {
            Maru.SetActive(true);
            Batu.SetActive(false);
        }
        else if (n < 0)
        {
            Maru.SetActive(false);
            Batu.SetActive(true);
        }
        else
        {
            Maru.SetActive(false);
            Batu.SetActive(false);
        }
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        Client.SelectMyCard(Index);
    }


    private Vector3 BeginPos;

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        BeginPos = eventData.position;
    }
    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        if (Client.InEffect)
            return;

        Vector3 pos = eventData.position;
        Vector3 target = pos - BeginPos;
        target.x = 0;
        target.y = System.Math.Max(target.y, 0);
        target.y = System.Math.Min(target.y, 50);

        Card.transform.GetChild(0).localPosition = target;
    }
    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        if (Client.InEffect)
            return;

        Vector3 pos = eventData.position;
        Vector3 target = pos - BeginPos;
        Card.transform.GetChild(0).localPosition = Vector3.zero;
        if (target.y >= 50)
        {
            Vector3 tmp = Card.transform.localPosition;
            tmp.y += 50;
            Card.transform.localPosition = tmp;
            Client.DecideCard(Index);
        }
    }
}
