using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityVector3;
using Unity.VisualScripting;

public class FlyToDeck : Action
{
    private Skeleton _sk;
    
    private Transform _target;
    private float _parachuteTime = 3f;
    private float _timer;
    public override void OnStart()
    {
        List<Transform> spawns = EnemyManager.Instance.EnemySpawns;
        _target = spawns[Random.Range(0, spawns.Count)];
        _timer = 0f;

        _sk = gameObject.GetComponent<Skeleton>();
        _sk.SwitchState(Skeleton.EnemyState.Flying);
    }

    public override TaskStatus OnUpdate()
    {
        _timer = Mathf.Clamp(_timer + Time.deltaTime, 0f, _parachuteTime);
        float f = _timer / _parachuteTime;

        gameObject.transform.position = Vector3.Lerp(EnemyManager.Instance.ParachuteSpawn.position, _target.position, f);

        if (f > 0.99f)
        {
            return TaskStatus.Success;
        }
        return TaskStatus.Running;
    }

    public override void OnEnd()
    {
        gameObject.transform.position = _target.position;
        gameObject.transform.SetParent(EnemyManager.Instance.gameObject.transform);
        
        _sk.SwitchState(Skeleton.EnemyState.Idle);
        _sk.IsGrounded = true;
    }
}
