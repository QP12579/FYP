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

        if (player.isLocalPlayer)
            Debug.Log(" is local ");

        if (player != null)
            Debug.Log(" player found ");

        if (player = null)
            Debug.Log(" player not found ");


        if (player != null && player.isLocalPlayer)
            
        {
            
            PlayerData.HP = player.HP;
            PlayerData.MP = player.MP;
            PlayerData.Level = player.level;

           
            player.CmdRequestSceneChange(targetSceneName);
        }
    }
}