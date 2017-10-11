using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    static Waypoint currentWaypoint;
    static float transitionTime;
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
    static RotationCalculator rotationCalculator = () => { return Mathf.Pow(Quaternion.Angle(Camera.main.transform.rotation, currentWaypoint.transform.rotation), 1.5f) / Mathf.Pow(transitionTime, 1.5f) * Time.deltaTime; };

    public delegate float RotationCalculator();

    void Update()
    {
        if (currentWaypoint != null)
        {
            transform.position = Vector3.SmoothDamp(transform.position, currentWaypoint.transform.position, ref transitionVelocity, transitionTime);
            transform.rotation = Quaternion.RotateTowards(Camera.main.transform.rotation, currentWaypoint.transform.rotation, rotationCalculator());
        }
    }

    public static void GoToWaypoint(Waypoint waypoint, float transitionTime, RotationCalculator rotationProgressCalculator)
    {
        CameraControl.rotationCalculator = rotationProgressCalculator;
        CameraControl.transitionTime = transitionTime;
        CameraControl.currentWaypoint = waypoint;
        initialPosition = Camera.main.transform.position;
    }

    public static void GoToWaypoint(Waypoint waypoint, float transitionTime)
    {
        //rotationCalculator = () => { return (targetRotationDelta.w - Camera.main.transform.rotation.w); };
        CameraControl.transitionTime = transitionTime;
        CameraControl.currentWaypoint = waypoint;
        initialPosition = Camera.main.transform.position;
    }
}
