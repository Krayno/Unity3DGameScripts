using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObjectWithLeftMouseButton : MonoBehaviour
{
    public float RotationSpeed;
    public Transform Target;

    private float InitialTargetRotationY;


    void Awake()
    {
        Target = gameObject.transform;
        InitialTargetRotationY = Target.rotation.eulerAngles.y;
    }

    void Update()
    {

        if (Input.GetMouseButton(0))
        {
            //If the player anything else to rotate the character, InitialTargetRotationY will become out of sync.
            if (InitialTargetRotationY != Target.rotation.eulerAngles.y)
            {
                InitialTargetRotationY = Target.rotation.eulerAngles.y;
            }

            float MouseXDelta = Input.GetAxis("Mouse X");

            InitialTargetRotationY -= MouseXDelta * Time.deltaTime * RotationSpeed;

            Target.rotation = Quaternion.Euler(Target.rotation.x, InitialTargetRotationY, Target.rotation.z);
        }
    }
}

