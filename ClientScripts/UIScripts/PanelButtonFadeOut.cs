using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelButtonFadeOut : MonoBehaviour
{
    public bool ButtonNotAttached;
    public CanvasGroup PanelGroup;

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

        StartCoroutine(Fade(PanelGroup, 1, 0, FadeDuration));
    }

    private IEnumerator Fade(CanvasGroup panel, float start, float end, float duration)
    {
        float Counter = 0;
        float Duration = duration;

        while (Counter < Duration)
        {
            Counter += Time.deltaTime;

            panel.alpha = Mathf.Lerp(start, end, Counter / Duration);

            yield return null;
        }

        panel.gameObject.SetActive(false);
    }

    private void Update()
    {
        //Closes the panel if the panel is open.
        if (Input.GetKeyDown(KeyCode.Escape) && PanelGroup.alpha == 1)
        {
            InitialiseFade();
        }
    }
}
