using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HorizontalResizePanel : MonoBehaviour, IDragHandler, IPointerEnterHandler, IEndDragHandler, IBeginDragHandler, IPointerExitHandler, IPointerDownHandler
{
    public Image ImageToShow;
    public RectTransform Panel;
    public Canvas Canvas;

    private ThirdPersonCamera ThirdPersonCamera;
    private Vector2 CurrentSize;

    public void Awake()
    {
        //Add the Player Preference if they do not have any.
        if (!PlayerPrefs.HasKey("ChatboxWidth"))
        {
            PlayerPrefs.SetFloat("ChatboxWidth", 550);
        }

        if (!PlayerPrefs.HasKey("ChatboxHeight"))
        {
            PlayerPrefs.SetFloat("ChatboxHeight", 400f * Canvas.scaleFactor);
        }
    }

    public void Start()
    {
        ImageToShow.gameObject.SetActive(false);

        //Apply the Player Preference.
        Panel.sizeDelta = new Vector2(PlayerPrefs.GetFloat("ChatboxWidth"), PlayerPrefs.GetFloat("ChatboxHeight"));
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        ImageToShow.gameObject.SetActive(true);
        Panel.sizeDelta = CurrentSize += new Vector2(eventData.delta.x / Canvas.scaleFactor, 0);
    }

    public void OnDrag(PointerEventData eventData)
    {
        ImageToShow.gameObject.SetActive(true);
        Panel.sizeDelta = CurrentSize += eventData.delta / Canvas.scaleFactor;

        Panel.sizeDelta = new Vector2(Mathf.Clamp(Panel.sizeDelta.x, 400f, Screen.currentResolution.width / 2), Mathf.Clamp(Panel.sizeDelta.y, 116.25f * Canvas.scaleFactor, Screen.currentResolution.height / 2));
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ThirdPersonCamera.enabled = true;
        ImageToShow.gameObject.SetActive(false);

        //Set the player preference.
        PlayerPrefs.SetFloat("ChatboxWidth", Panel.sizeDelta.x);
        PlayerPrefs.SetFloat("ChatboxHeight", Panel.sizeDelta.y);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        CurrentSize = Panel.sizeDelta;

        if (ThirdPersonCamera == null)
        {
            ThirdPersonCamera = Camera.main.gameObject.GetComponent<ThirdPersonCamera>();
            ThirdPersonCamera.enabled = false;
        }
        else
        {
            ThirdPersonCamera.enabled = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ImageToShow.gameObject.SetActive(true);
        ClientGlobals.DisableCameraZoom = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ImageToShow.gameObject.SetActive(false);
        ClientGlobals.DisableCameraZoom = false;
    }
}
