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

    [Header("Bounce Settings")]
    public float bounceHeight = 0.3f;
    private float _baseY;
    private int _moveState = 1; // 1 = Mulai di atas (kebalikan player yang mulai di 0)

    [Header("Rhythm Data")]
    public int beatCounter = 0;
    private bool _hasStarted = false;

    [Header("Pattern Data")]
    public string[] command;
    public int signalDuration = 2;

    [Header("Audio Settings (Python Style)")]
    public AudioSource monsterVoice;
    public AudioClip pattern1Sound;
    public AudioClip pattern2Sound;
    public AudioClip pattern3Sound;
    public AudioClip pattern4Sound;

    void Start()
    {
        InitializePattern();
        // Simpan posisi Y awal agar bounce tetap konsisten saat bergerak maju
        _baseY = transform.position.y;
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
        // Gerakan horizontal tetap menggunakan transform.Translate
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        // Update posisi Y secara terpisah berdasarkan _moveState
        float targetY = (_moveState == 1) ? _baseY + bounceHeight : _baseY;
        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);

        if (!_hasStarted && transform.position.x <= startTriggerX)
        {
            StartSequence();
        }

        if (_hasStarted && (currentState == MonsterState.DEMO || currentState == MonsterState.USER))
        {
            TimelineController tc = Object.FindAnyObjectByType<TimelineController>();
            if (tc != null)
            {
                float progress = beatCounter / 6f;
                tc.UpdateCursor(progress);
            }
        }

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

            TimelineController tc = Object.FindAnyObjectByType<TimelineController>();
            if (tc != null) tc.SpawnPattern(command);

            PlayVoice();
            Debug.Log($"Monster {monsterType} Start: Pattern {patternId}");
        }
    }

    void PlayVoice()
    {
        if (monsterVoice == null) return;
        monsterVoice.Stop();

        AudioClip clipToPlay = null;
        if (patternId == 1) clipToPlay = pattern1Sound;
        else if (patternId == 2) clipToPlay = pattern2Sound;
        else if (patternId == 3) clipToPlay = pattern3Sound;
        else if (patternId == 4) clipToPlay = pattern4Sound;

        if (clipToPlay != null)
        {
            monsterVoice.clip = clipToPlay;
            monsterVoice.Play();
        }
    }

    void UpdateMonsterBeat(int systemBeat)
    {
        // LOGIKA BOUNCE: Berlawanan dengan Player
        // Kita gunakan systemBeat (0 atau 1) yang dikirim dari RhythmManager
        if (systemBeat == 0)
        {
            // Jika player pindah ke 1 (atas), monster pindah ke 0 (bawah) atau sebaliknya
            _moveState = (_moveState == 0) ? 1 : 0;
        }

        if (!_hasStarted) return;

        beatCounter++;

        switch (currentState)
        {
            case MonsterState.DEMO:
                if (beatCounter >= 6) { currentState = MonsterState.SIGNAL; beatCounter = 0; }
                break;
            case MonsterState.SIGNAL:
                if (beatCounter >= signalDuration) { currentState = MonsterState.USER; beatCounter = 0; }
                break;
            case MonsterState.USER:
                if (beatCounter >= 6) { Debug.Log("User Phase Finished"); }
                break;
        }
    }
}