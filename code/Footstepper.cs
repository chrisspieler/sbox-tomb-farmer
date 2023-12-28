using Sandbox;

public sealed class Footstepper : Component
{
	public void DoFoostep()
	{
		var tr = Scene.Trace
			.Ray( Transform.Position, Transform.Position + Vector3.Down * 5f )
			.Radius( 5f )
			.Run();
		if ( tr.Hit && tr.GameObject.Components.TryGet( out CustomSurface surface ) )
		{
			Sound.Play( surface.LightFootstep, tr.HitPosition );
		}
	}
}
