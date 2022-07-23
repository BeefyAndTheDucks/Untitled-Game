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

    public int healTime = 4;

    public Sprite heart;
    public Sprite heartCracked;

    private readonly GameObject[] hearts = new GameObject[9];

    public FirstPersonController character;

    Rigidbody rb;

    Vector3 vel;
    float yvel;
    private int healthToLoose = 0;

    private int cooldown = 0;

    [HideInInspector] public bool alive = true;

    void Start()
    {
        rb = character.gameObject.GetComponent<Rigidbody>();

        character.landEvent += OnLand;

        vel = rb.velocity;
        yvel = vel.y;

        currentHeartAmount = heartAmount;
        
        for (int i = 0; i < heartAmount; i++)
        {
            hearts[i] = Instantiate(healthPrefab, healthTransform);
        }

        StartCoroutine(nameof(MainLoop));
    }

    void FixedUpdate()
    {
        vel = rb.velocity;
        yvel = vel.y;

        if (yvel <= -10.0f && cooldown <= 0)
        {
            cooldown = 30;
            healthToLoose++;
        }

        if (cooldown > 0)
        {
            cooldown--;
        }
    }

    void OnLand()
    {
        for (int i = 0; i < healthToLoose; i++)
        {
            currentHeartAmount--;
            
            RefreshHearts();
            healthToLoose--;
        }
    }

    IEnumerator GameOver()
    {
        if (alive)
        {
            alive = false;
            Cursor.lockState = CursorLockMode.None;
            character.playerCanMove = false;
            character.cameraCanMove = false;
            character.enableHeadBob = false;
        
            yield return new WaitForSeconds(0.5f);
            Image gameOverImage = gameOverGameObject.GetComponent<Image>();
            gameOverImage.color = new Color32(255, 255, 255, 0);
            gameOverGameObject.SetActive(true);

            for (int i = 0; i < tweenToAlpha; i += tweenSpeed)
            {
                gameOverImage.color = new Color32(255, 255, 255, (byte)i);
                yield return new WaitForSeconds(0.01f);
            }
        }
    }

    public void Respawn()
    {
        Cursor.lockState = CursorLockMode.Locked;
        character.playerCanMove = true;
        character.cameraCanMove = true;
        character.enableHeadBob = true;
        
        Debug.Log("Respawn");
        alive = true;
        RefillHearts();
        currentHeartAmount = heartAmount;
        gameOverGameObject.SetActive(false);
        StartCoroutine(nameof(MainLoop));

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void RefillHearts()
    {
        foreach (GameObject heartGameObject in hearts)
        {
            heartGameObject.GetComponent<Image>().sprite = heart;
        }
    }

    public void RefreshHearts()
    {
        RefillHearts();

        for (int i = 0; i < heartAmount - currentHeartAmount && i < heartAmount; i++)
        {
            hearts[i].GetComponent<Image>().sprite = heartCracked;
        }

        if (currentHeartAmount <= 0)
        {
            StartCoroutine(nameof(GameOver));
        }
    }
    
    IEnumerator MainLoop()
    {
        IEnumerator voidDamageCoroutine = VoidDamage();
        StartCoroutine(voidDamageCoroutine);
        
        while (alive)
        {
            if (!(currentHeartAmount >= heartAmount))
            {
                currentHeartAmount++;
                RefreshHearts();
            }

            yield return new WaitForSeconds(healTime);
        }
        
        StopCoroutine(voidDamageCoroutine);
    }

    IEnumerator VoidDamage()
    {
        while (alive)
        {
            if (character.transform.position.y < -60)
            {
                while (currentHeartAmount > 0)
                {
                    currentHeartAmount--;
                    RefreshHearts();
                    
                    yield return new WaitForSeconds(0.25f);
                }
            }

            yield return new WaitForFixedUpdate();
        }
    }
}
