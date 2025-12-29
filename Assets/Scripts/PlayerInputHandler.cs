using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    public AudioSource inputAudioSource;
    public AudioClip soundA;
    public AudioClip soundY; // Ganti W ke Y
    public AudioClip soundMiss;

    void Update()
    {
        MonsterLogic activeMonster = FindActiveMonster();
        if (activeMonster == null) return;

        // Input untuk Controller (A = Button 0, Y = Button 3 di sebagian besar mapping)
        // Atau gunakan nama tombol jika sudah diatur di Input Manager
        if (Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.Space))
        {
            HandleInput(activeMonster, "A", soundA);
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton3) || Input.GetKeyDown(KeyCode.Y))
        {
            HandleInput(activeMonster, "Y", soundY);
        }
    }

    private MonsterLogic FindActiveMonster()
    {
        MonsterLogic[] allMonsters = Object.FindObjectsByType<MonsterLogic>(FindObjectsSortMode.None);
        foreach (var m in allMonsters)
        {
            if (m.currentState == MonsterLogic.MonsterState.USER) return m;
        }
        return null;
    }

    void HandleInput(MonsterLogic monster, string inputKey, AudioClip clip)
    {
        TimelineController tc = Object.FindAnyObjectByType<TimelineController>();
        if (tc == null) return;

        float currentProgress = tc.GetCurrentProgress();
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