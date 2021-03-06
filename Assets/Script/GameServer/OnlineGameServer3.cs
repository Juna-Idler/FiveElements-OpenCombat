using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using System.Threading.Tasks;

public class OnlineGameServer3 : IGameServer
{

    [Serializable]
    private class InitialReceiveData
    {
        [Serializable]
        public class PlayerData
        {
            public string name;
        }
        public PlayerData y;
        public PlayerData r;
        public int battletime;
        public int damagetime;

        public string abort;
    }


    [Serializable]
    private class UpdateReceiveData
    {
        public int p;
        public int d;

        [Serializable]
        public class PlayerData
        {
            public int[] d;
            public int s;
            public int c;

            public UpdateData.PlayerData ToPlayerData()
            {
                return new UpdateData.PlayerData { draw = d, select = s, deckcount = c };
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

    private NativeWebSocket.WebSocket ws = null;

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
        CancelAction = () =>
        {
            Debug.Log("Connect Cancel");
            Terminalize();
            tcs.SetResult(false);
        };
        ws = new NativeWebSocket.WebSocket(serveruri.AbsoluteUri);

        ws.OnOpen += async () =>
        {
            Debug.Log("WS OnOpen:");

            if (ws == null)
                return;

            byte[] joincommand = System.Text.Encoding.UTF8.GetBytes($@"{{""command"":""Join"",""playername"":""{playername}"",""version"":{CardCatalog.Version}}}");
            await ws.Send(joincommand);
            Debug.Log("WS OnOpen:Send FirstCommand");
        };

        ws.OnMessage += (byte[] data) =>
        {
            Debug.Log("WS OnMessage:");
            if (InitialData == null)
            {
                string json = System.Text.Encoding.UTF8.GetString(data);
                InitialReceiveData idata = JsonUtility.FromJson<InitialReceiveData>(json);

                if (!string.IsNullOrEmpty(idata.abort))
                {
                    tcs.SetResult(false);
                    return;
                }

                InitialData = new InitialData()
                {
                    battleSelectTimeLimitSecond = idata.battletime,
                    damageSelectTimeLimitSecond = idata.damagetime,
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
                Callback(udata.ToUpdateData(), udata.a);
            }
        };

        ws.OnError += (string errMsg) =>
        {
            Debug.Log("WS OnErr:" + errMsg);
        };

        ws.OnClose += (NativeWebSocket.WebSocketCloseCode code) =>
        {
            Debug.Log("WS OnClose: " + code.ToString());
            Terminalize();
        };

        Debug.Log("WS Connect");
        _ = ws.Connect();
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

    void IGameServer.SendReady()
    {
        string ready_command = $@"{{""command"":""Ready""}}";
        _ = ws.SendText(ready_command);
    }

    void IGameServer.SendSelect(int phase, int index)
    {
        if (ws == null)
            return;
        string select_command = $@"{{""command"":""Select"",""phase"":{phase},""index"":{index}}}";
        //        ws.Send(System.Text.Encoding.UTF8.GetBytes(select_command));
        _ = ws.SendText(select_command);
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
            _ = ws.Close();
            ws = null;
        }
    }
}
