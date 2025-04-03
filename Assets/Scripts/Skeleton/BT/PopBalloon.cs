using System.Collections;
using System.Linq;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEditor;

public class PopBalloon : Action
{
    [SerializeField] private SharedGameObject _targetBalloon;
    private AnimatorStateInfo _animInfo;
    private Skeleton _sk;
    
    // As backup
    private float _animTimer = 0f;
    public override void OnStart()
    {
        _sk = gameObject.GetComponent<Skeleton>();
        _sk.SwitchState(Skeleton.EnemyState.Attacking);
        _animInfo = _sk.GetCurrentAnimStateInfo();
        _animTimer = _animInfo.length;
    }

    public override TaskStatus OnUpdate()
    {
        if (gameObject == null) return TaskStatus.Failure;
        
        _animTimer -= Time.deltaTime;
        if (_animInfo.normalizedTime >= 0.98f || _animTimer < 0f)
        {
            ShipController.Instance.PopBalloon(_targetBalloon.Value);
            return TaskStatus.Success;
        }

        return TaskStatus.Running;
    }

    public override void OnEnd()
    {
        _sk.SwitchState(Skeleton.EnemyState.Idle);
    }
}
