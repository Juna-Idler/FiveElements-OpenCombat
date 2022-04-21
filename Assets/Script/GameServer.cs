using System.Collections;
using System.Collections.Generic;

using System.Threading.Tasks;

public interface IGameServer
{
    public ClientData GetData();

    delegate void SendSelectCallback(ClientData data);
    public void SendSelect(int index,SendSelectCallback callback);

}

