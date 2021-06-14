using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsPanelGameWorld : MonoBehaviour
{
    private ThirdPersonCamera ThirdPersonCamera;

    //Im so tired, as long as it works I guess...
    private void Awake()
    {
        ThirdPersonCamera = Camera.main.GetComponent<ThirdPersonCamera>();
    }

    private void Update()
    {
        if (transform.parent.gameObject.activeInHierarchy)
        {
            ThirdPersonCamera.enabled = false;
        }
        else
        {
            ThirdPersonCamera.enabled = true;
        }
    }

    private void OnDisable()
    {
        if (ThirdPersonCamera != null)
        {
            ThirdPersonCamera.enabled = true;
        }
    }

}
