using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

public class OnlineGameServer : IGameServer
{
    [Serializable]
    private  class CardData
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

    private ClientWebSocket Socket = null;
    private System.Threading.CancellationTokenSource Cancellation = null;

    public void Cancel()
    {
        if (Cancellation != null)
        {
            Cancellation.Cancel();
            Cancellation.Dispose();
            Cancellation = null;
        }
    }
        public async Task<bool> TryConnect(System.Uri serveruri,string playername)
    {
        Terminalize();
        Socket = new ClientWebSocket();
        Cancellation = new CancellationTokenSource();
        try
        {
            await Socket.ConnectAsync(serveruri, Cancellation.Token);
            if (Socket.State == WebSocketState.Open)
            {
                //            JsonUtility.ToJson()
                byte[] joincommand = System.Text.Encoding.UTF8.GetBytes($@"{{""command"":""Join"",""playername"":""{playername}""}}");
                await Socket.SendAsync(new System.ArraySegment<byte>(joincommand), WebSocketMessageType.Text, true, Cancellation.Token);

                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                Task<WebSocketReceiveResult> result = Socket.ReceiveAsync(buffer, Cancellation.Token);
                await result;
                string json = System.Text.Encoding.UTF8.GetString(buffer.Array);
                InitialReceiveData data = JsonUtility.FromJson<InitialReceiveData>(json);

                InitialData = new InitialData()
                {
                    battleSelectTimeLimitSecond = data.battletime,
                    damageSelectTimeLimitSecond = data.damagetime,
                    myhand = data.y.hand.Select(c => c.ToCardData()).ToArray(),
                    rivalhand = data.r.hand.Select(c => c.ToCardData()).ToArray(),
                    mydeckcount = data.y.deckcount,
                    rivaldeckcount = data.r.deckcount,
                    myname = data.y.name,
                    rivalname = data.r.name
                };
                Cancellation.Dispose();
                Cancellation = null;

                System.Threading.SynchronizationContext context = System.Threading.SynchronizationContext.Current;
                _ = Task.Run(async () =>
                {
                    ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                    while (true)
                    {
                        WebSocketReceiveResult result = await Socket.ReceiveAsync(buffer, CancellationToken.None);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
//閉じるって言ってるのにこれ要るのか？
                            await Socket.CloseAsync((WebSocketCloseStatus)result.CloseStatus, result.CloseStatusDescription, CancellationToken.None);
                            break;
                        }
//WebSocketのバッファ？が足りない場合はくっつけて読む必要があるのか
//                        result.EndOfMessage
                        string json = System.Text.Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                        UpdateReceiveData data = JsonUtility.FromJson<UpdateReceiveData>(json);

                        context.Post(_ => Callback(data.ToUpdateData(), data.a), null);
                    }
                });

                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        if (Cancellation != null)
        {
            Cancellation.Dispose();
            Cancellation = null;
        }

        Socket.Dispose();
        Socket = null;
        return false;
    }


    private InitialData InitialData;

    InitialData IGameServer.GetInitialData()
    {
        return (Socket != null) ? InitialData : null;
    }
    private IGameServer.UpdateCallback Callback;
    void IGameServer.SetUpdateCallback(IGameServer.UpdateCallback callback)
    {
        Callback = callback;
    }

    void IGameServer.SendSelect(int phase,int index)
    {
        if (Socket == null)
            return;
        string select_command = $@"{{""command"":""Select"",""phase"":{phase},""index"":{index}}}";
        ArraySegment<byte> buffer = new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(select_command));
        _ = Socket.SendAsync(buffer, WebSocketMessageType.Text, true,CancellationToken.None);
    }
    void IGameServer.SendSurrender()
    {
        if (Socket == null)
            return;
        string select_command = $@"{{""command"":""End"",""reason"":""Surrender"",""message"":"" ""}}";
        ArraySegment<byte> buffer = new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(select_command));
        _ = Socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);

    }

    void IGameServer.Terminalize() { Terminalize(); }
    public void Terminalize()
    {
        Callback = null;
        if (Cancellation != null)
        {
            Cancellation.Cancel();
            Cancellation.Dispose();
            Cancellation = null;
        }
        if (Socket != null)
        {
            Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            Socket.Dispose();
            Socket = null;
        }
    }
}
