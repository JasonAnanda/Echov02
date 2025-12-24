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

    // --- SESUAI PYTHON: SUARA BERDASARKAN PATTERN_ID ---
    [Header("Audio Settings (Python PATTERN_SOUNDS)")]
    public AudioSource monsterVoice;
    public AudioClip pattern1Sound; // Suara pola 1 utuh
    public AudioClip pattern2Sound; // Suara pola 2 utuh
    public AudioClip pattern3Sound; // Suara pola 3 utuh
    public AudioClip pattern4Sound; // Suara pola 4 utuh (Type C)

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
        transform.Translate(Vector3.left * speed * Time.deltaTime);

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

            // --- DUPLIKASI PYTHON: play_voice() dipanggil saat sequence mulai ---
            PlayVoice();

            Debug.Log($"Monster {monsterType} Start: Pattern {patternId}");
        }
    }

    // --- DUPLIKASI 100% LOGIKA play_voice() PYTHON ---
    void PlayVoice()
    {
        if (monsterVoice == null) return;

        monsterVoice.Stop(); // PATTERN_SOUNDS[id].stop()

        AudioClip clipToPlay = null;
        if (patternId == 1) clipToPlay = pattern1Sound;
        else if (patternId == 2) clipToPlay = pattern2Sound;
        else if (patternId == 3) clipToPlay = pattern3Sound;
        else if (patternId == 4) clipToPlay = pattern4Sound;

        if (clipToPlay != null)
        {
            monsterVoice.clip = clipToPlay;
            monsterVoice.Play(); // PATTERN_SOUNDS[id].play()
        }
    }

    void UpdateMonsterBeat(int systemBeat)
    {
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