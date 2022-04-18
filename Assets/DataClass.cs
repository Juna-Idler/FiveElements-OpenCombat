using System.Collections;
using System.Collections.Generic;



public class ClientData
{
    public enum Phases { BattlePhase, DamagePhase, GameEndWin, GameEndLose, GameEndDraw }
    public class PlayerData
    {
        public CardData[] hand; //��D
        public int decknum;     //�f�b�L�̎c�薇��
        public CardData[] used; //�퓬�Ŏg�p�����J�[�h
        public CardData[] damage;  //�_���[�W�Ƃ��Ď̂Ă��J�[�h
    }

    public Phases phase;
//    public Phases lastphase;  //���O�̃t�F�C�Y

    public PlayerData myself;   //����
    public PlayerData rival; //����

    public int myselect;       //�����̑I�񂾎�D�̈ʒu
    public int rivalselect; //����̑I�񂾎�D�̈ʒu

    public int mydraw;    //�������J�[�h�̖����iHand�̌��̃J�[�h�j
    public int rivaldraw;

    public int damage;  //�퓬���̃_���[�W�l
}


public class CardData
{
    public enum FiveElements
    {
        ��,
        ��,
        �y,
        ��,
        ��
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



