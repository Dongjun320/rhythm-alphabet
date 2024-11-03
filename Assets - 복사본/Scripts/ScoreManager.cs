using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    private int currentScore = 0;  // 현재 점수
    private int maxScore;          // 최대 점수

    // 현재 점수를 외부에서 읽을 수 있는 속성
    public int CurrentScore
    {
        get { return currentScore; }
    }

    // 최대 점수를 외부에서 읽을 수 있는 속성
    public int MaxScore
    {
        get { return maxScore; }
    }

    public Image judgmentImage;     // 판정 이미지 표시용 Image 컴포넌트

    private RhythmGameController rhythmGameController;  // 리듬 게임 컨트롤러

    private void Start()
    {
        // 리듬 게임 컨트롤러 찾기
        rhythmGameController = FindObjectOfType<RhythmGameController>();
        if (rhythmGameController == null)
        {
            Debug.LogError("RhythmGameController를 찾을 수 없습니다. 확인해주세요.");
        }

        // 최대 점수 계산
        int totalNotes = CalculateTotalNotes();  // 노트의 총 개수
        maxScore = CalculateMaxScore(totalNotes); // 최대 점수 설정
    }

    // 노트의 총 개수 계산
    private int CalculateTotalNotes()
    {
        NoteManager noteManager = FindObjectOfType<NoteManager>();
        if (noteManager != null)
        {
            return noteManager.notePrefabs.Length;
        }
        else
        {
            Debug.LogError("NoteManager를 찾을 수 없습니다. 확인해주세요.");
            return 0;
        }
    }

    // 최대 점수 계산
    private int CalculateMaxScore(int totalNotes)
    {
        int perfectScore = Mathf.RoundToInt(rhythmGameController.maxScore * 1.0f / totalNotes); // 반올림
        return perfectScore * totalNotes;
    }

    // 점수 초기화
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreText();
    }

    // 점수 업데이트
    public void UpdateScore(int comboCount, float distance, Image perfectImage, Image goodImage, Image badImage, Image missImage)
    {
        if (distance <= rhythmGameController.perfectRange)
        {
            ShowJudgmentImage(perfectImage);  // 판정 이미지 표시
            AddScore(comboCount, rhythmGameController.perfectWeight); // 점수 추가
        }
        else if (distance <= rhythmGameController.coolRange)
        {
            ShowJudgmentImage(goodImage);     // 판정 이미지 표시
            AddScore(comboCount, rhythmGameController.coolWeight);    // 점수 추가
        }
        else if (distance <= rhythmGameController.goodRange)
        {
            ShowJudgmentImage(badImage);      // 판정 이미지 표시
            AddScore(comboCount, rhythmGameController.goodWeight);    // 점수 추가
        }
        else
        {
            ShowJudgmentImage(missImage);     // 판정 이미지 표시
            ResetCombo();                      // miss 판정 시 콤보 초기화
        }

        UpdateScoreText();  // UI 텍스트 업데이트
    }

    // 점수 추가
    private void AddScore(int comboCount, int judgmentWeight)
    {
        int addedScore = comboCount * judgmentWeight;  // 콤보와 가중치를 곱하여 추가될 점수 계산
        currentScore += addedScore;                    // 현재 점수에 추가
        // 최대 점수를 초과하지 않도록 제한
        if (currentScore > maxScore)
        {
            currentScore = maxScore;
        }
    }

    // 판정 이미지 표시
    private void ShowJudgmentImage(Image image)
    {
        if (judgmentImage != null && image != null)
        {
            judgmentImage.sprite = image.sprite;
            judgmentImage.gameObject.SetActive(true);
        }
    }

    // UI 텍스트 업데이트
    private void UpdateScoreText()
    {
        // 리듬 게임 컨트롤러에게 현재 스코어 업데이트를 알림
        if (rhythmGameController != null)
        {
            rhythmGameController.UpdateScoreText();
        }

        // 판정 이미지를 숨김
        if (judgmentImage != null)
        {
            judgmentImage.gameObject.SetActive(false);
        }
    }

    // 콤보 초기화
    private void ResetCombo()
    {
        // 리듬 게임 컨트롤러에게 콤보 초기화를 알림
        if (rhythmGameController != null)
        {
            rhythmGameController.ResetCombo();
        }
    }
}