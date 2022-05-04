using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class BattleAvatar : MonoBehaviour
{
    public SpriteRenderer BaseSprite;
    public SpriteRenderer PowerSprite;

    public int Power;


    public void Appearance(Vector3 pos,CardData data)
    {
        Power = data.Power;
        gameObject.transform.position = pos;
        BaseSprite.color = Card.ElementColors[(int)data.Element];
        PowerSprite.sprite = Card.NumberSprite(Power);
        gameObject.transform.localScale = new Vector3(0.8f, 0.8f, 1);
        gameObject.SetActive(true);

        DOTween.Sequence()
            .Append(BaseSprite.DOFade(1, 0.3f))
            .Join(PowerSprite.DOFade(1, 0.3f))
            .Join(gameObject.transform.DOScale(0.9f + 0.1f * Power, 0.1f));
       
    }
    public void Disappearance()
    {
        DOTween.Sequence()
            .Append(BaseSprite.DOFade(0, 1f))
            .Join(PowerSprite.DOFade(0, 1f))
            .Join(gameObject.transform.DOScale(0.8f, 1f))
            .AppendCallback(() => gameObject.SetActive(false));
    }

    public void Raise(int plus = 1)
    {
        Power += plus;
        DOTween.Sequence()
            .AppendCallback(() => PowerSprite.sprite = Card.NumberSprite(Power))
            .Append(gameObject.transform.DOScale(0.9f + 0.1f * Power, 0.1f));
    }
    public void Reduce(int minus = 1)
    {
        Power -= minus;
        if (Power < 0) Power = 0;
        DOTween.Sequence()
            .AppendCallback(() => PowerSprite.sprite = Card.NumberSprite(Power))
            .Append(gameObject.transform.DOScale(0.9f + 0.1f * Power, 0.1f));
    }

    public void Damage()
    {
        Vector3 n = gameObject.transform.position.normalized;
        DOTween.Sequence()
            .Append(gameObject.transform.DOMove(n * 50f, 0.2f).SetEase(Ease.InBack))
            .Append(gameObject.transform.DOMove(n * 350, 0.3f).SetEase(Ease.OutQuad))
//            .Append(gameObject.transform.DOMove(n * 400, 0.5f).SetEase(Ease.Linear))
            .Append(BaseSprite.DOFade(0, 0.4f).SetEase(Ease.InQuad))
            .Join(PowerSprite.DOFade(0, 0.4f).SetEase(Ease.InQuad))
            .Join(gameObject.transform.DOScale(3f, 0.4f).SetEase(Ease.InQuad))
            .AppendCallback(() => gameObject.SetActive(false));
    }
    public void Attack()
    {
        Vector3 n = gameObject.transform.position.normalized;
        DOTween.Sequence()
            .Append(gameObject.transform.DOMove(n * 50f, 0.2f).SetEase(Ease.InBack))
            .Append(gameObject.transform.DOMove(n * 70f, 0.2f).SetEase(Ease.OutCubic))
            .AppendInterval(0.3f)
            .Append(BaseSprite.DOFade(0, 0.5f).SetEase(Ease.InQuad))
            .Join(PowerSprite.DOFade(0, 0.5f).SetEase(Ease.InQuad))
            .Join(gameObject.transform.DOScale(0.8f, 0.5f).SetEase(Ease.OutQuad))
            .AppendCallback(() => gameObject.SetActive(false));
    }
}
