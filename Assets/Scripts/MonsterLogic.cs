using UnityEngine;
using System.Collections.Generic;

public class MonsterLogic : MonoBehaviour
{
    #region Data Structures
    public enum MonsterState { WAIT, USER }
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
    private float _startTime; // Penting untuk deteksi durasi

    [Header("Rhythm Tolerance Settings")]
    public float beatTolerance = 0.15f;
    private bool[] _hitRegistered;

    [Header("Pattern Data (Updated for 4 Beats)")]
    private float[] timingBeat;
    public string[] command;
    private int _score = 0;
    private int _totalNotes = 0;

    [Header("Audio Settings")]
    public AudioSource monsterVoice;
    public AudioClip pattern1Sound;
    public AudioClip pattern2Sound;
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

        // FIX: Pastikan input tidak mati sebelum kursor sampai ujung (2 detik)
        if (_hasStarted && currentState == MonsterState.USER)
        {
            if (Time.time - _startTime >= 2.1f) // Toleransi sedikit agar beat ke-4 tetap bisa dipukul
            {
                EndSequence();
            }
        }

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
        if (currentState == MonsterState.USER)
        {
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
        patternId = Random.Range(1, 3);
        switch (patternId)
        {
            case 1:
                timingBeat = new float[] { 0f, 1f, 2f, 2.5f, 3f };
                command = new string[] { "A", "A", "B", "B", "B" };
                break;
            case 2:
                timingBeat = new float[] { 0f, 1f, 1.5f, 2f, 2.5f, 3f };
                command = new string[] { "A", "B", "B", "A", "B", "B" };
                break;
        }

        _totalNotes = 0;
        foreach (string s in command) if (s == "B") _totalNotes++;
        _hitRegistered = new bool[command.Length];
    }

    public void StartSequence()
    {
        if (!_hasStarted && !isDefeated)
        {
            _hasStarted = true;
            _startTime = Time.time; // Catat waktu mulai di sini
            _score = 0;
            for (int i = 0; i < _hitRegistered.Length; i++) _hitRegistered[i] = false;

            currentState = MonsterState.USER;
            beatCounter = 0;

            TimelineController tc = Object.FindAnyObjectByType<TimelineController>();
            if (tc != null)
            {
                tc.SpawnPatternRhythmDoctor(timingBeat, command);
                tc.StartManualMovement();
            }

            PlayVoice();
            SetPlayerInput(true);
        }
    }

    void UpdateMonsterBeat(int systemBeat)
    {
        if (systemBeat == 0) _moveState = (_moveState == 1) ? 0 : 1;
        // JANGAN tambah beatCounter di sini untuk mematikan input, 
        // biarkan Update() yang menangani durasi 2 detik agar kursor sampai ujung.
    }

    void EndSequence()
    {
        SetPlayerInput(false);
        isDefeated = (_score >= _totalNotes);
        _hasStarted = false;
        currentState = MonsterState.WAIT;
        beatCounter = 0;
    }
    #endregion

    #region Input & Audio Helpers
    public bool CheckInput(string inputKey, float progress)
    {
        if (isDefeated || currentState != MonsterState.USER) return false;

        TimelineController tc = Object.FindAnyObjectByType<TimelineController>();
        float offsetBonus = 0;
        if (tc != null)
        {
            offsetBonus = tc.visualOffset / 2000f;
        }

        for (int i = 0; i < command.Length; i++)
        {
            if (command[i] != "B" || _hitRegistered[i]) continue;

            float targetProgress = (timingBeat[i] / 4.0f) + offsetBonus;
            float distance = Mathf.Abs(progress - targetProgress);

            if (distance <= beatTolerance)
            {
                _hitRegistered[i] = true;
                _score++;
                if (tc != null) tc.MarkDiamondHit(i);
                Debug.Log($"<color=green>SUCCESS HIT!</color> Beat {timingBeat[i]} at Progress {progress}");
                return true;
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
        AudioClip clip = (patternId == 1) ? pattern1Sound : pattern2Sound;
        if (clip != null) { monsterVoice.clip = clip; monsterVoice.Play(); }
    }
    #endregion
}