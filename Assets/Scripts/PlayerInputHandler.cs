using UnityEngine;
using System.Collections;

public class PlayerInputHandler : MonoBehaviour
{
    public AudioSource inputAudioSource;
    public AudioClip soundA;
    public AudioClip soundY;
    public AudioClip soundMiss;

    [Header("Visual Juice Settings")]
    public Transform playerSpriteTransform;
    public float bounceSpeed = 0.05f;
    public Vector3 squashScale = new Vector3(1.2f, 0.8f, 1f);
    public Vector3 stretchScale = new Vector3(0.8f, 1.2f, 1f);

    private Vector3 _originalScale;
    private Coroutine _bounceCoroutine;
    private int _lastProcessedFrame = -1;
    private float _activationCooldown = 0f;

    void Start()
    {
        if (playerSpriteTransform != null) _originalScale = playerSpriteTransform.localScale;
        else _originalScale = transform.localScale;
    }

    void OnEnable()
    {
        _activationCooldown = 0.1f; // Diperkecil sedikit agar responsif
    }

    void Update()
    {
        if (_activationCooldown > 0) { _activationCooldown -= Time.deltaTime; return; }

        MonsterLogic activeMonster = FindActiveMonster();
        if (activeMonster == null) { this.enabled = false; return; }

        if (Time.frameCount == _lastProcessedFrame) return;

        if (Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.Space))
        {
            ExecuteInput(activeMonster, "A", soundA);
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton3) || Input.GetKeyDown(KeyCode.Y))
        {
            ExecuteInput(activeMonster, "Y", soundY);
        }
    }

    void ExecuteInput(MonsterLogic monster, string key, AudioClip characterClip)
    {
        _lastProcessedFrame = Time.frameCount;
        TriggerPlayerBounce();

        if (inputAudioSource && characterClip) inputAudioSource.PlayOneShot(characterClip);
        HandleInput(monster, key);
    }

    private MonsterLogic FindActiveMonster()
    {
        MonsterLogic[] allMonsters = Object.FindObjectsByType<MonsterLogic>(FindObjectsSortMode.None);
        foreach (var m in allMonsters) if (m.currentState == MonsterLogic.MonsterState.USER) return m;
        return null;
    }

    void HandleInput(MonsterLogic monster, string inputKey)
    {
        if (monster == null) return;

        TimelineController tc = Object.FindAnyObjectByType<TimelineController>();
        if (tc == null) return;

        float currentProgress = tc.GetCurrentProgress();

        // FIX: Hapus pengecekan 0.01f agar input di awal layar (Beat 0) bisa masuk
        bool isHit = monster.CheckInput(inputKey, currentProgress);

        if (!isHit)
        {
            if (inputAudioSource && soundMiss) inputAudioSource.PlayOneShot(soundMiss);
            GlobalData.gauge += GlobalData.SMALL_FAIL;
            Debug.Log("MISS/SALAH TOMBOL! Gauge: " + GlobalData.gauge);
        }
    }

    #region Visual Juice Logic
    private void TriggerPlayerBounce()
    {
        Transform target = (playerSpriteTransform != null) ? playerSpriteTransform : transform;
        if (_bounceCoroutine != null) StopCoroutine(_bounceCoroutine);
        _bounceCoroutine = StartCoroutine(DoBounce(target));
    }

    IEnumerator DoBounce(Transform target)
    {
        target.localScale = squashScale;
        yield return new WaitForSeconds(bounceSpeed);
        target.localScale = stretchScale;
        yield return new WaitForSeconds(bounceSpeed);
        target.localScale = _originalScale;
    }
    #endregion
}