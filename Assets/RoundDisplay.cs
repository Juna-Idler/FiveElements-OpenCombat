using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundDisplay : MonoBehaviour
{
    public SpriteRenderer RoundText;
    public SpriteRenderer Battle;
    public SpriteRenderer Damage;
    public TwoDigits Round;

    public void ChangeRound(int round)
    {
        Round.Set(round);
        Battle.enabled = true;
        Damage.enabled = false;
    }
    public void ChangeDamagePhase()
    {
        Battle.enabled = false;
        Damage.enabled = true;

    }
}
