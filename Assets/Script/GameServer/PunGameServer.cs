using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

using Hashtable = ExitGames.Client.Photon.Hashtable;


public class PunGameServer : IGameServer,
    IConnectionCallbacks,
    IMatchmakingCallbacks,
    IOnEventCallback
{


    [System.Serializable]
    private class InitialReceiveData
    {
        [System.Serializable]
        public class PlayerData
        {
        }
        public PlayerData y;
        public PlayerData r;
        public int battletime;
        public int damagetime;
    }


    [System.Serializable]
    private class UpdateReceiveData
    {
        public int p;
        public int d;

        [System.Serializable]
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
    [System.Serializable]
    private class SendData
    {
        public int phase;
        public int index;
    }



    private GameProcessor GameProcessor;
    private int BattleSelectTimeLimitSecond = 15;
    private int DamageSelectTimeLimitSecond = 10;

    private int Select1 = -1;
    private int Select2 = -1;


    private TaskCompletionSource<bool> tcs;
    public void Cancel()
    {
        Debug.Log("Connect Cancel");
        tcs.SetResult(false);
    }

    public Task<bool> TryConnect(string name)
    {
        Terminalize();
        PhotonNetwork.AddCallbackTarget(this);
        tcs = new TaskCompletionSource<bool>();

        if (!PhotonNetwork.ConnectUsingSettings())
        {
            tcs.SetResult(false);
            return tcs.Task;
        }
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LocalPlayer.NickName = name;
        }

        return tcs.Task;
    }


    #region IConnectionCallbacks
    void IConnectionCallbacks.OnConnected()
    {}

    void IConnectionCallbacks.OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomOrCreateRoom(roomOptions: new RoomOptions { MaxPlayers = 2 });
    }

    void IConnectionCallbacks.OnDisconnected(DisconnectCause cause)
    { }
    void IConnectionCallbacks.OnRegionListReceived(RegionHandler regionHandler)
    { }
    void IConnectionCallbacks.OnCustomAuthenticationResponse(Dictionary<string, object> data)
    { }
    void IConnectionCallbacks.OnCustomAuthenticationFailed(string debugMessage)
    {}

    #endregion

    #region IMatchmakingCallbacks
    void IMatchmakingCallbacks.OnFriendListUpdate(List<FriendInfo> friendList) { }

    void IMatchmakingCallbacks.OnCreatedRoom()
    {
    }

    void IMatchmakingCallbacks.OnCreateRoomFailed(short returnCode, string message)
    { }
 
    void IMatchmakingCallbacks.OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;

            BattleSelectTimeLimitSecond = 15;
            DamageSelectTimeLimitSecond = 10;

            InitialReceiveData.PlayerData p1 = new InitialReceiveData.PlayerData
            {
            };
            InitialReceiveData.PlayerData p2 = new InitialReceiveData.PlayerData
            {
            };

            InitialReceiveData p1initial = new InitialReceiveData()
            {
                battletime = BattleSelectTimeLimitSecond,
                damagetime = DamageSelectTimeLimitSecond,
                y = p1,
                r = p2
            };
            InitialReceiveData p2initial = new InitialReceiveData()
            {
                battletime = BattleSelectTimeLimitSecond,
                damagetime = DamageSelectTimeLimitSecond,
                y = p2,
                r = p1
            };

            int p1number = PhotonNetwork.MasterClient.ActorNumber;
            int p2number = PhotonNetwork.MasterClient.GetNext().ActorNumber;

            RaiseEventOptions options = new RaiseEventOptions() { TargetActors = new int[] { p1number } };
            PhotonNetwork.RaiseEvent(RaiseEvent_Matched, JsonUtility.ToJson(p1initial), options, ExitGames.Client.Photon.SendOptions.SendReliable);
            options.TargetActors = new int[] { p2number };
            PhotonNetwork.RaiseEvent(RaiseEvent_Matched, JsonUtility.ToJson(p2initial), options, ExitGames.Client.Photon.SendOptions.SendReliable);
        }
    }

    void IMatchmakingCallbacks.OnJoinRoomFailed (short returnCode, string message)
    { }

    void IMatchmakingCallbacks.OnJoinRandomFailed (short returnCode, string message)
    { }

    void IMatchmakingCallbacks.OnLeftRoom ()
    { }
    #endregion


    private const byte RaiseEvent_Matched = 1;
    private const byte RaiseEvent_Update = 3;

    private const byte RaiseEvent_Ready = 4;
    private const byte RaiseEvent_Select = 5;
    private const byte RaiseEvent_Surrender = 6;


    private InitialReceiveData initialReceiveData;


    void IOnEventCallback.OnEvent(ExitGames.Client.Photon.EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case RaiseEvent_Matched:
                {
                    initialReceiveData = JsonUtility.FromJson<InitialReceiveData>((string)photonEvent.CustomData);
                    tcs.SetResult(true);
                }
                break;

            case RaiseEvent_Update:
                {
                    UpdateReceiveData data = JsonUtility.FromJson<UpdateReceiveData>((string)photonEvent.CustomData);

                    Callback(new UpdateData()
                    {
                        phase = data.p,
                        damage = data.d,
                        myself = data.y.ToPlayerData(),
                        rival = data.r.ToPlayerData()
                    }, null);
                }
                break;
            case RaiseEvent_Ready:
                {
                    if (photonEvent.Sender == PhotonNetwork.MasterClient.ActorNumber)
                    {
                        Select1 = 0;
                    }
                    else
                    {
                        Select2 = 0;
                    }
                    if (Select1 >= 0 && Select2 >= 0)
                    {
                        GameProcessor = new GameProcessor();

                        BattleSelectTimeLimitSecond = 15;
                        DamageSelectTimeLimitSecond = 10;

                        UpdateReceiveData.PlayerData p1 = new UpdateReceiveData.PlayerData
                        {
                            s = -1,
                            d = GameProcessor.Player1.draw.ToArray(),
                            c = GameProcessor.Player1.deck.Count
                        };
                        UpdateReceiveData.PlayerData p2 = new UpdateReceiveData.PlayerData
                        {
                            s = -1,
                            d = GameProcessor.Player2.draw.ToArray(),
                            c = GameProcessor.Player2.deck.Count
                        };

                        UpdateReceiveData p1update = new UpdateReceiveData()
                        {
                            p = GameProcessor.Phase,
                            d = GameProcessor.BattleDamage,
                            y = p1,
                            r = p2,
                            a = null,
                        };
                        UpdateReceiveData p2update = new UpdateReceiveData()
                        {
                            p = GameProcessor.Phase,
                            d = -GameProcessor.BattleDamage,
                            y = p2,
                            r = p1,
                            a = null,
                        };
                        int p1number = PhotonNetwork.MasterClient.ActorNumber;
                        int p2number = PhotonNetwork.MasterClient.GetNext().ActorNumber;

                        RaiseEventOptions options = new RaiseEventOptions() { TargetActors = new int[] { p1number } };
                        PhotonNetwork.RaiseEvent(RaiseEvent_Update, JsonUtility.ToJson(p1update), options, ExitGames.Client.Photon.SendOptions.SendReliable);
                        options.TargetActors = new int[] { p2number };
                        PhotonNetwork.RaiseEvent(RaiseEvent_Update, JsonUtility.ToJson(p2update), options, ExitGames.Client.Photon.SendOptions.SendReliable);

                        Select1 = Select2 = -1;
                    }
                }
                break;
            case RaiseEvent_Select:
                {
                    int sender = photonEvent.Sender;
                    int[] data = (int[])photonEvent.CustomData;
                    int phase = data[0];
                    int index = data[1];

                    if (sender == PhotonNetwork.MasterClient.ActorNumber)
                    {
                        Select1 = (GameProcessor.Phase == phase) ? index : 0;
                    }
                    else
                    {
                        Select2 = (GameProcessor.Phase == phase) ? index : 0;
                    }
                    if (Select1 >= 0 && Select2 >= 0 ||
                        ((GameProcessor.Phase & 1) == 1 &&
                         ((GameProcessor.BattleDamage > 0 && Select1 >= 0) ||
                          (GameProcessor.BattleDamage < 0 && Select2 >= 0))))
                    {
                        GameProcessor.Decide(Select1, Select2);

                        UpdateReceiveData.PlayerData p1 = new UpdateReceiveData.PlayerData
                        {
                            s = Select1,
                            d = GameProcessor.Player1.draw.ToArray(),
                            c = GameProcessor.Player1.deck.Count
                        };
                        UpdateReceiveData.PlayerData p2 = new UpdateReceiveData.PlayerData
                        {
                            s = Select2,
                            d = GameProcessor.Player2.draw.ToArray(),
                            c = GameProcessor.Player2.deck.Count
                        };

                        UpdateReceiveData p1update = new UpdateReceiveData()
                        {
                            p = GameProcessor.Phase,
                            d = GameProcessor.BattleDamage,
                            y = p1,
                            r = p2,
                            a = null,
                        };
                        UpdateReceiveData p2update = new UpdateReceiveData()
                        {
                            p = GameProcessor.Phase,
                            d = -GameProcessor.BattleDamage,
                            y = p2,
                            r = p1,
                            a = null,
                        };
                        int p1number = PhotonNetwork.MasterClient.ActorNumber;
                        int p2number = PhotonNetwork.MasterClient.GetNext().ActorNumber;

                        RaiseEventOptions options = new RaiseEventOptions() { TargetActors = new int[] { p1number } };
                        PhotonNetwork.RaiseEvent(RaiseEvent_Update, JsonUtility.ToJson(p1update), options, ExitGames.Client.Photon.SendOptions.SendReliable);
                        options.TargetActors = new int[] { p2number };
                        PhotonNetwork.RaiseEvent(RaiseEvent_Update, JsonUtility.ToJson(p2update), options, ExitGames.Client.Photon.SendOptions.SendReliable);

                        Select1 = Select2 = -1;
                    }
                }
                break;
            case RaiseEvent_Surrender:
                {
                    int sender = photonEvent.Sender;
                    UpdateData udata = new UpdateData()
                    {
                        phase = -1,
                        damage = sender == PhotonNetwork.LocalPlayer.ActorNumber ? 1 : -1,
                        myself = new UpdateData.PlayerData(),
                        rival = new UpdateData.PlayerData(),
                    };
                    Callback(udata, "Surrender");
                }
                break;
        }

        //        PhotonNetwork.RaiseEvent(eventCode,eventContent,)

    }


    private IGameServer.UpdateCallback Callback;
    void IGameServer.SetUpdateCallback(IGameServer.UpdateCallback callback) { Callback = callback; }

    InitialData IGameServer.GetInitialData()
    {
        Player myself = PhotonNetwork.LocalPlayer;
        Player rival = myself.GetNext();
        return new InitialData() {
            battleSelectTimeLimitSecond = initialReceiveData.battletime,
            damageSelectTimeLimitSecond = initialReceiveData.damagetime,
            myname = myself.NickName,
            rivalname = rival.NickName,
        };
    }

    void IGameServer.SendReady()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        RaiseEventOptions options = new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient };
        PhotonNetwork.RaiseEvent(RaiseEvent_Ready, null , options, ExitGames.Client.Photon.SendOptions.SendReliable);

    }

    void IGameServer.SendSelect(int phase, int index)
    {
        if (!PhotonNetwork.IsConnected)
            return;
        int[] contents = { phase, index };

        RaiseEventOptions options = new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient };
        PhotonNetwork.RaiseEvent(RaiseEvent_Select, contents , options, ExitGames.Client.Photon.SendOptions.SendReliable);
    }

    void IGameServer.SendSurrender()
    {
        if (!PhotonNetwork.IsConnected)
            return;
        string surrender_command = $@"{{""message"":"" ""}}";

        RaiseEventOptions options = new RaiseEventOptions() { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(RaiseEvent_Surrender, JsonUtility.ToJson(surrender_command), options, ExitGames.Client.Photon.SendOptions.SendReliable);
    }

    void IGameServer.Terminalize()
    {
        Terminalize();
    }

    public void Terminalize()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            PhotonNetwork.RemoveCallbackTarget(this);
        }
    }

}
