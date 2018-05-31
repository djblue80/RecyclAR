using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Customizable data for various types of spawnable items 
/// </summary>
[CreateAssetMenu]
public class SpawnedItem : ScriptableObject {

    public enum SpawnType
    {
        MetalWaste,
        PlasticWaste,
        OrganicWaste,
        ElectronicWaste,
        HazardousWaste,
        InertWaste,
        Animal
    }

    public GameObject prefab;

    /// <summary>
    /// Minimum level for this item to spawn
    /// </summary>
    public int minLevel = 1;

    public SpawnType spawnType;
}
