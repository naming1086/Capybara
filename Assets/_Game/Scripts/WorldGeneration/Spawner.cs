﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    public enum SpawnType { Pickup, Foliage, EnclosureGroup, Enclosure };

    public SpawnType spawnType;

    [Range(1, 100)]
    public int spawnChance = 60;

    void Start()
    {
        if (Random.Range(1, 100) <= spawnChance)
        {
            ObjectSpawner.Instance.PlaceObjectFromSpawner(ObjectManager.Instance.GetSpawnGroup(spawnType.ToString()), transform.position);
        }        
    }
}
