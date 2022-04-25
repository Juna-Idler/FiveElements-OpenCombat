using System.Collections;
using System.Collections.Generic;

using System.Threading.Tasks;

public interface IGameServer
{
    public InitialData GetInitialData();

    delegate void SendSelectCallback(UpdateData data);
    public void SendSelect(int phase,int index,SendSelectCallback callback);

    void Terminalize();
}

