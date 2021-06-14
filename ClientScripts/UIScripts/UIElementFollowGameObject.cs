using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIElementFollowGameObject : MonoBehaviour
{
    public Transform UIElement;
    public string CanvasElementTarget;

    private void Start()
    {
        UIElement.SetParent(GameObject.Find(CanvasElementTarget).transform, false);
        UIElement.rotation = Quaternion.identity;
    }

    private void OnDestroy()
    {
        if (UIElement != null)
        {
            Destroy(UIElement.gameObject);
        }
    }

    void Update()
    {
        UIElement.position = Camera.main.WorldToScreenPoint(transform.position);
    }
}
