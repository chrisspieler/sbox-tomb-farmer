using Sandbox;
using Sandbox.Utility;

public sealed class SkyFlicker : Component
{
	[Property] public Color HighTint { get; set; }
	[Property] public SkyBox2D Sky { get; set; }
	[Property] public float Speed { get; set; } = 1f;
	[Property] public float StartDelay { get; set; } = 5f;

	private TimeSince _started = 0f;

	protected override void OnUpdate()
	{
		if ( _started < StartDelay ) return;
		if ( !Sky.IsValid() ) return;

		var noise = Noise.Simplex( Time.Now * Speed ) * 2 - 1;
		var skyColor = Color.Lerp( Color.Black, HighTint, noise );
		Sky.Tint = skyColor;
		if ( skyColor == Color.Black )
		{
			Sky.Transform.Rotation = Rotation.Random;
		}
	}
}
