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
namespace Neo4Net.Values.Storable
{

	using CRS = Neo4Net.GraphDb.Spatial.CRS;
	using InvalidValuesArgumentException = Neo4Net.Values.utils.InvalidValuesArgumentException;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;

	public class CoordinateReferenceSystem : CRS
	{
		 public static readonly CoordinateReferenceSystem Cartesian = new CoordinateReferenceSystem( "cartesian", CRSTable.SrOrg, 7203, 2, false );
		 public static readonly CoordinateReferenceSystem Cartesian_3D = new CoordinateReferenceSystem( "cartesian-3d", CRSTable.SrOrg, 9157, 3, false );
		 public static readonly CoordinateReferenceSystem Wgs84 = new CoordinateReferenceSystem( "wgs-84", CRSTable.Epsg, 4326, 2, true );
		 public static readonly CoordinateReferenceSystem Wgs84_3d = new CoordinateReferenceSystem( "wgs-84-3d", CRSTable.Epsg, 4979, 3, true );

		 private static readonly CoordinateReferenceSystem[] _types = new CoordinateReferenceSystem[]{ Cartesian, Cartesian_3D, Wgs84, Wgs84_3d };

		 public static IEnumerable<CoordinateReferenceSystem> All()
		 {
			  return Iterables.asIterable( _types );
		 }

		 public static CoordinateReferenceSystem Get( int tableId, int code )
		 {
			  CRSTable table = CRSTable.find( tableId );
			  foreach ( CoordinateReferenceSystem type in _types )
			  {
					if ( type._table == table && type._code == code )
					{
						 return type;
					}
			  }
			  throw new InvalidValuesArgumentException( "Unknown coordinate reference system: " + tableId + "-" + code );
		 }

		 public static CoordinateReferenceSystem Get( CRS crs )
		 {
			  Objects.requireNonNull( crs );
			  return Get( crs.Href );
		 }

		 public static CoordinateReferenceSystem ByName( string name )
		 {
			  foreach ( CoordinateReferenceSystem type in _types )
			  {
					if ( type._name.Equals( name.ToLower() ) )
					{
						 return type;
					}
			  }

			  throw new InvalidValuesArgumentException( "Unknown coordinate reference system: " + name );
		 }

		 public static CoordinateReferenceSystem Get( string href )
		 {
			  foreach ( CoordinateReferenceSystem type in _types )
			  {
					if ( type._href.Equals( href ) )
					{
						 return type;
					}
			  }
			  throw new InvalidValuesArgumentException( "Unknown coordinate reference system: " + href );
		 }

		 public static CoordinateReferenceSystem Get( int code )
		 {
			  foreach ( CRSTable table in CRSTable.values() )
			  {
					string href = table.href( code );
					foreach ( CoordinateReferenceSystem type in _types )
					{
						 if ( type._href.Equals( href ) )
						 {
							  return type;
						 }
					}
			  }
			  throw new InvalidValuesArgumentException( "Unknown coordinate reference system code: " + code );
		 }

		 private readonly string _name;
		 private readonly CRSTable _table;
		 private readonly int _code;
		 private readonly string _href;
		 private readonly int _dimension;
		 private readonly bool _geographic;
		 private readonly CRSCalculator _calculator;

		 private CoordinateReferenceSystem( string name, CRSTable table, int code, int dimension, bool geographic )
		 {
			  Debug.Assert( name.ToLower().Equals(name) );
			  this._name = name;
			  this._table = table;
			  this._code = code;
			  this._href = table.href( code );
			  this._dimension = dimension;
			  this._geographic = geographic;
			  if ( geographic )
			  {
					this._calculator = new CRSCalculator.GeographicCalculator( dimension );
			  }
			  else
			  {
					this._calculator = new CRSCalculator.CartesianCalculator( dimension );
			  }
		 }

		 public override string ToString()
		 {
			  return _name;
		 }

		 public virtual int Code
		 {
			 get
			 {
				  return _code;
			 }
		 }

		 public virtual string Type
		 {
			 get
			 {
				  return _name;
			 }
		 }

		 public virtual string Href
		 {
			 get
			 {
				  return _href;
			 }
		 }

		 public virtual string Name
		 {
			 get
			 {
				  return _name;
			 }
		 }

		 public virtual CRSTable Table
		 {
			 get
			 {
				  return _table;
			 }
		 }

		 public virtual int Dimension
		 {
			 get
			 {
				  return _dimension;
			 }
		 }

		 public virtual bool Geographic
		 {
			 get
			 {
				  return _geographic;
			 }
		 }

		 public virtual CRSCalculator Calculator
		 {
			 get
			 {
				  return _calculator;
			 }
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  CoordinateReferenceSystem that = ( CoordinateReferenceSystem ) o;

			  return _href.Equals( that._href );
		 }

		 public override int GetHashCode()
		 {
			  return _href.GetHashCode();
		 }

	}

}