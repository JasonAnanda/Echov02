using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    public AudioSource inputAudioSource;
    public AudioClip soundA;
    public AudioClip soundY;
    public AudioClip soundMiss;

    // --- UPDATE: Variabel pencegah double-frame input ---
    private int _lastProcessedFrame = -1;

    void Update()
    {
        MonsterLogic activeMonster = FindActiveMonster();
        if (activeMonster == null) return;

        // --- UPDATE: Abaikan jika input diproses di frame yang sama dengan TargetSelector ---
        if (Time.frameCount == _lastProcessedFrame) return;

        // Input untuk Controller (A = Button 0, Y = Button 3)
        if (Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.Space))
        {
            HandleInput(activeMonster, "A", soundA);
            _lastProcessedFrame = Time.frameCount;
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton3) || Input.GetKeyDown(KeyCode.Y))
        {
            HandleInput(activeMonster, "Y", soundY);
            _lastProcessedFrame = Time.frameCount;
        }
    }

    private MonsterLogic FindActiveMonster()
    {
        MonsterLogic[] allMonsters = Object.FindObjectsByType<MonsterLogic>(FindObjectsSortMode.None);
        foreach (var m in allMonsters)
        {
            // Pastikan hanya mendeteksi monster yang sedang dalam giliran pemain
            if (m.currentState == MonsterLogic.MonsterState.USER) return m;
        }
        return null;
    }

    void HandleInput(MonsterLogic monster, string inputKey, AudioClip clip)
    {
        TimelineController tc = Object.FindAnyObjectByType<TimelineController>();
        if (tc == null) return;

        float currentProgress = tc.GetCurrentProgress();

        // --- UPDATE: Proteksi tambahan (Dead-zone Progress) ---
        // Jika kursor baru mulai (di bawah 2% jalan), abaikan input sisa dari TargetSelector
        if (currentProgress < 0.02f) return;

        bool isHit = monster.CheckInput(inputKey, currentProgress);

        if (isHit)
        {
            if (inputAudioSource && clip) inputAudioSource.PlayOneShot(clip);
        }
        else
        {
            if (inputAudioSource && soundMiss) inputAudioSource.PlayOneShot(soundMiss);
        }
    }
}