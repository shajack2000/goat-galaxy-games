using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using JetBrains.Annotations;

public class EnemySlime : EnemyBase
{
    public override void AIStateMachine()
    {
        base.AIStateMachine();
        switch (enemyState)
        {
        case EnemyState.InitiateAttack:
            // Debug.Log("EnemyState: InitiateAttack");
            Attack();
            enemyState = EnemyState.Attacking;
            break;
        case EnemyState.Attacking:
            // Debug.Log("EnemyState: Attacking");
            if (distanceToPlayer > 1)
            {
                enemyState = EnemyState.ChasingPlayer;
            }
            else if (isAttacking == false)
            {
                enemyState = EnemyState.InitiateAttack;
            }
            break;
        default:
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
}
