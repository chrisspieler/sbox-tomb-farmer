using Sandbox;
using Sandbox.Utility;

public sealed class PositionJitter : Component
{
	[Property] public float XSpeed { get; set; }
	[Property] public float XFactor { get; set; }
	[Property] public float YSpeed { get; set; }
	[Property] public float YFactor { get; set; }
	[Property] public float ZSpeed { get; set; }
	[Property] public float ZFactor { get; set; }

	protected override void OnUpdate()
	{
		var xJitter = Noise.Simplex( Time.Now * XSpeed, 0f ) * XFactor;
		var yJitter = Noise.Simplex( 0f, Time.Now * YSpeed ) * YFactor;
		var zJitter = Noise.Simplex( 0f, Time.Now * ZSpeed ) * ZFactor;
		Transform.LocalPosition = new Vector3( xJitter, yJitter, zJitter );
	}
}
