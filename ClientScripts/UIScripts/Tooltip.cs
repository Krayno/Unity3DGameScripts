using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public KeybindManager KeybindManager;
    public Transform tooltip;
    public Canvas Canvas;

    readonly Vector3[] toolTipWorldCorners = new Vector3[4];
    float toolTipWidth;

    private Vector3 TooltipOffset;

    private string sTooltip;
    private TMP_Text tooltipText;

    private RectTransform tooltipRect;
    private RectTransform tooltipTextRect;
    Vector2 tooltipTextMargin;

    //Content Size Fitter on the text is required with horizontal fitting = preferred.

    void Start()
    {
        tooltipRect = tooltip.GetComponent<RectTransform>();

        tooltipRect.GetWorldCorners(toolTipWorldCorners);
        toolTipWidth = toolTipWorldCorners[3].x - toolTipWorldCorners[0].x;

        tooltipText = tooltip.GetChild(0).GetComponent<TMP_Text>();
        tooltipTextRect = tooltip.GetChild(0).GetComponent<RectTransform>();
        tooltipTextMargin = new Vector2(15, 7.5f);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        //Set the text in the text object to the correct keybinding.
        sTooltip = tooltip.name.Replace("Tooltip", "");
        string sTooltipKeyAndMod;
        foreach (KeybindManager.Keybind x in KeybindManager.Keybinds.Values)
        {
            if (x.Name == sTooltip)
            {
                sTooltipKeyAndMod = x.Key.ToString();
                if (x.Alt)
                {
                    sTooltipKeyAndMod = "Alt + " + sTooltipKeyAndMod;
                }
                if (x.Control)
                {
                    sTooltipKeyAndMod = "Control + " + sTooltipKeyAndMod;
                }
                if (x.Shift)
                {
                    sTooltipKeyAndMod = "Shift + " + sTooltipKeyAndMod;
                }
                sTooltip = $"{sTooltip} <color=#ffa500ff>({sTooltipKeyAndMod})</color>";
            }
        }
        tooltipText.text = sTooltip;

        StartCoroutine("ShowTooltip");
    }

    private IEnumerator ShowTooltip()
    {
        while (true)
        {
            TooltipOffset = new Vector3(0, 48f * Canvas.scaleFactor, 0);
            float newPosX = TooltipOffset.x + transform.position.x;
            float newPosY = TooltipOffset.y + transform.position.y;
            tooltip.transform.position = new Vector3(newPosX, newPosY, 0);
            //Set the container of the text to the the text width.
            tooltipRect.sizeDelta = tooltipTextRect.sizeDelta + tooltipTextMargin;

            tooltipRect.GetWorldCorners(toolTipWorldCorners);
            toolTipWidth = toolTipWorldCorners[3].x - toolTipWorldCorners[0].x;

            if (toolTipWorldCorners[1].x < 0) // Left
            {
                newPosX = 0;
            }
            if (toolTipWorldCorners[3].y < 0) // Bottom
            {
                newPosY = 0;
            }
            if (toolTipWorldCorners[1].y > Screen.height) // Top
            {
                newPosY = transform.position.y - TooltipOffset.y;
            }
            if (toolTipWorldCorners[3].x > Screen.width) //Right
            {
                newPosX = Screen.width - (toolTipWidth / 2);
            }
            tooltip.position = new Vector3(newPosX, newPosY, 0);
            tooltip.gameObject.SetActive(true);

            yield return null;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopCoroutine("ShowTooltip");
        tooltip.gameObject.SetActive(false);
    }
}
