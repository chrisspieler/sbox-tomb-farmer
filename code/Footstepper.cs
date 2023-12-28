using Sandbox;

public sealed class Footstepper : Component
{
	[Property] public float FootstepVolumeScale { get; set; } = 1f;
	public void DoFoostep()
	{
		var tr = Scene.Trace
			.Ray( Transform.Position, Transform.Position + Vector3.Down * 5f )
			.Radius( 5f )
			.Run();
		if ( tr.Hit && tr.GameObject.Components.TryGet( out CustomSurface surface ) )
		{
			var hSnd = Sound.Play( surface.LightFootstep, tr.HitPosition );
			hSnd.Volume *= FootstepVolumeScale;
		}
	}
}
