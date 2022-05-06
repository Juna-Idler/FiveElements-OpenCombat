using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class RandomCommander : ICPUCommander
{
    public static readonly System.Random random = new();


    private ICPUCommander.Information Information;



    string ICPUCommander.Name { get; } = "CPU(Random)";

    int ICPUCommander.FirstSelect(int[] myhand, int[] rivalhand)
    {
        Information = new ICPUCommander.Information(myhand,rivalhand);

        return random.Next(0, Information.Myself.Hand.Count);
    }

    int ICPUCommander.BattleSelect(UpdateData data)
    {
        Information.Update(data);
        return random.Next(0, Information.Myself.Hand.Count);
    }

    int ICPUCommander.DamageSelect(UpdateData data)
    {
        Information.Update(data);
        if (data.damage > 0)
        {
            int index2 = 0;
            int min = 256;
            for (int i = 0; i < Information.Myself.Hand.Count; i++)
            {
                int p = CardCatalog.Get(Information.Myself.Hand[i]).Power;
                if (p < min)
                {
                    min = p;
                    index2 = i;
                }
            }
            return index2;
        }
        return -1;
    }

}

