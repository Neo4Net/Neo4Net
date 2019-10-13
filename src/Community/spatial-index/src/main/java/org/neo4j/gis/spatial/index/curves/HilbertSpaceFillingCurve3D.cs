using System.Collections.Generic;
using System.Diagnostics;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Gis.Spatial.Index.curves
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.gis.spatial.index.curves.HilbertSpaceFillingCurve3D.Direction3D.BACK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.gis.spatial.index.curves.HilbertSpaceFillingCurve3D.Direction3D.DOWN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.gis.spatial.index.curves.HilbertSpaceFillingCurve3D.Direction3D.FRONT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.gis.spatial.index.curves.HilbertSpaceFillingCurve3D.Direction3D.LEFT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.gis.spatial.index.curves.HilbertSpaceFillingCurve3D.Direction3D.RIGHT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.gis.spatial.index.curves.HilbertSpaceFillingCurve3D.Direction3D.UP;

	public class HilbertSpaceFillingCurve3D : SpaceFillingCurve
	{

		 /// <summary>
		 /// Utilities for rotating point values in binary about various axes
		 /// </summary>
		 internal class BinaryCoordinateRotationUtils3D
		 {
			  internal static int RotateNPointLeft( int value )
			  {
					return ( value << 1 ) & 0b111 | ( ( value & 0b100 ) >> 2 );
			  }

			  internal static int RotateNPointRight( int value )
			  {
					return ( value >> 1 ) | ( ( value & 0b001 ) << 2 );
			  }

			  internal static int XXOR( int value )
			  {
					return value ^ 0b100;
			  }

			  internal static int RotateYZ( int value )
			  {
					return value ^ 0b011;
			  }
		 }

		 /// <summary>
		 /// Description of the space filling curve structure
		 /// </summary>
		 internal class HilbertCurve3D : CurveRule
		 {
			  internal HilbertCurve3D[] Children;

			  internal HilbertCurve3D( params int[] npointValues ) : base( 3, npointValues )
			  {
					Debug.Assert( npointValues[0] == 0 || npointValues[0] == 3 || npointValues[0] == 5 || npointValues[0] == 6 );
			  }

			  public override CurveRule ChildAt( int npoint )
			  {
					return Children[npoint];
			  }

			  public override string ToString()
			  {
					return Name().ToString();
			  }

			  internal static string BinaryString( int value )
			  {
					string binary = "00" + Integer.toBinaryString( value );
					return StringHelper.SubstringSpecial( binary, binary.Length - 3, binary.Length );
			  }

			  internal virtual Direction3D Direction( int start, int end )
			  {
					end -= start;
					switch ( end )
					{
					case 1:
						 return FRONT; // move forward 000->001
					case 2:
						 return UP; // move up      000->010
					case 4:
						 return RIGHT; // move right   000->100
					case -4:
						 return LEFT; // move left    111->011
					case -2:
						 return DOWN; // move down    111->101
					case -1:
						 return BACK; // move back    111->110
					default:
						 throw new System.ArgumentException( "Illegal direction: " + end );
					}
			  }

			  internal virtual SubCurve3D Name()
			  {
					return new SubCurve3D( Direction( NpointValues[0], NpointValues[1] ), Direction( NpointValues[1], NpointValues[2] ), Direction( NpointValues[0], NpointValues[Length() - 1] ) );
			  }

			  /// <summary>
			  /// Rotate about the normal diagonal (the 000->111 diagonal). This simply involves
			  /// rotating the bits of all npoint values either left or right depending on the
			  /// direction of rotation, normal or reversed (positive or negative).
			  /// </summary>
			  internal virtual HilbertCurve3D RotateOneThirdDiagonalPos( bool direction )
			  {
					int[] newNpoints = new int[Length()];
					for ( int i = 0; i < Length(); i++ )
					{
						 if ( direction )
						 {
							  newNpoints[i] = BinaryCoordinateRotationUtils3D.RotateNPointRight( NpointValues[i] );
						 }
						 else
						 {
							  newNpoints[i] = BinaryCoordinateRotationUtils3D.RotateNPointLeft( NpointValues[i] );
						 }
					}
					return new HilbertCurve3D( newNpoints );
			  }

			  /// <summary>
			  /// Rotate about the neg-x diagonal (the 100->011 diagonal). This is similar to the
			  /// normal diagonal rotation, but with x-switched, so we XOR the x value before and after
			  /// the rotation, and rotate in the opposite direction to specified.
			  /// </summary>
			  internal virtual HilbertCurve3D RotateOneThirdDiagonalNeg( bool direction )
			  {
					int[] newNpoints = new int[Length()];
					for ( int i = 0; i < Length(); i++ )
					{
						 if ( direction )
						 {
							  newNpoints[i] = BinaryCoordinateRotationUtils3D.XXOR( BinaryCoordinateRotationUtils3D.RotateNPointLeft( BinaryCoordinateRotationUtils3D.XXOR( NpointValues[i] ) ) );
						 }
						 else
						 {
							  newNpoints[i] = BinaryCoordinateRotationUtils3D.XXOR( BinaryCoordinateRotationUtils3D.RotateNPointRight( BinaryCoordinateRotationUtils3D.XXOR( NpointValues[i] ) ) );
						 }
					}
					return new HilbertCurve3D( newNpoints );
			  }

			  /// <summary>
			  /// Rotate about the x-axis. This involves leaving x values the same, but xOR'ing the rest.
			  /// </summary>
			  internal virtual HilbertCurve3D RotateAboutX()
			  {
					int[] newNpoints = new int[Length()];
					for ( int i = 0; i < Length(); i++ )
					{
						 newNpoints[i] = BinaryCoordinateRotationUtils3D.RotateYZ( NpointValues[i] );
					}
					return new HilbertCurve3D( newNpoints );
			  }

			  internal virtual void BuildCurveTree( IDictionary<SubCurve3D, HilbertCurve3D> curves )
			  {
					if ( Children == null )
					{
						 MakeChildren( curves );
						 curves[Name()] = this;

						 foreach ( HilbertCurve3D child in Children )
						 {
							  child.BuildCurveTree( curves );
						 }
					}
			  }

			  internal virtual void MakeChildren( IDictionary<SubCurve3D, HilbertCurve3D> curves )
			  {
					Children = new HilbertCurve3D[Length()];
					Children[0] = Singleton( curves, RotateOneThirdDiagonalPos( true ) );
					Children[1] = Singleton( curves, RotateOneThirdDiagonalPos( false ) );
					Children[2] = Singleton( curves, RotateOneThirdDiagonalPos( false ) );
					Children[3] = Singleton( curves, RotateAboutX() );
					Children[4] = Singleton( curves, RotateAboutX() );
					Children[5] = Singleton( curves, RotateOneThirdDiagonalNeg( true ) );
					Children[6] = Singleton( curves, RotateOneThirdDiagonalNeg( true ) );
					Children[7] = Singleton( curves, RotateOneThirdDiagonalNeg( false ) );
			  }

			  internal virtual HilbertCurve3D Singleton( IDictionary<SubCurve3D, HilbertCurve3D> curves, HilbertCurve3D newCurve )
			  {
					return curves.computeIfAbsent( newCurve.Name(), key => newCurve );
			  }
		 }

		 internal enum Direction3D
		 {
			  Up,
			  Right,
			  Left,
			  Down,
			  Front,
			  Back
		 }

		 internal class SubCurve3D
		 {
			  internal readonly Direction3D FirstMove;
			  internal readonly Direction3D SecondMove;
			  internal readonly Direction3D OverallDirection;

			  internal SubCurve3D( Direction3D firstMove, Direction3D secondMove, Direction3D overallDirection )
			  {
					this.FirstMove = firstMove;
					this.SecondMove = secondMove;
					this.OverallDirection = overallDirection;
			  }

			  public override int GetHashCode()
			  {
					return java.util.Objects.hash( FirstMove, SecondMove, OverallDirection );
			  }

			  public override bool Equals( object obj )
			  {
					if ( obj == null || this.GetType() != obj.GetType() )
					{
						 return false;
					}
					SubCurve3D other = ( SubCurve3D ) obj;
					return this.FirstMove == other.FirstMove && this.SecondMove == other.SecondMove && this.OverallDirection == other.OverallDirection;
			  }

			  public override string ToString()
			  {
					return FirstMove.ToString() + SecondMove.ToString() + OverallDirection.ToString();
			  }
		 }

		 // this is left accessible to make debugging easier
		 internal static IDictionary<SubCurve3D, HilbertCurve3D> Curves = new LinkedHashMap<SubCurve3D, HilbertCurve3D>();

		 private static HilbertCurve3D BuildTheCurve()
		 {
			  // We start with a UFR curve
			  int[] npointValues = new int[] { 0b000, 0b010, 0b011, 0b001, 0b101, 0b111, 0b110, 0b100 };
			  HilbertCurve3D theCurve = new HilbertCurve3D( npointValues );

			  theCurve.BuildCurveTree( Curves );
			  return theCurve;
		 }

		 private static readonly HilbertCurve3D _theCurve = BuildTheCurve();

		 public const int MAX_LEVEL = 63 / 3 - 1;

		 public HilbertSpaceFillingCurve3D( Envelope range ) : this( range, MAX_LEVEL )
		 {
		 }

		 public HilbertSpaceFillingCurve3D( Envelope range, int maxLevel ) : base( range, maxLevel )
		 {
			  Debug.Assert( maxLevel <= MAX_LEVEL );
			  Debug.Assert( range.Dimension == 3 );
		 }

		 protected internal override CurveRule RootCurve()
		 {
			  return _theCurve;
		 }
	}

}