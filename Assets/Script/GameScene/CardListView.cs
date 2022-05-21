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
        RectTransform rect = GridLayoutGroup.GetComponent<RectTransform>();

        float xstep = rect.rect.width / 11;
        float xstart = -rect.rect.width / 2 + xstep;
        float ystep = -rect.rect.height / 4;
        float ystart = rect.rect.height / 2 + ystep;

        ListItem.List = this;
        for (int i = 0; i < 30;i++)
        {
            GameObject go = Instantiate(ListItemPrefab);
            Items[i] = go.GetComponent<ListItem>();
            Items[i].Index = i;
            go.transform.SetParent(GridLayoutGroup.transform);
            go.transform.localPosition = new Vector2(xstart + xstep * (i % 10), ystart + ystep * (i / 10));
        }
        //ã≠à¯Ç»ç¿ïWåvéZ
//        Canvas.enabled = true;
//        Canvas.ForceUpdateCanvases();
//        Canvas.enabled = false;
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
            Items[i].gameObject.SetActive(true);
            Items[i].Card = cards[i];
            Items[i].OriginalPosition = cards[i].transform.position;
            Items[i].OriginalScale = cards[i].transform.localScale;
            Items[i].OriginalActive = cards[i].activeSelf;
            cards[i].SetActive(true);
            SetSortingGroupOrder(cards[i], 1001 + i);
            cards[i].transform.DOMove(Items[i].transform.position, 0.3f);
            cards[i].transform.DOScale(0.7f, 0.3f);
        }
        for (int i = cards.Length;i < 30;i++)
        {
            Items[i].gameObject.SetActive(false);
        }
        ItemCount = cards.Length;

        Canvas.enabled = true;
    }

    public void Close()
    {
        for (int i = 0; i < ItemCount; i++)
        {
            SetSortingGroupOrder(Items[i].Card, 0);
            Items[i].Card.transform.position = Items[i].OriginalPosition;
            Items[i].Card.SetActive(Items[i].OriginalActive);
            Items[i].Card.transform.localScale = Items[i].OriginalScale;
            Items[i].Card = null;
        }
        ItemCount = 0;

        Canvas.enabled = false;
    }




}
