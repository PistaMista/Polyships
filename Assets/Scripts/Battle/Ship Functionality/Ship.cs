using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum ShipType
{
    BATTLESHIP,
    CRUISER,
    DESTROYER,
    CARRIER,
    PATROLBOAT
}

public class Ship : MonoBehaviour
{
    public struct ShipData
    {
        public int metadata;
        public static implicit operator ShipData(Ship ship)
        {
            return null;
        }
    }

    public int index;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
