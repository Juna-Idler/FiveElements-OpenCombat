using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class GameClient : MonoBehaviour
{
    private IGameServer Server;

    // Start is called before the first frame update
    void Start()
    {
        Server = GameSceneParam.GameServer;
        if (Server == null)
        {
            Server = new OfflineGameServer();
        }

        UpdateData data = Server.GetInitialData();

        if (CardPrefab == null)
            CardPrefab = Resources.Load<GameObject>("Card");
        Card.Client = this;
        HandSelector.Client = this;
        HandChecker.Client = this;

        CardArray = new GameObject[40];
        for (int i = 0; i < CardArray.Length; i++)
        {
            CardArray[i] = Instantiate(CardPrefab);
            CardArray[i].transform.parent = Cards.transform;
            CardArray[i].SetActive(false);
        }
        CardArrayIndex = 0;


        InitializeField(data);
        FrontCanvas.SetActive(false);
    }


    public class PlayerObjects
    {
        public Vector3 BattlePosition;
        public Vector3 UsedPosition;
        public Vector3 DamagePosition;
        public Vector3 DeckPosition;

        public GameObject Battle;
        
        public List<GameObject> Hand;   
        public List<GameObject> Used;   
        public List<GameObject> Damage; 
        public Text DeckCount;          
    }
    private int Phase;

    private PlayerObjects Myself;
    private PlayerObjects Rival;

    //Inspectorで設定するのが楽なのでpublic
    public HandSelector[] MyHandSelectors;
    public HandChecker[] RivalHandCheckers;

    public GameObject Cards;
    public GameObject FrontCanvas;
    public Text Message;

    public AudioClip AudioAttack;
    public AudioClip AudioAttackOffset;
    public AudioClip AudioDamage;

    public AudioSource AudioSource;


    private bool InEffect = false;

    private static GameObject CardPrefab;

    private GameObject[] CardArray;
    private int CardArrayIndex;

    private GameObject CreateCard(CardData card)
    {
        GameObject c = CardArray[CardArrayIndex++];
        c.GetComponent<Card>().Initialize(card);
        c.SetActive(true);
        return c;
    }


    static void SetSortingGroupOrder(GameObject card, int order)
    {
        SortingGroup sg = card.GetComponent<SortingGroup>();
        sg.sortingOrder = order;
    }

    public void InitializeField(UpdateData data)
    {
        foreach (GameObject c in CardArray)
        {
            c.SetActive(false);
        }
        CardArrayIndex = 0;

        Phase = data.phase;

        for (int i = 0; i < 5;i++)
        {
            MyHandSelectors[i].gameObject.SetActive(i < data.myself.draw.Length);
            MyHandSelectors[i].ResetAllOption();
        }
        for (int i = 0; i < 5; i++)
        {
            RivalHandCheckers[i].gameObject.SetActive(i < data.rival.draw.Length);
            RivalHandCheckers[i].ResetAllOption();
        }
        Canvas.ForceUpdateCanvases();

        Myself = new PlayerObjects();
        Rival = new PlayerObjects();

        Myself.BattlePosition = GameObject.Find("MyBattle").transform.position;
        Rival.BattlePosition = GameObject.Find("YourBattle").transform.position;
        Myself.UsedPosition = GameObject.Find("MyUsed").transform.position;
        Rival.UsedPosition = GameObject.Find("YourUsed").transform.position;
        Myself.DamagePosition = GameObject.Find("MyDamage").transform.position;
        Rival.DamagePosition = GameObject.Find("YourDamage").transform.position;
        Myself.DeckPosition = GameObject.Find("MyDeck").transform.position;
        Rival.DeckPosition = GameObject.Find("YourDeck").transform.position;


        Myself.Hand = new List<GameObject>(data.myself.draw.Length);
        for (int i = 0; i < data.myself.draw.Length; i++)
        {
            GameObject o = CreateCard(data.myself.draw[i]);
            o.transform.position = MyHandSelectors[i].transform.position;
            Myself.Hand.Add(o);
            MyHandSelectors[i].Card = o;
        }

        Myself.Used = new List<GameObject>(20);
        Myself.Damage = new List<GameObject>(20);
        GameObject tmp = GameObject.Find("MyDeckCounter");
        Myself.DeckCount = tmp.GetComponent<Text>();
        Myself.DeckCount.text = data.myself.deckcount.ToString();


        Rival.Hand = new List<GameObject>(data.rival.draw.Length);
        for (int i = 0; i < data.rival.draw.Length; i++)
        {
            GameObject o = CreateCard(data.rival.draw[i]);
            o.transform.position = RivalHandCheckers[i].transform.position;
            Rival.Hand.Add(o);
        }

        Rival.Used = new List<GameObject>(20);
        Rival.Damage = new List<GameObject>(20);
        tmp = GameObject.Find("YourDeckCounter");
        Rival.DeckCount = tmp.GetComponent<Text>();
        Rival.DeckCount.text = data.rival.deckcount.ToString();
    }

    private class MoveObject
    {
        public GameObject Object;
        public Vector3 Delta;

        public void Step()
        {
            Object.transform.position += Delta;
        }
    }

    public IEnumerator BattleEffect(UpdateData data)
    {
        Phase = data.phase;

        Myself.Battle = Myself.Hand[data.myself.select];
        Rival.Battle = Rival.Hand[data.rival.select];
        Myself.Hand.RemoveAt(data.myself.select);
        Rival.Hand.RemoveAt(data.rival.select);


        for (int i = 0; i < 5; i++)
        {
            MyHandSelectors[i].ResetAllOption();
            RivalHandCheckers[i].ResetAllOption();
            MyHandSelectors[i].gameObject.SetActive(i < Myself.Hand.Count + data.myself.draw.Length);
            RivalHandCheckers[i].gameObject.SetActive(i < Rival.Hand.Count + data.rival.draw.Length);
        }
        Canvas.ForceUpdateCanvases();

        SetSortingGroupOrder(Myself.Battle, 1);
        SetSortingGroupOrder(Rival.Battle, 1);

        MoveObject mybattle = new MoveObject { Object = Myself.Battle, Delta = (Myself.BattlePosition - Myself.Battle.transform.position) / 30 };
        MoveObject rivalbattle = new MoveObject { Object = Rival.Battle, Delta = (Rival.BattlePosition - Rival.Battle.transform.position) / 30 };

        if (data.damage < 0)
            AudioSource.PlayOneShot(AudioAttack);
        for (int i = 0; i < 30; i++)
        {
            mybattle.Step();
            rivalbattle.Step();
            yield return null;
        }
        if ( data.damage == 0)
        {
            AudioSource.PlayOneShot(AudioAttackOffset);
        }
        else if (data.damage > 0)
        {
            AudioSource.PlayOneShot(AudioDamage);
        }


        List<MoveObject> moves = new List<MoveObject>(12);

        for (int i = 0;i < data.myself.draw.Length ; i++)
        {
            GameObject o = CreateCard(data.myself.draw[i]);
            o.transform.position = Myself.DeckPosition;
            Myself.Hand.Add(o);
        }
        for (int i = 0; i < data.rival.draw.Length; i++)
        {
            GameObject o = CreateCard(data.rival.draw[i]);
            o.transform.position = Rival.DeckPosition;
            Rival.Hand.Add(o);
        }
        if (Phase < 0)
        {
            int mylife = data.myself.deckcount + Myself.Hand.Count - System.Convert.ToInt32(data.damage > 0);
            int rivallife = data.rival.deckcount + Rival.Hand.Count - System.Convert.ToInt32(data.damage < 0);
            if (mylife > rivallife)
                Message.text = "Win";
            else if (mylife < rivallife)
                Message.text = "Lose";
            else
                Message.text = "Draw";
            FrontCanvas.SetActive(true);
            InEffect = false;
            yield break;
        }

        if (Myself.Used.Count > 0)
            SetSortingGroupOrder(Myself.Used[Myself.Used.Count - 1], 0);
        if (Rival.Used.Count > 0)
            SetSortingGroupOrder(Rival.Used[Rival.Used.Count - 1], 0);

        Myself.Used.Add(Myself.Battle);
        Rival.Used.Add(Rival.Battle);
        if ((Phase & 1) == 0)
        {
            moves.Add(new MoveObject() { Object = Myself.Battle, Delta = (Myself.UsedPosition - Myself.Battle.transform.position) / 30 });
            moves.Add(new MoveObject() { Object = Rival.Battle, Delta = (Rival.UsedPosition - Rival.Battle.transform.position) / 30 });
        }

        for (int i = 0; i < Myself.Hand.Count; i++)
        {
            Vector3 step = (MyHandSelectors[i].transform.position - Myself.Hand[i].transform.position) / 30;
            moves.Add(new MoveObject { Object = Myself.Hand[i], Delta = step });
            MyHandSelectors[i].Card = Myself.Hand[i];
        }
        for (int i = 0; i < Rival.Hand.Count; i++)
        {
            Vector3 step = (RivalHandCheckers[i].transform.position - Rival.Hand[i].transform.position) / 30;
            moves.Add(new MoveObject { Object = Rival.Hand[i], Delta = step });
        }

        for (int i = 0; i < 30; i++)
        {
            foreach (MoveObject o in moves)
                o.Step();
            yield return null;
        }


        Myself.DeckCount.text = data.myself.deckcount.ToString();
        Rival.DeckCount.text = data.rival.deckcount.ToString();


        if ((Phase & 1) == 0)
        {
            for (int i = 0; i < Myself.Hand.Count; i++)
            {
                int j = CardData.Chemistry(Myself.Hand[i].GetComponent<Card>().CardData.Element, Myself.Used.Last().GetComponent<Card>().CardData.Element);
                MyHandSelectors[i].SetPlusMinus(j);
            }
            for (int i = 0; i < Rival.Hand.Count; i++)
            {
                int j = CardData.Chemistry(Rival.Hand[i].GetComponent<Card>().CardData.Element, Rival.Used.Last().GetComponent<Card>().CardData.Element);
                RivalHandCheckers[i].SetPlusMinus(j);
            }
            if (Myself.Used.Count > 1)
            {
                Myself.Used[Myself.Used.Count - 2].SetActive(false);
            }
            if (Rival.Used.Count > 1)
            {
                Rival.Used[Rival.Used.Count - 2].SetActive(false);
            }
        }
        InEffect = false;

        if ((Phase & 1) == 1 && data.damage <= 0)
        {
            DecideCard(-1);
        }
    }

    public IEnumerator DamageEffect(UpdateData data)
    {
        Phase = data.phase;

        List<MoveObject> moves = new List<MoveObject>(7)
        {
            new MoveObject() { Object = Myself.Battle, Delta = (Myself.UsedPosition - Myself.Battle.transform.position) / 30 },
            new MoveObject() { Object = Rival.Battle, Delta = (Rival.UsedPosition - Rival.Battle.transform.position) / 30 }
        };

        GameObject DeleteObject = null;
        if (data.damage > 0)
        {
            for (int i = 0; i < 5; i++)
            {
                MyHandSelectors[i].gameObject.SetActive(i < Myself.Hand.Count - 1);
            }
            Canvas.ForceUpdateCanvases();

            DeleteObject = Myself.Hand[data.myself.select];
            Myself.Hand.RemoveAt(data.myself.select);
            Myself.Damage.Add(DeleteObject);

            moves.Add(new MoveObject() { Object = DeleteObject, Delta = (Myself.DamagePosition - DeleteObject.transform.position) / 30 });
            SetSortingGroupOrder(DeleteObject, 1);
            for (int i = 0; i < Myself.Hand.Count; i++)
            { 
                moves.Add(new MoveObject() { Object = Myself.Hand[i], Delta = (MyHandSelectors[i].transform.position - Myself.Hand[i].transform.position) / 30 });
                MyHandSelectors[i].Card = Myself.Hand[i];
            }
        }
        else if (data.damage < 0)
        {
            for (int i = 0; i < 5; i++)
            {
                RivalHandCheckers[i].gameObject.SetActive(i < Rival.Hand.Count - 1);
            }
            Canvas.ForceUpdateCanvases();

            DeleteObject = Rival.Hand[data.rival.select];
            Rival.Hand.RemoveAt(data.rival.select);
            Rival.Damage.Add(DeleteObject);

            moves.Add(new MoveObject() { Object = DeleteObject, Delta = (Rival.DamagePosition - DeleteObject.transform.position) / 30 });
            SetSortingGroupOrder(DeleteObject, 1);
            for (int i = 0; i < Rival.Hand.Count; i++)
                moves.Add(new MoveObject() { Object = Rival.Hand[i], Delta = (RivalHandCheckers[i].transform.position - Rival.Hand[i].transform.position) / 30 });
        }

        for (int i = 0; i < 30; i++)
        {
            foreach (MoveObject o in moves)
                o.Step();
            yield return null;
        }

        if (Myself.Used.Count > 1)
        {
            Myself.Used[Myself.Used.Count - 2].SetActive(false);
        }

        if (Rival.Used.Count > 1)
        {
            Rival.Used[Rival.Used.Count - 2].SetActive(false);
        }


        DeleteObject.SetActive(false);



        for (int i = 0; i < Myself.Hand.Count; i++)
        {
            int j = CardData.Chemistry(Myself.Hand[i].GetComponent<Card>().CardData.Element, Myself.Used.Last().GetComponent<Card>().CardData.Element);
            MyHandSelectors[i].SetPlusMinus(j);
        }
        for (int i = 0; i < Rival.Hand.Count; i++)
        {
            int j = CardData.Chemistry(Rival.Hand[i].GetComponent<Card>().CardData.Element, Rival.Used.Last().GetComponent<Card>().CardData.Element);
            RivalHandCheckers[i].SetPlusMinus(j);
        }

        InEffect = false;
    }




    public void GoToTitle()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
    }


    public void SelectMyCard(int index)
    {
        if ((Phase & 1) == 0)
        {
            for (int i = 0; i < Rival.Hand.Count; i++)
            {
                int j = CardData.Judge(Myself.Hand[index].GetComponent<Card>().CardData,Rival.Hand[i].GetComponent<Card>().CardData,
                                        Myself.Used.LastOrDefault()?.GetComponent<Card>().CardData, Rival.Used.LastOrDefault()?.GetComponent<Card>().CardData);
                RivalHandCheckers[i].SetMaruBatu(j);
            }
        }
    }
    public void SelectRivalCard(int index)
    {
        if ((Phase & 1) == 0)
        {
            for (int i = 0; i < Myself.Hand.Count; i++)
            {
                int j = CardData.Judge(Rival.Hand[index].GetComponent<Card>().CardData, Myself.Hand[i].GetComponent<Card>().CardData,
                                        Rival.Used.LastOrDefault()?.GetComponent<Card>().CardData, Myself.Used.LastOrDefault()?.GetComponent<Card>().CardData);
                MyHandSelectors[i].SetMaruBatu(j);
            }
        }
    }

    public void DecideCard(int index)
    {
        if (InEffect)
            return;

        if (Phase < 0)
            return;

        InEffect = true;
        if ((Phase & 1) == 0)
        {
            Server.SendSelect(index, (data) => StartCoroutine(BattleEffect(data)));
        }
        else
        {
            Server.SendSelect(index, (data) => StartCoroutine(DamageEffect(data)));
        }
    }

}
