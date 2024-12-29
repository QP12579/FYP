using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI References")]
    public GameObject selectionUI;
    public Button[] characterSelectButtons;

    void Awake()
    {
        Instance = this;
    }
}
