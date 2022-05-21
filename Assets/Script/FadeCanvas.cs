using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;
using System;

public class FadeCanvas : MonoBehaviour
{
    public Image Image;
    public Color Color;

    private Tweener Tweener;

    public void Active(bool active = true)
    {
        Image.color = Color;
        gameObject.SetActive(active);
    }
    public void FadeIn(float sec, Action oncomplete = null,bool active = false)
    {
        Tweener?.Kill();

        Image.color = Color;
        gameObject.SetActive(true);
        Tweener = Image.DOFade(0f, sec);
        Tweener.onComplete = ()=> {
            Tweener = null;
            gameObject.SetActive(active);
            oncomplete?.Invoke();
        };
    }

    public void FadeOut(float sec, Action oncomplete = null,bool active = true)
    {
        Tweener?.Kill();

        gameObject.SetActive(true);
        Color c = Color;
        c.a = 0;
        Image.color = c;
        Tweener = Image.DOFade(1, sec);
        Tweener.onComplete = () => {
            Tweener = null;
            gameObject.SetActive(active);
            oncomplete?.Invoke();
        };
    }

    public void ForceComplete()
    {
        Tweener?.Kill(true);
    }
}
