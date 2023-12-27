using Sandbox;

public sealed class SpringArm : Component
{
	[Property] public GameObject TraceSource { get; set; }
	[Property] public float TraceRadius { get; set; } = 15f;
	[Property] public Vector3 DesiredPosition { get; set; }
	[Property] public float LerpSpeed { get; set; } = 10f;

	protected override void DrawGizmos()
	{
		Gizmo.Draw.Color = Color.Blue;
		Gizmo.Transform = Transform.Parent.Transform.World;
		Gizmo.Draw.LineSphere( new Sphere( DesiredPosition, 5f ) );
	}

	protected override void OnFixedUpdate()
	{
		var startPos = TraceSource.Transform.Position;
		var endPos = Transform.Parent.Transform.World.PointToWorld( DesiredPosition );
		var tr = Scene.Trace
			.Ray( startPos, endPos )
			.Radius( TraceRadius )
			.Run();
		if ( tr.Hit )
		{
			Transform.Position = tr.HitPosition;
		}
		else
		{
			Transform.Position = Transform.Position.LerpTo( endPos, LerpSpeed * Time.Delta );
		}
	}
}
