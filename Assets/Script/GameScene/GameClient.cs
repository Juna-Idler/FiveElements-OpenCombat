using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

using DG.Tweening;
using TMPro;

public class GameClient : MonoBehaviour
{
    private IGameServer Server;

    // Start is called before the first frame update
    void Start()
    {
        CardCatalog cc = CardCatalog.Instance;
        Server = GameSceneParam.GameServer;
        if (Server == null)
        {
            Server = new OfflineGameServer("Tester",new Level1Commander());
        }
        Server.SetUpdateCallback(UpdateCallback);

        InitialData data = Server.GetInitialData();

        GameObject myavatar = Instantiate(AvatarPrefab[GameSceneParam.MyAvatar]);
        Myself.Avatar = myavatar.GetComponent<PlayerAvatar>();
        GameObject rivalavatar = Instantiate(AvatarPrefab[GameSceneParam.RivalAvatar]);
        rivalavatar.transform.localPosition = new Vector2(-rivalavatar.transform.localPosition.x, rivalavatar.transform.localPosition.y);
        rivalavatar.transform.localScale = new Vector2(-rivalavatar.transform.localScale.x, rivalavatar.transform.localScale.y);
        Rival.Avatar = rivalavatar.GetComponent<PlayerAvatar>();

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

        CardListView.Initialize();
        InitializeField(data);
        FrontCanvas.SetActive(false);
        PhaseStartTime = Time.realtimeSinceStartup;
        TimeBar.SetActive(BattleTimeLimit);
    }

    void Update()
    {
        if (PhaseStartTime > 0)
        {
            float sec = Time.realtimeSinceStartup - PhaseStartTime;
            float remain = (((Phase & 1) == 0) ? BattleTimeLimit : DamageTimeLimit) - sec;
            if (remain < 0)
            {
                if (CardListView.IsOpen)
                    CardListView.Close();
                TimeBar.gameObject.SetActive(false);
                DecideCard(0);
            }
            else
            {
                TimeBar.SetTime(remain);
            }
        }
    }

    [System.Serializable]
    public class PlayerObjects
    {
        public GameObject Battle { get; set; }
        public GameObject Support { get; set; }

        public List<GameObject> Hand { get; set; }
        public List<GameObject> Used { get; set; }
        public List<GameObject> Damage { get; set; }


//Inspector上で初期化してもらう

        public Vector2 BattlePosition;
        public Vector2 UsedPosition;
        public Vector2 DamagePosition;
        public Vector2 DeckPosition;


        public Text Name;
        public TextMeshProUGUI DeckCount;
//        public TwoDigits DeckCount;
        public GameObject HandArea;
        public Image HandBackImage;

        public Arrow SupportArrow;
        public Arrow BattleArrow;


        public PlayerAvatar Avatar { get; set; }

        public BattleAvatar BattleAvatar;
    }

    public PlayerObjects Myself;
    public PlayerObjects Rival;

    private readonly List<HandSelector> MyHandSelectors = new List<HandSelector>(10);
    private readonly List<HandChecker> RivalHandCheckers = new List<HandChecker>(10);

    public int Phase { get; private set; }

    public TextMeshPro RoundText;


    public AudioSource BGMAudioSource;

    public GameObject WaitCircle;


    public TimeBar TimeBar;

    private float PhaseStartTime;

    private float BattleTimeLimit;
    private float DamageTimeLimit;



    public SettingsScreen Settings;

//カードリスト表示UI
    public CardListView CardListView;


    public GameObject FrontCanvas;
    public Text Message;




    public bool InEffect { get; private set; } = false;


    public GameObject CardPrefab;
    public GameObject HandSelectorPrefab;
    public GameObject HandCheckerPrefab;


    public GameObject[] AvatarPrefab;


    //ヒエラルキーで見やすくするためだけの空オブジェクト
    public GameObject Cards;

    public GameObject MyUsed;
    public GameObject RivalUsed;
    public GameObject MyDamage;
    public GameObject RivalDamage;


    private GameObject[] CardArray;
    private int CardArrayIndex;

    private GameObject CreateCard(int id,int order,bool active = true)
    {
        GameObject c = CardArray[CardArrayIndex++];
        c.GetComponent<Card>().Initialize(id);
        SetSortingGroupOrder(c, order);
        c.SetActive(active);
        return c;
    }


    static void SetSortingGroupOrder(GameObject card, int order)
    {
        SortingGroup sg = card.GetComponent<SortingGroup>();
        sg.sortingOrder = order;
    }

