using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

public class Level2Commander : ICPUCommander
{
    string ICPUCommander.Name { get; } = "CPU(Level2)";

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

    private class option
    {
        public int index;
        public int probability;
        public CardData cd;
    }

    private int BattleSelect()
    {
        CardData mysupport = (Information.Myself.Used.Count > 0) ? CardCatalog.Get(Information.Myself.Used[^1]) : null;
        CardData rivalsupport = (Information.Rival.Used.Count > 0) ? CardCatalog.Get(Information.Rival.Used[^1]) : null;

        List<option> opt = new List<option>();
        int max = -255;

        for (int i = 0; i < Information.Rival.Hand.Count; i++)
        {
            int probability = 0;
            CardData rcd = CardCatalog.Get(Information.Rival.Hand[i]);
            for (int j = 0; j < Information.Myself.Hand.Count; j++)
            {
                CardData cd = CardCatalog.Get(Information.Myself.Hand[j]);
                int r = CardData.Judge(rcd, cd, rivalsupport, mysupport);
                probability += (r > 0 ? 1 : 0) + (r < 0 ? -2 : 0);
            }
            if (probability > max)
            {
                opt.Clear();
                opt.Add(new option() { index = i, probability = probability, cd = rcd });
                max = probability;
            }
            else if (probability == max)
            {
                opt.Add(new option() { index = i, probability = probability, cd = rcd });
            }
        }
        option option = (opt.Count > 1) ? opt.OrderBy(o => o.cd.Power).First() : opt[0];
        if (option.probability == 4)
        {
            max = -255;
            int index = 0;
            for (int i = 0; i < Information.Myself.Hand.Count; i++)
            {
                int probability = 0;
                CardData now = CardCatalog.Get(Information.Myself.Hand[i]);
                for (int j = 0; j < Information.Myself.Hand.Count; j++)
                {
                    if (j == i)
                        continue;
                    CardData next = CardCatalog.Get(Information.Myself.Hand[j]);
                    int r = CardData.Chemistry(next.Element, now.Element);
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
        CardData target = option.cd;
        opt.Clear();
        max = -255;
        for (int i = 0; i < Information.Myself.Hand.Count; i++)
        {
            CardData cd = CardCatalog.Get(Information.Myself.Hand[i]);
            int r = CardData.Judge(cd, target, mysupport, rivalsupport);
            if (r > max)
            {
                opt.Clear();
                opt.Add(new option() { index = i, probability = r, cd = cd });
                max = r;
            }
            else if (r == max)
            {
                opt.Add(new option() { index = i, probability = r, cd = cd });
            }
        }
        option = (opt.Count > 1) ? opt.OrderBy(o => o.cd.Power).First() : opt[0];

        return option.index;
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
