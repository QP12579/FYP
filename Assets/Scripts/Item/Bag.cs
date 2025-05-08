using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Bag : Singleton<Bag>
{
    public int coins = 0;

    [Header("UI")]
    [SerializeField]
    private TextMeshProUGUI coinText;
    [SerializeField]
    private List<Image> bagBlocks = new List<Image>();
    List<Item> _items = new List<Item>();

    [Header("Reference")]
    [SerializeField]
    private SkillPanel _skillPanel;
    [SerializeField]
    private AbilityPanel _abilityPanel;
    [SerializeField]
    private PlayerBuffSystem _playerBuffSystem;

    private void Start()
    {
        if (_skillPanel == null)
            _skillPanel = FindObjectOfType<SkillPanel>();
        if (_abilityPanel == null) 
        _abilityPanel = FindObjectOfType<AbilityPanel>();
        if(_playerBuffSystem == null)
            _playerBuffSystem = FindObjectOfType<PlayerBuffSystem>();
    }

    public void UpdateBagUI()
    {
        coinText.text = coins.ToString("D3");
        for (int i = 0; i < _items.Count; i++)
        {
            bagBlocks[i].sprite = _items[i].icon;
            bagBlocks[i].color = Color.white;
        }
        for (int i = bagBlocks.Count - 1 ; i > _items.Count-1 ; i--)
        {
            bagBlocks[i].sprite = null;
            bagBlocks[i].color = Color.clear;
        }
    }
    public void AddCoins(int c = 1)
    {
        coins += c;
        UpdateBagUI();
    }

    public void AddSkillPoint(int p)
    {
        _skillPanel.AddSkillPoints(p);
    }

    public void AddBasePoint(int p)
    {
        _abilityPanel.AddBasePoint(p);
    }

    public void AddItem(Item item)
    {
        _items.Add(item);
        UpdateBagUI();
    }

    public void UseItem(Item item)
    {

        UpdateBagUI();
    }

    public void UseBroom()
    {
        _playerBuffSystem.ClearAllDebuffEffect();
    }
}
