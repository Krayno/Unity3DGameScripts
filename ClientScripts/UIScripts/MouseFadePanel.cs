using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MouseFadePanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public CanvasGroup Panel;
    public float FadeInAlpha;
    public float FadeOutAlpha;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Panel.alpha = FadeInAlpha;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Panel.alpha = FadeOutAlpha;
    }
}
