using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;


using DG.Tweening;


public class CardListView : MonoBehaviour
{
    public Canvas Canvas;
    public GameObject ListItemPrefab;
    public GameObject GridLayoutGroup;

    public ListItem[] Items = new ListItem[30];
    public int ItemCount;

    public void Initialize()
    {
        ListItem.List = this;
        for (int i = 0; i < 30;i++)
        {
            GameObject go = Instantiate(ListItemPrefab);
            Items[i] = go.GetComponent<ListItem>();
            Items[i].Index = i;
            go.transform.SetParent(GridLayoutGroup.transform);
        }
        //ã≠à¯Ç»ç¿ïWåvéZ
        Canvas.enabled = true;
        Canvas.ForceUpdateCanvases();
        Canvas.enabled = false;
    }

    static void SetSortingGroupOrder(GameObject card, int order)
    {
        SortingGroup sg = card.GetComponent<SortingGroup>();
        sg.sortingOrder = order;
    }

    public bool IsOpen => ItemCount > 0;

    public void Open(GameObject[] cards)
    {
        for (int i = 0; i < cards.Length;i++)
        {
            Items[i].Card = cards[i];
            Items[i].OriginalPosition = cards[i].transform.position;
            Items[i].OriginalActive = cards[i].activeSelf;
            cards[i].SetActive(true);
            SetSortingGroupOrder(cards[i], 51 + i);
            cards[i].transform.DOMove(Items[i].transform.position, 0.3f);
        }
        ItemCount = cards.Length;

        Canvas.enabled = true;
    }

    public void Close()
    {
        for (int i = 0; i < ItemCount; i++)
        {
            SetSortingGroupOrder(Items[i].Card, 1);
            Items[i].Card.transform.position = Items[i].OriginalPosition;
            Items[i].Card.SetActive(Items[i].OriginalActive);
            Items[i].Card = null;
        }
        ItemCount = 0;

        Canvas.enabled = false;
    }




}
