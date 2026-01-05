using UnityEngine;

public class MonsterLogic : MonoBehaviour
{
    #region Data Structures
    public enum MonsterState { WAIT, DEMO, SIGNAL, USER }
    public MonsterState currentState = MonsterState.WAIT;
    #endregion

    #region Inspector Variables
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

    [Header("Visual Feedback")]
    private SpriteRenderer _sr;
    private Color _originalColor;
    public Color highlightColor = Color.yellow;
    public float flashSpeed = 15f;

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
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _originalColor = _sr.color;

        InitializePattern();
        _baseY = transform.position.y;
    }

    void OnEnable() { RhythmManager.OnBeat += UpdateMonsterBeat; }
    void OnDisable() { RhythmManager.OnBeat -= UpdateMonsterBeat; }

    void Update()
    {
        HandleMovement();
        HandleVisuals();

        if (transform.position.x < -15f) Destroy(gameObject);
    }
    #endregion

    #region Core Logic: Movement & Visuals
    void HandleMovement()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);
        float targetY = (_moveState == 1) ? _baseY + bounceHeight : _baseY;
        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
    }

    void HandleVisuals()
    {
        // UPDATE: Kedipan sekarang aktif di fase DEMO, SIGNAL, dan USER
        if (currentState == MonsterState.DEMO || currentState == MonsterState.SIGNAL || currentState == MonsterState.USER)
        {
            // Smooth Sinusoidal Fade
            float lerp = (Mathf.Sin(Time.time * flashSpeed) + 1.0f) / 2.0f;
            _sr.color = Color.Lerp(_originalColor, highlightColor, lerp);
        }
        else if (isDefeated)
        {
            _sr.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }
        else
        {
            if (_sr.color != _originalColor) _sr.color = _originalColor;
        }
    }
    #endregion

    #region Core Logic: Pattern & Rhythm
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

    void UpdateMonsterBeat(int systemBeat)
    {
        if (systemBeat == 0) _moveState = (_moveState == 0) ? 1 : 0;

        if (!_hasStarted) return;
        beatCounter++;

        HandleStateSwitch();
    }

    void HandleStateSwitch()
    {
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
                    SetPlayerInput(true);
                    TimelineController tc = Object.FindAnyObjectByType<TimelineController>();
                    if (tc != null) tc.StartManualMovement(1.5f);
                }
                break;
            case MonsterState.USER:
                if (beatCounter >= 6)
                {
                    SetPlayerInput(false);
                    isDefeated = (_score >= _totalNotes);
                    _hasStarted = false;
                    currentState = MonsterState.WAIT;
                    beatCounter = 0;
                }
                break;
        }
    }
    #endregion

    #region Input & Audio Helpers
    public bool CheckInput(string inputKey, float progress)
    {
        if (isDefeated || currentState != MonsterState.USER) return true;

        float beatInterval = 1f / command.Length;
        for (int i = 0; i < command.Length; i++)
        {
            if (command[i] == "-" || _hitRegistered[i]) continue;

            float targetProgress = i * beatInterval;
            float currentTolerance = (i == 0) ? beatTolerance + 0.05f : beatTolerance;

            if (progress >= (targetProgress - currentTolerance) && progress <= (targetProgress + currentTolerance))
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

    void SetPlayerInput(bool state)
    {
        PlayerInputHandler pih = Object.FindAnyObjectByType<PlayerInputHandler>();
        if (pih != null) pih.enabled = state;
    }

    void PlayVoice()
    {
        if (monsterVoice == null) return;
        monsterVoice.Stop();
        AudioClip clip = (patternId == 1) ? pattern1Sound : (patternId == 2) ? pattern2Sound : (patternId == 3) ? pattern3Sound : pattern4Sound;
        if (clip != null) { monsterVoice.clip = clip; monsterVoice.Play(); }
    }
    #endregion
}