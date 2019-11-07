using System.Collections.Generic;

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
namespace Neo4Net.Test.mockito.mock
{

	using CRS = Neo4Net.GraphDb.Spatial.CRS;
	using Coordinate = Neo4Net.GraphDb.Spatial.Coordinate;
	using Geometry = Neo4Net.GraphDb.Spatial.Geometry;
	using Point = Neo4Net.GraphDb.Spatial.Point;

	public class SpatialMocks
	{
		 public static MockPoint MockPoint( double x, double y, CRS crs )
		 {
			  return new MockPoint( x, y, crs );
		 }

		 public static MockPoint3D MockPoint( double x, double y, double z, CRS crs )
		 {
			  return new MockPoint3D( x, y, z, crs );
		 }

		 public static MockGeometry MockGeometry( string geometryType, IList<Coordinate> coordinates, CRS crs )
		 {
			  return new MockGeometry( geometryType, coordinates, crs );
		 }

		 public static CRS MockWGS84()
		 {
			  return MockCRS( 4326, "WGS-84", "http://spatialreference.org/ref/epsg/4326/" );
		 }

		 public static CRS MockCartesian()
		 {
			  return MockCRS( 7203, "cartesian", "http://spatialreference.org/ref/sr-org/7203/" );
		 }

		 public static CRS MockWGS84_3D()
		 {
			  return MockCRS( 4979, "WGS-84-3D", "http://spatialreference.org/ref/epsg/4979/" );
		 }

		 public static CRS MockCartesian_3D()
		 {
			  return MockCRS( 9157, "cartesian-3D", "http://spatialreference.org/ref/sr-org/9157/" );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static Neo4Net.GraphDb.Spatial.CRS mockCRS(final int code, final String type, final String href)
		 private static CRS MockCRS( int code, string type, string href )
		 {
			  return new CRSAnonymousInnerClass( code, type, href );
		 }

		 private class CRSAnonymousInnerClass : CRS
		 {
			 private int _code;
			 private string _type;
			 private string _href;

			 public CRSAnonymousInnerClass( int code, string type, string href )
			 {
				 this._code = code;
				 this._type = type;
				 this._href = href;
			 }

			 public int Code
			 {
				 get
				 {
					  return _code;
				 }
			 }

			 public string Type
			 {
				 get
				 {
					  return _type;
				 }
			 }

			 public string Href
			 {
				 get
				 {
					  return _href;
				 }
			 }
		 }

		 private class MockPoint : MockGeometry, Point
		 {
			  internal readonly Coordinate Coordinate;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private MockPoint(final double x, final double y, final Neo4Net.GraphDb.Spatial.CRS crs)
			  internal MockPoint( double x, double y, CRS crs ) : base( "Point", new List<Coordinate>(), crs )
			  {
					this.Coordinate = new Coordinate( x, y );
					this.CoordinatesConflict.Add( this.Coordinate );
			  }
		 }

		 private class MockPoint3D : MockGeometry, Point
		 {
			  internal readonly Coordinate Coordinate;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private MockPoint3D(final double x, final double y, double z, final Neo4Net.GraphDb.Spatial.CRS crs)
			  internal MockPoint3D( double x, double y, double z, CRS crs ) : base( "Point", new List<Coordinate>(), crs )
			  {
					this.Coordinate = new Coordinate( x, y, z );
					this.CoordinatesConflict.Add( this.Coordinate );
			  }
		 }

		 private class MockGeometry : Geometry
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string GeometryTypeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IList<Coordinate> CoordinatesConflict;
			  protected internal readonly CRS Crs;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private MockGeometry(String geometryType, final java.util.List<Neo4Net.GraphDb.Spatial.Coordinate> coordinates, final Neo4Net.GraphDb.Spatial.CRS crs)
			  internal MockGeometry( string geometryType, IList<Coordinate> coordinates, CRS crs )
			  {
					this.GeometryTypeConflict = geometryType;
					this.CoordinatesConflict = coordinates;
					this.Crs = crs;
			  }

			  public virtual string GeometryType
			  {
				  get
				  {
						return GeometryTypeConflict;
				  }
			  }

			  public virtual IList<Coordinate> Coordinates
			  {
				  get
				  {
						return CoordinatesConflict;
				  }
			  }

			  public virtual CRS CRS
			  {
				  get
				  {
						return Crs;
				  }
			  }

			  public override string ToString()
			  {
					return GeometryTypeConflict;
			  }
		 }
	}

}