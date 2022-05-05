public interface IGameServer
{
    delegate void UpdateCallback(UpdateData data,string abort);
    void SetUpdateCallback(UpdateCallback callback);

    InitialData GetInitialData();

    void SendSelect(int phase,int index);

    void SendSurrender();

    void Terminalize();
}

