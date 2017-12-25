using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiscellaneousVariables : MonoBehaviour
{
    public int[] boardSizes;
    public GameObject[] shipPrefabs;
    public float playerCameraTransitionTime;
    [Range(0.00f, 1.00f)]
    public float boardTileSideLength;
    public float boardUIRenderHeight;
    public float boardCameraHeightModifier;
    public float boardDistanceFromCenter;
    public float flagVoxelScale;
    public float flagRenderHeight;
    public SlidingUserInterface_Master titleInterfaceMaster;
    public static MiscellaneousVariables it;

    void Start()
    {
        it = this;
        if (boardSizes.Length != 3)
        {
            Debug.LogError("Board size array is not the correct size!");
        }
    }


}
