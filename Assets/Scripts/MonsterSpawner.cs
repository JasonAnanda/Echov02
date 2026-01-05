using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject monsterAPrefab;
    public GameObject monsterCPrefab;

    [Header("Spawn Points")]
    public Transform spawnPoint;

    [Header("Interval Settings")]
    public float intervalA = 8f;   // Monster A setiap 8 detik
    public float intervalC = 20f;  // Monster C lebih jarang (contoh: 20 detik)

    [Header("Height Settings")]
    public float birdYOffset = 3.0f; // Tinggi terbang untuk tipe C

    void Start()
    {
        // Menjalankan spawn masing-masing dengan waktu yang berbeda
        if (monsterAPrefab != null)
        {
            InvokeRepeating("SpawnMonsterA", 1f, intervalA);
        }

        if (monsterCPrefab != null)
        {
            InvokeRepeating("SpawnMonsterC", 5f, intervalC); // Dimulai di detik ke-5 agar tidak bentrok barengan A di awal
        }
    }

    void SpawnMonsterA()
    {
        if (spawnPoint != null)
        {
            Instantiate(monsterAPrefab, spawnPoint.position, Quaternion.identity);
        }
    }

    void SpawnMonsterC()
    {
        if (spawnPoint != null)
        {
            // Tambahkan offset tinggi untuk tipe C (Burung)
            Vector3 birdPos = spawnPoint.position;
            birdPos.y += birdYOffset;

            Instantiate(monsterCPrefab, birdPos, Quaternion.identity);
        }
    }
}