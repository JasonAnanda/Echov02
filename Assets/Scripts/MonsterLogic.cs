using UnityEngine;

public class MonsterLogic : MonoBehaviour
{
    public enum MonsterState { WAIT, DEMO, SIGNAL, USER }
    public MonsterState currentState = MonsterState.WAIT;

    [Header("Movement Settings")]
    public float speed = 2.0f; // Sesuai Python: x -= speed
    public float startTriggerX = 0f; // Titik koordinat X saat monster mulai beraksi

    [Header("Rhythm Data")]
    public int beatCounter = 0;
    private bool _hasStarted = false;

    void OnEnable() { RhythmManager.OnBeat += UpdateMonsterBeat; }
    void OnDisable() { RhythmManager.OnBeat -= UpdateMonsterBeat; }

    void Update()
    {
        // MONSTER JALAN TERUS (Tanpa syarat stopX)
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        // Memicu fase aksi jika melewati titik tertentu, tapi tetap jalan terus
        if (!_hasStarted && transform.position.x <= startTriggerX)
        {
            _hasStarted = true;
            currentState = MonsterState.DEMO;
            Debug.Log("Monster reached trigger point: Phase DEMO Starts");
        }

        // Hapus monster jika sudah terlalu jauh ke kiri (keluar layar)
        if (transform.position.x < -15f)
        {
            Destroy(gameObject);
        }
    }

    void UpdateMonsterBeat(int systemBeat)
    {
        // Logika State Machine berjalan di latar belakang sambil monster bergerak
        if (!_hasStarted) return;

        beatCounter++;

        switch (currentState)
        {
            case MonsterState.DEMO:
                if (beatCounter >= 4) { currentState = MonsterState.SIGNAL; beatCounter = 0; }
                break;

            case MonsterState.SIGNAL:
                if (beatCounter >= 2) { currentState = MonsterState.USER; beatCounter = 0; }
                break;

            case MonsterState.USER:
                // Di sini nanti kita cek input pemain
                // Monster tidak hancur di sini, tapi di pengecekan posisi X (Update)
                if (beatCounter >= 4) { Debug.Log("Monster Phase Finished"); }
                break;
        }
    }
}