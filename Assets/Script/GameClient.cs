﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

using DG.Tweening;

public class GameClient : MonoBehaviour
{
    private IGameServer Server;

    // Start is called before the first frame update
    void Start()
    {
        Server = GameSceneParam.GameServer;
        if (Server == null)
        {
            Server = new OfflineGameServer("Tester");
        }
        Server.SetUpdateCallback(UpdateCallback);

        InitialData data = Server.GetInitialData();

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
        PhaseStartTime = Time.realtimeSinceStartup;
    }

    void Update()
    {
        if (PhaseStartTime > 0)
        {
            float sec = Time.realtimeSinceStartup - PhaseStartTime;
            float remain = (((Phase & 1) == 0) ? BattleTimeLimit : DamageTimeLimit) - sec;
            if (remain < 0)
                DecideCard(0);
            else
                Timer.text = remain.ToString("F");
        }
        else
        {
            Timer.text = "";
        }
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
    public int Phase { get; private set; }

    private PlayerObjects Myself;
    private PlayerObjects Rival;

    public Text MyName;
    public Text RivalName;

    //Inspectorで設定するのが楽なのでpublic
    public HandSelector[] MyHandSelectors;
    public HandChecker[] RivalHandCheckers;

    public Text Timer;
    private float PhaseStartTime;

    private float BattleTimeLimit;
    private float DamageTimeLimit;


    public GameObject Cards;

    public GameObject MyUsed;
    public GameObject RivalUsed;
    public GameObject MyDamage;
    public GameObject RivalDamage;


    public GameObject FrontCanvas;
    public Text Message;


    public AudioClip AudioAttack;
    public AudioClip AudioAttackOffset;
    public AudioClip AudioDamage;
    public AudioClip AudioRecover;

    public AudioSource AudioSource;

    public GameObject WaitCircle;

    public Arrow MySupportArrow;
    public Arrow RivalSupportArrow;

    public Arrow MyBattleArrow;
    public Arrow RivalBattleArrow;


    public BattleAvatar MyBattleAvatar;
    public BattleAvatar RivalBattleAvatar;

    public bool InEffect { get; private set; } = false;



    private static GameObject CardPrefab;

    private GameObject[] CardArray;
    private int CardArrayIndex;

    private GameObject CreateCard(int id)
    {
        GameObject c = CardArray[CardArrayIndex++];
        c.GetComponent<Card>().Initialize(id);
        c.SetActive(true);
        return c;
    }


    static void SetSortingGroupOrder(GameObject card, int order)
    {
        SortingGroup sg = card.GetComponent<SortingGroup>();
        sg.sortingOrder = order;
    }

    public void InitializeField(InitialData data)
    {
        foreach (GameObject c in CardArray)
        {
            c.SetActive(false);
        }
        CardArrayIndex = 0;

        Phase = 0;

        for (int i = 0; i < 5;i++)
        {
            MyHandSelectors[i].gameObject.SetActive(i < data.myhand.Length);
            MyHandSelectors[i].ResetAllOption();
        }
        for (int i = 0; i < 5; i++)
        {
            RivalHandCheckers[i].gameObject.SetActive(i < data.rivalhand.Length);
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


        Myself.Hand = new List<GameObject>(data.myhand.Length + 1);
        for (int i = 0; i < data.myhand.Length; i++)
        {
            GameObject o = CreateCard(data.myhand[i]);
            o.transform.position = MyHandSelectors[i].transform.position;
            SetSortingGroupOrder(o, 5);
            Myself.Hand.Add(o);
            MyHandSelectors[i].Card = o;
        }

        Myself.Used = new List<GameObject>(20);
        Myself.Damage = new List<GameObject>(20);
        GameObject tmp = GameObject.Find("MyDeckCounter");
        Myself.DeckCount = tmp.GetComponent<Text>();
        Myself.DeckCount.text = data.mydeckcount.ToString();


        Rival.Hand = new List<GameObject>(data.rivalhand.Length + 1);
        for (int i = 0; i < data.rivalhand.Length; i++)
        {
            GameObject o = CreateCard(data.rivalhand[i]);
            o.transform.position = RivalHandCheckers[i].transform.position;
            SetSortingGroupOrder(o, 5);
            Rival.Hand.Add(o);
        }

        Rival.Used = new List<GameObject>(20);
        Rival.Damage = new List<GameObject>(20);
        tmp = GameObject.Find("YourDeckCounter");
        Rival.DeckCount = tmp.GetComponent<Text>();
        Rival.DeckCount.text = data.rivaldeckcount.ToString();

        MyName.text = data.myname;
        RivalName.text = data.rivalname;

        BattleTimeLimit = data.battleSelectTimeLimitSecond;
        DamageTimeLimit = data.damageSelectTimeLimitSecond;

    }

    public IEnumerator BattleEffect(UpdateData data)
    {
        for (int i = 0; i < 5; i++)
        {
            MyHandSelectors[i].ResetAllOption();
            RivalHandCheckers[i].ResetAllOption();
            MyHandSelectors[i].gameObject.SetActive(i < Myself.Hand.Count - 1 + data.myself.draw.Length);
            RivalHandCheckers[i].gameObject.SetActive(i < Rival.Hand.Count - 1 + data.rival.draw.Length);
        }
        Canvas.ForceUpdateCanvases();

        Myself.Battle = Myself.Hand[data.myself.select];
        Rival.Battle = Rival.Hand[data.rival.select];

        SetSortingGroupOrder(Myself.Battle, 6);
        SetSortingGroupOrder(Rival.Battle, 6);


        //手札から戦場に移動
        Myself.Battle.transform.DOMove(Myself.BattlePosition, 0.5f);
        Rival.Battle.transform.DOMove(Rival.BattlePosition, 0.5f);
        yield return new WaitForSeconds(0.5f);



        CardData myBattleData = Myself.Battle.GetComponent<Card>().CardData;
        CardData rivalBattleData = Rival.Battle.GetComponent<Card>().CardData;

        MyBattleAvatar.Appearance(Myself.BattlePosition, myBattleData.Element, myBattleData.Power);
        RivalBattleAvatar.Appearance(Rival.BattlePosition, rivalBattleData.Element, rivalBattleData.Power);



        int mypower = myBattleData.Power;
        int rivalpower = rivalBattleData.Power;

        if (Myself.Used.Count > 0)
        {
            CardData mySupportData = Myself.Used[^1].GetComponent<Card>().CardData;
            int c = CardData.Chemistry(myBattleData.Element, mySupportData.Element);
            if (c > 0)
            {
                mypower++;
                MyBattleAvatar.Raise(mypower);
                MySupportArrow.StartAnimationPlus();
            }
            else if (c < 0)
            {
                mypower = mypower - 1 < 0 ? 0 : mypower - 1;
                MyBattleAvatar.Reduce(mypower);
                MySupportArrow.StartAnimationMinus();
            }
            CardData rivalSupportData = Rival.Used[^1].GetComponent<Card>().CardData;
            c = CardData.Chemistry(rivalBattleData.Element, rivalSupportData.Element);
            if (c > 0)
            {
                rivalpower++;
                RivalBattleAvatar.Raise(rivalpower);
                RivalSupportArrow.StartAnimationPlus();
            }
            else if (c < 0)
            {
                rivalpower = rivalpower - 1 < 0 ? 0 : rivalpower - 1;
                RivalBattleAvatar.Reduce(rivalpower);
                RivalSupportArrow.StartAnimationMinus();
            }
        }
        yield return new WaitForSeconds(0.5f);
        {
            int c1 = CardData.Chemistry(myBattleData.Element, rivalBattleData.Element);
            int c2 = CardData.Chemistry(rivalBattleData.Element, myBattleData.Element);
            if (c1 > 0)
            {
                mypower++;
                MyBattleAvatar.Raise(mypower);
                MyBattleArrow.StartAnimationPlus();
            }
            else if (c1 < 0)
            {
                mypower = mypower - 1 < 0 ? 0 : mypower - 1;
                MyBattleAvatar.Reduce(mypower);
                MyBattleArrow.StartAnimationMinus();
            }

            if (c2 > 0)
            {
                rivalpower++;
                RivalBattleAvatar.Raise(rivalpower);
                RivalBattleArrow.StartAnimationPlus();
            }
            else if (c2 < 0)
            {
                rivalpower = rivalpower - 1 < 0 ? 0 : rivalpower - 1;
                RivalBattleAvatar.Reduce(rivalpower);
                RivalBattleArrow.StartAnimationMinus();
            }
        }

        yield return new WaitForSeconds(0.5f);

        MyBattleAvatar.Disappearance();
        RivalBattleAvatar.Disappearance();

        SetSortingGroupOrder(Myself.Battle, 1);
        SetSortingGroupOrder(Rival.Battle, 1);

        if (data.damage < 0)
            AudioSource.PlayOneShot(AudioAttack);
        else if ( data.damage == 0)
        {
            AudioSource.PlayOneShot(AudioAttackOffset);
        }
        else if (data.damage > 0)
        {
            AudioSource.PlayOneShot(AudioDamage);
        }

        for (int i = 0;i < data.myself.draw.Length ; i++)
        {
            GameObject o = CreateCard(data.myself.draw[i]);
            o.transform.position = Myself.DeckPosition;
            SetSortingGroupOrder(o,5);
            Myself.Hand.Add(o);
        }
        for (int i = 0; i < data.rival.draw.Length; i++)
        {
            GameObject o = CreateCard(data.rival.draw[i]);
            o.transform.position = Rival.DeckPosition;
            SetSortingGroupOrder(o,5);
            Rival.Hand.Add(o);
        }


        if (data.phase < 0)
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
            Phase = data.phase;
            InEffect = false;

            Server.Terminalize();
            Server = null;
            yield break;
        }




        if (Myself.Used.Count > 0)
            SetSortingGroupOrder(Myself.Used[^1], 0);
        if (Rival.Used.Count > 0)
            SetSortingGroupOrder(Rival.Used[^1], 0);

        Myself.Used.Add(Myself.Battle);
        Rival.Used.Add(Rival.Battle);

        Myself.Battle.transform.SetParent(MyUsed.transform);
        Rival.Battle.transform.SetParent(RivalUsed.transform);

        if ((data.phase & 1) == 0)
        {
            Myself.Battle.transform.DOMove(Myself.UsedPosition, 0.5f);
            Rival.Battle.transform.DOMove(Rival.UsedPosition, 0.5f);
        }
        Myself.Hand.RemoveAt(data.myself.select);
        Rival.Hand.RemoveAt(data.rival.select);
        for (int i = 0; i < Myself.Hand.Count; i++)
        {
            Myself.Hand[i].transform.DOMove(MyHandSelectors[i].transform.position, 0.5f);
            MyHandSelectors[i].Card = Myself.Hand[i];
        }
        for (int i = 0; i < Rival.Hand.Count; i++)
        {
            Rival.Hand[i].transform.DOMove(RivalHandCheckers[i].transform.position, 0.5f);
        }

        yield return new WaitForSeconds(0.5f);


        Myself.DeckCount.text = data.myself.deckcount.ToString();
        Rival.DeckCount.text = data.rival.deckcount.ToString();

        if ((data.phase & 1) == 0)
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
                Myself.Used[^2].SetActive(false);
            }
            if (Rival.Used.Count > 1)
            {
                Rival.Used[^2].SetActive(false);
            }
        }
        Phase = data.phase;
        InEffect = false;

