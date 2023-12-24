using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.Utility;

[Title( "Advanced Light" )]
[Category( "Light" )]
[Icon( "light_mode" )]
[EditorHandle( "materials/gizmo/pointlight.png" )]
public sealed class AdvancedLight : Component, Component.IColorProvider, Component.ExecuteInEditor, Component.ITintable
{
	private SceneLight _sceneObject;

	[Property]
	public Color BaseColor { get; set; } = "#E9FAFF";
	[Property, Range(0, 20)]
	public float ConstantAttenuation { get; set; } = 0f;
	[Property, Range(0, 20)]
	public float LinearAttenuation { get; set; } = 0f;
	[Property, Range(0, 20)]
	public float QuadraticAttenuation { get; set; } = 1f;
	[Property]
	public float Radius { get; set; } = 400f;
	[Property, Range( 0, 1 )]
	public float FlickerScale { get; set; } = 0.5f;
	[Property, Range( 0, 100 )]
	public float FlickerSpeed { get; set; } = 10f;
	[Property]
	public bool CastShadows { get; set; } = true;


	Color IColorProvider.ComponentColor => BaseColor;

	Color ITintable.Color
	{
		get
		{
			return BaseColor;
		}
		set
		{
			BaseColor = value;
		}
	}

	protected override void DrawGizmos()
	{
		using ( Gizmo.Scope( $"light-{GetHashCode()}" ) )
		{
			if ( Gizmo.IsSelected )
			{
				Gizmo.Draw.Color = BaseColor.WithAlpha( 0.9f );
				Gizmo.GizmoDraw draw = Gizmo.Draw;
				Sphere sphere = new Sphere( Vector3.Zero, Radius );
				draw.LineSphere( in sphere, 12 );
			}

			if ( Gizmo.IsHovered && Gizmo.Settings.Selection )
			{
				Gizmo.Draw.Color = BaseColor.WithAlpha( 0.4f );
				Gizmo.GizmoDraw draw2 = Gizmo.Draw;
				Sphere sphere = new Sphere( Vector3.Zero, Radius );
				draw2.LineSphere( in sphere, 12 );
			}
		}
	}

	protected override void OnEnabled()
	{
		Assert.True( _sceneObject == null );
		Assert.NotNull( base.Scene );
		_sceneObject = new SceneLight( base.Scene.SceneWorld, base.Transform.Position, Radius, BaseColor );
		_sceneObject.FogLighting = SceneLight.FogLightingMode.Dynamic;
	}

	protected override void OnDisabled()
	{
		_sceneObject?.Delete();
		_sceneObject = null;
	}

	protected override void OnPreRender()
	{
		if ( _sceneObject.IsValid() )
		{
			_sceneObject.Transform = base.Transform.World;
			_sceneObject.LightColor = BaseColor * GetFlicker();
			_sceneObject.Radius = Radius;
			_sceneObject.ShadowsEnabled = CastShadows;
			_sceneObject.ConstantAttenuation = ConstantAttenuation;
			_sceneObject.LinearAttenuation = LinearAttenuation;
			_sceneObject.QuadraticAttenuation = QuadraticAttenuation;
			// Log the three attenuation values as they already exist on the _sceneObject
			//Log.Info( $"ConstantAttenuation: {_sceneObject.ConstantAttenuation}" );
			//Log.Info( $"LinearAttenuation: {_sceneObject.LinearAttenuation}" );
			//Log.Info( $"QuadraticAttenuation: {_sceneObject.QuadraticAttenuation}" );

		}
	}

	private float GetFlicker()
	{
		var flicker = Noise.Simplex( Time.Now * FlickerSpeed * 10 );
		flicker *= FlickerScale;
		return 1 - flicker;
	}

	//
	// Summary:
	//     Tags have been updated - lets update our light's tags
	protected override void OnTagsChanged()
	{
		_sceneObject.Tags.SetFrom( base.Tags );
	}
}
