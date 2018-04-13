using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Test : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        Predicate<int> d1 = x => { return x == 1; };
        Predicate<int> d2 = x => { return x > 0; };
        d1 += d2;

        Debug.Log(d1(1));
    }

    // Update is called once per frame
    void Update()
    {

    }
}
