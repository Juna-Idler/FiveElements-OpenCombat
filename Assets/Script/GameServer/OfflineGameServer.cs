using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class OfflineGameServer : IGameServer
{
    private GameProcessor GameProcessor;
    private string PlayerName;

    private ICPUCommander Commander;
    private int Result;

    public OfflineGameServer(string name, ICPUCommander commander)
    {
        Initialize(name,commander);
    }
    public void Initialize(string name,ICPUCommander commander)
    {
        GameProcessor = new GameProcessor();
        PlayerName = name;
        Commander = commander;
    }

    private static UpdateData.PlayerData CreateUpdatePlayerData(GameProcessor.PlayerData data)
    {
        return new UpdateData.PlayerData { draw = data.draw.ToArray(), select = data.select, deckcount = data.deck.Count };
    }


    InitialData IGameServer.GetInitialData()
    {
        int[] hand1 = GameProcessor.Player1.hand.ToArray();
        int[] hand2 = GameProcessor.Player2.hand.ToArray();
        Result = Commander.FirstSelect(hand2, hand1);

        return new InitialData()
        {
            battleSelectTimeLimitSecond = 15,
            damageSelectTimeLimitSecond = 10,
            myhand = hand1,
            rivalhand = hand2,
            mydeckcount = GameProcessor.Player1.deck.Count,
            rivaldeckcount = GameProcessor.Player2.deck.Count,
            myname = PlayerName,
            rivalname = Commander.Name
        };
    }

    private IGameServer.UpdateCallback Callback;
    void IGameServer.SetUpdateCallback(IGameServer.UpdateCallback callback)
    {
        Callback = callback;
    }

    void IGameServer.SendSelect(int phase,int index)
    {
        System.Threading.SynchronizationContext context = System.Threading.SynchronizationContext.Current;
        int index2 = Result;
        GameProcessor.Decide(index, index2);

        int p = GameProcessor.Phase;
        int damage = GameProcessor.BattleDamage;
        UpdateData.PlayerData p1 = CreateUpdatePlayerData(GameProcessor.Player1);
        UpdateData.PlayerData p2 = CreateUpdatePlayerData(GameProcessor.Player2);
        UpdateData p1update = new() { phase = p, damage = damage, myself = p1, rival = p2 };
        UpdateData p2update = new() { phase = p, damage = -damage, myself = p2, rival = p1 };

        if ((GameProcessor.Phase & 1) == 1)
        {
            Result = Commander.DamageSelect(p2update);
        }
        else
        {
            Result = Commander.BattleSelect(p2update);
        }
        Callback(p1update, null);
    }

     void IGameServer.SendSurrender()
    {
        Callback(new UpdateData() { damage = 1, }, "Surrender");
    }

    void IGameServer.Terminalize()
    {
    }

}
