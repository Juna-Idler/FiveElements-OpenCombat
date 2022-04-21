using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class OfflineGameServer : IGameServer
{
    private static readonly System.Random random = new System.Random();

    public class PlayerData
    {
        public List<CardData> hand = new List<CardData>(5);
        public LinkedList<CardData> deck;
        public List<CardData> used = new List<CardData>(20);
        public List<CardData> damage = new List<CardData>(10);

        public PlayerData()
        {
            CardData[] array = new CardData[20];
            int index = 0;
            foreach (CardData.FiveElements e in System.Enum.GetValues(typeof(CardData.FiveElements)))
            {
                for (int i = 1; i < 3; i++)
                {
                    array[index++] = new CardData(e, i);
                    array[index++] = new CardData(e, i);
                }
            }
            deck = new LinkedList<CardData>(array.OrderBy(x => random.Next()));
            for (int i = 0; i < 4; i++)
            {
                DrawCard();
            }
        }
        public int DrawCard()
        {
            if (deck.Count > 0)
            {
                hand.Add(deck.Last.Value);
                deck.RemoveLast();
                return 1;
            }
            return 0;
        }
        public ClientData.PlayerData CreateClientPlayerData()
        {
            return new ClientData.PlayerData
            {
                hand = hand.ToArray(),
                used = used.ToArray(),
                damage = damage.ToArray(),
                decknum = deck.Count
            };
        }
    }
    private ClientData.Phases Phase;
    private int BattleDamage; //BattlePhaseでダメージが発生した（+:Player1にダメージ -:Player2にダメージ）
    private PlayerData Player1;
    private PlayerData Player2;

    public OfflineGameServer()
    {
        Initialize();
    }
    public void Initialize()
    {
        Phase = ClientData.Phases.BattlePhase;
        Player1 = new PlayerData();
        Player2 = new PlayerData();

        Data = new ClientData();
    }

    private ClientData Data;

    ClientData IGameServer.GetData()
    {
        Data.myself = Player1.CreateClientPlayerData();
        Data.rival = Player2.CreateClientPlayerData();

        Data.myselect = 0;
        Data.rivalselect = 0;
        Data.damage = 0;
        Data.mydraw = 0;

        return Data;
    }

    void IGameServer.SendSelect(int index,IGameServer.SendSelectCallback callback)
    {
        index = System.Math.Min(System.Math.Max(0, index), Player1.hand.Count - 1);

        switch (Phase)
        {
            case ClientData.Phases.BattlePhase:
                Battle(index, random.Next(0, Player2.hand.Count));
                break;
            case ClientData.Phases.DamagePhase:
                Damage(index);
                break;
            case ClientData.Phases.GameEnd:
                break;
        }

        callback(Data);
    }

    void Battle(int index1, int index2)
    {
        CardData battle1 = Player1.hand[index1];
        CardData battle2 = Player2.hand[index2];

        Player1.hand.RemoveAt(index1);
        Player2.hand.RemoveAt(index2);

        Data.myselect = index1;
        Data.rivalselect = index2;

        CardData support1 = Player1.used.LastOrDefault();
        CardData support2 = Player2.used.LastOrDefault();

        int battleresult = CardData.Judge(battle1, battle2, support1, support2);

        int life1 = Player1.hand.Count + Player1.deck.Count - System.Convert.ToInt32(battleresult < 0);
        int life2 = Player2.hand.Count + Player2.deck.Count - System.Convert.ToInt32(battleresult > 0);
        if (life1 <= 0 || life2 <= 0)   //決着がつく場合
        {
            Phase = ClientData.Phases.GameEnd;

            Data.phase = Phase;
            Data.damage = System.Convert.ToInt32(battleresult < 0) - System.Convert.ToInt32(battleresult > 0);
            Data.mydraw = Data.rivaldraw = 0;

            Data.myself = Player1.CreateClientPlayerData();
            Data.rival = Player2.CreateClientPlayerData();

            return;
        }

        Player1.used.Add(battle1);
        Player2.used.Add(battle2);

        Data.mydraw = Player1.DrawCard();
        Data.rivaldraw = Player2.DrawCard();

        Phase = ClientData.Phases.DamagePhase;

        if (battleresult > 0)
        {
            Data.rivaldraw += Player2.DrawCard();
            BattleDamage = -1;
        }
        else if (battleresult < 0)
        {
            Data.mydraw += Player1.DrawCard();
            BattleDamage = 1;
        }
        else
        {
            Phase = ClientData.Phases.BattlePhase;
            BattleDamage = 0;
        }
        Data.phase = Phase;
        Data.damage = BattleDamage;
        Data.myself = Player1.CreateClientPlayerData();
        Data.rival = Player2.CreateClientPlayerData();

    }


    void Damage(int index)
    {
        Data.myselect = Data.rivalselect = -1;

        if (BattleDamage > 0)
        {
            Player1.hand.RemoveAt(index);
            Data.myselect = index;
        }
        else if (BattleDamage < 0)
        {
            int min = 256, oindex = 0;
            for (int i = 0; i < Player2.hand.Count; i++)
            {
                if (Player2.hand[i].Power < min)
                {
                    min = Player2.hand[i].Power;
                    oindex = i;
                }
            }
            Player2.hand.RemoveAt(oindex);
            Data.rivalselect = oindex;
        }

        Phase = ClientData.Phases.BattlePhase;

        Data.phase = Phase;
        Data.damage = 0;
        Data.mydraw = Data.rivaldraw = 0;
        Data.myself = Player1.CreateClientPlayerData();
        Data.rival = Player2.CreateClientPlayerData();
    }
}
