using Sandbox.Citizen;

namespace Sandbox;

public sealed class Beeliner : Component
{
	[Property] public GameObject Target { get; set; }
	[Property] public GameObject Body { get; set; }
	[Property] public CharacterController Controller { get; set; }
	[Property] public CitizenAnimationHelper AnimationHelper { get; set; }
	[Property] public Footstepper Footsteps 
	{
		get => _footsteps;
		set
		{
			_footsteps = value;
			if ( Body.IsValid() && Body.Components.TryGet<SkinnedModelRenderer>( out var model ) )
			{
				model.OnFootstepEvent += _ => HandleFootstep();
			}
		}
	}
	private Footstepper _footsteps;
	[Property] public float Speed { get; set; } = 100f;
	[Property] public bool ZeroZ { get; set; } = true;
	[Property] public float StopDistance { get; set; } = 50f;

	protected override void OnEnabled()
	{
		if ( !Body.IsValid() || !Body.Components.TryGet<SkinnedModelRenderer>( out var model ) )
			return;

		model.OnFootstepEvent += _ => HandleFootstep();
	}

	protected override void OnUpdate()
	{
		if ( !Body.IsValid() || !Target.IsValid() ) return;
		var targetPos = Target.Transform.Position;
		var targetDir = (targetPos - Transform.Position).WithZ( 0f ).Normal;
		Body.Transform.Rotation = Rotation.LookAt( targetDir );
		UpdateAnimation();
	}

	protected override void OnFixedUpdate()
	{
		if ( !Controller.IsValid() || !Target.IsValid() ) return;

		var targetPos = Target.Transform.Position;
		if ( targetPos.Distance( Controller.Transform.Position ) <= StopDistance )
		{
			Controller.Velocity = 0f;
			return;
		}
		var targetDir = ( targetPos - Transform.Position ).Normal;
		var wishVelocity = targetDir * Speed;
		Controller.Accelerate( wishVelocity );
		Controller.ApplyFriction( 4.0f );
		Controller.Move();
		if ( ZeroZ )
		{
			Controller.Velocity = Controller.Velocity.WithZ( 0f );
		}
	}

	private void UpdateAnimation()
	{
		if ( Controller.IsValid() )
		{
			AnimationHelper.WithVelocity( Controller.Velocity );
			AnimationHelper.IsGrounded = AnimationHelper.IsGrounded;
		}
	}

	private void HandleFootstep()
	{
		if ( !Footsteps.IsValid() ) return;
		if ( Controller.IsValid() )
		{
			if ( Controller.Velocity.Length < 10f ) return;
			if ( !Controller.IsOnGround ) return;
		}
		Footsteps.DoFoostep();
	}
}
