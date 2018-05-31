using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// An entity that spawns objects at a feature point provided by the active AR API.
/// </summary>
public class ItemSpawner : MonoBehaviour {

    private WaitForSeconds cachedSpawnWait;

    /// <summary>
    /// Maximum time offset for items to start spawning. In seconds.
    /// </summary>
    const float maxStartOffset = 5;


    /// <summary>
    /// Unity start function
    /// </summary>
    private void Start()
    {
        cachedSpawnWait = new WaitForSeconds(Random.Range(GameManager.Instance.currentMaxSpawnWait, GameManager.MaxSpawnWait));
        StartCoroutine(_SpawnObjects());
    }

    /// <summary>
    /// Coroutine to spawn objects indefinitely. Based on the GameManager's current spawn pool.
    /// </summary>
    /// <returns></returns>
    private IEnumerator _SpawnObjects()
    {
        yield return new WaitForSeconds(Random.Range(0, maxStartOffset));
        int spawnNdx;
        while (true)
        {
            spawnNdx = Random.Range(0, GameManager.Instance.currentSpawnPool.Count);
            GameObject instance = Instantiate(GameManager.Instance.currentSpawnPool[spawnNdx], transform.position, Quaternion.identity,GameManager.Instance.inGameObjectRoot);
            instance.transform.localScale = Vector3.one * GameManager.UnitScaleMultiplier;
            yield return cachedSpawnWait;
        }
    }

}
