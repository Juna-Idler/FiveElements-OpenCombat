using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class PlayerAvatar : MonoBehaviour
{

    public AudioClip AudioAttack;
    public AudioClip AudioAttackOffset;
    public AudioClip AudioDamage;
    public AudioClip AudioRecover;

    public AudioClip AudioWin;

    public SpriteResolver Resolver;

    public enum Expression { ïÅí , ï¬Ç∂, ì{ÇË, ã¡Ç´, äÏÇ—, í…Ç› };

    public void ChangeExpression(Expression expression)
    {
        Resolver.SetCategoryAndLabel(Resolver.GetCategory(), expression.ToString());
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
