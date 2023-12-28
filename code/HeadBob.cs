using System;
using Sandbox;

public sealed class HeadBob : Component
{
	[Property] public CharacterController Controller { get; set; }
	[Property] public GameObject Target { get; set; }
	[Property] public float BobSpeed { get; set; } = 10f;
	[Property] public float BobAmplitude { get; set; } = 0.1f;
	[Property] public float CurrentYBob { get; private set; }
	[Property] public float CurrentZBob { get; private set; }
	[Property] public Vector3 InitialPosition { get; private set; }

	protected override void OnStart()
	{
		InitialPosition = Target.Transform.LocalPosition;
	}

	protected override void OnUpdate()
	{
		if ( !Controller.IsOnGround )
			return;

		var velocityFactor = Controller.Velocity.Length.LerpInverse( 0f, 200f );
		var currentBobSpeed = Controller.Velocity.Length > 150f
			? BobSpeed * 2f
			: BobSpeed;
		CurrentYBob = MathF.Sin( Time.Now * currentBobSpeed / 2f ) * BobAmplitude;
		CurrentZBob = MathF.Cos( Time.Now * currentBobSpeed ) * BobAmplitude;
		var bobVector = new Vector3( 0f, CurrentYBob, CurrentZBob );
		var scaledBob = Vector3.Zero.LerpTo( bobVector, velocityFactor );
		Target.Transform.LocalPosition = InitialPosition + scaledBob;
	}
}
