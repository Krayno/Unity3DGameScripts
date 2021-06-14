using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SwitchScenesButton : MonoBehaviour
{
    public string Scene;
    private Button Button;

    void Awake()
    {
        Button = transform.GetComponent<Button>();

        Button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        SoundManager.Instance.Sounds["UIOpenClose"].Play();

        ClientGlobals.WorldServer.Disconnect();

        SceneManager.LoadSceneAsync(Scene);
    }
}
