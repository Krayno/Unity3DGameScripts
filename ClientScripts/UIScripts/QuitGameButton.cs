using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuitGameButton : MonoBehaviour
{
    private Button Button;

    private void Awake()
    {
        Button = GetComponent<Button>();
        Button.onClick.AddListener(QuitGame);
    }

    private void QuitGame()
    {
        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();

        Application.Quit();
    }
}
