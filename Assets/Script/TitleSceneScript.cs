using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneScript : MonoBehaviour
{

    public void Click()
    {
        IGameServer server = new OfflineGameServer();
        
        GameSceneParam.GameServer = server;

        SceneManager.LoadScene("GameScene");


    }
}
