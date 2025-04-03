using System.Collections;
using System.Linq;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class GetTargetBalloon : Action
{
    [SerializeField] private SharedGameObject _targetBalloon;
    
    public override void OnStart()
    {
        GameObject nearestBalloon = null;
        float minDistance = Mathf.Infinity;
        foreach (GameObject balloon in ShipController.Instance.Balloons)
        {
            float distance = Vector3.Distance(transform.position, balloon.transform.position);
            
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestBalloon = balloon;
            }
        }
        
        _targetBalloon.SetValue(nearestBalloon);
    }

    public override void OnEnd()
    {
        Skeleton sk = gameObject.GetComponent<Skeleton>();
        sk.SwitchState(Skeleton.EnemyState.Walking);
    }
}
