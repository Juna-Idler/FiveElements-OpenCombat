using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandChecker : MonoBehaviour,IPointerClickHandler
{
    public static GameClient Client;
    public int Index;

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
        Client.SelectRivalCard(Index);
    }
}
