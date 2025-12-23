using UnityEngine;
using System;

public class RhythmManager : MonoBehaviour
{
    [Header("Rhythm Settings")]
    public float systemBPM = 240f;
    public float tickBPM = 120f;
    public float beatTolerance = 0.12f;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource tickSource;

    public static event Action<int> OnBeat; // Pastikan penulisan tepat
    public static RhythmManager Instance;

    private double _nextSystemBeatTime;
    private double _nextTickTime;
    private float _systemInterval;
    private float _tickInterval;

    public int systemBeatCounter { get; private set; }
    public bool canInput { get; private set; }

    void Awake() { Instance = this; }

    void Start()
    {
        _systemInterval = 60f / systemBPM;
        _tickInterval = 60f / tickBPM;

        double startTime = AudioSettings.dspTime + 0.5;
        _nextSystemBeatTime = startTime;
        _nextTickTime = startTime;

        if (bgmSource != null) bgmSource.PlayScheduled(startTime);
    }

    void Update()
    {
        double currentDsp = AudioSettings.dspTime;

        // 1. Tick Sound
        if (currentDsp >= _nextTickTime)
        {
            if (tickSource != null) tickSource.PlayOneShot(tickSource.clip, 0.3f);
            _nextTickTime += _tickInterval;
        }

        // 2. System Logic
        if (currentDsp >= _nextSystemBeatTime)
        {
            systemBeatCounter = (systemBeatCounter + 1) % 2;
            OnBeat?.Invoke(systemBeatCounter);
            _nextSystemBeatTime += _systemInterval;
        }

        // 3. Input Window
        float diff = (float)Math.Abs(currentDsp - (_nextSystemBeatTime - _systemInterval));
        canInput = diff <= beatTolerance;

        // 4. Input Handler
        if (Input.GetKeyDown(KeyCode.JoystickButton0)) ProcessInput("A");
        if (Input.GetKeyDown(KeyCode.JoystickButton3)) ProcessInput("Y");
    }

    private void ProcessInput(string key)
    {
        if (canInput) Debug.Log("HIT: " + key);
        else HandleMiss();
    }

    private void HandleMiss()
    {
        GlobalData.gauge += GlobalData.SMALL_FAIL;
        Debug.Log("MISS! Gauge: " + GlobalData.gauge);
    }
}