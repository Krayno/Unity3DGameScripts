using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RotateObjectWithButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Transform Target;
    public bool RotatingLeft;
    public float RotationSpeed;

    public void OnPointerDown(PointerEventData eventData)
    {
        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();

        StartCoroutine(RotateCharacter());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopAllCoroutines();
    }

    private IEnumerator RotateCharacter()
    {
        float TargetRotationY = Target.rotation.eulerAngles.y;
        float Direction = RotatingLeft ? 1 : -1;

        while(true)
        {
            TargetRotationY += Direction * Time.deltaTime * RotationSpeed;
            Target.rotation = Quaternion.Euler(Target.rotation.x, TargetRotationY, Target.rotation.z);
            yield return null;
        }
    }
}
