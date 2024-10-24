using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerController : CharacterClass
{
    // Player attributes
    public UnityEvent<float, int> AttackEvent;
    public UnityEvent<float> DamageEvent;
    [Header("Shooting")]
    public GameObject projectilePrefab;       // The projectile prefab to instantiate
    public Transform projectileSpawnPoint;    // Where the projectile will spawn
    public float projectileSpeed = 20f;       // Speed of the projectile
    public float projectileDamage = 10f;      // Damage dealt by the projectile
    public AudioClip attackSound;
    public int attackDelayInMilli = 300;      // Attack delay in milliseconds. After the delay, the distance between enemy and player is calculated to decide if attack was valid or not. 

    [System.Serializable]
    public class ItemProperties
    {
        public float totalAmount;
        public float quantity;
        public float value;

        public ItemProperties(float newQuantity, float newValue)
        {
            quantity = newQuantity;
            value = newValue;
            totalAmount = quantity * value;
        }
    }
    private Dictionary<string, ItemProperties> inventory = new Dictionary<string, ItemProperties>();

    [Header("Inputs")]
    [SerializeField]
    private InputMap inputs;

    [Header("Camera")]
    [SerializeField]
    private Transform cameraTransform;

    [Header("UI")]
    [SerializeField]
    public TextMeshProUGUI coinCounterText; // For Unity UI Text
    public TextMeshProUGUI healthCounterText; // For Unity UI Text


    [Header("SceneManagement")]
    private string lastScene;

    private void Awake()
    {
        groundMask = ~(1 << LayerMask.GetMask("Ground"));
        inputs = GetComponent<InputMap>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        ResetJump();
    }

    // Start is called before the first frame update
    void Start()
    {
        moveSpeed = 10f;
        rotationSpeed = 360f;
        jumpForce = 100f;
        jumpTime = 3f;
        jumpCooldown = 0.5f;
        airSpeedMultiplier = 0.6f;
        attackPower = 25f;
        health = 100f;
        attackDistanceThreshold = 3f;

        healthBar = GetComponentInChildren<HealthBar>();
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(health);
        }


        UpdateAllCounters();
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        #region Grounded Check
        isGrounded = Physics.CheckSphere(rb.transform.position, 0.2f, groundMask);
        #endregion

        #region Movement Control

        #region Configure Camera Relative Movement

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        Vector3 forwardRelative = vertical * camForward;
        Vector3 rightRelataive = horizontal * camRight;

        Vector3 moveDir = forwardRelative + rightRelataive;

        #endregion

        Vector3 move = new Vector3(moveDir.x, 0f, moveDir.z);

        if (isGrounded)
            rb.MovePosition(transform.position + move * moveSpeed * Time.deltaTime);
        else
            rb.MovePosition(transform.position + move * moveSpeed * airSpeedMultiplier * Time.deltaTime);

        isRunning = move != Vector3.zero && isGrounded;
        #endregion

    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("isRunning", isRunning);
        // Debug.Log("isAttacking:" + isAttacking.ToString());

        if (SceneManager.GetActiveScene().name == "OtherCharacter")
        {
            if (Input.GetMouseButtonDown(0) && !isAttacking)
            {
                Debug.Log("Right mouse button clicked - calling Shoot() in 'OtherCharacter' scene");
                Shoot();
            }
        }

        if (SceneManager.GetActiveScene().name == "SampleScene")
        {
            if (Input.GetMouseButtonDown(0) && !isAttacking)
            {
                Debug.Log("Right mouse button clicked - calling Shoot() in 'OtherCharacter' scene");
                attackEnemy();
            }
        }
        // Enemy attack control. Attack when clicking left click.
        // TODO might need to update to input string name to account for controller.
        

        if (inputs.jump && isGrounded && !isJumping)
        {
            isJumping = true;
            Jump(1f);
        }
        healthBar.SetHealth(health);

        if (health <= 0)
        {
             lastScene = SceneManager.GetActiveScene().name;

             PlayerPrefs.SetString("LastPlayedScene", lastScene);

            
             SceneManager.LoadScene("DeathScene");
        }
        

        if (Input.GetKeyDown(KeyCode.Alpha1) && !isAttacking)
        {
            HealSelf();
        }

    }
    void Shoot()
    {
        Debug.Log("Shoot() method is being called");

        // Trigger the shooting animation (if you have one)
        animator.Play("Defend");

        // Instantiate the projectile at the spawn point
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);

        // Ignore collision between the projectile and the player
        Physics.IgnoreCollision(projectile.GetComponent<Collider>(), GetComponent<Collider>());

        // Set the projectile's speed and damage
        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.speed = projectileSpeed;
            projScript.damage = projectileDamage;
        }
        else
        {
            Debug.LogError("Projectile script not found on the projectile prefab.");
        }

        // Set the projectile's direction
        projectile.transform.forward = transform.forward;
    }

    void attackEnemy()
    {
        animator.Play("Attack01");
        isAttacking = true;
        // Reset the attacking state after the attack animation finishes
        StartCoroutine(ResetAttackState());

        // Play attack sound
        PlaySoundEffect(attackSound);

        // Find nearby enemies
        AttackEvent?.Invoke(attackPower, attackDelayInMilli);
    }


    public void CollectItem(CollectibleItem item)
    {
        if (!inventory.ContainsKey(item.itemName))
        {
            // Debug.LogWarning("Adding new item to inventory");
            ItemProperties properties = new ItemProperties(item.quantity, item.value);
            inventory.Add(item.itemName, properties);
        }
        else
        {
            ItemProperties properties = inventory[item.itemName];
            properties.quantity += item.quantity;
            properties.totalAmount += item.totalAmount;
            inventory[item.itemName] = properties;
            // Debug.LogWarning("Item already exists in inventory. Updating amount");
        }

        // Debug.Log($"Collected: {item.itemName}");
        UpdateAllCounters();
    }

    public ItemProperties GetItemProperties(string key)
    {
        if (inventory.TryGetValue(key, out ItemProperties properties))
        {
            // Debug.LogWarning("Item found in inventory.");
            return properties;
        }
        else
        {
            // Debug.LogWarning("Item not found in inventory.");
            return null;
        }
    }

    void UpdateAllCounters()
    {
        UpdateCoinCounter();
        UpdateHealthPackCounter();
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        animator.Play("GetHit");
    }

    void UpdateCoinCounter()
    {
        ItemProperties properties = GetItemProperties("Coin");
        if(properties != null)
        {
            coinCounterText.text = properties.totalAmount.ToString();
        }
        else
        {
            coinCounterText.text = "0";
        }
    }

    void RemoveCoins(float amount)
    {
        ItemProperties properties = GetItemProperties("Coin");
        if(properties != null)
        {
            if(properties.totalAmount != 0)
            {
                properties.totalAmount -= amount;
                if (properties.totalAmount < 0) {
                    properties.totalAmount = 0;
                }
                inventory["Coin"] = properties;
                Debug.Log("Removing coins");
                Debug.Log(inventory["Coin"].quantity);
                UpdateCoinCounter();
            }
            else
            {
                Debug.Log("No coins to remove.");
            }
        }
        else
        {
            Debug.Log("No coins to remove.");
        }
    }

    void UpdateHealthPackCounter()
    {
        ItemProperties properties = GetItemProperties("Health");
        if(properties != null)
        {
            healthCounterText.text = properties.quantity.ToString();
        }
        else
        {
            healthCounterText.text = "0";
        }
    }

    void HealSelf()
    {
        ItemProperties properties = GetItemProperties("Health");
        if(properties != null)
        {
            if(properties.quantity != 0 && health < 100)
            {
                Heal(properties.value);
                properties.totalAmount -= properties.quantity * properties.value;
                if (properties.totalAmount < 0) {
                    properties.totalAmount = 0;
                }
                properties.quantity--;
                inventory["Health"] = properties;
                Debug.Log("Healed");
                UpdateHealthPackCounter();
            }
            else
            {
                Debug.Log("No health to heal.");
            }
        }
        else
        {
            Debug.Log("No health to heal.");
        }
    }
}
