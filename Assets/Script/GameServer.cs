using System.Collections;
using System.Collections.Generic;

using System.Threading.Tasks;

public interface IGameServer
{
    public UpdateData GetInitialData();

    delegate void SendSelectCallback(UpdateData data);
    public void SendSelect(int index,SendSelectCallback callback);

    void Terminalize();
}

