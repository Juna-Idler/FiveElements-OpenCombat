using System.Collections;
using System.Collections.Generic;


public class UpdateData
{
    public int phase;
    public int damage;

    public class PlayerData
    {
        public CardData[] draw;
        public int select;
        public int deckcount;
    }
    public PlayerData myself;
    public PlayerData rival;
}

public class InitialData
{
    public int battleSelectTimeLimitSecond;
    public int damageSelectTimeLimitSecond;

    public string myname;
    public string rivalname;

    public CardData[] myhand;
    public CardData[] rivalhand;
    public int mydeckcount;
    public int rivaldeckcount;
}

public class AbortMessage
{
    public string reason;
    public int game;
}


public class CardData
{
    public enum FiveElements
    {
        木,
        火,
        土,
        金,
        水
    }

    public FiveElements Element;
    public int Power;

    public CardData(FiveElements elementt,int power)
    {
        Element = elementt;
        Power = power;
    }


    public static int Judge(CardData a_battle, CardData b_battle, CardData a_support = null, CardData b_support = null)
    {
        int a_supportpower = (a_support != null ? Chemistry(a_battle.Element, a_support.Element) : 0);
        int a_power = a_battle.Power + a_supportpower + Chemistry(a_battle.Element, b_battle.Element);
        int b_supportpower = (b_support != null ? Chemistry(b_battle.Element, b_support.Element) : 0);
        int b_power = b_battle.Power + b_supportpower + Chemistry(b_battle.Element, a_battle.Element);

        return a_power - b_power;
    }

    public static readonly int[] table = new int[]{
            1, 0, 0,-1, 1,
            1, 1, 0, 0,-1,
           -1, 1, 1, 0, 0,
            0,-1, 1, 1, 0,
            0, 0,-1, 1, 1
        };
    public static int Chemistry(FiveElements dest, FiveElements src)
    {
        return table[((int)dest) * 5 + ((int)src)];
    }


}



