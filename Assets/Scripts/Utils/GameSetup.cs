using UnityEngine;

public class GameSetup : MonoBehaviour
{
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private Vector3 portalPosition = new Vector3(0, 0, 10);

    void Start()
    {
        
        SpawnPortals();
    }

    private void SpawnPortals()
    {
       
        Instantiate(portalPrefab, portalPosition, Quaternion.identity);
    }
}