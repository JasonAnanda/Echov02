using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject monsterPrefab;
    public Transform spawnPoint;
    public float spawnInterval = 8f; // Munculkan monster setiap 8 detik

    void Start()
    {
        InvokeRepeating("SpawnMonster", 1f, spawnInterval);
    }

    void SpawnMonster()
    {
        if (monsterPrefab != null && spawnPoint != null)
        {
            Instantiate(monsterPrefab, spawnPoint.position, Quaternion.identity);
        }
    }
}