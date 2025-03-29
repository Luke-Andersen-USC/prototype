using System.Collections;
using System.Linq;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class PopBalloon : Action
{
    [SerializeField] private SharedGameObject _targetBalloon;
    
    public override void OnStart()
    {
        ShipController.Instance.PopBalloon(_targetBalloon.Value);
    }
}
