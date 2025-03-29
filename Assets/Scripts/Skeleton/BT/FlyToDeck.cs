using System.Collections;
using System.Linq;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class FlyToDeck : Action
{
    public override void OnStart()
    {
        // needed setup
    }

    public override TaskStatus OnUpdate()
    {
        // while lerping, return TaskStatus.Running
        // upon Success return TaskStatus.Success

        return TaskStatus.Success;
    }
}
