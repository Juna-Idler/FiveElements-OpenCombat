using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class OfflineGameServer : IGameServer
{
    private GameProcessor GameProcessor;
    private string PlayerName;


    public OfflineGameServer(string name)
    {
        Initialize(name);
    }
    public void Initialize(string name)
    {
        GameProcessor = new GameProcessor();
        PlayerName = name;
    }

    private static UpdateData.PlayerData CreateUpdatePlayerData(GameProcessor.PlayerData data)
    {
        return new UpdateData.PlayerData { draw = data.draw.ToArray(), select = data.select, deckcount = data.deck.Count };
    }

    private UpdateData ToUpdateData()
    {
        return new UpdateData()
        {
            phase = GameProcessor.Phase,
            damage = GameProcessor.BattleDamage,
            myself = CreateUpdatePlayerData(GameProcessor.Player1),
            rival = CreateUpdatePlayerData(GameProcessor.Player2)
        };
    }

    InitialData IGameServer.GetInitialData()
    {
        return new InitialData()
        {
            battleSelectTimeLimitSecond = 15,
            damageSelectTimeLimitSecond = 10,
            myhand = GameProcessor.Player1.hand.ToArray(),
            rivalhand = GameProcessor.Player2.hand.ToArray(),
            mydeckcount = GameProcessor.Player1.deck.Count,
            rivaldeckcount = GameProcessor.Player2.deck.Count,
            myname = PlayerName,
            rivalname = "CPU"
        };
    }

    private IGameServer.UpdateCallback Callback;
    void IGameServer.SetUpdateCallback(IGameServer.UpdateCallback callback)
    {
        Callback = callback;
    }

    void IGameServer.SendSelect(int phase,int index)
    {
        int index2 = 0;
        if ((GameProcessor.Phase & 1 )== 1 && GameProcessor.BattleDamage < 0)
        {

            int min = 256;
            for (int i = 0; i < GameProcessor.Player2.hand.Count; i++)
            {
                if (GameProcessor.Player2.hand[i].Power < min)
                {
                    min = GameProcessor.Player2.hand[i].Power;
                    index2 = i;
                }
            }
        }
        else
        {
            index2 = GameProcessor.random.Next(0, GameProcessor.Player2.hand.Count);
        }

        GameProcessor.Decide(index, index2);

        Callback(ToUpdateData(),null);
    }

     void IGameServer.SendSurrender()
    {
        Callback(new UpdateData() { damage = 1, }, "Surrender");
    }

    void IGameServer.Terminalize()
    {
    }

}
