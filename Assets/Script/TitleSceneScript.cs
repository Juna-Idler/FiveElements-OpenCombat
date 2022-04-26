using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleSceneScript : MonoBehaviour
{
    public Text Connect;
    private bool Connecting = false;

    public Text Name;

    private readonly OnlineGameServer Server = new OnlineGameServer();

    public async void OnlineButtonClick()
    {
        if (Connecting)
        {
            Server.Cancel();
            Connect.text = "VS Online";
            Connecting = false;
        }
        else
        {
            Connecting = true;
            Connect.text = "Connecting";
            if (await Server.TryConnect(new System.Uri("ws://localhost:8080/"),Name.text))
            {
                GameSceneParam.GameServer = Server;

                SceneManager.LoadScene("GameScene");
            }
            else
            {
                Connect.text = "VS Online";
                Connecting = false;
            }
        }
    }

    public void CPUButtonClick()
    {
        IGameServer server = new OfflineGameServer(Name.text);

        GameSceneParam.GameServer = server;

        SceneManager.LoadScene("GameScene");
    }

}
