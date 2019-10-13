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
//	import static org.neo4j.gis.spatial.index.curves.HilbertSpaceFillingCurve2D.Direction2D.DOWN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.gis.spatial.index.curves.HilbertSpaceFillingCurve2D.Direction2D.LEFT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.gis.spatial.index.curves.HilbertSpaceFillingCurve2D.Direction2D.RIGHT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.gis.spatial.index.curves.HilbertSpaceFillingCurve2D.Direction2D.UP;

	public class HilbertSpaceFillingCurve2D : SpaceFillingCurve
	{

		 /// <summary>
		 /// Description of the space filling curve structure
		 /// </summary>
		 internal class HilbertCurve2D : CurveRule
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal CurveRule[] ChildrenConflict;

			  internal HilbertCurve2D( params int[] npointValues ) : base( 2, npointValues )
			  {
					Debug.Assert( npointValues[0] == 0 || npointValues[0] == 3 );
			  }

			  internal virtual Direction2D Direction( int end )
			  {
					int start = NpointValues[0];
					end -= start;
					switch ( end )
					{
					case 1:
						 return UP; // move up      00->01
					case 2:
						 return RIGHT; // move right   00->10
					case -2:
						 return LEFT; // move left    11->01
					case -1:
						 return DOWN; // move down    11->10
					default:
						 throw new System.ArgumentException( "Illegal direction: " + end );
					}
			  }

			  internal virtual Direction2D Name()
			  {
					return Direction( NpointValues[1] );
			  }

			  internal virtual params CurveRule[] Children
			  {
				  set
				  {
						this.ChildrenConflict = value;
				  }
			  }

			  public override CurveRule ChildAt( int npoint )
			  {
					return ChildrenConflict[npoint];
			  }

			  public override string ToString()
			  {
					return Name().ToString();
			  }
		 }

		 internal enum Direction2D
		 {
			  Up,
			  Right,
			  Left,
			  Down
		 }

		 private static Dictionary<Direction2D, HilbertCurve2D> _curves = new Dictionary<Direction2D, HilbertCurve2D>( typeof( Direction2D ) );

		 private static void AddCurveRule( params int[] npointValues )
		 {
			  HilbertCurve2D curve = new HilbertCurve2D( npointValues );
			  Direction2D name = curve.Name();
			  if ( !_curves.ContainsKey( name ) )
			  {
					_curves[name] = curve;
			  }
		 }

		 private static void SetChildren( Direction2D parent, params Direction2D[] children )
		 {
			  HilbertCurve2D curve = _curves[parent];
			  HilbertCurve2D[] childCurves = new HilbertCurve2D[children.Length];
			  for ( int i = 0; i < children.Length; i++ )
			  {
					childCurves[i] = _curves[children[i]];
			  }
			  curve.Children = childCurves;
		 }

		 private static readonly HilbertCurve2D _curveUp;

		 static HilbertSpaceFillingCurve2D()
		 {
			  AddCurveRule( 0, 1, 3, 2 );
			  AddCurveRule( 0, 2, 3, 1 );
			  AddCurveRule( 3, 1, 0, 2 );
			  AddCurveRule( 3, 2, 0, 1 );
			  SetChildren( UP, RIGHT, UP, UP, LEFT );
			  SetChildren( RIGHT, UP, RIGHT, RIGHT, DOWN );
			  SetChildren( DOWN, LEFT, DOWN, DOWN, RIGHT );
			  SetChildren( LEFT, DOWN, LEFT, LEFT, UP );
			  _curveUp = _curves[UP];
		 }

		 public const int MAX_LEVEL = 63 / 2 - 1;

		 public HilbertSpaceFillingCurve2D( Envelope range ) : this( range, MAX_LEVEL )
		 {
		 }

		 public HilbertSpaceFillingCurve2D( Envelope range, int maxLevel ) : base( range, maxLevel )
		 {
			  Debug.Assert( maxLevel <= MAX_LEVEL );
			  Debug.Assert( range.Dimension == 2 );
		 }

		 protected internal override CurveRule RootCurve()
		 {
			  return _curveUp;
		 }

	}

}