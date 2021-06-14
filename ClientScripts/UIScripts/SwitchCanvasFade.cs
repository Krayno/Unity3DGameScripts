using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchCanvasFade : MonoBehaviour
{
    public CanvasGroup FadeOutCanvas;
    public CanvasGroup FadeInCanvas;
    public bool Manual;

    private Button Button;
    private float FadeDuration;


    private void Awake()
    {
        FadeDuration = 0.3f;
        Button = GetComponent<Button>();
        Button.onClick.AddListener(InitialiseFade);
    }

    private void InitialiseFade()
    {
        if (!Manual && FadeOutCanvas.alpha == 1 && FadeInCanvas.alpha == 0)
        {
            //Play OpenClose Sound.
            SoundManager.Instance.Sounds["UIOpenClose"].Play();

            StartCoroutine(FadeOutIn(FadeOutCanvas, FadeInCanvas, FadeDuration));
        }
    }

    public void ManualInitialiseFade()
    {
        StartCoroutine(FadeOutIn(FadeOutCanvas, FadeInCanvas, FadeDuration));
    }

    private IEnumerator FadeOutIn(CanvasGroup FadeOutCanvas, CanvasGroup FadeInCanvas, float duration)
    {
        float Counter = 0;
        float Duration = duration;

        while (Counter < Duration && FadeOutCanvas.alpha != 0)
        {
            Counter += Time.deltaTime;

            FadeOutCanvas.alpha = Mathf.Lerp(1, 0, Counter / Duration);

            yield return null;
        }

        FadeInCanvas.gameObject.SetActive(true);

        Counter = 0;
        Duration = duration;

        while (Counter < Duration && FadeInCanvas.alpha != 1)
        {
            Counter += Time.deltaTime;

            FadeInCanvas.alpha = Mathf.Lerp(0, 1, Counter / Duration);

            yield return null;
        }

        FadeOutCanvas.gameObject.SetActive(false);
    }

    private void Update()
    {
        //Closes the panel if the panel is open.
        if (Input.GetKeyDown(KeyCode.Escape) && FadeInCanvas.alpha == 1)
        {
            InitialiseFade();
        }
    }
}
