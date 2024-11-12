using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using JetBrains.Annotations;

public class EnemyMiniBossDog : EnemyBase
{
    private const int ACTION_DELAY_DEFAULT = 500;
    private int actionDelay = ACTION_DELAY_DEFAULT; // give delay in action because minibossdog is stupid!

    public override void AIStateMachine()
    {
        if (actionDelay > 0)
        {
            actionDelay--;
            return;
        }

        base.AIStateMachine();
        switch (enemyState)
        {
        case EnemyState.ChasingPlayer:
            animator.SetBool("isMoving", true);
            break;
        case EnemyState.InitiateAttack:
            animator.SetBool("isMoving", false);
            base.AIStateMachine();
            Attack();
            enemyState = EnemyState.Attacking;
            actionDelay = ACTION_DELAY_DEFAULT;
            break;
        case EnemyState.Attacking:
            animator.SetBool("isMoving", false);
            base.AIStateMachine();
            if (distanceToPlayer > 1)
            {
                actionDelay = ACTION_DELAY_DEFAULT;
                enemyState = EnemyState.ChasingPlayer;
            }
            else if (isAttacking == false)
            {
                actionDelay = ACTION_DELAY_DEFAULT;
                enemyState = EnemyState.InitiateAttack;
            }
            break;
        default:
            animator.SetBool("isMoving", false);
            base.AIStateMachine();
            break;
        }
    }

    public override void Attack()
    {
        animator.Play("Attack01");
        isAttacking = true;
        // Reset the attacking state after the attack animation finishes
        StartCoroutine(ResetAttackState());

        base.Attack();
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        actionDelay = ACTION_DELAY_DEFAULT;
        if (health > 0)
        {
            animator.StopPlayback();
            animator.Play("GetHit");
        }
    }

    protected override void Die()
    {
        animator.StopPlayback();
        animator.Play("Die01");
        StartCoroutine(DieCoroutine(animator.GetCurrentAnimatorStateInfo(0).length*2));

        base.Die();
    }
}
