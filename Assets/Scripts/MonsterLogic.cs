using UnityEngine;

public class MonsterLogic : MonoBehaviour
{
    public enum MonsterState { WAIT, DEMO, SIGNAL, USER }
    public MonsterState currentState = MonsterState.WAIT;

    [Header("Movement Settings")]
    public float speed = 5.0f; // x -= speed di Python
    public float stopX = -2.0f; // Titik di mana monster berhenti untuk beraksi

    [Header("Rhythm Data")]
    public int beatCounter = 0;
    private bool _hasStarted = false;

    void OnEnable() { RhythmManager.OnBeat += UpdateMonsterBeat; }
    void OnDisable() { RhythmManager.OnBeat -= UpdateMonsterBeat; }

    void Update()
    {
        // Logika pergerakan x -= speed (Hanya bergerak jika belum sampai target)
        if (transform.position.x > stopX)
        {
            transform.Translate(Vector3.left * speed * Time.deltaTime);
        }
        else if (!_hasStarted)
        {
            _hasStarted = true;
            currentState = MonsterState.DEMO;
            Debug.Log("Monster Arrived: Phase DEMO Starts");
        }
    }

    void UpdateMonsterBeat(int systemBeat)
    {
        // Logika State Machine berdasarkan ketukan (Beat)
        if (!_hasStarted) return;

        beatCounter++;

        switch (currentState)
        {
            case MonsterState.DEMO:
                // Di sini nanti kita panggil PlayCue() untuk memberi contoh suara
                if (beatCounter >= 4) { currentState = MonsterState.SIGNAL; beatCounter = 0; }
                break;

            case MonsterState.SIGNAL:
                // Jeda singkat sebelum giliran pemain
                if (beatCounter >= 2) { currentState = MonsterState.USER; beatCounter = 0; }
                break;

            case MonsterState.USER:
                // Di sini kita cek input pemain
                if (beatCounter >= 4) { Destroy(gameObject); } // Monster pergi setelah selesai
                break;
        }
    }
}