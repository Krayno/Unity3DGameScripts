using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelButtonFadeIn : MonoBehaviour
{
    public bool ButtonNotAttached;
    public CanvasGroup PanelGroup;
    public Canvas Canvas;

    private float FadeDuration;
    private Button Button;

    void Awake()
    {
        FadeDuration = 0.3f;
        if (!ButtonNotAttached)
        {
            Button = GetComponent<Button>();
            Button.onClick.AddListener(InitialiseFade);
        }
    }

    public void InitialiseFade()
    {
        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();

        if (PanelGroup.alpha == 1)
        {
            StartCoroutine(Fade(PanelGroup, 1, 0, FadeDuration));
        }
        else
        {
            StartCoroutine(Fade(PanelGroup, 0, 1, FadeDuration));
            PanelGroup.transform.SetAsLastSibling();

        }
    }

    private IEnumerator Fade(CanvasGroup panel, float start, float end, float duration)
    {
        float Counter = 0;
        float Duration = duration;

        panel.gameObject.SetActive(true);

        //Make sure the panel is within the screen size.
        RectTransform Panel = panel.GetComponent<RectTransform>();
        if (Canvas != null)
        {
            RectTransform CanvasRect = Canvas.GetComponent<RectTransform>();

            var sizeDelta = CanvasRect.sizeDelta - Panel.sizeDelta;
            var panelPivot = Panel.pivot;
            var position = Panel.anchoredPosition;
            position.x = Mathf.Clamp(position.x, -sizeDelta.x * panelPivot.x, sizeDelta.x * (1 - panelPivot.x));
            position.y = Mathf.Clamp(position.y, -sizeDelta.y * panelPivot.y, sizeDelta.y * (1 - panelPivot.y));
            Panel.anchoredPosition = position;
        }

        while (Counter < Duration)
        {
            Counter += Time.deltaTime;

            panel.alpha = Mathf.Lerp(start, end, Counter / Duration);

            yield return null;
        }
    }
}
