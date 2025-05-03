using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] private string targetSceneName;
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;

    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();

        if (player != null && player.isLocalPlayer)
        {
            
            PlayerData.HP = player.HP;
            PlayerData.MP = player.MP;
            PlayerData.Level = player.level;

           
            player.CmdRequestSceneChange(targetSceneName);
        }
    }
}