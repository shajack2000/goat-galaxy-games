using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class WaveManager : MonoBehaviour
{
    public PlayerController player;
    public GameObject fakeBoxEnemyPrefab;
    public UnityEvent waveEvent;
    private List<GameObject> currentEnemies = new List<GameObject>();
    private int currentWave = 0;
    private bool isSpawningWave = false; // Flag to prevent multiple waves from starting
    private float waveSpawnDelay = 2f;

    [SerializeField]
    private GameObject shopComponent;
    public bool shopIsOpen;

    [SerializeField] 
    private TextMeshProUGUI waveCounterText; // For Unity UI Text


    // Start is called before the first frame update
    void Start()
    {
        waveEvent.AddListener(RequestNextWave);
        StartWave();

        if (shopComponent == null)
            Debug.Log("No shop component was assigned to the WaveManager");
    }

    void StartWave()
    {
        currentWave++; // update wave count
        UpdateWaveCounter();
        for (int i = 0; i < 3; i++)
        {
            SpawnEnemy();
        }
    }

    void UpdateWaveCounter()
    {
        waveCounterText.text = "Wave " + currentWave.ToString();
    }

    void SpawnEnemy()
    {
        Debug.Log("Spawning enemy");
        Vector3 randomPosition = GetRandomPosition();
        GameObject newEnemy = Instantiate(fakeBoxEnemyPrefab, randomPosition, Quaternion.identity);
        EnemyBase enemyScript = newEnemy.GetComponent<EnemyBase>();

        // Add event listener: player attack ---> enemy takes damage
        UnityEngine.Events.UnityAction<float> onPlayerAttackAction = (float damage) => HandlePlayerAttack(enemyScript, damage);
        player.AttackEvent.AddListener(onPlayerAttackAction);

        enemyScript.AttackEvent.AddListener(player.TakeDamage);
        enemyScript.OnEnemyDeath += () => RemoveEnemyListener(onPlayerAttackAction, enemyScript);
        enemyScript.player = player;

        currentEnemies.Add(newEnemy);
    }

    Vector3 GetRandomPosition()
    {
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * 6f;
        return new Vector3(randomCircle.x, 1f, randomCircle.y);
    }

    void HandlePlayerAttack(EnemyBase enemy, float damage)
    {
        if (enemy != null)
        {
            // Debug.Log("HandlePlayerAttack called");
            float distance = Vector3.Distance(player.transform.position, enemy.transform.position);

            // Hit condition1: Distance smaller than threshold
            bool withinDistance = distance <= player.attackDistanceThreshold;
            bool withinAngle = math.abs(Vector3.Angle(player.transform.forward, enemy.transform.position - player.transform.position)) < 90 ;
            if (withinDistance && withinAngle)
            {
                // Debug.Log("Enemy got damage");
                enemy.TakeDamage(damage);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        currentEnemies.RemoveAll(enemy => enemy == null);

        waveEvent.Invoke();

        if (shopIsOpen)
            Cursor.lockState = CursorLockMode.None;
    }


    private void RequestNextWave()
    {
        if (currentEnemies.Count == 0 && !isSpawningWave)
        {
            OpenShop();
            StartCoroutine(StartNextWave());
        }
    }

    IEnumerator StartNextWave()
    {
        isSpawningWave = true; // Set the flag to true to prevent multiple triggers
        yield return new WaitWhile(() => shopIsOpen == true); // Don't start next wave until shop is closed
        yield return new WaitForSeconds(waveSpawnDelay); // Delay before starting next wave. We don't want the wave to start immediately after the shop closes.

        StartWave();
        isSpawningWave = false; // Reset the flag after spawning the wave
    }

    void RemoveEnemyListener(UnityEngine.Events.UnityAction<float> action, EnemyBase enemy)
    {
        player.AttackEvent.RemoveListener(action);
        currentEnemies.Remove(enemy.gameObject); // Safely remove the enemy from the list
    }

    public void OpenShop()
    {
        Debug.Log("Shop opened");
        shopComponent.SetActive(true);
        Time.timeScale = 0;
        shopIsOpen = true;
    }

    public void CloseShop()
    {
        shopComponent.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        shopIsOpen = false;
    }
}
