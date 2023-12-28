using System;
using Sandbox;

public sealed class HeadBob : Component
{
	[Property] public CharacterController Controller { get; set; }
	[Property] public GameObject Target { get; set; }
	[Property] public float BobSpeed { get; set; } = 10f;
	[Property] public float BobAmplitude { get; set; } = 0.1f;
	[Property] public Footstepper Footstep { get; set; }
	[Property] public float CurrentYBob => _currentYBob;
	private float _currentYBob;
	[Property] public float CurrentZBob => _currentZBob;
	private float _currentZBob;
	[Property] public Vector3 InitialPosition => _initialPosition;
	private Vector3 _initialPosition;

	protected override void OnStart()
	{
		_initialPosition = Target.Transform.LocalPosition;
	}

	private TimeSince _lastFootstep;

	protected override void OnUpdate()
	{
		if ( !Controller.IsOnGround )
			return;

		var velocityFactor = Controller.Velocity.Length.LerpInverse( 0f, 200f );
		var currentBobSpeed = Controller.Velocity.Length > 150f
			? BobSpeed * 2f
			: BobSpeed;
		var footIsDown = MathF.Abs( Time.Now * currentBobSpeed % MathF.Tau ) < 0.1f;
		if ( Footstep.IsValid() && _lastFootstep > 0.2f && velocityFactor > 0.1f && footIsDown )
		{
			Footstep.DoFoostep();
			_lastFootstep = 0f;
		}
		_currentYBob = MathF.Sin( Time.Now * currentBobSpeed / 2f ) * BobAmplitude;
		_currentZBob = MathF.Cos( Time.Now * currentBobSpeed ) * BobAmplitude;
		var bobVector = new Vector3( 0f, _currentYBob, _currentZBob );
		var scaledBob = Vector3.Zero.LerpTo( bobVector, velocityFactor );
		Target.Transform.LocalPosition = InitialPosition + scaledBob;
	}
}
