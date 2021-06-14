using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnMouseOverDisableCameraControls : MonoBehaviour, IPointerDownHandler
{
    private ThirdPersonCamera ThirdPersonCamera;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (ThirdPersonCamera != null && Input.GetMouseButtonDown(0))
        {
            ThirdPersonCamera.enabled = false;
        }
    }

    public void Update()
    {
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButton(1))
        {
            if (ThirdPersonCamera != null)
            {
                ThirdPersonCamera.enabled = true;
            }
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            ClientGlobals.DisableCameraZoom = true;
        }
        else
        {
            ClientGlobals.DisableCameraZoom = false;
        }
    }

    void Awake()
    {
        StartCoroutine("GetThirdPersonCamera");
    }

    IEnumerator GetThirdPersonCamera()
    {
        while (ThirdPersonCamera == null)
        {
            if (GameObject.FindWithTag("MainCamera"))
            {
                ThirdPersonCamera = GameObject.FindWithTag("MainCamera").GetComponent<ThirdPersonCamera>();
            }
            yield return new WaitForSeconds(1);
        }
    }
}
