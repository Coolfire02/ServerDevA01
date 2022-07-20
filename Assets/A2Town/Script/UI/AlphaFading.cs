using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlphaFading : MonoBehaviour
{

    public float fadeTime = 1.0f;
    Image img;

    private void Start()
    {
        img = GetComponent<Image>();
        if(img == null)
            img = GetComponentInChildren<Image>();
    }

    public void FadeIn()
    {
        FadeIn(fadeTime);
    }

    public void FadeIn(float time)
    {
        StartCoroutine(FadeInImage(time));
    }

    public void FadeOut()
    {
        FadeOut(fadeTime);
    }

    public void FadeOut(float time)
    {
        StartCoroutine(FadeOutImage(time));
    }

    public void FadeInThenOut(float totalTime)
    {
        StartCoroutine(FadeInThenOutImage(totalTime * 0.5f));
    }

    public bool isCurrentlyFading()
    {
        if (img.color.a == 1.0f || img.color.a == 0.0f) return false;
        return true;
    }

    public void SetToOpaque()
    {
        Color matColor = img.color;
        img.color = new Color(matColor.r, matColor.g, matColor.b, 1.0f);
    }
    IEnumerator FadeOutImage(float fadeSpeed)
    {
        Color matColor = img.color;
        float alphaValue = img.color.a;

        while (img.color.a > 0f)
        {
            alphaValue -= Time.deltaTime / fadeSpeed;
            img.color = new Color(matColor.r, matColor.g, matColor.b, alphaValue);
            yield return null;
        }
        img.color = new Color(matColor.r, matColor.g, matColor.b, 0f);
    }

    IEnumerator FadeInImage(float fadeSpeed)
    {
        Color matColor = img.color;
        float alphaValue = img.color.a;
        //print("Original alpha " + alphaValue.ToString());
        while (img.color.a < 1.0f)
        {
            //print("Upping alpha value to " + alphaValue.ToString());
            alphaValue += Time.deltaTime / fadeSpeed;
            img.color = new Color(matColor.r, matColor.g, matColor.b, alphaValue);
            yield return null;
        }
        img.color = new Color(matColor.r, matColor.g, matColor.b, 1.0f);
    }

    IEnumerator FadeInThenOutImage(float fadeSpeed)
    {
        Color matColor = img.color;
        float alphaValue = img.color.a;

        while (img.color.a < 1.0f)
        {
            alphaValue += Time.deltaTime / fadeSpeed;
            img.color = new Color(matColor.r, matColor.g, matColor.b, alphaValue);
            yield return null;
        }
        img.color = new Color(matColor.r, matColor.g, matColor.b, 1.0f);

        matColor = img.color;
        alphaValue = img.color.a;

        while (img.color.a > 0f)
        {
            alphaValue -= Time.deltaTime / fadeSpeed;
            img.color = new Color(matColor.r, matColor.g, matColor.b, alphaValue);
            yield return null;
        }
        img.color = new Color(matColor.r, matColor.g, matColor.b, 0f);
    }

}
