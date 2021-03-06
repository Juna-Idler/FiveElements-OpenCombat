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
    private GameObject DragCard;
    private int DragPhase;

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        BeginPos = eventData.position;
        DragCard = Client.InEffect ? null: Card;
        DragPhase = Client.Phase;
    }
    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        if (DragCard == null)
            return;
        if (Client.InEffect || Client.Phase < 0)
        {
            DragCard.transform.position = gameObject.transform.position;
            DragCard = null;
            return;
        }

        Vector3 pos = eventData.position;
        Vector3 target = pos - BeginPos;
        target.x = 0;
        target.y = System.Math.Min(System.Math.Max(target.y, 0), 50);

        DragCard.transform.position = gameObject.transform.position + target;
    }
    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        if (DragCard == null)
            return;
        Vector3 pos = eventData.position;
        Vector3 target = pos - BeginPos;
        if (target.y >= 50 && DragPhase == Client.Phase)
        {
            Client.DecideCard(Index);
        }
        else
        {
            DragCard.transform.position = gameObject.transform.position;
        }
    }
}
