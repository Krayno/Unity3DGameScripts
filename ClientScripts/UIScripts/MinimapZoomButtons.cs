using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinimapZoomButtons : MonoBehaviour
{
    public Button ZoomInButton;
    public Button ZoomOutButton;
    public TMP_Text ZoomLevelText;
    public Camera MinimapCamera;

    public int MaximumCameraZoom;
    public int MinimumCameraZoom;

    void Awake()
    {
        //Add listeners to the Zoom Buttons.
        ZoomInButton.onClick.AddListener(MinimapZoomIn);
        ZoomOutButton.onClick.AddListener(MinimapZoomOut);
    }

    private void MinimapZoomOut()
    {
        if (MinimapCamera.orthographicSize < MaximumCameraZoom)
        {
            MinimapCamera.orthographicSize += 5;
            ZoomLevelText.text = (((MinimapCamera.orthographicSize - MinimumCameraZoom) / 5) + 1).ToString();
        }
    }

    private void MinimapZoomIn()
    {
        if (MinimapCamera.orthographicSize > MinimumCameraZoom)
        {
            MinimapCamera.orthographicSize -= 5;
            ZoomLevelText.text = (((MinimapCamera.orthographicSize - MinimumCameraZoom) / 5) + 1).ToString();
        }
    }
}
