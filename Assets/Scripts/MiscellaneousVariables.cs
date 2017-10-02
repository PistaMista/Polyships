using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiscellaneousVariables : MonoBehaviour
{
    public int[] boardSizes;
    public GameObject[] shipPrefabs;
    public float playerCameraTransitionTime;
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
