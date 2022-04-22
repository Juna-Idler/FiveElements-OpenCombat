using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneScript : MonoBehaviour
{

    public async void OnlineButtonClick()
    {
        OnlineGameServer onserver = new OnlineGameServer();
        await onserver.Initialize();

        GameSceneParam.GameServer = onserver;

        SceneManager.LoadScene("GameScene");
    }

    public void CPUButtonClick()
    {
        IGameServer server = new OfflineGameServer();

        GameSceneParam.GameServer = server;

        SceneManager.LoadScene("GameScene");
    }

}
