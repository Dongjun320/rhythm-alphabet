using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartButton : MonoBehaviour
{
    // 버튼 클릭 이벤트에 연결할 함수
    public void RestartScene()
    {
        // 현재 씬을 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}