        if ((data.phase & 1) == 1 && data.damage <= 0)
        {
            DecideCard(-1);
        }
        else
            PhaseStartTime = Time.realtimeSinceStartup;

   }

    public IEnumerator DamageEffect(UpdateData data)
    {
        Myself.Battle.transform.DOMove(Myself.UsedPosition, 0.5f);
        Rival.Battle.transform.DOMove(Rival.UsedPosition, 0.5f);

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
            DeleteObject.transform.SetParent(MyDamage.transform);

            DeleteObject.transform.DOMove(Myself.DamagePosition, 0.5f);
            SetSortingGroupOrder(DeleteObject, 10);
            for (int i = 0; i < Myself.Hand.Count; i++)
            {
                Myself.Hand[i].transform.DOMove(MyHandSelectors[i].transform.position, 0.5f);
                MyHandSelectors[i].Card = Myself.Hand[i];
            }
            AudioSource.PlayOneShot(AudioRecover);
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
            DeleteObject.transform.SetParent(RivalDamage.transform);


            DeleteObject.transform.DOMove(Rival.DamagePosition, 0.5f);
            SetSortingGroupOrder(DeleteObject, 10);
            for (int i = 0; i < Rival.Hand.Count; i++)
                Rival.Hand[i].transform.DOMove(RivalHandCheckers[i].transform.position, 0.5f);
        }

        yield return new WaitForSeconds(0.5f);

        if (Myself.Used.Count > 1)
        {
            Myself.Used[^2].SetActive(false);
        }

        if (Rival.Used.Count > 1)
        {
            Rival.Used[^2].SetActive(false);
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

        Phase = data.phase;
        InEffect = false;
        PhaseStartTime = Time.realtimeSinceStartup;
    }




    public void GoToTitle()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
    }


    public void SelectMyCard(int index)
    {
        if ((Phase & 1) == 0)
        {
            CardData mysupport = (Myself.Used.Count > 0) ? Myself.Used.Last().GetComponent<Card>().CardData : null;
            CardData rivalsupport = (Rival.Used.Count > 0) ? Rival.Used.Last().GetComponent<Card>().CardData : null;
            for (int i = 0; i < Rival.Hand.Count; i++)
            {
                int j = CardData.Judge(Myself.Hand[index].GetComponent<Card>().CardData,Rival.Hand[i].GetComponent<Card>().CardData,
                                       mysupport, rivalsupport);
                RivalHandCheckers[i].SetMaruBatu(j);
            }
        }
    }
    public void SelectRivalCard(int index)
    {
        if ((Phase & 1) == 0)
        {
            CardData mysupport = (Myself.Used.Count > 0) ? Myself.Used.Last().GetComponent<Card>().CardData : null;
            CardData rivalsupport = (Rival.Used.Count > 0) ? Rival.Used.Last().GetComponent<Card>().CardData : null;
            for (int i = 0; i < Myself.Hand.Count; i++)
            {
                int j = CardData.Judge(Rival.Hand[index].GetComponent<Card>().CardData, Myself.Hand[i].GetComponent<Card>().CardData,
                                       rivalsupport, mysupport);
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
        PhaseStartTime = -1;

        if (index >= 0 && index < Myself.Hand.Count)
        {
            Vector3 tmp = Myself.Hand[index].transform.localPosition;
            tmp.y += 50;
            Myself.Hand[index].transform.localPosition = tmp;
        }

        if ((Phase & 1) == 0)
        {
            WaitCircle.SetActive(true);
            Server.SendSelect(Phase, index);
        }
        else
        {
            Server.SendSelect(Phase, index);
        }
    }

    void UpdateCallback(UpdateData data, string abort)
    {
        StartCoroutine(UpdateCoroutine(data,abort));
    }
    private Coroutine EffectCoroutin = null;
    public IEnumerator UpdateCoroutine(UpdateData data, string abort)
    {
        WaitCircle.SetActive(false);

        if (EffectCoroutin != null)
        {
            yield return EffectCoroutin;
            EffectCoroutin = null;
        }

        if (Phase < 0)
            yield break;
        if (abort != null)
        {
            Phase = -1;
            if (data.damage > 0)
                Message.text = abort + " Lose";
            else if (data.damage < 0)
                Message.text = abort +" Win";
            else
                Message.text = abort + " Draw";

            FrontCanvas.SetActive(true);
            InEffect = false;
            Server.Terminalize();

            yield break;
        }
        if (data == null)
            yield break;
        InEffect = true;
        PhaseStartTime = -1;

        if ((Phase & 1) == 0)
        {
            EffectCoroutin = StartCoroutine(BattleEffect(data));
        }
        else
        {
            EffectCoroutin = StartCoroutine(DamageEffect(data));
        }
    }
    public void Surrender()
    {
        Server?.SendSurrender();
    }

}
