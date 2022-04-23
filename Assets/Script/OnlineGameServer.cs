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
    private class ReceiveData
    {
        public int p;
        public int d;

        [Serializable]
        public class PlayerData
        {
            [Serializable]
            public class CardData
            {
                public int e;
                public int p;

                public global::CardData ToCardData() { return new global::CardData((global::CardData.FiveElements)e, p); }
            }
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

    private ClientWebSocket Socket;

    public async Task Initialize()
    {
        Socket = new ClientWebSocket();

        System.Uri uri = new System.Uri("ws://localhost:8080/");

        await Socket.ConnectAsync(uri,System.Threading.CancellationToken.None);

        if (Socket.State == WebSocketState.Open)
        {
            //            JsonUtility.ToJson()
            byte[] joincommand = System.Text.Encoding.UTF8.GetBytes("{\"command\":\"Join\"}");
            await Socket.SendAsync(new System.ArraySegment<byte>(joincommand), WebSocketMessageType.Text,true,System.Threading.CancellationToken.None);

            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
            Task<WebSocketReceiveResult> result = Socket.ReceiveAsync(buffer, System.Threading.CancellationToken.None);
            await result;
            string json = System.Text.Encoding.UTF8.GetString(buffer.Array);
            ReceiveData data = JsonUtility.FromJson<ReceiveData>(json);

            Data = data.ToUpdateData();
        }
    }


    private UpdateData Data;

     UpdateData IGameServer.GetInitialData()
    {
        return Data;
    }

    void IGameServer.SendSelect(int index, IGameServer.SendSelectCallback callback)
    {
        System.Threading.SynchronizationContext context = System.Threading.SynchronizationContext.Current;
        Task.Run(async () =>
        {
//            SendData send = new SendData { command = "Select", phase = Data.phase, index = index };
//            string select_command = JsonUtility.ToJson(send);
            string select_command = $@"{{""command"":""Select"",""phase"":{Data.phase},""index"":{index}}}";
            ArraySegment<byte> buffer = new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(select_command));
            await Socket.SendAsync(buffer, WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);

            buffer = new ArraySegment<byte>(new byte[1024]);
            WebSocketReceiveResult result = await Socket.ReceiveAsync(buffer,System.Threading.CancellationToken.None);
            string json = System.Text.Encoding.UTF8.GetString(buffer.Array);
            ReceiveData data = JsonUtility.FromJson<ReceiveData>(json);

            Data = data.ToUpdateData();

            context.Post(_ => callback(Data), null);
        });
    }

    void IGameServer.Terminalize()
    {
        Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        Socket = null;
    }
}
