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

    public bool LocalMode = false;


    public Button CPUButton;
    public Button OnlineButton;
    public Button PUNButton;

    public InputField NameInput;

    public SettingsScreen Settings;

    public FadeCanvas FadeCanvas;
    public float FadeDuration;

    private readonly RandomCommander Random = new RandomCommander();
    private readonly Level1Commander Level1 = new Level1Commander();
    private readonly Level2Commander Level2 = new Level2Commander();

    private ICPUCommander Commander;


    //    private readonly OnlineGameServer Server = new OnlineGameServer();
    //    private readonly OnlineGameServer2 Server = new OnlineGameServer2();
    private readonly OnlineGameServer3 Server = new OnlineGameServer3();
    private readonly PunGameServer PunServer = new PunGameServer();


    public SpriteRenderer MyRenderer;
    public SpriteRenderer RivalRenderer;

    public Sprite[] Avatars;

    public UnityEngine.Audio.AudioMixer AudioMixer;


    private void Start()
    {
        MyRenderer.sprite = Avatars[GameSceneParam.MyAvatar];
        RivalRenderer.sprite = Avatars[GameSceneParam.RivalAvatar];

        Commander = Level1;

        string name = PlayerPrefs.GetString("name", "");
        NameInput.GetComponent<InputField>().text = name;

        FadeCanvas.FadeIn(FadeDuration);
    }

    public async void OnlineButtonClick()
    {
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


            string server = ServerUrl;
            if (LocalMode)
                server = "ws://localhost:8080/";

            if (await Server.TryConnect(new System.Uri(server), name))
            {
                GameSceneParam.GameServer = Server;
                FadeCanvas.FadeOut(FadeDuration, () => SceneManager.LoadScene("GameScene"));
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
                FadeCanvas.FadeOut(FadeDuration, () => SceneManager.LoadScene("GameScene"));
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

        IGameServer server = new OfflineGameServer(name,Commander);

        GameSceneParam.GameServer = server;
        FadeCanvas.FadeOut(FadeDuration, () => SceneManager.LoadScene("GameScene"));
    }
    public void ChangeLevel(int index)
    {
        switch(index)
        {
            case 1:
                Commander = Level1;
                break;
            case 2:
                Commander = Level2;
                break;

            case 0:
            default:
                Commander = Random;
                break;
        }
    }

    public void SettingsOpen()
    {
        Settings.Open();
    }

    public void RestoreAudioSettings()
    {
        if (PlayerPrefs.HasKey("BGM"))
        {
            AudioMixer.SetFloat("BGM", PlayerPrefs.GetFloat("BGM"));
        }
        if (PlayerPrefs.HasKey("SE"))
        {
            AudioMixer.SetFloat("SE", PlayerPrefs.GetFloat("SE"));
        }
        if (PlayerPrefs.HasKey("Voice"))
        {
            AudioMixer.SetFloat("Voice", PlayerPrefs.GetFloat("Voice"));
        }
        if (PlayerPrefs.HasKey("Mute"))
        {
            AudioListener.volume = PlayerPrefs.GetInt("Mute") != 0 ? 1 : 0;
        }
    }


    public void ChangeMyAvatar()
    {
        GameSceneParam.MyAvatar++;
        if (GameSceneParam.MyAvatar > 2)
            GameSceneParam.MyAvatar = 0;
        MyRenderer.sprite = Avatars[GameSceneParam.MyAvatar];
    }

    public void ChangeRivalAvatar()
    {
        GameSceneParam.RivalAvatar++;
        if (GameSceneParam.RivalAvatar > 2)
            GameSceneParam.RivalAvatar = 0;
        RivalRenderer.sprite = Avatars[GameSceneParam.RivalAvatar];
    }

}
