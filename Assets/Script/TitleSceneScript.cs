using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleSceneScript : MonoBehaviour
{
    private bool Connecting = false;
    private bool ConnectingPUN = false;

    public Text Name;

    public string ServerUrl;



    public Button CPUButton;
    public Button OnlineButton;
    public Button PUNButton;

    public InputField NameInput;


//    private readonly OnlineGameServer Server = new OnlineGameServer();
//    private readonly OnlineGameServer2 Server = new OnlineGameServer2();
    private readonly OnlineGameServer3 Server = new OnlineGameServer3();
    private readonly PunGameServer PunServer = new PunGameServer();

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
            PUNButton.interactable = true;
            NameInput.interactable = true;
            Server.Cancel();
            label.text = "VS Online";
            Connecting = false;
        }
        else
        {
            CPUButton.interactable = false;
            PUNButton.interactable = false;
            NameInput.interactable = false;

            Connecting = true;
            label.text = "Connecting...";

            string name = NameInput.GetComponent<InputField>().text;
            PlayerPrefs.SetString("name", name);

            if (await Server.TryConnect(new System.Uri(ServerUrl), name))
            {
                    GameSceneParam.GameServer = Server;

                SceneManager.LoadScene("GameScene");
            }
            else
            {
                CPUButton.interactable = true;
                PUNButton.interactable = true;
                NameInput.interactable = true;
                label.text = "VS Online";
                Connecting = false;
            }
        }
    }

    public async void PUNOnlineButtonClick()
    {
        Text label = PUNButton.transform.GetChild(0).gameObject.GetComponent<Text>();
        if (ConnectingPUN)
        {
            CPUButton.interactable = true;
            OnlineButton.interactable = true;
            NameInput.interactable = true;
            PunServer.Cancel();
            label.text = "VS Online(PUN2)";
            ConnectingPUN = false;
        }
        else
        {
            CPUButton.interactable = false;
            OnlineButton.interactable = false;
            NameInput.interactable = false;

            ConnectingPUN = true;
            label.text = "Connecting...";

            string name = NameInput.GetComponent<InputField>().text;
            PlayerPrefs.SetString("name", name);

            if (await PunServer.TryConnect(name))
            {
                GameSceneParam.GameServer = PunServer;

                SceneManager.LoadScene("GameScene");
            }
            else
            {
                CPUButton.interactable = true;
                OnlineButton.interactable = true;
                NameInput.interactable = true;
                label.text = "VS Online(PUN2)";
                ConnectingPUN = false;
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
