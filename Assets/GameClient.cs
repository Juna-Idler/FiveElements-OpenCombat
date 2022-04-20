using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class GameClient : MonoBehaviour
{

    private readonly OfflineGameServer OfflineGameServer = new OfflineGameServer();

    private IGameServer Server;

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
    private ClientData.Phases Phase;

    private ClientData Data;

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

    public void InitializeField(ClientData data)
    {
        foreach (GameObject c in CardArray)
        {
            c.SetActive(false);
        }
        CardArrayIndex = 0;

        Data = data;
        Phase = data.phase;

        for (int i = 0; i < 5;i++)
        {
            MyHandSelectors[i].gameObject.SetActive(i < data.myself.hand.Length);
            MyHandSelectors[i].ResetAllOption();
        }
        for (int i = 0; i < 5; i++)
        {
            RivalHandCheckers[i].gameObject.SetActive(i < data.rival.hand.Length);
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


        Myself.Hand = new List<GameObject>(data.myself.hand.Length);
        for (int i = 0; i < data.myself.hand.Length; i++)
        {
            GameObject o = CreateCard(data.myself.hand[i]);
            o.transform.position = MyHandSelectors[i].transform.position;
            Myself.Hand.Add(o);
            MyHandSelectors[i].Card = o;
        }

        Myself.Used = new List<GameObject>(20);
        foreach (var cd in data.myself.used)
        {
            GameObject o = CreateCard(cd);
            o.SetActive(false);
            Myself.Used.Add(o);
        }
        Myself.Damage = new List<GameObject>(20);
        foreach (var cd in data.myself.damage)
        {
            GameObject o = CreateCard(cd);
            o.SetActive(false);
            Myself.Damage.Add(o);
        }
        GameObject tmp = GameObject.Find("MyDeckCounter");
        Myself.DeckCount = tmp.GetComponent<Text>();
        Myself.DeckCount.text = data.myself.decknum.ToString();


        Rival.Hand = new List<GameObject>(data.rival.hand.Length);
        for (int i = 0; i < data.rival.hand.Length; i++)
        {
            GameObject o = CreateCard(data.rival.hand[i]);
            o.transform.position = RivalHandCheckers[i].transform.position;
            Rival.Hand.Add(o);
        }

        Rival.Used = new List<GameObject>(20);
        foreach (var cd in data.rival.used)
        {
            GameObject o = CreateCard(cd);
            o.SetActive(false);
            Rival.Used.Add(o);
        }
        Rival.Damage = new List<GameObject>(20);
        foreach (var cd in data.rival.damage)
        {
            GameObject o = CreateCard(cd);
            o.SetActive(false);
            Rival.Damage.Add(o);
        }
        tmp = GameObject.Find("YourDeckCounter");
        Rival.DeckCount = tmp.GetComponent<Text>();
        Rival.DeckCount.text = data.rival.decknum.ToString();


        switch (data.phase)
        {
            case ClientData.Phases.BattlePhase:
                {
                    if (Myself.Used.Count > 0)
                    {
                        GameObject b = Myself.Used.Last();
                        b.SetActive(true);
                        b.transform.position = Myself.UsedPosition;

                        for (int i = 0; i < data.myself.hand.Length;i++)
                        {
                            int j = CardData.Chemistry(data.myself.hand[i].Element, data.myself.used.Last().Element);
                            MyHandSelectors[i].SetPlusMinus(j);
                        }
                    }
                    if (Rival.Used.Count > 0)
                    {
                        GameObject b = Rival.Used.Last();
                        b.SetActive(true);
                        b.transform.position = Rival.UsedPosition;

                        for (int i = 0; i < data.rival.hand.Length; i++)
                        {
                            int j = CardData.Chemistry(data.rival.hand[i].Element, data.rival.used.Last().Element);
                            RivalHandCheckers[i].SetPlusMinus(j);
                        }
                    }
                }
                break;
            case ClientData.Phases.DamagePhase:
                {
                    if (Myself.Used.Count > 1)
                    {
                        GameObject u = Myself.Used[Myself.Used.Count-2];
                        u.SetActive(true);
                        u.transform.position = Myself.UsedPosition;
                    }
                    Myself.Battle = Myself.Used.Last();
                    Myself.Battle.SetActive(true);
                    Myself.Battle.transform.position = Myself.BattlePosition;

                    if (Rival.Used.Count > 1)
                    {
                        GameObject u = Rival.Used[Rival.Used.Count - 2];
                        u.SetActive(true);
                        u.transform.position = Rival.UsedPosition;
                    }
                    Rival.Battle = Rival.Used.Last();
                    Rival.Battle.SetActive(true);
                    Rival.Battle.transform.position = Rival.BattlePosition;

                }
                break;
        }

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

    public IEnumerator BattleEffect(ClientData data)
    {
        Data = data;
        InEffect = true;
        Debug.Log("start coroutin:BattleEffect");

        for (int i = 0; i < 5; i++)
        {
            MyHandSelectors[i].ResetAllOption();
            RivalHandCheckers[i].ResetAllOption();
            MyHandSelectors[i].gameObject.SetActive(i < data.myself.hand.Length);
            RivalHandCheckers[i].gameObject.SetActive(i < data.rival.hand.Length);
        }
        Canvas.ForceUpdateCanvases();

        Myself.Battle = Myself.Hand[data.myselect];
        Rival.Battle = Rival.Hand[data.rivalselect];

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

        Myself.Hand.RemoveAt(data.myselect);
        Rival.Hand.RemoveAt(data.rivalselect);




        List<MoveObject> moves = new List<MoveObject>(12);

        switch (data.phase)
        {
            case ClientData.Phases.BattlePhase:
                Myself.Used.Add(Myself.Battle);
                Rival.Used.Add(Rival.Battle);

                if (Myself.Used.Count > 1)
                    SetSortingGroupOrder(Myself.Used[Myself.Used.Count - 2], 0);
                if (Rival.Used.Count > 1)
                    SetSortingGroupOrder(Rival.Used[Rival.Used.Count - 2], 0);
                moves.Add(new MoveObject() { Object = Myself.Battle, Delta = (Myself.UsedPosition - Myself.Battle.transform.position) / 30 });
                moves.Add(new MoveObject() { Object = Rival.Battle, Delta = (Rival.UsedPosition - Rival.Battle.transform.position) / 30 });
                break;

            case ClientData.Phases.DamagePhase:
                Myself.Used.Add(Myself.Battle);
                Rival.Used.Add(Rival.Battle);
                break;

            case ClientData.Phases.GameEnd:
                {
                    int mylife = data.myself.decknum + data.myself.hand.Length - System.Convert.ToInt32(data.damage > 0);
                    int rivallife = data.rival.decknum + data.rival.hand.Length - System.Convert.ToInt32(data.damage < 0);
                    if (mylife > rivallife)
                        Message.text = "Win";
                    else if (mylife < rivallife)
                        Message.text = "Lose";
                    else
                        Message.text = "Draw";
                    FrontCanvas.SetActive(true);
                    InEffect = false;
                }
                yield break;
        }



        for (int i = data.myself.hand.Length - data.mydraw; i < data.myself.hand.Length; i++)
        {
            GameObject o = CreateCard(data.myself.hand[i]);
            o.transform.position = Myself.DeckPosition;
            Myself.Hand.Add(o);
        }
        for (int i = 0; i < Myself.Hand.Count; i++)
        {
            Vector3 step = (MyHandSelectors[i].transform.position - Myself.Hand[i].transform.position) / 30;
            moves.Add(new MoveObject { Object = Myself.Hand[i], Delta = step });
            MyHandSelectors[i].Card = Myself.Hand[i];
        }

        for (int i = data.rival.hand.Length - data.rivaldraw; i < data.rival.hand.Length; i++)
        {
            GameObject o = CreateCard(data.rival.hand[i]);
            o.transform.position = Rival.DeckPosition;
            Rival.Hand.Add(o);
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


        Myself.DeckCount.text = data.myself.decknum.ToString();
        Rival.DeckCount.text = data.rival.decknum.ToString();

        Phase = data.phase;

        if (Phase == ClientData.Phases.BattlePhase)
        {
            for (int i = 0; i < data.myself.hand.Length; i++)
            {
                int j = CardData.Chemistry(data.myself.hand[i].Element, data.myself.used.Last().Element);
                MyHandSelectors[i].SetPlusMinus(j);
            }
            for (int i = 0; i < data.rival.hand.Length; i++)
            {
                int j = CardData.Chemistry(data.rival.hand[i].Element, data.rival.used.Last().Element);
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

        if (data.phase == ClientData.Phases.DamagePhase && data.damage <= 0)
        {
            DecideCard(0);
        }

    }

    public IEnumerator DamageEffect(ClientData data)
    {
        Data = data;
        InEffect = true;
        Debug.Log("start coroutin:DamageEffect");
        for (int i = 0; i < 5; i++)
        {
            MyHandSelectors[i].gameObject.SetActive(i < data.myself.hand.Length);
            RivalHandCheckers[i].gameObject.SetActive(i < data.rival.hand.Length);
        }
        Canvas.ForceUpdateCanvases();


        if (Myself.Used.Count > 1)
            SetSortingGroupOrder(Myself.Used[Myself.Used.Count - 2], 0);
        if (Rival.Used.Count > 1)
            SetSortingGroupOrder(Rival.Used[Rival.Used.Count - 2], 0);

        List<MoveObject> moves = new List<MoveObject>(7)
        {
            new MoveObject() { Object = Myself.Battle, Delta = (Myself.UsedPosition - Myself.Battle.transform.position) / 30 },
            new MoveObject() { Object = Rival.Battle, Delta = (Rival.UsedPosition - Rival.Battle.transform.position) / 30 }
        };


        if (data.myselect >= 0)
        {
            moves.Add(new MoveObject() { Object = Myself.Hand[data.myselect], Delta = (Myself.DamagePosition - Myself.Hand[data.myselect].transform.position) / 30 });
            SetSortingGroupOrder(Myself.Hand[data.myselect], 1);
            for (int i = 0; i < data.myselect; i++)
                moves.Add(new MoveObject() { Object = Myself.Hand[i], Delta = (MyHandSelectors[i].transform.position - Myself.Hand[i].transform.position) / 30 });
            for (int i = data.myselect + 1;i < Myself.Hand.Count;i++)
                moves.Add(new MoveObject() { Object = Myself.Hand[i], Delta = (MyHandSelectors[i-1].transform.position - Myself.Hand[i].transform.position) / 30 });
        }
        else if (data.rivalselect >= 0)
        {
            moves.Add(new MoveObject() { Object = Rival.Hand[data.rivalselect], Delta = (Rival.DamagePosition - Rival.Hand[data.rivalselect].transform.position) / 30 });
            SetSortingGroupOrder(Rival.Hand[data.rivalselect], 1);
            for (int i = 0; i < data.rivalselect; i++)
                moves.Add(new MoveObject() { Object = Rival.Hand[i], Delta = (RivalHandCheckers[i].transform.position - Rival.Hand[i].transform.position) / 30 });
            for (int i = data.rivalselect + 1; i < Rival.Hand.Count; i++)
                moves.Add(new MoveObject() { Object = Rival.Hand[i], Delta = (RivalHandCheckers[i - 1].transform.position - Rival.Hand[i].transform.position) / 30 });
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



        if (data.myselect >= 0)
        {
            GameObject o = Myself.Hand[data.myselect];
            Myself.Hand.RemoveAt(data.myselect);
            Myself.Damage.Add(o);
            o.SetActive(false);
        }
        if (data.rivalselect >= 0)
        {
            GameObject o = Rival.Hand[data.rivalselect];
            Rival.Hand.RemoveAt(data.rivalselect);
            Rival.Damage.Add(o);
            o.SetActive(false);
        }

        for (int i = 0; i < Myself.Hand.Count; i++)
        {
            Myself.Hand[i].transform.position = MyHandSelectors[i].transform.position;
            MyHandSelectors[i].Card = Myself.Hand[i];
        }
        for (int i = 0; i < Rival.Hand.Count; i++)
        {
            Rival.Hand[i].transform.position = RivalHandCheckers[i].transform.position;
        }

        Phase = data.phase;

        for (int i = 0; i < data.myself.hand.Length; i++)
        {
            int j = CardData.Chemistry(data.myself.hand[i].Element, data.myself.used.Last().Element);
            MyHandSelectors[i].SetPlusMinus(j);
        }
        for (int i = 0; i < data.rival.hand.Length; i++)
        {
            int j = CardData.Chemistry(data.rival.hand[i].Element, data.rival.used.Last().Element);
            RivalHandCheckers[i].SetPlusMinus(j);
        }
        InEffect = false;
    }


    // Start is called before the first frame update
    void Start()
    {
        OfflineGameServer.Initialize();
        Server = OfflineGameServer;

        ClientData data = Server.GetData();

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

    public void Retry()
    {
        OfflineGameServer.Initialize();
        ClientData data = Server.GetData();

        InitializeField(data);
        FrontCanvas.SetActive(false);
    }


    public void SelectMyCard(int index)
    {
        if (Phase == ClientData.Phases.BattlePhase)
        {
            for (int i = 0; i < Data.rival.hand.Length;i++)
            {
                int j = CardData.Judge(Data.myself.hand[index], Data.rival.hand[i], Data.myself.used.LastOrDefault(), Data.rival.used.LastOrDefault());
                RivalHandCheckers[i].SetMaruBatu(j);
            }
        }
    }
    public void SelectRivalCard(int index)
    {
        if (Phase == ClientData.Phases.BattlePhase)
        {
            for (int i = 0; i < Data.myself.hand.Length; i++)
            {
                int j = CardData.Judge(Data.myself.hand[i], Data.rival.hand[index], Data.myself.used.LastOrDefault(), Data.rival.used.LastOrDefault());
                MyHandSelectors[i].SetMaruBatu(j);
            }
        }
    }

    public void DecideCard(int index)
    {
        if (InEffect)
            return;
        switch (Phase)
        {
            case ClientData.Phases.BattlePhase:
                {
                    Server.SendSelect(index, (data) => StartCoroutine(BattleEffect(data)));

                    break;
                }
            case ClientData.Phases.DamagePhase:
                {
                    Server.SendSelect(index, (data) => StartCoroutine(DamageEffect(data)));

                    break;
                }
            default:
                break;
        }
    }



}
