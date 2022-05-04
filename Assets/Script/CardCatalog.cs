using System.Collections;
using UnityEngine;

public class CardCatalog
{
    [System.Serializable]
    private struct JsonCatalog
    {
        [System.Serializable]
        public struct Card
        {
            public int ID;
            public int Element;
            public int Power;
        }
        public Card[] CardCatalog;
    }

    static CardCatalog()
    {
        TextAsset text = Resources.Load<TextAsset>("cardcatalog");
        string json = text.text;

        Instance = new CardCatalog();

        JsonCatalog catalog = JsonUtility.FromJson<JsonCatalog>(json);

        Instance.Catalog = new CardData[catalog.CardCatalog.Length];
        for (int i = 0; i < catalog.CardCatalog.Length; i++)
        {
            Instance.Catalog[i] = new CardData(catalog.CardCatalog[i].ID, (CardData.FiveElements)catalog.CardCatalog[i].Element, catalog.CardCatalog[i].Power);
        }
    }


    public static CardCatalog Instance { get; private set; }

    private CardData[] Catalog;

    public CardData this[int id] { get { return Instance.Catalog[id]; } private set { } }

    public static CardData Get(int id) { return Instance.Catalog[id]; }


}
