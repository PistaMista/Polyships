using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Test : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        Action x = () => { };
        x += () => Debug.Log("Test Worked");
        x();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
