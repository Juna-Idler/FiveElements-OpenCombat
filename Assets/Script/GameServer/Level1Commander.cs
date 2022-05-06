using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class Level1Commander : ICPUCommander
{
    string ICPUCommander.Name { get; } = "CPU(Level1)";

    private ICPUCommander.Information Information;

    private TaskCompletionSource<int> tcs;

    int ICPUCommander.FirstSelect(int[] myhand, int[] rivalhand)
    {
        Information = new ICPUCommander.Information(myhand, rivalhand);
        tcs = new TaskCompletionSource<int>();


        return BattleSelect();
    }
    int ICPUCommander.BattleSelect(UpdateData data)
    {
        Information.Update(data);
        return BattleSelect();
    }

    int ICPUCommander.DamageSelect(UpdateData data)
    {
        Information.Update(data);
        if (data.damage > 0)
            return DamageeSelect();
        return -1;
    }

    private int BattleSelect()
    {
        CardData mysupport = (Information.Myself.Used.Count > 0) ? CardCatalog.Get(Information.Myself.Used[^1]) : null;
        CardData rivalsupport = (Information.Rival.Used.Count > 0) ? CardCatalog.Get(Information.Rival.Used[^1]) : null;

        int max = -256;
        int index = 0;

        for (int i = 0; i < Information.Myself.Hand.Count; i++)
        {
            int probability = 0;
            CardData mycd = CardCatalog.Get(Information.Myself.Hand[i]);
            for (int j = 0; j < Information.Rival.Hand.Count; j++)
            {
                CardData cd = CardCatalog.Get(Information.Rival.Hand[j]);
                int r = CardData.Judge(mycd, cd, mysupport, rivalsupport);
                probability += (r > 0 ? 1 : 0) + (r < 0 ? -1 : 0);
            }
            if (probability > max)
            {
                max = probability;
                index = i;
            }
        }

        return index;
    }

    private int DamageeSelect()
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


}
