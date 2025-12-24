using UnityEngine;

public class MonsterLogic : MonoBehaviour
{
    public enum MonsterState { WAIT, DEMO, SIGNAL, USER }
    public MonsterState currentState = MonsterState.WAIT;

    [Header("Monster Identity")]
    public string monsterType = "A";
    public int patternId;

    [Header("Movement Settings")]
    public float speed = 2.0f;
    public float startTriggerX = 0f;

    [Header("Rhythm Data")]
    public int beatCounter = 0;
    private bool _hasStarted = false;

    [Header("Pattern Data")]
    public string[] command;
    public int signalDuration = 2;

    void Start()
    {
        InitializePattern();
    }

    void InitializePattern()
    {
        if (monsterType == "C")
        {
            patternId = 4;
            command = new string[] { "Y", "-" };
            signalDuration = 2;
        }
        else
        {
            patternId = Random.Range(1, 4);
            if (patternId == 1) command = new string[] { "A", "-", "A", "-", "A", "-" };
            else if (patternId == 2) command = new string[] { "A", "-", "-", "A", "A", "-" };
            else if (patternId == 3) command = new string[] { "A", "A", "-", "A", "A", "-" };
            signalDuration = 2;
        }
    }

    void OnEnable() { RhythmManager.OnBeat += UpdateMonsterBeat; }
    void OnDisable() { RhythmManager.OnBeat -= UpdateMonsterBeat; }

    void Update()
    {
        // 1. Gerakan monster tetap stabil
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        // 2. Memicu StartSequence jika melewati titik koordinat X tertentu
        if (!_hasStarted && transform.position.x <= startTriggerX)
        {
            StartSequence();
        }

        // 3. UPDATE VISUAL KURSOR: Menggerakkan kursor di Timeline saat fase demo/user
        if (_hasStarted && (currentState == MonsterState.DEMO || currentState == MonsterState.USER))
        {
            TimelineController tc = Object.FindAnyObjectByType<TimelineController>();
            if (tc != null)
            {
                // Menghitung progress (0.0 ke 1.0) berdasarkan beatCounter (total 6 beat)
                float progress = beatCounter / 6f;
                tc.UpdateCursor(progress);
            }
        }

        // 4. Hapus monster jika sudah keluar layar kiri
        if (transform.position.x < -15f)
        {
            Destroy(gameObject);
        }
    }

    public void StartSequence()
    {
        if (!_hasStarted)
        {
            _hasStarted = true;
            currentState = MonsterState.DEMO;
            beatCounter = 0;

            // Memanggil TimelineController untuk menampilkan pola notes visual
            TimelineController tc = Object.FindAnyObjectByType<TimelineController>();
            if (tc != null)
            {
                tc.SpawnPattern(command);
            }

            Debug.Log($"Monster {monsterType} Start: Pattern {patternId}");
        }
    }

    void UpdateMonsterBeat(int systemBeat)
    {
        if (!_hasStarted) return;

        beatCounter++;

        switch (currentState)
        {
            case MonsterState.DEMO:
                if (beatCounter >= 4) { currentState = MonsterState.SIGNAL; beatCounter = 0; }
                break;

            case MonsterState.SIGNAL:
                if (beatCounter >= signalDuration) { currentState = MonsterState.USER; beatCounter = 0; }
                break;

            case MonsterState.USER:
                if (beatCounter >= 4) { Debug.Log("User Phase Finished"); }
                break;
        }
    }
}