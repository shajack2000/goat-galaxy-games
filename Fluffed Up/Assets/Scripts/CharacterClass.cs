using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterClass : MonoBehaviour
{
    #region Character Attributes
    // Movement
    public float moveSpeed;
    public float rotationSpeed;
    public bool isRunning;

    // Health
    public HealthBar healthBar;
    public float health;
    public float maxHealth;

    // Attack
    public float attackPower;
    public float attackSpeed;
    public float attackDistanceThreshold;
    public bool isAttacking = false;

    // Defense
    public float defense;
    #endregion

    #region Character Status Effects
    // Status effects
    public bool isBurning;
    public bool isFrozen;
    public float burnDamage;
    public float burnDuration;
    public float freezeDuration;
    #endregion


    #region Grounded Attributes
    [Header("Grounded Attributes")]
    public bool isGrounded;
    public LayerMask groundMask;
    #endregion

    #region Jump Attributes
    [Header("Jump Attributes")]
    public float jumpForce;
    public float jumpCooldown;
    public float jumpTime;
    public float airSpeedMultiplier;
    public float gravityMultiplier;
    public bool isJumping;
    #endregion

    // Character animator and rigidbody
    public Animator animator;
    public Rigidbody rb;


    private void Start()
    {
        // TODO initialize base values for characters
    }

    public void Jump(float modifier)
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(rb.transform.up * jumpForce, ForceMode.Impulse);
        Invoke("ResetJump", jumpCooldown * modifier);
    }

    public void ResetJump()
    {
        isJumping = false;
    }

    public IEnumerator ResetAttackState()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        isAttacking = false; // Reset attacking state after the action is done
    }

    public void ApplyBurn(float duration)
    {
        if (!isBurning)
        {
            isBurning = true;
            StartCoroutine(BurnCoroutine(duration));
        }
    }

    private IEnumerator BurnCoroutine(float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            TakeDamage(burnDamage * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isBurning = false; // Reset burning state
    }

    public void ApplyFreeze(float duration)
    {
        if (!isFrozen)
        {
            Debug.Log($"{gameObject.name} is frozen.");
            isFrozen = true;
            float origMoveSpeed = moveSpeed;
            moveSpeed = 0; // Stop movement
            StartCoroutine(FreezeCoroutine(origMoveSpeed, duration));
        }
    }

    private IEnumerator FreezeCoroutine(float origMoveSpeed, float duration)
    {
        yield return new WaitForSeconds(duration);
        moveSpeed = origMoveSpeed; // Reset to original speed
        isFrozen = false; // Reset frozen state
    }

    public virtual void Heal(float amount)
    {
        // Debug.Log($"{gameObject.name} is taking {damage} damage.");
        health += amount;
        // Debug.Log($"{gameObject.name} health is now {health}");

        if (health > 100)
        {
            health = 100;
        }

        if (healthBar != null)
        {
            healthBar.SetHealth(health);
        }
    }

        public virtual void TakeDamage(float damage)
    {
        // Debug.Log($"{gameObject.name} is taking {damage} damage.");
        health -= damage;
        // Debug.Log($"{gameObject.name} health is now {health}");

        if (health <= 0)
        {
            Die();
        }

        if (healthBar != null)
        {
            healthBar.SetHealth(health);
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        Destroy(gameObject);
    }
}
