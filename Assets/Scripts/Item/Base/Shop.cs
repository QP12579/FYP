using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class Shop : NetworkBehaviour
{
    [SerializeField]
    private List<Transform> spawnPoints;
    [SerializeField]
    private List<GameObject> spawnItems;
    [SerializeField]
    private ItemType[] canSpawntype;

    public float tariffs = 2;
    private List<ShopItem> spawnedItems;

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
            CmdSpawnItems(FindShopObject(willSpawnTypes[i]), spawnPoints[i]);
        }
    }

    GameObject FindShopObject(ItemType type)
    {
        foreach (var item in spawnItems) 
        {
            ShopItem shopItem = item.GetComponent<ShopItem>();
            Debug.Log("Get " + shopItem);
            spawnedItems.Add(shopItem);
            if(shopItem.itemPrefab.item.Type == type)
            {
                Debug.Log(item);
                return item;

            }
        }
        
        return null;
    }

    [Command] 
    void CmdSpawnItems(GameObject spawnObject, Transform spawnpoint)
    {
        GameObject ShopItem = Instantiate(spawnObject, spawnpoint.position, spawnpoint.rotation);

        NetworkServer.Spawn(ShopItem);
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

    public void IncreaseTariffs()
    {
        foreach (var item in spawnedItems) 
        {
            item.cost = (int)(item.cost * tariffs);
        }
    }
}
