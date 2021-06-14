using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapPanel : MonoBehaviour, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public KeybindManager KeybindManager;
    public Camera MapCamera;
    ThirdPersonCamera ThirdPersonCamera;
    public GameObject Player;

    public static float MapCameraZoom = 50;
    float DragSpeed;

    public Canvas Canvas;

    public bool MouseIsIn;

    private void Start()
    {
        MouseIsIn = false;
        MapCamera = GameObject.Find("MapCamera").GetComponent<Camera>();
        ThirdPersonCamera = Camera.main.gameObject.GetComponent<ThirdPersonCamera>();
    }

    private void OnEnable()
    {
        Player = GameObject.FindWithTag("Player");
        MapCamera.transform.position = Player.transform.position;
    }

    private void OnDisable()
    {
        if (ThirdPersonCamera != null)
        {
            ThirdPersonCamera.enabled = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            ThirdPersonCamera.enabled = false;
            DragSpeed = (0.089f * MapCameraZoom / 32) / Canvas.scaleFactor;
            Vector2 mouseDelta = eventData.delta;
            MapCamera.transform.position -= new Vector3(mouseDelta.x * DragSpeed, 0, mouseDelta.y * DragSpeed); //Moving camera.
        }
        else
        {
            ThirdPersonCamera.enabled = true;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ThirdPersonCamera.enabled = true;
        MouseIsIn = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MouseIsIn = false;
        ThirdPersonCamera.enabled = true;
    }

    void Update()
    {
        if (MouseIsIn)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                MapCameraZoom -= Input.GetAxis("Mouse ScrollWheel") * 20;
                MapCameraZoom = Mathf.Clamp(MapCameraZoom, 20, 500);
                MapCamera.orthographicSize = MapCameraZoom;
            }
        }
    }
}
