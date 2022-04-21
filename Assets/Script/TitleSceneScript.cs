using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneScript : MonoBehaviour
{

    public async void Click()
    {
        //        IGameServer server = new OfflineGameServer();
        OnlineGameServer onserver = new OnlineGameServer();
        await onserver.Initialize();

        GameSceneParam.GameServer = onserver;

        SceneManager.LoadScene("GameScene");


    }
}
