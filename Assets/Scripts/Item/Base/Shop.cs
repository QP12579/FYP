using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [SerializeField]
    private List<Transform> spawnPoints;
    [SerializeField]
    private List<GameObject> spawnItems;
    [SerializeField]
    private ItemType[] canSpawntype;

    // Start is called before the first frame update
    void Start()
    {
        ReadyToOpenShop();
    }

    void ReadyToOpenShop()
    {
        List<ItemType> willSpawnTypes = GetRandomElements(canSpawntype, spawnPoints.Count);
        for (int i = 0; i < spawnPoints.Count; i++) 
        {
            SpawnItems(FindShopObject(willSpawnTypes[i]), spawnPoints[i]);
        }
    }

    GameObject FindShopObject(ItemType type)
    {
        foreach (var item in spawnItems) 
        {
            if(item.GetComponent<ShopItem>().itemPrefab.item.Type == type)
            {
                return item;
            }
        }
        Debug.Log("");
        return null;
    }

    void SpawnItems(GameObject spawnObject, Transform spawnpoint)
    {
        GameObject ShopItem = Instantiate(
            spawnObject,
            spawnpoint.position,
            spawnpoint.rotation
            );
    }

    private List<T> GetRandomElements<T>(T[] array, int count)
    {
        List<T> list = new List<T>(array);
        List<T> result = new List<T>();

        while (result.Count < count && list.Count > 0)
        {
            int index = Random.Range(0, list.Count);
            result.Add(list[index]);
            list.RemoveAt(index);
        }
        return result;
    }
}
