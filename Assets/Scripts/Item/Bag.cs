using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bag : MonoBehaviour
{
    List<BaseItem> _items = new List<BaseItem>();

    public void GetItem(BaseItem item)
    {
        _items.Add(item);
    }
}
