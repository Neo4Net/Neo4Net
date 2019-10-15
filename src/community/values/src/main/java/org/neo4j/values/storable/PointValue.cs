using System;
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
namespace Neo4Net.Values.Storable
{

	using CRS = Neo4Net.Graphdb.spatial.CRS;
	using Coordinate = Neo4Net.Graphdb.spatial.Coordinate;
	using Point = Neo4Net.Graphdb.spatial.Point;
	using IHashFunction = Neo4Net.Hashing.HashFunction;
	using Neo4Net.Values;
	using InvalidValuesArgumentException = Neo4Net.Values.utils.InvalidValuesArgumentException;
	using PrettyPrinter = Neo4Net.Values.utils.PrettyPrinter;
	using MapValue = Neo4Net.Values.@virtual.MapValue;


	public class PointValue : ScalarValue, Point, IComparable<PointValue>
	{
		 // WGS84 is the CRS w/ lowest table/code at the time of writing this. Update as more CRSs gets added.
		 public static readonly PointValue MinValue = new PointValue( CoordinateReferenceSystem.Wgs84, -180D, -90 );
		 // Cartesian_3D is the CRS w/ highest table/code at the time of writing this. Update as more CRSs gets added.
		 public static readonly PointValue MaxValue = new PointValue( CoordinateReferenceSystem.Cartesian_3D, long.MaxValue, long.MaxValue, long.MaxValue );

		 internal static readonly PointValue MinValueWgs84 = new PointValue( CoordinateReferenceSystem.Wgs84, -180D, -90 );
		 internal static readonly PointValue MaxValueWgs84 = new PointValue( CoordinateReferenceSystem.Wgs84, 180D, 90 );
		 internal static readonly PointValue MinValueWgs84_3d = new PointValue( CoordinateReferenceSystem.Wgs84_3d, -180D, -90, long.MinValue );
		 internal static readonly PointValue MaxValueWgs84_3d = new PointValue( CoordinateReferenceSystem.Wgs84_3d, 180D, 90, long.MaxValue );
		 internal static readonly PointValue MinValueCartesian = new PointValue( CoordinateReferenceSystem.Cartesian, long.MinValue, long.MinValue );
		 internal static readonly PointValue MaxValueCartesian = new PointValue( CoordinateReferenceSystem.Cartesian, long.MaxValue, long.MaxValue );
		 internal static readonly PointValue MinValueCartesian_3d = new PointValue( CoordinateReferenceSystem.Cartesian_3D, long.MinValue, long.MinValue, long.MinValue );
		 internal static readonly PointValue MaxValueCartesian_3d = new PointValue( CoordinateReferenceSystem.Cartesian_3D, long.MaxValue, long.MaxValue, long.MaxValue );

		 private CoordinateReferenceSystem _crs;
		 private double[] _coordinate;

