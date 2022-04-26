using System.Collections;
using System.Collections.Generic;

using System.Threading.Tasks;

public interface IGameServer
{
    public InitialData GetInitialData();

    delegate void UpdateCallback(UpdateData data,AbortMessage abort);

    public void SetUpdateCallback(UpdateCallback callback);

    public void SendSelect(int phase,int index);

    public void SendSurrender();

    void Terminalize();
}

