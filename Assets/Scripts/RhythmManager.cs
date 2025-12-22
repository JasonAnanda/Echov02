using UnityEngine;
using System;

public class RhythmManager : MonoBehaviour
{
    [Header("Rhythm Settings")]
    public float systemBPM = 240f;
    public float tickBPM = 120f; // Kita pisahkan agar pasti 120
    public float beatTolerance = 0.12f;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource tickSource;

    public static event Action<int> OnBeat;

    private double _nextSystemBeatTime;
    private double _nextTickTime;
    private float _systemInterval;
    private float _tickInterval;

    public int systemBeatCounter { get; private set; }
    public bool canInput { get; private set; }

    void Start()
    {
        _systemInterval = 60f / systemBPM; // 0.25s
        _tickInterval = 60f / tickBPM;     // 0.50s

        double startTime = AudioSettings.dspTime + 0.5;
        _nextSystemBeatTime = startTime;
        _nextTickTime = startTime;

        if (bgmSource != null)
        {
            bgmSource.PlayScheduled(startTime);
        }
    }

    void Update()
    {
        double currentDsp = AudioSettings.dspTime;

        // 1. Handle Suara TICK (Pasti 120 BPM)
        if (currentDsp >= _nextTickTime)
        {
            if (tickSource != null)
            {
                tickSource.PlayOneShot(tickSource.clip, 0.3f);
            }
            _nextTickTime += _tickInterval;
        }

        // 2. Handle Logika SISTEM (240 BPM) untuk Input & Monster
        if (currentDsp >= _nextSystemBeatTime)
        {
            systemBeatCounter = (systemBeatCounter + 1) % 2;
            OnBeat?.Invoke(systemBeatCounter);
            _nextSystemBeatTime += _systemInterval;
        }

        // 3. Toleransi Input
        float diff = (float)Math.Abs(currentDsp - (_nextSystemBeatTime - _systemInterval));
        canInput = diff <= beatTolerance;
    }
}