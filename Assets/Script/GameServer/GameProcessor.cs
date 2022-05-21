using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class GameProcessor
{
//    public static readonly System.Random random = new();

    public class PlayerData
    {
        public List<int> hand = new(5);
        public LinkedList<int> deck;
        public List<int> used = new(25);
        public List<int> damage = new(15);

        public int select = -1;
        public List<int> draw = new(2);

        public PlayerData()
        {
            int[] array = new int[25];
            for (int i = 0; i < 5; i++)
            {
                array[i] = i + 1;
                array[5 + i] = i + 1;
                array[10 + i] = i + 1;
                array[15 + i] = i + 5 + 1;
                array[20 + i] = i + 10 + 1;
            }
            deck = new LinkedList<int>(array.OrderBy(x => UnityEngine.Random.Range(int.MinValue, int.MaxValue)));
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
        CardCatalog catalog = CardCatalog.Instance;
        CardData battle1 = catalog[Player1.hand[index1]];
        CardData battle2 = catalog[Player2.hand[index2]];

        Player1.select = index1;
        Player2.select = index2;

        Player1.hand.RemoveAt(index1);
        Player2.hand.RemoveAt(index2);

        CardData support1 = Player1.used.Count == 0 ? null : catalog[Player1.used[^1]];
        CardData support2 = Player2.used.Count == 0 ? null : catalog[Player2.used[^1]];

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

        Player1.used.Add(battle1.ID);
        Player2.used.Add(battle2.ID);



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
