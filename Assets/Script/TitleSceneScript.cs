using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleSceneScript : MonoBehaviour
{
    private bool Connecting = false;

    public Text Name;

    public string ServerUrl;



    public Button CPUButton;
    public Button OnlineButton;

    public InputField NameInput;


//    private readonly OnlineGameServer Server = new OnlineGameServer();
//    private readonly OnlineGameServer2 Server = new OnlineGameServer2();
//    private readonly OnlineGameServer3 Server = new OnlineGameServer3();
    private readonly PunGameServer Server = new PunGameServer();

    private void Start()
    {
        string name = PlayerPrefs.GetString("name", "");
        NameInput.GetComponent<InputField>().text = name;
    }

    public async void OnlineButtonClick()
    {
//        ServerUrl = "ws://localhost:8080/";
        Text label = OnlineButton.transform.GetChild(0).gameObject.GetComponent<Text>();
        if (Connecting)
        {
            CPUButton.interactable = true;
            NameInput.interactable = true;
            Server.Cancel();
            label.text = "VS Online";
            Connecting = false;
        }
        else
        {
            CPUButton.interactable = false;
            NameInput.interactable = false;

            Connecting = true;
            label.text = "Connecting...";

            string name = NameInput.GetComponent<InputField>().text;
            PlayerPrefs.SetString("name", name);

//            if (await Server.TryConnect(new System.Uri(ServerUrl), name))
            if (await Server.TryConnect(name))
            {
                    GameSceneParam.GameServer = Server;

                SceneManager.LoadScene("GameScene");
            }
            else
            {
                label.text = "VS Online";
                Connecting = false;
            }
        }
    }

    public void CPUButtonClick()
    {
        string name = NameInput.GetComponent<InputField>().text;
        PlayerPrefs.SetString("name", name);

        IGameServer server = new OfflineGameServer(name);

        GameSceneParam.GameServer = server;

        SceneManager.LoadScene("GameScene");
    }

}
