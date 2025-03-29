using System.Collections;
using System.Linq;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class SpawnActions : Action
{
    [SerializeField] private SharedGameObject _player;
    
    public override void OnStart()
    {
        _player.SetValue(PlayerManager.Instance.Players[0]);
    }
}
