using UnityEngine;
using System.Collections;

public class PlayerInputHandler : MonoBehaviour
{
    public AudioSource inputAudioSource;
    public AudioClip soundA; // Nakama
    public AudioClip soundY; // Chigau
    public AudioClip soundMiss;

    [Header("Visual Juice Settings")]
    public Transform playerSpriteTransform;
    public float bounceSpeed = 0.05f;

    [Tooltip("Ukuran saat memantul ke bawah (Gepeng). Contoh: X=1.2, Y=0.8")]
    public Vector3 squashScale = new Vector3(1.2f, 0.8f, 1f);

    [Tooltip("Ukuran saat memantul ke atas (Lonjong). Contoh: X=0.8, Y=1.2")]
    public Vector3 stretchScale = new Vector3(0.8f, 1.2f, 1f);

    private Vector3 _originalScale;
    private Coroutine _bounceCoroutine;
    private int _lastProcessedFrame = -1;
    private float _activationCooldown = 0f;

    void Start()
    {
        // Menyimpan scale awal sebagai referensi untuk kembali normal
        if (playerSpriteTransform != null) _originalScale = playerSpriteTransform.localScale;
        else _originalScale = transform.localScale;
    }

    void OnEnable()
    {
        _activationCooldown = 0.2f;
    }

    void Update()
    {
        if (_activationCooldown > 0) { _activationCooldown -= Time.deltaTime; return; }

        MonsterLogic activeMonster = FindActiveMonster();
        if (activeMonster == null) { this.enabled = false; return; }

        if (Time.frameCount == _lastProcessedFrame) return;

        // INPUT TOMBOL A (NAKAMA)
        if (Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.Space))
        {
            ExecuteInput(activeMonster, "A", soundA);
        }
        // INPUT TOMBOL Y (CHIGAU)
        else if (Input.GetKeyDown(KeyCode.JoystickButton3) || Input.GetKeyDown(KeyCode.Y))
        {
            ExecuteInput(activeMonster, "Y", soundY);
        }
    }

    void ExecuteInput(MonsterLogic monster, string key, AudioClip characterClip)
    {
        _lastProcessedFrame = Time.frameCount;

        // 1. VISUAL: Karakter selalu memantul dengan settingan Vector3
        TriggerPlayerBounce();

        // 2. AUDIO: Suara karakter (Nakama/Chigau) SELALU BERBUNYI kapanpun ditekan
        if (inputAudioSource && characterClip) inputAudioSource.PlayOneShot(characterClip);

        // 3. LOGIKA: Validasi Hit atau Miss
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

        if (currentProgress < 0.01f) return;

        bool isHit = monster.CheckInput(inputKey, currentProgress);

        if (!isHit)
        {
            if (inputAudioSource && soundMiss) inputAudioSource.PlayOneShot(soundMiss);

            GlobalData.gauge += GlobalData.SMALL_FAIL;
            Debug.Log("MISS/SALAH TOMBOL! Gauge: " + GlobalData.gauge);
        }
    }

    #region Visual Juice Logic (Adjustable)
    private void TriggerPlayerBounce()
    {
        Transform target = (playerSpriteTransform != null) ? playerSpriteTransform : transform;

        if (_bounceCoroutine != null) StopCoroutine(_bounceCoroutine);
        _bounceCoroutine = StartCoroutine(DoBounce(target));
    }

    IEnumerator DoBounce(Transform target)
    {
        // Tahap 1: Squash (Gepeng sesuai Vector3 di Inspector)
        target.localScale = squashScale;
        yield return new WaitForSeconds(bounceSpeed);

        // Tahap 2: Stretch (Lonjong sesuai Vector3 di Inspector)
        target.localScale = stretchScale;
        yield return new WaitForSeconds(bounceSpeed);

        // Tahap 3: Kembali ke Original
        target.localScale = _originalScale;
    }
    #endregion
}