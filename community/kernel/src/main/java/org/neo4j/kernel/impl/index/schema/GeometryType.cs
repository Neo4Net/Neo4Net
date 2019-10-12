using System;

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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{

	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;
	using PointValue = Org.Neo4j.Values.Storable.PointValue;
	using Value = Org.Neo4j.Values.Storable.Value;
	using ValueGroup = Org.Neo4j.Values.Storable.ValueGroup;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;

	internal class GeometryType : Type
	{
		 // Affected key state:
		 // long0 (rawValueBits)
		 // long1 (coordinate reference system tableId)
		 // long2 (coordinate reference system code)
		 // long3 (dimensions)
		 // long1Array (coordinates), use long1Array so that it doesn't clash mentally with long0Array in GeometryArrayType

		 // code+table for points (geometry) is 3B in total
		 private static readonly int _maskCode = 0b00000011_11111111_11111111;
		 private static readonly int _maskDimensionsRead = 0b00011100_00000000_00000000;
		 //                                                  ^ this bit is reserved for future expansion of number of dimensions
		 private static readonly int _maskTableRead = 0b11000000_00000000_00000000;
		 private static readonly int _shiftDimensions = Integer.bitCount( _maskCode );
		 private static readonly int _shiftTable = _shiftDimensions + 1 + Integer.bitCount( _maskDimensionsRead );
		 private static readonly int _maskTablePut = ( int )( ( uint )_maskTableRead >> _shiftTable );
		 private static readonly int _maskDimensionsPut = ( int )( ( uint )_maskDimensionsRead >> _shiftDimensions );

		 internal GeometryType( sbyte typeId ) : base( ValueGroup.GEOMETRY, typeId, PointValue.MIN_VALUE, PointValue.MAX_VALUE )
		 {
		 }

		 internal override int ValueSize( GenericKey state )
		 {
			  int coordinatesSize = Dimensions( state ) * GenericKey.SizeGeometryCoordinate;
			  return GenericKey.SIZE_GEOMETRY_HEADER + GenericKey.SizeGeometry + coordinatesSize;
		 }

		 internal static int Dimensions( GenericKey state )
		 {
			  return toIntExact( state.Long3 );
		 }

		 internal override void CopyValue( GenericKey to, GenericKey from )
		 {
			  to.Long0 = from.Long0;
			  to.Long1 = from.Long1;
			  to.Long2 = from.Long2;
			  to.Long3 = from.Long3;
			  int dimensions = dimensions( from );
			  to.Long1Array = EnsureBigEnough( to.Long1Array, dimensions );
			  Array.Copy( from.Long1Array, 0, to.Long1Array, 0, dimensions );
			  to.SpaceFillingCurve = from.SpaceFillingCurve;
		 }

		 internal override Value AsValue( GenericKey state )
		 {
			  AssertHasCoordinates( state );
			  CoordinateReferenceSystem crs = CoordinateReferenceSystem.get( ( int ) state.Long1, ( int ) state.Long2 );
			  return AsValue( state, crs, 0 );
		 }

		 internal static PointValue AsValue( GenericKey state, CoordinateReferenceSystem crs, int offset )
		 {
			  double[] coordinates = new double[Dimensions( state )];
			  for ( int i = 0; i < coordinates.Length; i++ )
			  {
					coordinates[i] = Double.longBitsToDouble( state.Long1Array[offset + i] );
			  }
			  return Values.pointValue( crs, coordinates );
		 }

		 internal override int CompareValue( GenericKey left, GenericKey right )
		 {
			  return Compare( left.Long0, left.Long1, left.Long2, left.Long3, left.Long1Array, 0, right.Long0, right.Long1, right.Long2, right.Long3, right.Long1Array, 0 );
		 }

		 internal override void PutValue( PageCursor cursor, GenericKey state )
		 {
			  PutCrs( cursor, state.Long1, state.Long2, state.Long3 );
			  PutPoint( cursor, state.Long0, state.Long3, state.Long1Array, 0 );
		 }

		 internal override bool ReadValue( PageCursor cursor, int size, GenericKey into )
		 {
			  return ReadCrs( cursor, into ) && ReadPoint( cursor, into );
		 }

		 internal override string ToString( GenericKey state )
		 {
			  string asValueString = HasCoordinates( state ) ? AsValue( state ).ToString() : "NO_COORDINATES";
			  return format( "Geometry[tableId:%d, code:%d, rawValue:%d, value:%s", state.Long1, state.Long2, state.Long0, asValueString );
		 }

		 /// <summary>
		 /// This method will compare along the curve, which is not a spatial comparison, but is correct
		 /// for comparison within the space filling index as long as the original spatial range has already
		 /// been decomposed into a collection of 1D curve ranges before calling down into the GPTree.
		 /// If value on space filling curve is equal then raw comparison of serialized coordinates is done.
		 /// This way points are only considered equal in index if they have the same coordinates and not if
		 /// they only happen to occupy same value on space filling curve.
		 /// </summary>
		 internal static int Compare( long thisLong0, long thisLong1, long thisLong2, long thisLong3, long[] thisLong1Array, int thisCoordinatesOffset, long thatLong0, long thatLong1, long thatLong2, long thatLong3, long[] thatLong1Array, int thatCoordinatesOffset )
		 {
			  int tableIdComparison = Integer.compare( ( int ) thisLong1, ( int ) thatLong1 );
			  if ( tableIdComparison != 0 )
			  {
					return tableIdComparison;
			  }

			  int codeComparison = Integer.compare( ( int ) thisLong2, ( int ) thatLong2 );
			  if ( codeComparison != 0 )
			  {
					return codeComparison;
			  }

			  int derivedValueComparison = Long.compare( thisLong0, thatLong0 );
			  if ( derivedValueComparison != 0 )
			  {
					return derivedValueComparison;
			  }

			  long dimensions = Math.Min( thisLong3, thatLong3 );
			  for ( int i = 0; i < dimensions; i++ )
			  {
					// It's ok to compare the coordinate value here without deserializing them
					// because we are only defining SOME deterministic order so that we can
					// correctly separate unique points from each other, even if they collide
					// on the space filling curve.
					int coordinateComparison = Long.compare( thisLong1Array[thisCoordinatesOffset + i], thatLong1Array[thatCoordinatesOffset + i] );
					if ( coordinateComparison != 0 )
					{
						 return coordinateComparison;
					}
			  }
			  return 0;
		 }

		 internal static void PutCrs( PageCursor cursor, long long1, long long2, long long3 )
		 {
			  AssertValueWithin( long1, _maskTablePut, "tableId" );
			  AssertValueWithin( long2, _maskCode, "code" );
			  AssertValueWithin( long3, _maskDimensionsPut, "dimensions" );
			  int header = ( int )( ( long1 << _shiftTable ) | ( long3 << _shiftDimensions ) | long2 );
			  Put3BInt( cursor, header );
		 }

		 private static void AssertValueWithin( long value, int maskAllowed, string name )
		 {
			  if ( ( value & ~maskAllowed ) != 0 )
			  {
					throw new System.ArgumentException( "Expected 0 < " + name + " <= " + maskAllowed + ", but was " + value );
			  }
		 }

		 internal static void PutPoint( PageCursor cursor, long long0, long long3, long[] long1Array, int long1ArrayOffset )
		 {
			  cursor.PutLong( long0 );
			  for ( int i = 0; i < long3; i++ )
			  {
					cursor.PutLong( long1Array[long1ArrayOffset + i] );
			  }
		 }

		 /// <summary>
		 /// This check exists because of how range queries are performed, where one range gets broken down into multiple
		 /// sub-ranges following a space filling curve. These sub-ranges doesn't have exact coordinates associated with them,
		 /// only the derived 1D comparison value. The sub-range querying is only initialized into keys acting as from/to
		 /// markers for a query and so should never be used for writing into the tree or generating values from,
		 /// so practically it's not a problem, merely an inconvenience and slight inconsistency for this value type.
		 /// </summary>
		 /// <param name="state"> holds the key state. </param>
		 internal static void AssertHasCoordinates( GenericKey state )
		 {
			  if ( !HasCoordinates( state ) )
			  {
					throw new System.InvalidOperationException( "This geometry key doesn't have coordinates and can therefore neither be persisted nor generate point value." );
			  }
		 }

		 internal static bool HasCoordinates( GenericKey state )
		 {
			  return state.Long3 != 0 && state.Long1Array != null;
		 }

		 internal static GenericKey NoCoordinates
		 {
			 set
			 {
				  value.Long3 = 0;
			 }
		 }

		 private static void Put3BInt( PageCursor cursor, int value )
		 {
			  cursor.PutShort( ( short ) value );
			  cursor.PutByte( ( sbyte )( ( int )( ( uint )value >> ( sizeof( short ) * 8 ) ) ) );
		 }

		 internal static bool ReadCrs( PageCursor cursor, GenericKey into )
		 {
			  int header = Read3BInt( cursor );
			  into.Long1 = ( int )( ( uint )( header & _maskTableRead ) >> _shiftTable );
			  into.Long2 = header & _maskCode;
			  into.Long3 = ( int )( ( uint )( header & _maskDimensionsRead ) >> _shiftDimensions );
			  return true;
		 }

		 private static bool ReadPoint( PageCursor cursor, GenericKey into )
		 {
			  into.Long0 = cursor.Long;
			  // into.long3 have just been read by readCrs, before this method is called
			  int dimensions = dimensions( into );
			  into.Long1Array = EnsureBigEnough( into.Long1Array, dimensions );
			  for ( int i = 0; i < dimensions; i++ )
			  {
					into.Long1Array[i] = cursor.Long;
			  }
			  return true;
		 }

		 private static int Read3BInt( PageCursor cursor )
		 {
			  int low = cursor.Short & 0xFFFF;
			  int high = cursor.Byte & 0xFF;
			  return high << ( sizeof( short ) * 8 ) | low;
		 }

		 internal virtual void Write( GenericKey state, long derivedSpaceFillingCurveValue, double[] coordinate )
		 {
			  state.Long0 = derivedSpaceFillingCurveValue;
			  state.Long1Array = EnsureBigEnough( state.Long1Array, coordinate.Length );
			  for ( int i = 0; i < coordinate.Length; i++ )
			  {
					state.Long1Array[i] = System.BitConverter.DoubleToInt64Bits( coordinate[i] );
			  }
			  state.Long3 = coordinate.Length;
		 }

		 protected internal override void AddTypeSpecificDetails( StringJoiner joiner, GenericKey state )
		 {
			  joiner.add( "long0=" + state.Long0 );
			  joiner.add( "long1=" + state.Long1 );
			  joiner.add( "long2=" + state.Long2 );
			  joiner.add( "long3=" + state.Long3 );
			  joiner.add( "long1Array=" + Arrays.ToString( state.Long1Array ) );
		 }
	}

}