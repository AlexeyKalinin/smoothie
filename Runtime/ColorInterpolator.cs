using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Smoothie;
using Sirenix.OdinInspector;

public class ColorInterpolator
{
    private MonoBehaviour context;
    private SmoothieAnimationStyles animationStyles;
    private ColorInterpolator.Config colorConfig;

    public class Config
    {
        public float speed;
        public float alphaMultiplier;
    }

    public ColorInterpolator(MonoBehaviour context, SmoothieAnimationStyles animationStyles)
    {
        this.context = context;
        this.animationStyles = animationStyles;
    }

    public void ApplyColorChange(SmoothieAnimationStyles.ShowHideConfig config, Graphic[] background, Graphic[] text, Graphic[] select, Graphic[] shadow)
    {
        if (!config.ChangeColors && !config.ChangeAlpha) return;
        
        colorConfig.speed = 1.0f;
        colorConfig.alphaMultiplier = config.alphaMultiplier;
        
        if (config.ChangeColors) {
            ApplyColorToGraphics(background);
            ApplyColorToGraphics(text);
            ApplyColorToGraphics(select);
            ApplyColorToGraphics(shadow);
        }
        
        if (config.ChangeAlpha) {
            ApplyAlphaToGraphics(background);
            ApplyAlphaToGraphics(text);
            ApplyAlphaToGraphics(select);
            ApplyAlphaToGraphics(shadow);
        }
    }

    private void ApplyColorToGraphics(Graphic[] graphics)
    {
        foreach (var graphic in graphics)
        {
            Color startColor = graphic.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, colorConfig.alphaMultiplier);
            context.StartCoroutine(SmoothColorChange(graphic, startColor, endColor, colorConfig.speed));
        }
    }

    private void ApplyAlphaToGraphics(Graphic[] graphics)
    {
        foreach (var graphic in graphics)
        {
            Color startColor = graphic.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, colorConfig.alphaMultiplier);
            graphic.color = endColor;
        }
    }



    private IEnumerator SmoothColorChange(Graphic graphic, Color start, Color end, float speed)
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * speed;
            graphic.color = Color.Lerp(start, end, t);
            yield return null;
        }
    }
}