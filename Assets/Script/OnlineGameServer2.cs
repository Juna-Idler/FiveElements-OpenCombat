using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using System.Threading.Tasks;


public class OnlineGameServer2 : IGameServer
{
    [Serializable]
    private class CardData
    {
        public int e;
        public int p;

        public global::CardData ToCardData() { return new global::CardData((global::CardData.FiveElements)e, p); }
    }

    [Serializable]
    private class InitialReceiveData
    {
        [Serializable]
        public class PlayerData
        {
            public string name;
            public CardData[] hand;
            public int deckcount;
        }
        public PlayerData y;
        public PlayerData r;
        public int battletime;
        public int damagetime;
    }


    [Serializable]
    private class UpdateReceiveData
    {
        public int p;
        public int d;

        [Serializable]
        public class PlayerData
        {
            public CardData[] d;
            public int s;
            public int c;

            public UpdateData.PlayerData ToPlayerData()
            {
                return new UpdateData.PlayerData { draw = d.Select(c => c.ToCardData()).ToArray(), select = s, deckcount = c };
            }
        }
        public PlayerData y;
        public PlayerData r;

        public string a;

        public UpdateData ToUpdateData()
        {
            return new UpdateData() { phase = p, damage = d, myself = y.ToPlayerData(), rival = r.ToPlayerData() };
        }
    }
    [Serializable]
    private class SendData
    {
        public string command = "Select";
        public int phase;
        public int index;
    }

    private HybridWebSocket.WebSocket ws = null;

    public void Cancel()
    {
        if (ws != null)
            CancelAction();
    }
    private Action CancelAction;
    public Task<bool> TryConnect(System.Uri serveruri, string playername)
    {
        Terminalize();
        var tcs = new TaskCompletionSource<bool>();
        CancelAction = () => { Debug.Log("Connect Cancel"); Terminalize(); tcs.SetResult(false); };
        ws = HybridWebSocket.WebSocketFactory.CreateInstance(serveruri.AbsoluteUri);

        ws.OnOpen += () =>
        {
            Debug.Log("WS OnOpen:");

            byte[] joincommand = System.Text.Encoding.UTF8.GetBytes($@"{{""command"":""Join"",""playername"":""{playername}""}}");

            ws.Send(joincommand);
        };

        System.Threading.SynchronizationContext context = System.Threading.SynchronizationContext.Current;
        ws.OnMessage += (byte[] data) =>
        {
            Debug.Log("WS OnMessage:");
            if (InitialData == null)
            {
                string json = System.Text.Encoding.UTF8.GetString(data);
                InitialReceiveData idata = JsonUtility.FromJson<InitialReceiveData>(json);

                InitialData = new InitialData()
                {
                    battleSelectTimeLimitSecond = idata.battletime,
                    damageSelectTimeLimitSecond = idata.damagetime,
                    myhand = idata.y.hand.Select(c => c.ToCardData()).ToArray(),
                    rivalhand = idata.r.hand.Select(c => c.ToCardData()).ToArray(),
                    mydeckcount = idata.y.deckcount,
                    rivaldeckcount = idata.r.deckcount,
                    myname = idata.y.name,
                    rivalname = idata.r.name
                };
                Debug.Log("InitialData:" + json);
                tcs.SetResult(true);
            }
            else
            {
                string json = System.Text.Encoding.UTF8.GetString(data);
                UpdateReceiveData udata = JsonUtility.FromJson<UpdateReceiveData>(json);

                Debug.Log("UpdateData:" + json);
                context.Post(_ => Callback(udata.ToUpdateData(), udata.a), null);
            }
        };

        ws.OnError += (string errMsg) =>
        {
            Debug.Log("WS OnErr:" + errMsg);
        };

        ws.OnClose += (HybridWebSocket.WebSocketCloseCode code) =>
        {
            Debug.Log("WS closed with code: " + code.ToString());
            Terminalize();
        };

        Debug.Log("WS Connect");
        ws.Connect();
        Debug.Log("WS Connecting... Wait");


        return tcs.Task;
    }

    private InitialData InitialData;

    InitialData IGameServer.GetInitialData()
    {
        return (ws != null) ? InitialData : null;
    }
    private IGameServer.UpdateCallback Callback;
    void IGameServer.SetUpdateCallback(IGameServer.UpdateCallback callback)
    {
        Callback = callback;
    }

    void IGameServer.SendSelect(int phase, int index)
    {
        if (ws == null)
            return;
        string select_command = $@"{{""command"":""Select"",""phase"":{phase},""index"":{index}}}";
        ws.Send(System.Text.Encoding.UTF8.GetBytes(select_command));
        Debug.Log("Send:" + select_command);
    }
    void IGameServer.SendSurrender()
    {
        if (ws == null)
            return;
        string surrender_command = $@"{{""command"":""End"",""reason"":""Surrender"",""message"":"" ""}}";

        ws.Send(System.Text.Encoding.UTF8.GetBytes(surrender_command));
    }

    void IGameServer.Terminalize() { Terminalize(); }
    public void Terminalize()
    {
        InitialData = null;
        Callback = null;
        if (ws != null)
        {
            ws.Close();
            ws = null;
        }
    }


}
