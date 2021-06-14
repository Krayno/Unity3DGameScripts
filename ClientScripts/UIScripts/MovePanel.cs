using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MovePanel : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerDownHandler
{
    public RectTransform Panel;
    public Canvas Canvas;
    private RectTransform CanvasRect;
    ThirdPersonCamera ThirdPersonCamera;

    private void Awake()
    {
        CanvasRect = Canvas.GetComponent<RectTransform>();
    }

    private void Start()
    {
        ThirdPersonCamera = Camera.main.gameObject.GetComponent<ThirdPersonCamera>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId == -1)
        {
            ThirdPersonCamera.enabled = false;
            Panel.anchoredPosition += eventData.delta / Canvas.scaleFactor;

            //Original script created by RunningIVIan on the Unity Forums.
            //Link: https://forum.unity.com/threads/keep-ui-objects-inside-screen.523766/
            var sizeDelta = CanvasRect.sizeDelta - Panel.sizeDelta;
            var panelPivot = Panel.pivot;
            var position = Panel.anchoredPosition;
            position.x = Mathf.Clamp(position.x, -sizeDelta.x * panelPivot.x, sizeDelta.x * (1 - panelPivot.x));
            position.y = Mathf.Clamp(position.y, -sizeDelta.y * panelPivot.y, sizeDelta.y * (1 - panelPivot.y));
            Panel.anchoredPosition = position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ThirdPersonCamera.enabled = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Panel.SetAsLastSibling();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerId == -1)
        {
            ThirdPersonCamera.enabled = false;
        }
    }
}
