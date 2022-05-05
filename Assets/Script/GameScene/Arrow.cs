using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;


public class Arrow : MonoBehaviour
{

    private Sequence Sequence;

    private void Start()
    {
        Sequence = DOTween.Sequence();
        Sequence.Append(gameObject.transform.DOScale(0.6f, 0.1f).SetEase(Ease.Linear))
                .Append(gameObject.transform.DOScale(0.4f, 0.4f))
                .AppendCallback(() => { gameObject.SetActive(false); })
                .SetAutoKill(false).SetLink(gameObject);
        //Ç«Ç§Ç¢Ç§ñÛÇ©pauseÇ∑ÇÈÇ∆èââÒÇÃrestartÇ™ã@î\ÇµÇ»Ç¢ÇÃÇ≈Ç©ÇÁâÒÇ∑
    }
    public void StartAnimation()
    {
        gameObject.SetActive(true);
        Sequence.Restart();
    }

    public void SetColor(Color color)
    {
        gameObject.GetComponent<SpriteRenderer>().color = color;
    }

    public void StartAnimationPlus()
    {
        gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        gameObject.SetActive(true);
        Sequence.Restart();
    }
    public void StartAnimationMinus()
    {
        gameObject.GetComponent<SpriteRenderer>().color = Color.blue;
        gameObject.SetActive(true);
        Sequence.Restart();
    }

}
