using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class HealthManager : MonoBehaviour
{
    public Slider healthBarSlider;
    public AudioSource gameOverSound;
    public GameObject mainContainer;
    public Image fadeOutImage;
    public float duration = 2f;
    public float initialHealth = 100f;

    private bool isGameOver = false;

    private void Start()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = initialHealth;
            healthBarSlider.value = initialHealth;
        }

        fadeOutImage.color = new Color(1f, 1f, 1f, 0f);
    }

    public void DecreaseHealth(float amount)
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.value -= amount;

            if (healthBarSlider.value <= 0 && !isGameOver)
            {
                isGameOver = true;
                GameOver();
            }
        }
    }

    public void IncreaseHealth(float amount)
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.value += amount;
        }
    }

    private void GameOver()
    {
        if (gameOverSound != null)
        {
            gameOverSound.Play();
        }

        StartCoroutine(FadeOutAndLoadSpecificScene());
    }

    private IEnumerator FadeOutAndLoadSpecificScene()
    {
        float timer = 0f;

        if (mainContainer != null)
        {
            mainContainer.SetActive(false);
        }

        while (timer <= duration)
        {
            fadeOutImage.color = new Color(1f, 1f, 1f, Mathf.Lerp(0f, 1f, timer / duration));

            timer += Time.deltaTime;
            yield return null;
        }

        // 원하는 씬의 이름으로 로드
        LoadSpecificScene("GameOver");
    }

    private void LoadSpecificScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}