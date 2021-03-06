using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsScreen : MonoBehaviour
{
    public AudioMixer AudioMixer;

    public Slider BGMSlider;
    public Slider SESlider;
    public Slider VoiceSlider;

    public Toggle MuteToggle;
    public Button SurrenderButton;

    private System.Action Surrender;

    public void Start()
    {
        AudioMixer.GetFloat("BGM", out float bgm);
        AudioMixer.GetFloat("SE", out float se);
        AudioMixer.GetFloat("Voice", out float voice);

        BGMSlider.value = FromDb(bgm);
        SESlider.value = FromDb(se);
        VoiceSlider.value = FromDb(voice);

        MuteToggle.isOn = AudioListener.volume == 0;
    }



    public void Open(System.Action surrender = null)
    {
        gameObject.GetComponent<Canvas>().enabled = true;

        Surrender = surrender;
        SurrenderButton.gameObject.SetActive(surrender != null);
    }
    public void Close()
    {
        gameObject.GetComponent<Canvas>().enabled = false;
    }


    public void ChangeBGM(float v)
    {
        float db = ToDb(v);
        PlayerPrefs.SetFloat("BGM", db);
        AudioMixer.SetFloat("BGM", db);
    }

    public void ChangeSE(float v)
    {
        float db = ToDb(v);
        PlayerPrefs.SetFloat("SE", db);
        AudioMixer.SetFloat("SE", ToDb(v));
    }

    public void ChangeVoice(float v)
    {
        float db = ToDb(v);
        PlayerPrefs.SetFloat("Voice", db);
        AudioMixer.SetFloat("Voice", ToDb(v));
    }

    public void ToggleMute(bool check)
    {
        PlayerPrefs.SetInt("Mute", check ? 0 : 1);
        AudioListener.volume = check ? 0 : 1;
    }



    public void SurrenderPush()
    {
        Close();
        Surrender();
    }

    private static float ToDb(float v)
    {
        return Mathf.Log10(Mathf.Clamp(v, 0.0001f, 1f)) * 20f;
    }
    private static float FromDb(float db)
    {
        return Mathf.Pow(10f, Mathf.Clamp(db, -80f, 0) / 20f);
    }
}
