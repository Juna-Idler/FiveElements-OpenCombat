using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class GameProcessor
{
    public static readonly System.Random random = new System.Random();

    public class PlayerData
    {
        public List<CardData> hand = new List<CardData>(5);
        public LinkedList<CardData> deck;
        public List<CardData> used = new List<CardData>(20);
        public List<CardData> damage = new List<CardData>(10);

        public int select = -1;
        public List<CardData> draw = new List<CardData>(2);

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
            DrawCard(4);
        }
        public void DrawCard(int count)
        {
            draw.Clear();
            for (int i = 0; i < count; i++)
            {
                if (deck.Count > 0)
                {
                    hand.Add(deck.Last.Value);
                    draw.Add(deck.Last.Value);
                    deck.RemoveLast();
                }
            }
        }
    }
    public int Phase { get; private set; }
    public int BattleDamage { get; private set; } //BattlePhaseでダメージが発生した（+:Player1にダメージ -:Player2にダメージ）
    public PlayerData Player1 { get; private set; }
    public PlayerData Player2 { get; private set; }


    public GameProcessor()
    {
        Initialize();
    }
    public void Initialize()
    {
        Phase = 0;
        BattleDamage = 0;
        Player1 = new PlayerData();
        Player2 = new PlayerData();
    }


    public void Decide(int index1, int index2)
    {
        if (Phase < 0)
            return;

        index1 = System.Math.Min(System.Math.Max(0, index1), Player1.hand.Count - 1);
        index2 = System.Math.Min(System.Math.Max(0, index2), Player2.hand.Count - 1);

        if ((Phase & 1) == 1)
        {
            if (BattleDamage > 0)
            {
                Player1.damage.Add(Player1.hand[index1]);
                Player1.hand.RemoveAt(index1);
                Player1.select = index1;
            }
            else if (BattleDamage < 0)
            {
                Player2.damage.Add(Player2.hand[index2]);
                Player2.hand.RemoveAt(index2);
                Player2.select = index2;
            }
            Phase++;

            Player1.DrawCard(0);
            Player2.DrawCard(0);
        }
        else
        {
            Battle(index1, index2);
        }
    }


    private void Battle(int index1, int index2)
    {
        CardData battle1 = Player1.hand[index1];
        CardData battle2 = Player2.hand[index2];

        Player1.select = index1;
        Player2.select = index2;

        Player1.hand.RemoveAt(index1);
        Player2.hand.RemoveAt(index2);

        CardData support1 = Player1.used.LastOrDefault();
        CardData support2 = Player2.used.LastOrDefault();

        int battleresult = CardData.Judge(battle1, battle2, support1, support2);

        int life1 = Player1.hand.Count + Player1.deck.Count - System.Convert.ToInt32(battleresult < 0);
        int life2 = Player2.hand.Count + Player2.deck.Count - System.Convert.ToInt32(battleresult > 0);
        if (life1 <= 0 || life2 <= 0)   //決着がつく場合
        {
            Phase = -1;
            BattleDamage = -battleresult;
            Player1.DrawCard(0);
            Player2.DrawCard(0);
            return;
        }

        Player1.used.Add(battle1);
        Player2.used.Add(battle2);



        if (battleresult > 0)
        {
            Player1.DrawCard(1);
            Player2.DrawCard(2);
            BattleDamage = -1;
        }
        else if (battleresult < 0)
        {
            Player1.DrawCard(2);
            Player2.DrawCard(1);
            BattleDamage = 1;
        }
        else
        {
            Player1.DrawCard(1);
            Player2.DrawCard(1);
            BattleDamage = 0;
        }
        Phase += 1 + ((battleresult == 0) ? 1 : 0);
    }

}
