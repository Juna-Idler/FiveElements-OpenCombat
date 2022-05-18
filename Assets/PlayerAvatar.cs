using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class PlayerAvatar : MonoBehaviour
{
    public AudioSource AudioSource;

    public AudioClip AudioAttack;
    public AudioClip AudioAttackOffset;
    public AudioClip AudioDamage;
    public AudioClip AudioRecover;

    public AudioClip AudioWin;

    public SpriteResolver Resolver;

    public enum Expression { •’Ê, •Â‚¶, “{‚è, ‹Á‚«, Šì‚Ñ, ’É‚Ý };

    public void ChangeExpression(Expression expression)
    {
        Resolver.SetCategoryAndLabel(Resolver.GetCategory(), expression.ToString());
    }

    public enum SpeakOn {Attack,Offset,Damage,Recover,Win };
    public void Speak(SpeakOn on)
    {
        AudioClip clip = on switch
        {
            SpeakOn.Attack => AudioAttack,
            SpeakOn.Offset => AudioAttackOffset,
            SpeakOn.Damage => AudioDamage,
            SpeakOn.Recover => AudioRecover,
            SpeakOn.Win => AudioWin,
            _ => throw new System.NotImplementedException(),
        };
        AudioSource.PlayOneShot(clip);
    }

    /*
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    */
}
