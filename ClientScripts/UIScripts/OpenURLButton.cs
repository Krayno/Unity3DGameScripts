using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenURLButton : MonoBehaviour
{
    public string URL;

    private Button Button;

    private void Awake()
    {
        Button = GetComponent<Button>();
        Button.onClick.AddListener(OpenURL);
    }

    private void OpenURL()
    {
        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();

        Application.OpenURL(URL);
    }
}
