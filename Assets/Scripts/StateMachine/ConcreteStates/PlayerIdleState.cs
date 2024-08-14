using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerState
{
    private Player player;
    public PlayerIdleState(Player player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
        this.player = player;
    }

    public override void EnterState()
    {
        base.EnterState();
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}
