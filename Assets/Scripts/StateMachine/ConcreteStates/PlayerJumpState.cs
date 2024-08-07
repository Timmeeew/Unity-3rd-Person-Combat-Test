using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerJumpState : PlayerState
{
    private Player player;

    public PlayerJumpState(Player player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
        this.player = player;
    }

    public override void EnterState()
    {
        base.EnterState();
        player.StartCoroutine(JumpDelay());
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

    private IEnumerator JumpDelay()
    {
        //Start Jumping Animation
        player.animator.SetTrigger("Jump"); 
        player.isRunning = false; 
        player.moveDirection = Vector3.zero;
        yield return new WaitForSeconds(0.5f);
        player.ySpeed = 20f;
        //Add force to Rigidbody
        player.rb.velocity = new Vector3(player.rb.velocity.x, 0f, player.rb.velocity.z);
        player.rb.AddForce(Vector3.up * 20f, ForceMode.Impulse);
        //Update Animator Booleans
        player.animator.SetBool("Landing", false);
        // Ensure the falling animation plays
        player.animator.SetBool("IsFalling", true); 
        player.animator.ResetTrigger("Jump");
        yield return new WaitForSeconds(1f);
        // Transition back to Idle state
        player.PlayerStateMachine.ChangeState(player.PlayerIdleState); 
    }
}
