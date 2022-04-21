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
        public ClientData.Phases p;
        public int d;

        [Serializable]
        public class PlayerData
        {
            [Serializable]
            public class CardData
            {
                public int e;
                public int p;

                public global::CardData DataClassCardData() { return new global::CardData((global::CardData.FiveElements)e, p); }
            }
            public CardData[] d;
            public int s;
            public int c;
        }
        public PlayerData y;
        public PlayerData r;
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

            Data = new ClientData()
            {
                phase = data.p,
                damage = data.d,
                myself = new ClientData.PlayerData
                {
                    hand = data.y.d.Select(h=>new global::CardData((global::CardData.FiveElements)h.e,h.p)).ToArray(),
                    used = Array.Empty<CardData>(),
                    damage = Array.Empty<CardData>(),
                    decknum = data.y.c,
                    select = -1,
                    drawcount = 0
                },
                rival = new ClientData.PlayerData
                {
                    hand = data.r.d.Select(h => new global::CardData((global::CardData.FiveElements)h.e, h.p)).ToArray(),
                    used = Array.Empty<CardData>(),
                    damage = Array.Empty<CardData>(),
                    decknum = data.r.c,
                    select = -1,
                    drawcount = 0
                }
            };
        }
    }


    private ClientData Data;

    ClientData IGameServer.GetData()
    {
        return Data;
    }

    void IGameServer.SendSelect(int index, IGameServer.SendSelectCallback callback)
    {
        System.Threading.SynchronizationContext context = System.Threading.SynchronizationContext.Current;
        Task.Run(async () =>
        {
            string select_command = "{\"command\":\"Select\",\"index\":"+ index + "}";
            ArraySegment<byte> buffer = new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(select_command));
            await Socket.SendAsync(buffer, WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);

            buffer = new ArraySegment<byte>(new byte[1024]);
            WebSocketReceiveResult result = await Socket.ReceiveAsync(buffer,System.Threading.CancellationToken.None);
            string json = System.Text.Encoding.UTF8.GetString(buffer.Array);
            ReceiveData data = JsonUtility.FromJson<ReceiveData>(json);

            Data = Update(Data, data);

            context.Post(_ => callback(Data), null);
        });
    }

    static ClientData Update(ClientData data,ReceiveData receive)
    {
        return new ClientData()
        {
            myself = Update(data.myself, receive.y, data.phase, data.damage),
            rival = Update(data.rival, receive.r, data.phase, -data.damage),
            phase = receive.p,
            damage = receive.d
        };
    }

    static ClientData.PlayerData Update(ClientData.PlayerData data, ReceiveData.PlayerData receive,ClientData.Phases phase,int damage)
    {
        ClientData.PlayerData r = new ClientData.PlayerData();

        r.select = receive.s;
        r.hand = data.hand;
        r.used = data.used;
        r.damage = data.damage;
        r.decknum = receive.c;
        r.drawcount = receive.d.Length;

        List<CardData> cards = new List<CardData>(20);
        CardData c = data.hand[receive.s];
        if (phase == ClientData.Phases.BattlePhase)
        {
            cards.AddRange(data.hand);
            cards.RemoveAt(receive.s);
            cards.AddRange(receive.d.Select(h => new global::CardData((global::CardData.FiveElements)h.e, h.p)));
            r.hand = cards.ToArray();
            cards.Clear();

            cards.AddRange(data.used);
            cards.Add(c);
            r.used = cards.ToArray();
            cards.Clear();
        }
        else if (phase == ClientData.Phases.DamagePhase && damage > 0)
        {
            cards.AddRange(data.hand);
            cards.RemoveAt(receive.s);
            r.hand = cards.ToArray();
            cards.Clear();

            cards.AddRange(data.damage);
            cards.Add(c);
            r.damage = cards.ToArray();
            cards.Clear();
        }
        return r;
    }


}
