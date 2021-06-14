using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnButtonGameObjectActive : MonoBehaviour
{
    public GameObject Object;

    private void Awake()
    {
        transform.GetComponent<Button>().onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        Object.SetActive(!Object.activeSelf);
    }
}
