using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MinimapZoom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text ZoomLevelText;
    public Camera MinimapCamera;

    public int MaximumCameraZoom;
    public int MinimumCameraZoom;

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartCoroutine("MouseOverMinimap");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
    }

    private IEnumerator MouseOverMinimap()
    {
        while (true)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0 && MinimapCamera.orthographicSize > MinimumCameraZoom)
            {
                MinimapCamera.orthographicSize -= 5;
                ZoomLevelText.text = (((MinimapCamera.orthographicSize - MinimumCameraZoom) / 5) + 1).ToString();
            }

            if (Input.GetAxis("Mouse ScrollWheel") < 0 && MinimapCamera.orthographicSize < MaximumCameraZoom)
            {
                MinimapCamera.orthographicSize += 5;
                ZoomLevelText.text = (((MinimapCamera.orthographicSize - MinimumCameraZoom) / 5) + 1).ToString();
            }

            yield return null;
        }
    }
}
