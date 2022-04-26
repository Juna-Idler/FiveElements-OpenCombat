using System.Collections;
using System.Collections.Generic;

using System.Threading.Tasks;

public interface IGameServer
{
    delegate void UpdateCallback(UpdateData data,string abort);
    public void SetUpdateCallback(UpdateCallback callback);

    public InitialData GetInitialData();

    public void SendSelect(int phase,int index);

    public void SendSurrender();

    void Terminalize();
}

