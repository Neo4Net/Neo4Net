using System.Diagnostics;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Gis.Spatial.Index.curves
{

	public class ZOrderSpaceFillingCurve2D : SpaceFillingCurve
	{

		 /// <summary>
		 /// Description of the space filling curve structure
		 /// </summary>
		 internal class ZOrderCurve2D : CurveRule
		 {

			  internal ZOrderCurve2D( params int[] npointValues ) : base( 2, npointValues )
			  {
					Debug.Assert( npointValues[0] == 1 && npointValues[3] == 2 );
			  }

			  public override CurveRule ChildAt( int npoint )
			  {
					return this;
			  }

			  public override string ToString()
			  {
					return "Z";
			  }
		 }

		 private static readonly ZOrderCurve2D _rootCurve = new ZOrderCurve2D( 1, 3, 0, 2 );

		 public const int MAX_LEVEL = 63 / 2 - 1;

		 public ZOrderSpaceFillingCurve2D( Envelope range ) : this( range, MAX_LEVEL )
		 {
		 }

		 public ZOrderSpaceFillingCurve2D( Envelope range, int maxLevel ) : base( range, maxLevel )
		 {
			  Debug.Assert( maxLevel <= MAX_LEVEL );
			  Debug.Assert( range.Dimension == 2 );
		 }

		 protected internal override CurveRule RootCurve()
		 {
			  return _rootCurve;
		 }
	}

}