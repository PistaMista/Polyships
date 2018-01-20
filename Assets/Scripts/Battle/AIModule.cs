﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AIModule : ScriptableObject
{
    [Serializable]
    public struct AIModuleData
    {
        public static implicit operator AIModuleData(AIModule module)
        {
            AIModuleData result;
            return result;
        }
    }
    public Player owner;

    public virtual void Initialize(AIModuleData data)
    {

    }

    public virtual void AssignReferences(AIModuleData data)
    {

    }

    public void DoTurn()
    {
        if (owner.board.ships == null)
        {
            owner.board.AddShips();
            PlaceShips();
            Battle.main.NextTurn();
        }
        else
        {
            Attack();
        }
    }

    void PlaceShips()
    {
        // float targetTorpedoAttack = UnityEngine.Random.Range(0.00f, 1.00f);
        // float targetTorpedoDefense = UnityEngine.Random.Range(0.00f, 1.00f);

        // float targetShipDensity = UnityEngine.Random.Range(0.00f, 1.00f);
        foreach (Ship ship in owner.board.placementInfo.placedShips)
        {
            ship.Pickup();
            ship.Place(null);
        }
        owner.board.ReevaluateTiles();


        // for (int i = 0; i < owner.board.ships.Length; i++)
        // {
        //     Ship highestRatedShip = owner.board.ships[0];
        //     float highestShipRating = 0;

        //     //DETERMINE WHICH SHIP TO SELECT NEXT
        //     foreach (Ship ship in owner.board.placementInfo.notplacedShips)
        //     {
        //         float shipRating = 0;

        //         shipRating *= UnityEngine.Random.Range(0.9f, 1.1f);
        //         if (shipRating > highestShipRating)
        //         {
        //             highestShipRating = shipRating;
        //             highestRatedShip = ship;
        //         }
        //     }

        //     highestRatedShip.Pickup();

        // }

        //Place ships randomly
        for (int i = 0; i < owner.board.ships.Length; i++)
        {
            Ship ship = owner.board.ships[i];
            ship.gameObject.SetActive(false);
            ship.Pickup();
            for (int x = 0; x < ship.maxHealth; x++)
            {
                owner.board.SelectTileForPlacement(owner.board.placementInfo.selectableTiles[UnityEngine.Random.Range(0, owner.board.placementInfo.selectableTiles.Count)]);
            }

            if (owner.board.placementInfo.selectableTiles.Count == 0)
            {
                PlaceShips();
                break;
            }
        }
    }

    void Attack()
    {

    }
}
