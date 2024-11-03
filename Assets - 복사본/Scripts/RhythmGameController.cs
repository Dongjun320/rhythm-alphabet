using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RhythmGameController : MonoBehaviour
{
    public Transform judgmentLine;
    public float perfectRange = 0.1f;
    public float coolRange = 0.2f;
    public float goodRange = 0.3f;
    public float badRange = 0.5f;
    public Image perfectImage;
    public Image goodImage;
    public Image badImage;
    public Image missImage;
    public Text comboText;
    public Text scoreText;
    public Image judgmentImage;
    public AudioClip hitSound;
    private AudioSource musicSource;
    private AudioSource hitSoundSource;
    private NoteManager noteManager;
    private ScoreManager scoreManager;
    private HealthManager healthManager; // 추가: HealthManager 인스턴스

    private double musicStartTime;
    private int comboCount = 0;

    public int maxScore = 350000;
    public int perfectWeight = 100;
    public int coolWeight = 50;
    public int goodWeight = 10;

    private void Start()
    {
        noteManager = FindObjectOfType<NoteManager>();
        healthManager = FindObjectOfType<HealthManager>();
        scoreManager = GetComponent<ScoreManager>(); // 변경: GetComponent를 통해 ScoreManager 인스턴스 획득

        if (noteManager == null || healthManager == null || scoreManager == null) // 변경: scoreManager 추가 체크
        {
            Debug.LogError("NoteManager, HealthManager, 또는 ScoreManager를 찾을 수 없습니다. 확인해주세요.");
            return;
        }

        HideAllJudgmentImages();

        musicSource = gameObject.AddComponent<AudioSource>();
        hitSoundSource = gameObject.AddComponent<AudioSource>();

        AudioClip musicClip = Resources.Load<AudioClip>("OMG");
        hitSound = Resources.Load<AudioClip>("Hit_drum");

        if (musicClip != null && hitSound != null)
        {
            musicSource.clip = musicClip;
            musicSource.volume = 0.8f;
            double delayTime = 2.81;
            musicStartTime = AudioSettings.dspTime + delayTime;
            musicSource.PlayScheduled(musicStartTime);
        }
        else
        {
            Debug.LogError("음악 파일이나 사운드 파일을 찾을 수 없습니다. Resources 폴더에 파일이 존재하는지 확인하십시오.");
        }

        comboCount = 0;
        UpdateComboText();
        UpdateScoreText();
    }

    private void Update()
    {
        for (int i = 0; i < noteManager.notePrefabs.Length; i++)
        {
            Key key = (Key)(Key.A + i);
            if (Keyboard.current[key].wasPressedThisFrame)
            {
                CheckNoteJudgment(i);
            }
        }

        HandleNotesPassedJudgmentLine();
    }

    public void ResetCombo()
    {
        comboCount = 0;
        UpdateComboText();
    }

    private void CheckNoteJudgment(int noteIndex)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(judgmentLine.position, badRange);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Note"))
            {
                NoteMove noteMove = collider.GetComponent<NoteMove>();
                float distance = Mathf.Abs(collider.transform.position.x - judgmentLine.position.x);

                if (distance <= badRange)
                {
                    string inputKey = noteMove.NoteType;

                    Debug.Log($"Input Key: {inputKey}, Correct Note Type: {noteManager.GetNoteTypeByIndex(noteIndex)}, Distance: {distance}");

                    if (noteManager.IsNoteTypeCorrect(inputKey, noteIndex))
                    {
                        if (distance <= perfectRange)
                        {
                            ShowJudgmentImage(perfectImage);
                        }
                        else if (distance <= coolRange)
                        {
                            ShowJudgmentImage(goodImage);
                        }
                        else if (distance <= goodRange)
                        {
                            ShowJudgmentImage(badImage);
                        }
                        else
                        {
                            ShowJudgmentImage(missImage);
                        }

                        Destroy(collider.gameObject);

                        // 노트가 정확하게 판정선에 도달하면 콤보 증가
                        comboCount++;
                        UpdateComboText();

                        // 타격음 재생
                        hitSoundSource.volume = 0.3f;
                        hitSoundSource.PlayOneShot(hitSound);

                        // 점수 및 체력 업데이트
                        UpdateScoreAndHealth(distance);
                    }
                    else
                    {
                        ShowJudgmentImage(missImage);
                        Destroy(collider.gameObject);

                        // 노트가 놓침 판정이면 콤보 초기화
                        comboCount = 0;
                        UpdateComboText();

                        // 체력 감소
                        healthManager.DecreaseHealth(10f);
                    }
                }
            }
        }
    }

    private void HandleNotesPassedJudgmentLine()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(judgmentLine.position, badRange);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Note"))
            {
                NoteMove note = collider.GetComponent<NoteMove>();

                if (note.transform.position.x < judgmentLine.position.x - badRange)
                {
                    float distance = Mathf.Abs(note.transform.position.x - judgmentLine.position.x);

                    if (distance <= perfectRange)
                    {
                        ShowJudgmentImage(perfectImage);
                    }
                    else if (distance <= coolRange)
                    {
                        ShowJudgmentImage(goodImage);
                    }
                    else if (distance <= goodRange)
                    {
                        ShowJudgmentImage(badImage);
                    }
                    else
                    {
                        ShowJudgmentImage(missImage);
                    }

                    Destroy(collider.gameObject);

                    comboCount = 0;
                    UpdateComboText();

                    // 추가: miss 판정 시 체력 감소
                    healthManager.DecreaseHealth(10f);
                }
            }
        }
    }

    private void UpdateScoreAndHealth(float distance)
    {
        // ScoreManager를 통해 점수 업데이트
        scoreManager.UpdateScore(comboCount, distance, perfectImage, goodImage, badImage, missImage);

        // 콤보 10 당 체력 증가
        if (comboCount % 10 == 0)
        {
            healthManager.IncreaseHealth(10f);
        }
    }

    private void ShowJudgmentImage(Image image)
    {
        HideAllJudgmentImages();
        image.gameObject.SetActive(true);
    }

    private void HideAllJudgmentImages()
    {
        perfectImage.gameObject.SetActive(false);
        goodImage.gameObject.SetActive(false);
        badImage.gameObject.SetActive(false);
        missImage.gameObject.SetActive(false);
    }

    private void UpdateComboText()
    {
        comboText.text = comboCount.ToString();
    }

    // 추가: ScoreText 업데이트 메서드
    public void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"{scoreManager.CurrentScore}";
        }
    }
}