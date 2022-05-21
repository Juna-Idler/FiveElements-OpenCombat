
//マッチングが完了した、ゲームのサーバ（審判、GM）とのアクセスインターフェイス

public interface IGameServer
{
    delegate void UpdateCallback(UpdateData data,string abort);
//サーバからの通信を受信するコールバック
    void SetUpdateCallback(UpdateCallback callback);

//初期データ（このゲームのルールパラメータとマッチング時に提出したお互いのデータ）
    InitialData GetInitialData();

//ゲーム開始準備完了を送信
//これ以後、サーバからゲーム進行のUpdateCallbackが呼び出される
    void SendReady();

//ゲームでの選択を送信
    void SendSelect(int phase,int index);

//即時ゲーム終了（降参）を送信
    void SendSurrender();

//このインターフェイスの破棄
    void Terminalize();
}

public class UpdateData
{
    public int phase;
    public int damage;

    public class PlayerData
    {
        public int[] draw;
        public int select;
        public int deckcount;
    }
    public PlayerData myself;
    public PlayerData rival;
}

public class InitialData
{
    public int battleSelectTimeLimitSecond;
    public int damageSelectTimeLimitSecond;

    public string myname;
    public string rivalname;


}
