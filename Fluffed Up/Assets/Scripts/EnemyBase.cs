using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

public class EnemyBase : CharacterClass
{
    public enum EnemyState
    {
        ChasingPlayer,
        Idle,
        InitiateAttack,
        Attacking,
    };

    [System.Serializable]
    public class ItemDrop
    {
        public GameObject prefab;
        public float dropChance; // Probability as a percentage (0 to 100)
    }

    // Enemy Events
    public UnityEvent<float> AttackEvent;
    public UnityEvent<float> DamageEvent;
    public UnityAction OnEnemyDeath; // Trigger to remove event listner in player

    // AI to track player
    public NavMeshAgent navMeshAgent;
    public EnemyState enemyState;
    public PlayerController player; // Player object to be set by WaveManager
    protected float distanceToPlayer;

    // Enemy Drops
    [SerializeField]
    private List<ItemDrop> itemDrops; // List of item drops with chances

    void Start()
    {
        animator = GetComponent<Animator>();
        enemyState = EnemyState.Idle;
        navMeshAgent = GetComponent<NavMeshAgent>();
        // Temporary initialization since this is the base.
        // However, we can utilize this method for inherited classes.
        InitializeStat(100f, 1f);
        healthBar = GetComponentInChildren<HealthBar>();
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(health);
        }
    }

    void Update()
    {
        AIStateMachine();
    }

    public virtual void AIStateMachine()
    {
        distanceToPlayer  = (transform.position - player.transform.position).magnitude;
        switch (enemyState)
        {
        case EnemyState.Idle:
            // Debug.Log("EnemyState: Idle");
            if (distanceToPlayer > 1)
                enemyState = EnemyState.ChasingPlayer;
            break;
        case EnemyState.ChasingPlayer:
            // Debug.Log("EnemyState: ChasingPlayer");
            if (distanceToPlayer > 1)
            {
                navMeshAgent.SetDestination(player.transform.position);
            }
            else //TODO: if player is dead, enemy should go to idle. 
            {
                enemyState = EnemyState.InitiateAttack;
            }
            break;
        default:
            break;
        }
    }

    public void InitializeStat(float health, float attackPower)
    {
        this.health = health;
        this.maxHealth = health;
        this.attackPower = attackPower;
    }

    public virtual void Attack()
    {
        AttackEvent?.Invoke(attackPower);
    }

    protected override void Die()
    {
        // Randomly select an item to drop based on drop chances
        if (itemDrops.Count > 0)
        {
            float randomValue = Random.Range(0f, 100f); // Random value between 0 and 100
            float cumulativeChance = 0f;

            foreach (var itemDrop in itemDrops)
            {
                cumulativeChance += itemDrop.dropChance;

                if (randomValue <= cumulativeChance)
                {
                    Vector3 dropPosition = transform.position + new Vector3(0, 0.5f, 0);
                    Instantiate(itemDrop.prefab, dropPosition, Quaternion.identity);
                    Debug.Log("Dropped item: " + itemDrop.prefab.name);
                    break; // Exit loop after dropping an item
                }
            }
        }

        OnEnemyDeath?.Invoke();

        // Destroy game object from parent CharacterClass
        base.Die();
    }
}
