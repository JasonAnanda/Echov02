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
    private int _moveState = 1;

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

    // --- UPDATE: SUARA CUE TURN PLAYER ---
    public AudioClip cueSound;

    void Start()
    {
        InitializePattern();
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
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        float targetY = (_moveState == 1) ? _baseY + bounceHeight : _baseY;
        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);

        if (!_hasStarted && transform.position.x <= startTriggerX)
        {
            StartSequence();
        }

        // --- UPDATE: Logika UpdateCursor dihapus karena kursor bergerak otomatis ---
        // Kursor sekarang dikontrol secara internal oleh TimelineController setelah dipicu.

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
            // Munculkan pola diamond, tapi kursor jangan jalan dulu
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
        if (systemBeat == 0)
        {
            _moveState = (_moveState == 0) ? 1 : 0;
        }

        if (!_hasStarted) return;

        beatCounter++;

        switch (currentState)
        {
            case MonsterState.DEMO:
                if (beatCounter >= 6)
                {
                    currentState = MonsterState.SIGNAL;
                    beatCounter = 0;

                    if (monsterVoice != null && cueSound != null)
                        monsterVoice.PlayOneShot(cueSound);
                }
                break;

            case MonsterState.SIGNAL:
                if (beatCounter >= signalDuration)
                {
                    currentState = MonsterState.USER;
                    beatCounter = 0;

                    // --- UPDATE: Kursor MULAI meluncur saat gilirian PLAYER (Linear) ---
                    TimelineController tc = Object.FindAnyObjectByType<TimelineController>();
                    if (tc != null)
                    {
                        // Durasi 6 beat fase USER. 
                        // Jika interval beat adalah 0.5 detik, maka durasi meluncur adalah 3 detik.
                        float beatInterval = 0.25f;
                        float totalDuration = 6 * beatInterval;
                        tc.StartManualMovement(totalDuration);
                    }
                }
                break;

            case MonsterState.USER:
                if (beatCounter >= 6)
                {
                    // Update: Logic ShowTimeline(false) dihapus agar timeline tetap tampil
                    Debug.Log("User Phase Finished");
                    _hasStarted = false;
                    currentState = MonsterState.WAIT;
                }
                break;
        }
    }
}