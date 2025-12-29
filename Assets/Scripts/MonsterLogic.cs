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
    public bool isDefeated = false;

    // --- UPDATE: Sistem Toleransi Ritme (Sesuai Python) ---
    [Header("Rhythm Tolerance Settings")]
    public float beatTolerance = 0.15f; // Sesuai BEAT_TOLERANCE di Python
    private bool[] _hitRegistered;      // Untuk mencegah double hit pada satu note

    [Header("Pattern Data")]
    public string[] command;
    public int signalDuration = 2;
    private int _score = 0;
    private int _totalNotes = 0;

    [Header("Audio Settings (Python Style)")]
    public AudioSource monsterVoice;
    public AudioClip pattern1Sound;
    public AudioClip pattern2Sound;
    public AudioClip pattern3Sound;
    public AudioClip pattern4Sound;
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
            command = new string[] { "Y", "-", "-", "-", "-", "-" }; // Disamakan panjangnya menjadi 6
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

        _totalNotes = 0;
        foreach (string s in command) if (s != "-") _totalNotes++;

        // Inisialisasi status hit tiap slot note
        _hitRegistered = new bool[command.Length];
    }

    void OnEnable() { RhythmManager.OnBeat += UpdateMonsterBeat; }
    void OnDisable() { RhythmManager.OnBeat -= UpdateMonsterBeat; }

    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);
        float targetY = (_moveState == 1) ? _baseY + bounceHeight : _baseY;
        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);

        if (transform.position.x < -15f)
        {
            Destroy(gameObject);
        }
    }

    public void StartSequence()
    {
        if (!_hasStarted && !isDefeated)
        {
            _hasStarted = true;
            _score = 0;
            // Reset status hit setiap mulai sequence baru
            for (int i = 0; i < _hitRegistered.Length; i++) _hitRegistered[i] = false;

            currentState = MonsterState.DEMO;
            beatCounter = 0;

            TimelineController tc = Object.FindAnyObjectByType<TimelineController>();
            if (tc != null) tc.SpawnPattern(command);

            PlayVoice();
            Debug.Log($"Monster {monsterType} Start. Target Score: {_totalNotes}");
        }
    }

    // --- UPDATE UTAMA: Logika Pengecekan Input Presisi ---
    public bool CheckInput(string inputKey, float progress)
    {
        if (isDefeated) return false;

        // Interval antar beat adalah 1/6 (0.166...) karena ada 6 slot
        float beatInterval = 1f / command.Length;

        for (int i = 0; i < command.Length; i++)
        {
            // Lewati jika slot kosong atau note sudah pernah kena hit
            if (command[i] == "-" || _hitRegistered[i]) continue;

            // Target progress untuk note ke-i
            float targetProgress = i * beatInterval;

            // Cek apakah progress saat ini masuk dalam jendela toleransi
            if (progress >= (targetProgress - beatTolerance) &&
                progress <= (targetProgress + beatTolerance))
            {
                // Cek kecocokan tombol
                if (command[i] == inputKey)
                {
                    _hitRegistered[i] = true; // Tandai agar tidak bisa di-hit lagi
                    _score++;

                    TimelineController tc = Object.FindAnyObjectByType<TimelineController>();
                    if (tc != null) tc.MarkDiamondHit(i);

                    Debug.Log($"<color=green>HIT!</color> Note {i} pada progress {progress}");
                    return true;
                }
            }
        }

        Debug.Log($"<color=red>MISS!</color> Tidak ada note dalam toleransi pada progress {progress}");
        return false;
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
                    TimelineController tc = Object.FindAnyObjectByType<TimelineController>();
                    // Durasi movement kursor adalah 6 beat murni agar sinkron
                    if (tc != null) tc.StartManualMovement(1.5f);
                }
                break;

            case MonsterState.USER:
                if (beatCounter >= 6)
                {
                    if (_score >= _totalNotes)
                    {
                        Debug.Log("Monster Tuntas!");
                        isDefeated = true;
                        GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    }
                    else
                    {
                        Debug.Log($"Gagal! Score: {_score}/{_totalNotes}");
                    }

                    _hasStarted = false;
                    currentState = MonsterState.WAIT;
                    beatCounter = 0;
                }
                break;
        }
    }
}