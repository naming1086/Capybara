using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSpawner : Singleton<GridSpawner>
{
    [SerializeField]
    private SpawnObject[] collection;

    [SerializeField]
    private int xWidth, zWidth;

    public void SpawnGrid()
    {
        int index = Random.Range(0, collection.Length);

        Vector3 origin = new Vector3(Random.Range(0, 10), 0, Random.Range(0, 10));

        for (int x = 0; x < xWidth; x++)
        {
            for (int z = 0; z < zWidth; z++)
            {
                Vector3 spawnPos = new Vector3(x * collection[index].bounds.x, 0, z * collection[index].bounds.z) + origin;
                SpawnTile(collection[index].gameObject, spawnPos, Quaternion.identity);
            }
        }
    }

    private void SpawnTile(GameObject obj, Vector3 pos, Quaternion rot)
    {
        GameObject tile = Instantiate(obj, pos, rot);
    }
}
