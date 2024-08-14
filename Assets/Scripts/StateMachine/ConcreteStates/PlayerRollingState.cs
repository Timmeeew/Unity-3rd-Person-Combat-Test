using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRollingState : PlayerState
{
    private Player player;

    public PlayerRollingState(Player player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
        this.player = player;
    }

    public override void EnterState()
    {
        base.EnterState();
        player.StartCoroutine(RollDelay());
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

    private IEnumerator RollDelay()
    {
        //Set roll trigger
        player.animator.SetTrigger("Roll");
        //Apply a foce foward
        player.rb.AddForce(player.transform.forward * 100f, ForceMode.Impulse);
        yield return new WaitForSeconds(0.5f);
        player.PlayerStateMachine.ChangeState(player.PlayerIdleState);
    }

}
