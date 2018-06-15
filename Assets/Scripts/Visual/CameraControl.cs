using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public static Waypoint currentWaypoint;
    public static float TransitionProgress
    {
        get
        {
            float totalDistance = Vector3.Distance(initialPosition, currentWaypoint.transform.position);
            float remainingDistance = Vector3.Distance(Camera.main.transform.position, currentWaypoint.transform.position);
            return (totalDistance - remainingDistance) / totalDistance;
        }
    }

    static Vector3 transitionVelocity;
    static Vector3 initialPosition;
    static RotationCalculator rotationCalculator = () => { return Mathf.Pow(Quaternion.Angle(Camera.main.transform.rotation, currentWaypoint.transform.rotation), 1.5f) / Mathf.Pow(1.0f, 1.5f) * Time.deltaTime; };

    public delegate float RotationCalculator();

    void Update()
    {
        if (currentWaypoint != null)
        {
            transform.position = Vector3.SmoothDamp(transform.position, currentWaypoint.transform.position, ref transitionVelocity, currentWaypoint.transitionTime);
            transform.rotation = Quaternion.RotateTowards(Camera.main.transform.rotation, currentWaypoint.transform.rotation, rotationCalculator());
        }
    }

    public static void GoToWaypoint(Waypoint waypoint, RotationCalculator rotationProgressCalculator)
    {
        rotationCalculator = rotationProgressCalculator;
        currentWaypoint = waypoint;
        initialPosition = Camera.main.transform.position;
    }

    public static void GoToWaypoint(Waypoint waypoint)
    {
        currentWaypoint = waypoint;
        initialPosition = Camera.main.transform.position;
    }

    public static float CalculateCameraWaypointHeight(Vector2 size)
    {
        float aspect = Camera.main.aspect;
        float verticalFov = Camera.main.fieldOfView;
        float horizontalFov = Mathf.Atan(Mathf.Tan(verticalFov / 2.0f * Mathf.Deg2Rad) * aspect) * Mathf.Rad2Deg * 2.0f;

        float verticalHeight = (1 / Mathf.Tan(Mathf.Deg2Rad * verticalFov / 2.0f)) * (size.y / 2.0f);
        float horizontalHeight = (1 / Mathf.Tan(Mathf.Deg2Rad * horizontalFov / 2.0f)) * (size.x / 2.0f);

        return verticalHeight > horizontalHeight ? verticalHeight : horizontalHeight;
    }
}