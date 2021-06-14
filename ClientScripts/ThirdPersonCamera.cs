using UnityEngine;
using UnityEngine.EventSystems;

public class ThirdPersonCamera : MonoBehaviour
{
    //Original script created by Z0hann on the Unity Forums. Slightly modified by myself, Krayno.
    //Link: https://answers.unity.com/questions/1489636/third-person-wow-like-camera.html

    public Transform target;
 
    public float targetHeight = 1.7f;
    public float distance = 5.0f;
    public float offsetFromWall = 0.1f;
 
    public float maxDistance = 20;
    public float minDistance = .6f;
    public float speedDistance = 5;
 
    public float xSpeed = 200.0f;
    public float ySpeed = 200.0f;
 
    public int yMinLimit = -40;
    public int yMaxLimit = 80;
 
    public int zoomRate = 100;
 
    public float rotationDampening = 3.0f;
    public float zoomDampening = 5.0f;
 
    public LayerMask collisionLayers = -1;
 
    private float xDeg = 0.0f;
    private float yDeg = 0.0f;
    private float currentDistance;
    private float desiredDistance;
    private float correctedDistance;

    void Start ()
    {
        //Somehow sets the camera to be always facing the direction the player while looking down at the player at 30 degrees.
        //- Krayno
        xDeg = target.rotation.eulerAngles.y;
        yDeg = 30;

        currentDistance = distance;
        desiredDistance = distance;
        correctedDistance = distance;
 
        // Make the rigid body not change rotation
        if (gameObject.GetComponent<Rigidbody>())
        {
        gameObject.GetComponent<Rigidbody>().freezeRotation = true;
        }

    }
 
    /**
    * Camera logic on LateUpdate to only update after all character movement logic has been handled.
    */
    void LateUpdate ()
    {
        Vector3 vTargetOffset;
 
        // Don't do anything if target is not defined
        if (!target)
            return;

        // If left mouse button is held, control camera. If right mouse button is held, control camera and player rotation.
        // Modified this section - Krayno
        if (Input.GetMouseButton(0))
        {
            xDeg += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            yDeg -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
        }
        else if (Input.GetMouseButton(1))
        {
            target.rotation = Quaternion.Euler(target.rotation.x, xDeg, target.rotation.z);
            xDeg += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            yDeg -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
        }

        // calculate the desired distance
        if (!ClientGlobals.DisableCameraZoom)
        {
            desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.fixedDeltaTime * zoomRate * speedDistance;
        }
        desiredDistance = Mathf.Clamp (desiredDistance, minDistance, maxDistance);

        yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);

        // set camera rotation
        Quaternion rotation = Quaternion.Euler(yDeg, xDeg, 0);
        correctedDistance = desiredDistance;
 
        // calculate desired camera position
        vTargetOffset = new Vector3 (0, -targetHeight, 0);
        Vector3 position = target.position - (rotation * Vector3.forward * desiredDistance + vTargetOffset);
 
        // check for collision using the true target's desired registration point as set by user using height
        RaycastHit collisionHit;
        Vector3 trueTargetPosition = new Vector3(target.position.x, target.position.y, target.position.z) - vTargetOffset;
 
        // if there was a collision, correct the camera position and calculate the corrected distance
        bool isCorrected = false;
        if (Physics.Linecast (trueTargetPosition, position, out collisionHit, collisionLayers.value))
        {
            // calculate the distance from the original estimated position to the collision location,
            // subtracting out a safety "offset" distance from the object we hit.  The offset will help
            // keep the camera from being right on top of the surface we hit, which usually shows up as
            // the surface geometry getting partially clipped by the camera's front clipping plane.

            if (collisionHit.transform.tag != "Player" || collisionHit.transform.tag == "NetworkPlayer") //Prevent camera from colliding with the players and npcs. -Krayno
            {
                correctedDistance = Vector3.Distance(trueTargetPosition, collisionHit.point) - offsetFromWall;
                isCorrected = true;
            }
        }
 
        // For smoothing, lerp distance only if either distance wasn't corrected, or correctedDistance is more than currentDistance
        currentDistance = !isCorrected || correctedDistance > currentDistance ? Mathf.MoveTowards (currentDistance, correctedDistance, Time.deltaTime * zoomDampening) : correctedDistance;
 
        // keep within legal limits
        currentDistance = Mathf.Clamp (currentDistance, minDistance, maxDistance);
 
        // recalculate position based on the new currentDistance
        position = target.position - (rotation * Vector3.forward * currentDistance + vTargetOffset);

        transform.rotation = rotation;
        transform.position = position;
    }
 
    private static float ClampAngle (float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp (angle, min, max);
    }
}
