using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TogglePanelButtonFade : MonoBehaviour
{
    public CanvasGroup InstancePanelGroup;
    public bool ManualClose;

    private float FadeDuration;
    private Button Button;

    public static List<CanvasGroup> PanelGroupList;
    public Canvas Canvas;

    public CanvasGroup BackpackCanvasGroup;

    void Awake()
    {
        FadeDuration = 0.3f;
        Button = GetComponent<Button>();
        Button.onClick.AddListener(InitialiseFade);

        PanelGroupList = new List<CanvasGroup>();

        if (!ManualClose)
        {
            StartCoroutine("Close");
        }
    }

    public void InitialiseFade()
    {

        //If the Game Menu is currently open, don't open any other panels.
        if (PanelGroupList.Count > 0 && InstancePanelGroup.name != "GameMenuPanelGroup" && PanelGroupList[PanelGroupList.Count - 1].gameObject.name == "GameMenuPanelGroup")
        {
            return;
        }

        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();

        //If the button pressed is the GameMenu Button, toggle backpack button off.
        if (gameObject.name == "GameMenuButton")
        {
            if (BackpackCanvasGroup.alpha == 1)
            {
                BackpackCanvasGroup.alpha = 0;
                BackpackCanvasGroup.gameObject.SetActive(false);
            }
        }

        //If the button is the backpack button, ignore other buttons completely.
        if (gameObject.name == "BackpackButton")
        {
            if (Button.gameObject.activeInHierarchy)
            {
                StartCoroutine(Fade(InstancePanelGroup, InstancePanelGroup.alpha, InstancePanelGroup.gameObject.activeInHierarchy ? 0 : 1, FadeDuration));
            }

            return;
        }

        //If this button was clicked and this buttons instance panel group is not open, close all other panels in the panelgrouplist and start fading this instances panel group.
        if (InstancePanelGroup.alpha != 1 || !InstancePanelGroup.gameObject.activeSelf)
        {
            foreach(CanvasGroup Panel in PanelGroupList)
            {
                Panel.alpha = 0;
                Panel.gameObject.SetActive(false);
            }
            PanelGroupList.Clear();

            InstancePanelGroup.alpha = 0;
            InstancePanelGroup.gameObject.SetActive(false);
        }

        //Close the panel that was last opened.
        CanvasGroup PanelGroup;

        if (PanelGroupList.Count == 0)
        {
            PanelGroup = InstancePanelGroup;
        }
        else
        {
            PanelGroup = PanelGroupList[PanelGroupList.Count - 1];
        }


        //Prevents Fade Start And End Arguements both being the same number.
        if (PanelGroup.alpha == (PanelGroup.gameObject.activeSelf ? 0f : 1f))
        {
            if (PanelGroup.alpha == 1)
            {
                StartCoroutine(Fade(PanelGroup, PanelGroup.alpha, 0, FadeDuration));
            }
            else
            {
                StartCoroutine(Fade(PanelGroup, PanelGroup.alpha, 0, FadeDuration));
            }
            StartCoroutine(Fade(PanelGroup, PanelGroup.alpha, 0, FadeDuration));
            return;
        }

        if (Button.gameObject.activeInHierarchy)
        {
            StartCoroutine(Fade(PanelGroup, PanelGroup.alpha, PanelGroup.gameObject.activeInHierarchy ? 0 : 1, FadeDuration));
        }
    }

    private IEnumerator Fade(CanvasGroup panel, float start, float end, float duration)
    {
        float Counter = 0;
        float Duration = duration;

        if (start == 0) //If fading in, activate the panel..
        {
            panel.transform.SetAsLastSibling();
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

            if (panel.gameObject.name != "BackpackPanelGroup")
            {
                PanelGroupList.Add(panel);
            }
        }

        while (Counter < Duration)
        {
            Counter += Time.deltaTime;

            panel.alpha = Mathf.Lerp(start, end, Counter / Duration);

            yield return null;
        }

        if (start == 1) //If fading out and it has completed, deactivate the panel.
        {
            panel.gameObject.SetActive(false);

            if (panel.gameObject.name != "BackpackPanelGroup")
            {
                PanelGroupList.Remove(panel);
            }
        }

    }

    private IEnumerator Close()
    {
        while (!ManualClose)
        {
            //Closes the panel if the panel is open.
            if (Input.GetKeyDown(KeyCode.Escape) && PanelGroupList.Count > 0 && PanelGroupList[PanelGroupList.Count - 1].alpha == 1)
            {
                InitialiseFade();
            }

            yield return null;
        }
    }
}
