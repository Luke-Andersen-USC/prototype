using System;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class Skeleton : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Walking,
        Attacking,
        Flying,
        Dead,
        Falling
    }

    private BehaviorTree _bt;
    private EnemyState _currentState;
    private Animator _animator;

    public bool IsGrounded = false;
    
    void Awake()
    {
        _animator = GetComponent<Animator>();
        _bt = GetComponent<BehaviorTree>();

        if (_bt == null)
        {
            Debug.LogWarning("No reference to bt for " + gameObject.name);
        }
    }

    private void Start()
    {
        SwitchState(EnemyState.Idle);
    }

    public void Die()
    {
        if (_currentState != EnemyState.Dead)
        {
            SwitchState(EnemyState.Dead);
            StartCoroutine(DestroySkeleton());
        }
    }

    private IEnumerator DestroySkeleton()
    {
        _bt.enabled = false;
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        float timer = stateInfo.length;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        
        EnemyManager.Instance.DestroyEnemy(gameObject);
    }

    public AnimatorStateInfo GetCurrentAnimStateInfo()
    {
        return _animator.GetCurrentAnimatorStateInfo(0);
    }
    
    public void SwitchState(EnemyState state)
    {
        if (_currentState != state)
        {
            _currentState = state;
            _animator.SetInteger("EnemyState", (int)_currentState);
        }
    }
}
