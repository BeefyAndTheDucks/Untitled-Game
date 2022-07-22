using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;
using Image = UnityEngine.UI.Image;

public class PlayerHealthManager : MonoBehaviour
{
    public Transform healthTransform;
    public GameObject healthPrefab;

    public GameObject gameOverGameObject;

    public int tweenSpeed = 10;
    [Range(0, 255)]
    public int tweenToAlpha = 200;

    public int heartAmount = 9;
    int currentHeartAmount;

    public Sprite heart;
    public Sprite heartCracked;

    private readonly GameObject[] hearts = new GameObject[9];

    public FirstPersonController character;

    void Start()
    {
        currentHeartAmount = heartAmount;
        
        for (int i = 0; i < heartAmount; i++)
        {
            hearts[i] = Instantiate(healthPrefab, healthTransform);
        }

        StartCoroutine(nameof(TestHealth));
    }

    IEnumerator GameOver()
    {
        yield return new WaitForSeconds(0.5f);
        Image gameOverImage = gameOverGameObject.GetComponent<Image>();
        gameOverImage.color = new Color32(255, 255, 255, 0);
        gameOverGameObject.SetActive(true);
        
        Cursor.lockState = CursorLockMode.None;
        character.playerCanMove = false;
        character.cameraCanMove = false;
        character.enableHeadBob = false;
        
        for (int i = 0; i < tweenToAlpha; i += tweenSpeed)
        {
            gameOverImage.color = new Color32(255, 255, 255, (byte)i);
            yield return new WaitForSeconds(0.01f);
        }
    }

    public void Respawn()
    {
        Cursor.lockState = CursorLockMode.Locked;
        character.playerCanMove = true;
        character.cameraCanMove = true;
        character.enableHeadBob = true;
        
        Debug.Log("Respawn");
        RefillHearts();
        currentHeartAmount = heartAmount;
        gameOverGameObject.SetActive(false);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void RefillHearts()
    {
        foreach (GameObject heartGameObject in hearts)
        {
            heartGameObject.GetComponent<Image>().sprite = heart;
        }
    }

    public void LoseHealth()
    {
        currentHeartAmount--;
        
        RefillHearts();

        for (int i = 0; i < heartAmount - currentHeartAmount; i++)
        {
            hearts[i].GetComponent<Image>().sprite = heartCracked;
        }

        if (currentHeartAmount <= 0)
        {
            StartCoroutine(nameof(GameOver));
        }
    }

    IEnumerator TestHealth()
    {
        yield return new WaitForSeconds(2);
        Debug.Log("Starting");

        for (int i = 0; i < heartAmount; i++)
        {
            LoseHealth();
            yield return new WaitForSeconds(0.25f);
        }
    }
}
