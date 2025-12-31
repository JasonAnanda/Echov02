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

    [Header("Rhythm Tolerance Settings")]
    public float beatTolerance = 0.15f;
    private bool[] _hitRegistered;

    [Header("Pattern Data")]
    public string[] command;
    public int signalDuration = 2;
    private int _score = 0;
    private int _totalNotes = 0;

    [Header("Audio Settings")]
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
            command = new string[] { "Y", "-", "-", "-", "-", "-" };
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
        _hitRegistered = new bool[command.Length];
    }

    void OnEnable() { RhythmManager.OnBeat += UpdateMonsterBeat; }
    void OnDisable() { RhythmManager.OnBeat -= UpdateMonsterBeat; }

    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);
        float targetY = (_moveState == 1) ? _baseY + bounceHeight : _baseY;
        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);

        if (transform.position.x < -15f) Destroy(gameObject);
    }

    public void StartSequence()
    {
        if (!_hasStarted && !isDefeated)
        {
            _hasStarted = true;
            _score = 0;
            for (int i = 0; i < _hitRegistered.Length; i++) _hitRegistered[i] = false;

            currentState = MonsterState.DEMO;
            beatCounter = 0;

            TimelineController tc = Object.FindAnyObjectByType<TimelineController>();
            if (tc != null) tc.SpawnPattern(command);

            PlayVoice();
        }
    }

    // PROTEKSI: CheckInput tidak lapor Miss jika bukan giliran Player
    public bool CheckInput(string inputKey, float progress)
    {
        if (isDefeated || currentState != MonsterState.USER) return true;

        float beatInterval = 1f / command.Length;
        for (int i = 0; i < command.Length; i++)
        {
            if (command[i] == "-" || _hitRegistered[i]) continue;
            float targetProgress = i * beatInterval;

            if (progress >= (targetProgress - beatTolerance) && progress <= (targetProgress + beatTolerance))
            {
                if (command[i] == inputKey)
                {
                    _hitRegistered[i] = true;
                    _score++;
                    TimelineController tc = Object.FindAnyObjectByType<TimelineController>();
                    if (tc != null) tc.MarkDiamondHit(i);
                    return true;
                }
            }
        }
        return false;
    }

    void PlayVoice()
    {
        if (monsterVoice == null) return;
        monsterVoice.Stop();
        AudioClip clip = (patternId == 1) ? pattern1Sound : (patternId == 2) ? pattern2Sound : (patternId == 3) ? pattern3Sound : pattern4Sound;
        if (clip != null) { monsterVoice.clip = clip; monsterVoice.Play(); }
    }

    void UpdateMonsterBeat(int systemBeat)
    {
        if (systemBeat == 0) _moveState = (_moveState == 0) ? 1 : 0;
        if (!_hasStarted) return;
        beatCounter++;

        switch (currentState)
        {
            case MonsterState.DEMO:
                if (beatCounter >= 6) { currentState = MonsterState.SIGNAL; beatCounter = 0; if (cueSound != null) monsterVoice.PlayOneShot(cueSound); }
                break;
            case MonsterState.SIGNAL:
                if (beatCounter >= signalDuration)
                {
                    currentState = MonsterState.USER;
                    beatCounter = 0;
                    // NYALAKAN INPUT SAAT USER TURN
                    PlayerInputHandler pih = Object.FindAnyObjectByType<PlayerInputHandler>();
                    if (pih != null) pih.enabled = true;

                    TimelineController tc = Object.FindAnyObjectByType<TimelineController>();
                    if (tc != null) tc.StartManualMovement(1.5f);
                }
                break;
            case MonsterState.USER:
                if (beatCounter >= 6)
                {
                    // MATIKAN INPUT SAAT USER TURN BERAKHIR
                    PlayerInputHandler pih = Object.FindAnyObjectByType<PlayerInputHandler>();
                    if (pih != null) pih.enabled = false;

                    isDefeated = (_score >= _totalNotes);
                    if (isDefeated) GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    _hasStarted = false;
                    currentState = MonsterState.WAIT;
                    beatCounter = 0;
                }
                break;
        }
    }
}