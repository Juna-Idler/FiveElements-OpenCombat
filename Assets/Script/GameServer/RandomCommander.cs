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

    Task<int> ICPUCommander.FirstSelect(int[] myhand, int[] rivalhand)
    {
        Information = new ICPUCommander.Information(myhand,rivalhand);

        return Task.FromResult(random.Next(0, Information.Myself.Hand.Count));
    }

    Task<int> ICPUCommander.Select(UpdateData data)
    {
        Information.Update(data);
        int index2 = 0;
        if ((data.phase & 1) == 1 && data.damage > 0)
        {
            int min = 256;
            for (int i = 0; i <  Information.Myself.Hand.Count; i++)
            {
                int p = CardCatalog.Get(Information.Myself.Hand[i]).Power;
                if (p < min)
                {
                    min = p;
                    index2 = i;
                }
            }
            return Task.FromResult(index2);
        }
        else
        {
            return Task.FromResult(random.Next(0, Information.Myself.Hand.Count));
        }
    }

}