		 internal PointValue( CoordinateReferenceSystem crs, params double[] coordinate )
		 {
			  this._crs = crs;
			  this._coordinate = coordinate;
			  foreach ( double c in coordinate )
			  {
					if ( !Double.isFinite( c ) )
					{
						 throw new InvalidValuesArgumentException( "Cannot create a point with non-finite coordinate values: " + Arrays.ToString( coordinate ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void writeTo(ValueWriter<E> writer) throws E
		 public override void WriteTo<E>( ValueWriter<E> writer ) where E : Exception
		 {
			  writer.WritePoint( CoordinateReferenceSystem, _coordinate );
		 }

		 public override string PrettyPrint()
		 {
			  PrettyPrinter prettyPrinter = new PrettyPrinter();
			  this.WriteTo( prettyPrinter );
			  return prettyPrinter.Value();
		 }

		 public override ValueGroup ValueGroup()
		 {
			  return ValueGroup.Geometry;
		 }

		 public override NumberType NumberType()
		 {
			  return NumberType.NoNumber;
		 }

		 public override bool Equals( Value other )
		 {
			  if ( other is PointValue )
			  {
					PointValue pv = ( PointValue ) other;
					return Arrays.Equals( this._coordinate, pv._coordinate ) && this.CoordinateReferenceSystem.Equals( pv.CoordinateReferenceSystem );
			  }
			  return false;
		 }

		 public virtual bool Equals( Point other )
		 {
			  if ( !other.CRS.Href.Equals( this.CRS.Href ) )
			  {
					return false;
			  }
			  // TODO: This can be an assert
			  IList<double> otherCoordinate = other.Coordinate.Coordinate;
			  if ( otherCoordinate.Count != this._coordinate.Length )
			  {
					return false;
			  }
			  for ( int i = 0; i < this._coordinate.Length; i++ )
			  {
					if ( otherCoordinate[i] != this._coordinate[i] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public override bool Eq( object other )
		 {
			  return other != null && ( ( other is Value && Equals( ( Value ) other ) ) || ( other is Point && Equals( ( Point ) other ) ) );
		 }

		 public override int CompareTo( PointValue other )
		 {
			  int cmpCRS = Integer.compare( this._crs.Code, other._crs.Code );
			  if ( cmpCRS != 0 )
			  {
					return cmpCRS;
			  }

			  // TODO: This is unnecessary and can be an assert. Is it even correct? This implies e.g. that all 2D points are before all 3D regardless of x and y
			  if ( this._coordinate.Length > other._coordinate.Length )
			  {
					return 1;
			  }
			  else if ( this._coordinate.Length < other._coordinate.Length )
			  {
					return -1;
			  }

			  for ( int i = 0; i < _coordinate.Length; i++ )
			  {
					int cmpVal = this._coordinate[i].CompareTo( other._coordinate[i] );
					if ( cmpVal != 0 )
					{
						 return cmpVal;
					}
			  }
			  return 0;
		 }

		 internal override int UnsafeCompareTo( Value otherValue )
		 {
			  return CompareTo( ( PointValue ) otherValue );
		 }

		 public override Comparison UnsafeTernaryCompareTo( Value otherValue )
		 {
			  PointValue other = ( PointValue ) otherValue;

			  if ( this._crs.Code != other._crs.Code || this._coordinate.Length != other._coordinate.Length )
			  {
					return Comparison.UNDEFINED;
			  }

			  int eq = 0;
			  int gt = 0;
			  int lt = 0;
			  for ( int i = 0; i < _coordinate.Length; i++ )
			  {
					int cmpVal = this._coordinate[i].CompareTo( other._coordinate[i] );
					if ( cmpVal > 0 )
					{
						 gt++;
					}
					else if ( cmpVal < 0 )
					{
						 lt++;
					}
					else
					{
						 eq++;
					}
			  }
			  if ( eq == _coordinate.Length )
			  {
					return Comparison.EQUAL;
			  }
			  else if ( gt == _coordinate.Length )
			  {
					return Comparison.GREATER_THAN;
			  }
			  else if ( lt == _coordinate.Length )
			  {
					return Comparison.SMALLER_THAN;
			  }
			  else if ( lt == 0 )
			  {
					return Comparison.GREATER_THAN_AND_EQUAL;
			  }
			  else if ( gt == 0 )
			  {
					return Comparison.SMALLER_THAN_AND_EQUAL;
			  }
			  else
			  {
					return Comparison.UNDEFINED;
			  }
		 }

		 public override Point AsObjectCopy()
		 {
			  return this;
		 }

		 public virtual CoordinateReferenceSystem CoordinateReferenceSystem
		 {
			 get
			 {
				  return _crs;
			 }
		 }

		 /*
		  * Consumers must not modify the returned array.
		  */
		 public virtual double[] Coordinate()
		 {
			  return this._coordinate;
		 }

		 public override int ComputeHash()
		 {
			  int result = 1;
			  result = 31 * result + NumberValues.hash( _crs.Code );
			  result = 31 * result + NumberValues.hash( _coordinate );
			  return result;
		 }

		 public override long UpdateHash( IHashFunction hashFunction, long hash )
		 {
			  hash = hashFunction.Update( hash, _crs.Code );
			  foreach ( double v in _coordinate )
			  {
					hash = hashFunction.Update( hash, System.BitConverter.DoubleToInt64Bits( v ) );
			  }
			  return hash;
		 }

		 public override T Map<T>( ValueMapper<T> mapper )
		 {
			  return mapper.MapPoint( this );
		 }

		 public override string ToString()
		 {
			  string coordString = _coordinate.Length == 2 ? format( "x: %s, y: %s", _coordinate[0], _coordinate[1] ) : format( "x: %s, y: %s, z: %s", _coordinate[0], _coordinate[1], _coordinate[2] );
			  return format( "point({%s, crs: '%s'})", coordString, CoordinateReferenceSystem.Name ); //TODO: Use getTypeName -> Breaking change
		 }

		 public override string TypeName
		 {
			 get
			 {
				  return "Point";
			 }
		 }

		 /// <summary>
		 /// The string representation of this object when indexed in string-only indexes, like lucene, for equality search only. This should normally only
		 /// happen when points are part of composite indexes, because otherwise they are indexed in the spatial index.
		 /// </summary>
		 public virtual string ToIndexableString()
		 {
			  CoordinateReferenceSystem crs = CoordinateReferenceSystem;
			  return format( "P:%d-%d%s", crs.Table.TableId, crs.Code, Arrays.ToString( _coordinate ) );
		 }

		 public virtual IList<Coordinate> Coordinates
		 {
			 get
			 {
				  return singletonList( new Coordinate( _coordinate ) );
			 }
		 }

		 public virtual CRS CRS
		 {
			 get
			 {
				  return _crs;
			 }
		 }

		 /// <summary>
		 /// Checks if this point is greater than (or equal) to lower and smaller than (or equal) to upper.
		 /// </summary>
		 /// <param name="lower"> point this value should be greater than </param>
		 /// <param name="includeLower"> governs if the lower comparison should be inclusive </param>
		 /// <param name="upper"> point this value should be smaller than </param>
		 /// <param name="includeUpper"> governs if the upper comparison should be inclusive </param>
		 /// <returns> true if this value is within the described range </returns>
		 public virtual bool? WithinRange( PointValue lower, bool includeLower, PointValue upper, bool includeUpper )
		 {
			  // Unbounded
			  if ( lower == null && upper == null )
			  {
					return true;
			  }

			  // Invalid bounds (lower greater than upper)
			  if ( lower != null && upper != null )
			  {
					Comparison comparison = lower.UnsafeTernaryCompareTo( upper );
					if ( comparison == Comparison.UNDEFINED || comparison == Comparison.GREATER_THAN || comparison == Comparison.GREATER_THAN_AND_EQUAL )
					{
						 return null;
					}
			  }

			  // Lower bound defined
			  if ( lower != null )
			  {
					Comparison comparison = this.UnsafeTernaryCompareTo( lower );
					if ( comparison == Comparison.UNDEFINED )
					{
						 return null;
					}
					else if ( comparison == Comparison.SMALLER_THAN || comparison == Comparison.SMALLER_THAN_AND_EQUAL || ( comparison == Comparison.EQUAL || comparison == Comparison.GREATER_THAN_AND_EQUAL ) && !includeLower )
					{
						 if ( upper != null && this.UnsafeTernaryCompareTo( upper ) == Comparison.UNDEFINED )
						 {
							  return null;
						 }
						 else
						 {
							  return false;
						 }
					}
			  }

			  // Upper bound defined
			  if ( upper != null )
			  {
					Comparison comparison = this.UnsafeTernaryCompareTo( upper );
					if ( comparison == Comparison.UNDEFINED )
					{
						 return null;
					}
					else if ( comparison == Comparison.GREATER_THAN || comparison == Comparison.GREATER_THAN_AND_EQUAL || ( comparison == Comparison.EQUAL || comparison == Comparison.SMALLER_THAN_AND_EQUAL ) && !includeUpper )
					{
						 return false;
					}
			  }

			  return true;
		 }

		 public static PointValue FromMap( MapValue map )
		 {
			  PointBuilder fields = new PointBuilder();
			  map.Foreach( ( key, value ) => fields.assign( key.ToLower(), value ) );
			  return FromInputFields( fields );
		 }

		 public static PointValue Parse( CharSequence text )
		 {
			  return PointValue.Parse( text, null );
		 }

		 /// <summary>
		 /// Parses the given text into a PointValue. The information stated in the header is saved into the PointValue
		 /// unless it is overridden by the information in the text
		 /// </summary>
		 /// <param name="text"> the input text to be parsed into a PointValue </param>
		 /// <param name="fieldsFromHeader"> must be a value obtained from <seealso cref="parseHeaderInformation(CharSequence)"/> or null </param> </returns> </param>
		 /// <returns> a PointValue instance with information from the {<param name="fieldsFromHeader">} and {<param name="text">} </param>
		 public static PointValue Parse( CharSequence text, CSVHeaderInformation fieldsFromHeader )
		 {
			  PointBuilder fieldsFromData = ParseHeaderInformation( text );
			  if ( fieldsFromHeader != null )
			  {
					// Merge InputFields: Data fields override header fields
					if ( !( fieldsFromHeader is PointBuilder ) )
					{
						 throw new System.InvalidOperationException( "Wrong header information type: " + fieldsFromHeader );
					}
					fieldsFromData.MergeWithHeader( ( PointBuilder ) fieldsFromHeader );
			  }
			  return FromInputFields( fieldsFromData );
		 }

		 public static PointBuilder ParseHeaderInformation( CharSequence text )
		 {
			  PointBuilder fields = new PointBuilder();
			  Value.ParseHeaderInformation( text, "point", fields );
			  return fields;
		 }

		 private static CoordinateReferenceSystem FindSpecifiedCRS( PointBuilder fields )
		 {
			  string crsValue = fields.Crs;
			  int sridValue = fields.Srid;
			  if ( !string.ReferenceEquals( crsValue, null ) && sridValue != -1 )
			  {
					throw new InvalidValuesArgumentException( "Cannot specify both CRS and SRID" );
			  }
			  else if ( !string.ReferenceEquals( crsValue, null ) )
			  {
					return CoordinateReferenceSystem.ByName( crsValue );
			  }
			  else if ( sridValue != -1 )
			  {
					return CoordinateReferenceSystem.Get( sridValue );
			  }
			  else
			  {
					return null;
			  }
		 }

		 /// <summary>
		 /// This contains the logic to decide the default coordinate reference system based on the input fields
		 /// </summary>
		 private static PointValue FromInputFields( PointBuilder fields )
		 {
			  CoordinateReferenceSystem crs = FindSpecifiedCRS( fields );
			  double[] coordinates;

			  if ( fields.X != null && fields.Y != null )
			  {
					coordinates = fields.Z != null ? new double[]{ fields.X.Value, fields.Y.Value, fields.Z.Value } : new double[]{ fields.X.Value, fields.Y.Value };
					if ( crs == null )
					{
						 crs = coordinates.Length == 3 ? CoordinateReferenceSystem.Cartesian_3D : CoordinateReferenceSystem.Cartesian;
					}
			  }
			  else if ( fields.Latitude != null && fields.Longitude != null )
			  {
					if ( fields.Z != null )
					{
						 coordinates = new double[]{ fields.Longitude.Value, fields.Latitude.Value, fields.Z.Value };
					}
					else if ( fields.Height != null )
					{
						 coordinates = new double[]{ fields.Longitude.Value, fields.Latitude.Value, fields.Height.Value };
					}
					else
					{
						 coordinates = new double[]{ fields.Longitude.Value, fields.Latitude.Value };
					}
					if ( crs == null )
					{
						 crs = coordinates.Length == 3 ? CoordinateReferenceSystem.Wgs84_3d : CoordinateReferenceSystem.Wgs84;
					}
					if ( !crs.Geographic )
					{
						 throw new InvalidValuesArgumentException( "Geographic points does not support coordinate reference system: " + crs + ". This is set either in the csv header or the actual data column" );
					}
			  }
			  else
			  {
					if ( crs == CoordinateReferenceSystem.Cartesian )
					{
						 throw new InvalidValuesArgumentException( "A " + CoordinateReferenceSystem.Cartesian.Name + " point must contain 'x' and 'y'" );
					}
					else if ( crs == CoordinateReferenceSystem.Cartesian_3D )
					{
						 throw new InvalidValuesArgumentException( "A " + CoordinateReferenceSystem.Cartesian_3D.Name + " point must contain 'x', 'y' and 'z'" );
					}
					else if ( crs == CoordinateReferenceSystem.Wgs84 )
					{
						 throw new InvalidValuesArgumentException( "A " + CoordinateReferenceSystem.Wgs84.Name + " point must contain 'latitude' and 'longitude'" );
					}
					else if ( crs == CoordinateReferenceSystem.Wgs84_3d )
					{
						 throw new InvalidValuesArgumentException( "A " + CoordinateReferenceSystem.Wgs84_3d.Name + " point must contain 'latitude', 'longitude' and 'height'" );
					}
					throw new InvalidValuesArgumentException( "A point must contain either 'x' and 'y' or 'latitude' and 'longitude'" );
			  }

			  if ( crs.Dimension != coordinates.Length )
			  {
					throw new InvalidValuesArgumentException( "Cannot create point with " + crs.Dimension + "D coordinate reference system and " + coordinates.Length + " coordinates. Please consider using equivalent " + coordinates.Length + "D coordinate reference system" );
			  }
			  return Values.PointValue( crs, coordinates );
		 }

		 /// <summary>
		 /// For accessors from cypher.
		 /// </summary>
		 public virtual Value Get( string fieldName )
		 {
			 return PointFields.fromName( fieldName ).get( this );
		 }

		 internal virtual DoubleValue GetNthCoordinate( int n, string fieldName, bool onlyGeographic )
		 {
			  if ( onlyGeographic && !this.CoordinateReferenceSystem.Geographic )
			  {
					throw new InvalidValuesArgumentException( "Field: " + fieldName + " is not available on cartesian point: " + this );
			  }
			  else if ( n >= this.Coordinate().Length )
			  {
					throw new InvalidValuesArgumentException( "Field: " + fieldName + " is not available on point: " + this );
			  }
			  else
			  {
					return Values.DoubleValue( _coordinate[n] );
			  }
		 }

		 private class PointBuilder : CSVHeaderInformation
		 {
			  internal string Crs;
			  internal double? X;
			  internal double? Y;
			  internal double? Z;
			  internal double? Longitude;
			  internal double? Latitude;
			  internal double? Height;
			  internal int Srid = -1;

			  public override void Assign( string key, object value )
			  {
					switch ( key.ToLower() )
					{
					case "crs":
						 CheckUnassigned( Crs, key );
						 AssignTextValue( key, value, str => Crs = QuotesPattern.matcher( str ).replaceAll( "" ) );
						 break;
					case "x":
						 CheckUnassigned( X, key );
						 AssignFloatingPoint( key, value, i => X = i );
						 break;
					case "y":
						 CheckUnassigned( Y, key );
						 AssignFloatingPoint( key, value, i => Y = i );
						 break;
					case "z":
						 CheckUnassigned( Z, key );
						 AssignFloatingPoint( key, value, i => Z = i );
						 break;
					case "longitude":
						 CheckUnassigned( Longitude, key );
						 AssignFloatingPoint( key, value, i => Longitude = i );
						 break;
					case "latitude":
						 CheckUnassigned( Latitude, key );
						 AssignFloatingPoint( key, value, i => Latitude = i );
						 break;
					case "height":
						 CheckUnassigned( Height, key );
						 AssignFloatingPoint( key, value, i => Height = i );
						 break;
					case "srid":
						 if ( Srid != -1 )
						 {
							  throw new InvalidValuesArgumentException( string.Format( "Duplicate field '{0}' is not allowed.", key ) );
						 }
						 AssignIntegral( key, value, i => Srid = i );
						 break;
					default:
				break;
					}
			  }

			  internal virtual void MergeWithHeader( PointBuilder header )
			  {
					this.Crs = string.ReferenceEquals( this.Crs, null ) ? header.Crs : this.Crs;
					this.X = this.X == null ? header.X : this.X;
					this.Y = this.Y == null ? header.Y : this.Y;
					this.Z = this.Z == null ? header.Z : this.Z;
					this.Longitude = this.Longitude == null ? header.Longitude : this.Longitude;
					this.Latitude = this.Latitude == null ? header.Latitude : this.Latitude;
					this.Height = this.Height == null ? header.Height : this.Height;
					this.Srid = this.Srid == -1 ? header.Srid : this.Srid;
			  }

			  internal virtual void AssignTextValue( string key, object value, System.Action<string> assigner )
			  {
					if ( value is string )
					{
						 assigner( ( string ) value );
					}
					else if ( value is TextValue )
					{
						 assigner( ( ( TextValue ) value ).StringValue() );
					}
					else
					{
						 throw new InvalidValuesArgumentException( string.Format( "Cannot assign {0} to field {1}", value, key ) );
					}
			  }

			  internal virtual void AssignFloatingPoint( string key, object value, System.Action<double> assigner )
			  {
					if ( value is string )
					{
						 assigner( AssertConvertible( () => double.Parse((string) value) ) );
					}
					else if ( value is IntegralValue )
					{
						 assigner( ( ( IntegralValue ) value ).DoubleValue() );
					}
					else if ( value is FloatingPointValue )
					{
						 assigner( ( ( FloatingPointValue ) value ).DoubleValue() );
					}
					else
					{
						 throw new InvalidValuesArgumentException( string.Format( "Cannot assign {0} to field {1}", value, key ) );
					}
			  }

			  internal virtual void AssignIntegral( string key, object value, System.Action<int> assigner )
			  {
					if ( value is string )
					{
						 assigner( AssertConvertible( () => int.Parse((string) value) ) );
					}
					else if ( value is IntegralValue )
					{
						 assigner( ( int )( ( IntegralValue ) value ).LongValue() );
					}
					else
					{
						 throw new InvalidValuesArgumentException( string.Format( "Cannot assign {0} to field {1}", value, key ) );
					}
			  }

			  internal virtual T AssertConvertible<T>( System.Func<T> func ) where T : Number
			  {
					try
					{
						 return func();
					}
					catch ( System.FormatException e )
					{
						 throw new InvalidValuesArgumentException( e.Message, e );
					}
			  }

			  internal virtual void CheckUnassigned( object key, string fieldName )
			  {
					if ( key != null )
					{
						 throw new InvalidValuesArgumentException( string.Format( "Duplicate field '{0}' is not allowed.", fieldName ) );
					}
			  }
		 }
	}

}