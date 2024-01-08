using System;

namespace Sandbox;

public struct Vector3Int : IEquatable<Vector3Int>
{
	public int x { get; set; }
	public int y { get; set; }
	public int z { get; set; }

	public static readonly Vector3Int Zero = new( 0 );
	public static readonly Vector3Int One = new( 1 );

	public Vector3Int( int value )
	{
		x = value;
		y = value;
		z = value;
	}

	public Vector3Int( int x, int y )
	{
		this.x = x;
		this.y = y;
		z = 0;
	}

	public Vector3Int( int x, int y, int z )
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public float Length()
		=> (float)Math.Sqrt( x * x + y * y + z * z );

	public Vector3Int Normalize()
	{
		if ( x == 0 && y == 0 && z == 0 )
			return Zero;
		if ( x == 0 && y == 0 )
			return new( 0, 0, z / Math.Abs( z ) );
		if ( x == 0 && z == 0 )
			return new( 0, y / Math.Abs( y ), 0 );
		if ( y == 0 && z == 0 )
			return new( x / Math.Abs( x ), 0, 0 );
		if ( x == 0 )
			return new( 0, y / Math.Abs( y ), z / Math.Abs( z ) );
		if ( y == 0 )
			return new( x / Math.Abs( x ), 0, z / Math.Abs( z ) );
		if ( z == 0 )
			return new( x / Math.Abs( x ), y / Math.Abs( y ), 0 );
		return new( x / Math.Abs( x ), y / Math.Abs( y ), z / Math.Abs( z ) );
	}

	public override string ToString()
		=> $"({x}, {y}, {z})";

	public bool Equals( Vector3Int other )
		=> other.x == x && other.y == y && other.z == z;

	public override bool Equals( object obj )
		=> obj is Vector3Int other && Equals( other );

	public override int GetHashCode()
		=> HashCode.Combine( x, y, z );

	public static bool operator ==( Vector3Int a, Vector3Int b )
		=> a.Equals( b );

	public static bool operator !=( Vector3Int a, Vector3Int b )
		=> !a.Equals( b );

	public static Vector3Int operator +( Vector3Int a, Vector3Int b )
		=> new( a.x + b.x, a.y + b.y, a.z + b.z );

	public static Vector3Int operator -( Vector3Int a, Vector3Int b )
		=> new( a.x - b.x, a.y - b.y, a.z - b.z );

	public static Vector3Int operator *( Vector3Int a, int b )
		=> new( a.x * b, a.y * b, a.z * b );

	public static Vector3Int operator /( Vector3Int a, int b )
		=> new( a.x / b, a.y / b, a.z / b );

	public static implicit operator Vector3Int( Vector3 v )
		=> new( (int)MathF.Round(v.x), (int)MathF.Round(v.y), (int)MathF.Round(v.z) );

	public static implicit operator Vector3( Vector3Int v )
		=> new( v.x, v.y, 0 );

	public static implicit operator Vector3Int( (int x, int y, int z) t )
		=> new( t.x, t.y, t.z );

	public static Vector3Int operator *( Vector3Int a, Rotation b )
		=> (Vector3)a * b;
}
