using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    float twirl = 1;
    float cameraSpeed = 5;

    void Update()
    {
        twirl += (1 * cameraSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(32, twirl, 0);
    }
}
