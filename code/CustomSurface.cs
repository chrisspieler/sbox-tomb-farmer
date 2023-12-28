using Sandbox;

public sealed class CustomSurface : Component
{
	[Property] public SoundEvent LightFootstep { get; set; }
	[Property] public SoundEvent HeavyFootstep { get; set; }
}
