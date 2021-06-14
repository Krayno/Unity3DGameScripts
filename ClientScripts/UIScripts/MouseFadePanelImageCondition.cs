using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseFadePanelImageCondition : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public CanvasGroup Panel;
    public GameObject Condition;
    public float FadeInAlpha;
    public float FadeOutAlpha;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Panel.alpha = FadeInAlpha;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!Condition.activeInHierarchy)
        {
            Panel.alpha = FadeOutAlpha;
        }
    }
}