    private void SetHandCount(int myhandcount,int rivalhandcount)
    {
        if (myhandcount > MyHandSelectors.Count)
        {
            for (int i = MyHandSelectors.Count; i < myhandcount; i++)
            {
                GameObject go = Instantiate(HandSelectorPrefab);
                HandSelector hs = go.GetComponent<HandSelector>();
                MyHandSelectors.Add(hs);
                go.transform.SetParent(Myself.HandArea.transform);
                hs.Index = i;
            }
        }
        if (rivalhandcount > RivalHandCheckers.Count)
        {
            for (int i = RivalHandCheckers.Count; i < rivalhandcount; i++)
            {
                GameObject go = Instantiate(HandCheckerPrefab);
                HandChecker hc = go.GetComponent<HandChecker>();
                RivalHandCheckers.Add(hc);
                go.transform.SetParent(Rival.HandArea.transform);
                hc.Index = i;
            }
        }

        {
            RectTransform rect = Myself.HandArea.GetComponent<RectTransform>();
            float step = rect.sizeDelta.x / (myhandcount + 1);
            float start = -rect.sizeDelta.x / 2 + step;
            if (step < 100 + 10)
            {
                start = -rect.sizeDelta.x / 2 + 50 + 5;
                step = (rect.sizeDelta.x - (100 + 10)) / (myhandcount + 1 - 2);
            }

            for (int i = 0; i < MyHandSelectors.Count; i++)
            {
                if (i < myhandcount)
                {
                    MyHandSelectors[i].gameObject.transform.localPosition = new Vector2(start + step * i, 0);
                    MyHandSelectors[i].gameObject.SetActive(true);
                    MyHandSelectors[i].ResetAllOption();
                }
                else
                {
                    MyHandSelectors[i].gameObject.SetActive(false);
                }
            }
        }
        {
            RectTransform rect = Rival.HandArea.GetComponent<RectTransform>();
            float step = rect.sizeDelta.x / (rivalhandcount + 1);
            float start = -rect.sizeDelta.x / 2 + step;
            if (step < 100 + 10)
            {
                start = -rect.sizeDelta.x / 2 + 50 + 5;
                step = (rect.sizeDelta.x - (100 + 10)) / (rivalhandcount + 1 - 2);
            }

            for (int i = 0; i < RivalHandCheckers.Count; i++)
            {
                if (i < rivalhandcount)
                {
                    RivalHandCheckers[i].gameObject.transform.localPosition = new Vector2(start + step * i, 0);
                    RivalHandCheckers[i].gameObject.SetActive(true);
                    RivalHandCheckers[i].ResetAllOption();
                }
                else
                {
                    RivalHandCheckers[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void InitializeField(InitialData data)
    {
        foreach (GameObject c in CardArray)
        {
            c.SetActive(false);
        }
        CardArrayIndex = 0;

        Phase = 0;

        RoundText.text = $"Round {Phase / 2 + 1}\nBattle";

        SetHandCount(data.myhand.Length, data.rivalhand.Length);


        Myself.Hand = new List<GameObject>(data.myhand.Length + 1);
        for (int i = 0; i < data.myhand.Length; i++)
        {
            GameObject o = CreateCard(data.myhand[i],i + 101);
            o.transform.position = MyHandSelectors[i].transform.position;
            Myself.Hand.Add(o);
            MyHandSelectors[i].Card = o;
        }

        Myself.Used = new List<GameObject>(20);
        Myself.Damage = new List<GameObject>(20);
        Myself.DeckCount.text = data.mydeckcount.ToString();


        Rival.Hand = new List<GameObject>(data.rivalhand.Length + 1);
        for (int i = 0; i < data.rivalhand.Length; i++)
        {
            GameObject o = CreateCard(data.rivalhand[i],i + 101);
            o.transform.position = RivalHandCheckers[i].transform.position;
            Rival.Hand.Add(o);
        }

        Rival.Used = new List<GameObject>(20);
        Rival.Damage = new List<GameObject>(20);
        Rival.DeckCount.text = data.rivaldeckcount.ToString();

        Myself.Name.text = data.myname;
        Rival.Name.text = data.rivalname;

        BattleTimeLimit = data.battleSelectTimeLimitSecond;
        DamageTimeLimit = data.damageSelectTimeLimitSecond;

       
    }

    public IEnumerator BattleEffect(UpdateData data)
    {
//前処理
        Myself.Support = Myself.Battle;
        Rival.Support = Rival.Battle;
        Myself.Battle = Myself.Hand[data.myself.select];
        Rival.Battle = Rival.Hand[data.rival.select];

        Myself.Used.Add(Myself.Battle);
        Rival.Used.Add(Rival.Battle);
        Myself.Hand.RemoveAt(data.myself.select);
        Rival.Hand.RemoveAt(data.rival.select);
        Myself.Battle.transform.SetParent(MyUsed.transform);
        Rival.Battle.transform.SetParent(RivalUsed.transform);

        for (int i = 0; i < data.myself.draw.Length; i++)
        {
            GameObject o = CreateCard(data.myself.draw[i],0,false);
            o.transform.position = Myself.DeckPosition;
            Myself.Hand.Add(o);
        }
        for (int i = 0; i < data.rival.draw.Length; i++)
        {
            GameObject o = CreateCard(data.rival.draw[i],0,false);
            o.transform.position = Rival.DeckPosition;
            Rival.Hand.Add(o);
        }
        SetHandCount(Myself.Hand.Count, Rival.Hand.Count);
        for (int i = 0; i < Myself.Hand.Count; i++)
        {
            MyHandSelectors[i].Card = Myself.Hand[i];
            SetSortingGroupOrder(Myself.Hand[i], i + 101);
        }
        for (int i = 0; i < Rival.Hand.Count; i++)
        {
            SetSortingGroupOrder(Rival.Hand[i], i + 101);
        }

        //手札から戦場に移動

        const float move_time = 0.5f;
        Myself.Battle.transform.DOMove(Myself.BattlePosition, move_time);
        Rival.Battle.transform.DOMove(Rival.BattlePosition, move_time);
        yield return new WaitForSeconds(move_time);

        SetSortingGroupOrder(Myself.Battle, 10);
        SetSortingGroupOrder(Rival.Battle, 10);


        //戦闘結果をシミュレートするためのカードデータ
        CardData myBattleData = Myself.Battle.GetComponent<Card>().CardData;
        CardData rivalBattleData = Rival.Battle.GetComponent<Card>().CardData;

//演出用の戦闘体
        Myself.BattleAvatar.Appearance(Myself.BattlePosition, myBattleData);
        Rival.BattleAvatar.Appearance(Rival.BattlePosition, rivalBattleData);

//サポートエフェクト
        if (Myself.Support != null)
        {
            CardData mySupportData = Myself.Support.GetComponent<Card>().CardData;
            int c = CardData.Chemistry(myBattleData.Element, mySupportData.Element);
            if (c > 0)
            {
                Myself.BattleAvatar.Raise();
                Myself.SupportArrow.StartAnimationPlus();
            }
            else if (c < 0)
            {
                Myself.BattleAvatar.Reduce();
                Myself.SupportArrow.StartAnimationMinus();
            }
            CardData rivalSupportData = Rival.Support.GetComponent<Card>().CardData;
            c = CardData.Chemistry(rivalBattleData.Element, rivalSupportData.Element);
            if (c > 0)
            {
                Rival.BattleAvatar.Raise();
                Rival.SupportArrow.StartAnimationPlus();
            }
            else if (c < 0)
            {
                Rival.BattleAvatar.Reduce();
                Rival.SupportArrow.StartAnimationMinus();
            }
            yield return new WaitForSeconds(0.5f);
        }
//バトル相性エフェクト
        {
            int c1 = CardData.Chemistry(myBattleData.Element, rivalBattleData.Element);
            int c2 = CardData.Chemistry(rivalBattleData.Element, myBattleData.Element);
            if (c1 > 0)
            {
                Myself.BattleAvatar.Raise();
                Myself.BattleArrow.StartAnimationPlus();
            }
            else if (c1 < 0)
            {
                Myself.BattleAvatar.Reduce();
                Myself.BattleArrow.StartAnimationMinus();
            }

            if (c2 > 0)
            {
                Rival.BattleAvatar.Raise();
                Rival.BattleArrow.StartAnimationPlus();
            }
            else if (c2 < 0)
            {
                Rival.BattleAvatar.Reduce();
                Rival.BattleArrow.StartAnimationMinus();
            }
        }
        yield return new WaitForSeconds(0.5f);

//最終値でなんか戦闘の勝敗演出
        const float result_time = 1f;


        if (data.damage < 0)
        {
            Myself.BattleAvatar.Attack();
            Rival.BattleAvatar.Damage();
            Rival.Avatar.ChangeExpression(PlayerAvatar.Expression.痛み);

            Myself.Avatar.Speak(PlayerAvatar.SpeakOn.Attack);
            Rival.Avatar.Speak(PlayerAvatar.SpeakOn.Damage);
        }
        else if (data.damage > 0)
        {
            Rival.BattleAvatar.Attack();
            Myself.BattleAvatar.Damage();
            Myself.Avatar.ChangeExpression(PlayerAvatar.Expression.痛み);

            Rival.Avatar.Speak(PlayerAvatar.SpeakOn.Attack);
            Myself.Avatar.Speak(PlayerAvatar.SpeakOn.Damage);
        }
        else if (data.damage == 0)
        {
            Myself.BattleAvatar.Attack();
            Rival.BattleAvatar.Attack();
            Myself.Avatar.Speak(PlayerAvatar.SpeakOn.Offset);
            Rival.Avatar.Speak(PlayerAvatar.SpeakOn.Offset);
            Myself.Avatar.ChangeExpression(PlayerAvatar.Expression.驚き);
            Rival.Avatar.ChangeExpression(PlayerAvatar.Expression.驚き);
        }

        yield return new WaitForSeconds(result_time);


//ゲームの勝敗が決まった
        if (data.phase < 0)
        {
            int mylife = data.myself.deckcount + Myself.Hand.Count - System.Convert.ToInt32(data.damage > 0);
            int rivallife = data.rival.deckcount + Rival.Hand.Count - System.Convert.ToInt32(data.damage < 0);
            if (mylife > rivallife)
            {
                Message.text = "Win";
                Myself.Avatar.Speak(PlayerAvatar.SpeakOn.Win);
                Myself.Avatar.ChangeExpression(PlayerAvatar.Expression.喜び);
            }
            else if (mylife < rivallife)
            {
                Message.text = "Lose";
                Rival.Avatar.Speak(PlayerAvatar.SpeakOn.Win);
                Rival.Avatar.ChangeExpression(PlayerAvatar.Expression.喜び);
            }
            else
            {
                Message.text = "Draw";
                Myself.Avatar.ChangeExpression(PlayerAvatar.Expression.閉じ);
                Rival.Avatar.ChangeExpression(PlayerAvatar.Expression.閉じ);
            }
            FrontCanvas.SetActive(true);
            Phase = data.phase;
            InEffect = false;

            Server.Terminalize();
            Server = null;
            yield break;
        }

        //対戦後演出
        const float after_time = 0.5f;

        if ((data.phase & 1) == 0)
        {
            Myself.Battle.transform.DOMove(Myself.UsedPosition, after_time);
            Rival.Battle.transform.DOMove(Rival.UsedPosition, after_time);
        }
        for (int i = 0; i < Myself.Hand.Count; i++)
        {
            Myself.Hand[i].SetActive(true);
            Myself.Hand[i].transform.DOMove(MyHandSelectors[i].transform.position, after_time);
        }
        for (int i = 0; i < Rival.Hand.Count; i++)
        {
            Rival.Hand[i].SetActive(true);
            Rival.Hand[i].transform.DOMove(RivalHandCheckers[i].transform.position, after_time);
        }
        Myself.DeckCount.text = data.myself.deckcount.ToString();
        Rival.DeckCount.text = data.rival.deckcount.ToString();

        yield return new WaitForSeconds(after_time);


        //フェイズ移行処理
        InEffect = false;
        if ((data.phase & 1) == 0)
        {
            RoundText.text = $"Round {Phase / 2 + 1}\nBattle";
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
            SetSortingGroupOrder(Myself.Battle, 0);
            SetSortingGroupOrder(Rival.Battle, 0);
            if (Myself.Support != null) Myself.Support.SetActive(false);
            if (Rival.Support != null) Rival.Support.SetActive(false);

            Myself.Avatar.ChangeExpression(PlayerAvatar.Expression.普通);
            Rival.Avatar.ChangeExpression(PlayerAvatar.Expression.普通);

            TimeBar.SetActive(BattleTimeLimit);
        }
        else
        {
            RoundText.text = $"Round {Phase / 2 + 1}\nDamage";
            if (data.damage <= 0)
            {
                Rival.HandBackImage.color = new Color(1, 0, 0, 100f / 256f);
                DecideCard(-1);
                yield break;
            }
            Myself.HandBackImage.color = new Color(1, 0, 0, 100f / 256f);
            TimeBar.SetActive(DamageTimeLimit);
        }
        PhaseStartTime = Time.realtimeSinceStartup;
    }

    public IEnumerator DamageEffect(UpdateData data)
    {
        GameObject DeleteObject = null;
        if (data.damage > 0)
        {
            SetHandCount(Myself.Hand.Count - 1, Rival.Hand.Count);

            DeleteObject = Myself.Hand[data.myself.select];
            Myself.Hand.RemoveAt(data.myself.select);
            Myself.Damage.Add(DeleteObject);
            DeleteObject.transform.SetParent(MyDamage.transform);

            DeleteObject.transform.DOMove(Myself.DamagePosition, 0.5f);
            for (int i = 0; i < Myself.Hand.Count; i++)
            {
                Myself.Hand[i].transform.DOMove(MyHandSelectors[i].transform.position, 0.5f);
                MyHandSelectors[i].Card = Myself.Hand[i];
            }
            Myself.Avatar.Speak(PlayerAvatar.SpeakOn.Recover);
        }
        else if (data.damage < 0)
        {
            SetHandCount(Myself.Hand.Count, Rival.Hand.Count - 1);

            DeleteObject = Rival.Hand[data.rival.select];
            Rival.Hand.RemoveAt(data.rival.select);
            Rival.Damage.Add(DeleteObject);
            DeleteObject.transform.SetParent(RivalDamage.transform);


            DeleteObject.transform.DOMove(Rival.DamagePosition, 0.5f);
            for (int i = 0; i < Rival.Hand.Count; i++)
                Rival.Hand[i].transform.DOMove(RivalHandCheckers[i].transform.position, 0.5f);

            Rival.Avatar.Speak(PlayerAvatar.SpeakOn.Recover);
        }

        //
        Myself.Battle.transform.DOMove(Myself.UsedPosition, 0.5f);
        Rival.Battle.transform.DOMove(Rival.UsedPosition, 0.5f);

        yield return new WaitForSeconds(0.5f);

        Rival.Avatar.ChangeExpression(PlayerAvatar.Expression.普通);
        Myself.Avatar.ChangeExpression(PlayerAvatar.Expression.普通);

        SetSortingGroupOrder(Myself.Battle, 0);
        SetSortingGroupOrder(Rival.Battle, 0);
        if (Myself.Support != null) Myself.Support.SetActive(false);
        if (Rival.Support != null) Rival.Support.SetActive(false);

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
        Myself.HandBackImage.color = new Color(1, 1, 1, 100f / 256f);
        Rival.HandBackImage.color = new Color(1, 1, 1, 100f / 256f);

        RoundText.text = $"Round {Phase / 2 + 1}\nBattle";

        InEffect = false;
        PhaseStartTime = Time.realtimeSinceStartup;
        TimeBar.SetActive(BattleTimeLimit);
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
/*
            for (int i = 0; i < Myself.Hand.Count; i++)
            {
                Image image = MyHandSelectors[i].gameObject.GetComponent<Image>();
                image.color = index == i ? new Color(1, 1, 0, 0.5f) : Color.clear;
            }
*/
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

    public void ClickMyUsed()
    {
        if (!InEffect && Myself.Used.Count > 0)
            CardListView.Open(Myself.Used.ToArray());
    }
    public void ClickRivalUsed()
    {
        if (!InEffect && Rival.Used.Count > 0)
            CardListView.Open(Rival.Used.ToArray());
    }

    public void ClickMyDamage()
    {
        if (!InEffect && Myself.Damage.Count > 0)
            CardListView.Open(Myself.Damage.ToArray());
    }
    public void ClickRivalDamage()
    {
        if (!InEffect && Rival.Damage.Count > 0)
            CardListView.Open(Rival.Damage.ToArray());
    }

    public void DecideCard(int index)
    {
        if (InEffect)
            return;

        if (Phase < 0)
            return;

        InEffect = true;
        PhaseStartTime = -1;
        TimeBar.gameObject.SetActive(false);



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
        TimeBar.gameObject.SetActive(false);

        if ((Phase & 1) == 0)
        {
            Phase = data.phase;
            EffectCoroutin = StartCoroutine(BattleEffect(data));
        }
        else
        {
            Phase = data.phase;
            EffectCoroutin = StartCoroutine(DamageEffect(data));
        }
    }

    public void SettingsOpen()
    {
        Settings.Open(Surrender);
    }
    private void Surrender()
    {
        Server?.SendSurrender();
    }

}
