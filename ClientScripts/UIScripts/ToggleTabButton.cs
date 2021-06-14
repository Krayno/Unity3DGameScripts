using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleTabButton : MonoBehaviour
{
    public GameObject Tab;

    private Button Button;
    public List<Button> ButtonGroup;
    private List<GameObject> TabGroup;

    void Awake()
    {
        Button = GetComponent<Button>();
        Button.onClick.AddListener(ToggleTab);

        //Relies on the Tabs to share the same parent.
        TabGroup = new List<GameObject>();
        foreach(Transform TabTransform in Tab.transform.parent)
        {
            TabGroup.Add(TabTransform.gameObject);
        }
    }

    private void ToggleTab()
    {
        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();

        //Disable the clicked button, enable the other buttons.
        foreach (Button Button in ButtonGroup)
        {
            Button.interactable = Button != this.Button;
        }

        //Enable the tab, disable all other tabs.
        foreach (GameObject Tab in TabGroup)
        {
            Tab.SetActive(Tab == this.Tab);
        }
    }
}
