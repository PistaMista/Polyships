using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiscellaneousVariables : MonoBehaviour
{

    public static MiscellaneousVariables it;

    void Start ()
    {
        it = this;
    }


}
