using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class InputfieldNavigation : MonoBehaviour
{
    public List<TMP_InputField> InputFields;
    public KeyCode NavigationKey;

    private int Index = 0;

    private void Awake()
    {
        InputFields = new List<TMP_InputField>();
    }

    private void Start()
    {
        EventSystem.current.SetSelectedGameObject(InputFields[Index++].gameObject);
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (Index == InputFields.Count)
            {
                Index = 0;
            }
            EventSystem.current.SetSelectedGameObject(InputFields[Index++].gameObject);
        }
    }
}
