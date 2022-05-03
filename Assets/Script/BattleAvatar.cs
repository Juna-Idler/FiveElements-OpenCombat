using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class BattleAvatar : MonoBehaviour
{
    public SpriteRenderer Base;
    public SpriteRenderer Power;

    private Sequence Sequence;
    // Start is called before the first frame update
    void Start()
    {

    }
    public void Appearance(Vector3 pos,CardData.FiveElements element,int power)
    {
        gameObject.transform.position = pos;
        Base.color = Card.ElementColors[(int)element];
        Power.sprite = Card.NumberSprite(power);
        gameObject.transform.localScale = new Vector3(0.9f + 0.1f * power, 0.9f + 0.1f * power, 1);
        gameObject.SetActive(true);

        Sequence = DOTween.Sequence();
        Sequence.Append(Base.DOFade(1, 0.3f));
        Sequence.Join(Power.DOFade(1, 0.3f));
    }
    public void Disappearance()
    {
        Sequence = DOTween.Sequence();
        Sequence.Append(Base.DOFade(0, 1f))
                .Join(Power.DOFade(0, 1f))
                .AppendCallback(() => gameObject.SetActive(false))
                .SetAutoKill(true);
    }

    public void Raise(int power)
    {
        Sequence = DOTween.Sequence();
        Sequence.AppendCallback(() => Power.sprite = Card.NumberSprite(power))
                .Append(gameObject.transform.DOScale(0.9f + 0.1f * power, 0.1f));
    }
    public void Reduce(int power)
    {
        Sequence = DOTween.Sequence();
        Sequence.AppendCallback(() => Power.sprite = Card.NumberSprite(power))
                .Append(gameObject.transform.DOScale(0.9f + 0.1f * power, 0.1f));
    }
}
