using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    public AudioSource inputAudioSource;
    public AudioClip soundA;
    public AudioClip soundY;
    public AudioClip soundMiss;

    private int _lastProcessedFrame = -1;
    private float _activationCooldown = 0f;

    void OnEnable()
    {
        // Jeda 0.2 detik agar klik konfirmasi tidak dianggap miss
        _activationCooldown = 0.2f;
    }

    void Update()
    {
        if (_activationCooldown > 0) { _activationCooldown -= Time.deltaTime; return; }

        MonsterLogic activeMonster = FindActiveMonster();
        if (activeMonster == null) { this.enabled = false; return; }

        if (Time.frameCount == _lastProcessedFrame) return;

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
        foreach (var m in allMonsters) if (m.currentState == MonsterLogic.MonsterState.USER) return m;
        return null;
    }

    void HandleInput(MonsterLogic monster, string inputKey, AudioClip clip)
    {
        TimelineController tc = Object.FindAnyObjectByType<TimelineController>();
        if (tc == null || tc.GetCurrentProgress() < 0.05f) return;

        bool isHit = monster.CheckInput(inputKey, tc.GetCurrentProgress());

        if (isHit)
        {
            if (inputAudioSource && clip) inputAudioSource.PlayOneShot(clip);
        }
        else
        {
            // HANYA tambah gauge jika benar-benar sedang USER turn
            if (monster.currentState == MonsterLogic.MonsterState.USER)
            {
                GlobalData.gauge += GlobalData.SMALL_FAIL;
                if (inputAudioSource && soundMiss) inputAudioSource.PlayOneShot(soundMiss);
                Debug.Log("MISS! Gauge: " + GlobalData.gauge);
            }
        }
    }
}