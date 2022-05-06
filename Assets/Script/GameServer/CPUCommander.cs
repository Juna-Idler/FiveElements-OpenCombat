using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICPUCommander
{
    string Name { get; }

    int FirstSelect(int[] myhand, int[] rivalhand);

    int BattleSelect(UpdateData data);

    int DamageSelect(UpdateData data);


    public class Information
    {
        public class Player
        {
            public List<int> Hand = new(5);
            public List<int> Used = new(20);
            public List<int> Damage = new(10);
        }
        public Player Myself = new Player();
        public Player Rival = new Player();

        public Information(int[] myhand,int[] rivalhand)
        {
            Myself.Hand.AddRange(myhand);
            Rival.Hand.AddRange(rivalhand);
        }

        public void Update(UpdateData data)
        {
            if (data.phase < 0)
                return;
            if ((data.phase & 1) == 0)
            {
                if (data.damage == 0)
                {
                    Myself.Used.Add(Myself.Hand[data.myself.select]);
                    Myself.Hand.RemoveAt(data.myself.select);
                    Rival.Used.Add(Rival.Hand[data.rival.select]);
                    Rival.Hand.RemoveAt(data.rival.select);
                }
                else
                {
                    if (data.damage > 0)
                    {
                        Myself.Damage.Add(Myself.Hand[data.myself.select]);
                        Myself.Hand.RemoveAt(data.myself.select);
                    }
                    else if (data.damage < 0)
                    {
                        Rival.Damage.Add(Rival.Hand[data.rival.select]);
                        Rival.Hand.RemoveAt(data.rival.select);
                    }
                }
            }
            else
            {
                Myself.Used.Add(Myself.Hand[data.myself.select]);
                Myself.Hand.RemoveAt(data.myself.select);
                Rival.Used.Add(Rival.Hand[data.rival.select]);
                Rival.Hand.RemoveAt(data.rival.select);
            }
            Myself.Hand.AddRange(data.myself.draw);
            Rival.Hand.AddRange(data.rival.draw);
        }
    }
}