using System;

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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using Point = Neo4Net.Graphdb.spatial.Point;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using PointArray = Neo4Net.Values.Storable.PointArray;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Neo4Net.Values.Storable;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.EMPTY_LONG_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GeometryType.assertHasCoordinates;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GeometryType.dimensions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GeometryType.hasCoordinates;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GeometryType.putCrs;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GeometryType.putPoint;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GeometryType.readCrs;

	/// <summary>
	/// Handles <seealso cref="PointValue[]"/>.
	/// 
	/// Note about lazy initialization of <seealso cref="GenericKey"/> data structures: a point type is special in that it contains a <seealso cref="CoordinateReferenceSystem"/>,
	/// which dictates how much space it will occupy. When serializing a <seealso cref="PointArray"/> into <seealso cref="GenericKey"/> (via the logic in this class)
	/// the <seealso cref="CoordinateReferenceSystem"/> isn't known at initialization, where only the type and array length is known.
	/// This is why some state is initialize lazily when observing the first point in the array.
	/// </summary>
	internal class GeometryArrayType : AbstractArrayType<PointValue>
	{
		 // Affected key state:
		 // long0Array (rawValueBits)
		 // long1Array (coordinates)
		 // long1 (coordinate reference system tableId)
		 // long2 (coordinate reference system code)
		 // long3 (dimensions)

		 internal GeometryArrayType( sbyte typeId ) : base( ValueGroup.GEOMETRY_ARRAY, typeId, ( o1, o2, i ) -> GeometryType.Compare( o1.long0Array[i], o1.long1, o1.long2, o1.long3, o1.long1Array, ( int ) o1.long3 * i, o2.long0Array[i], o2.long1, o2.long2, o2.long3, o2.long1Array, ( int ) o2.long3 * i ), null, null, null, null, null )
		 {
		 }

		 internal override int ValueSize( GenericKey state )
		 {
			  return GenericKey.SIZE_GEOMETRY_HEADER + ArrayKeySize( state, GenericKey.SizeGeometry + dimensions( state ) * GenericKey.SizeGeometryCoordinate );
		 }

		 internal override void CopyValue( GenericKey to, GenericKey from, int length )
		 {
			  InitializeArray( to, length, null );
			  Array.Copy( from.Long0Array, 0, to.Long0Array, 0, length );
			  to.Long1 = from.Long1;
			  to.Long2 = from.Long2;
			  to.Long3 = from.Long3;
			  int dimensions = dimensions( from );
			  to.Long1Array = EnsureBigEnough( to.Long1Array, dimensions * length );
			  Array.Copy( from.Long1Array, 0, to.Long1Array, 0, dimensions * length );
			  to.SpaceFillingCurve = from.SpaceFillingCurve;
		 }

		 internal override void InitializeArray( GenericKey key, int length, Neo4Net.Values.Storable.ValueWriter_ArrayType arrayType )
		 {
			  key.Long0Array = EnsureBigEnough( key.Long0Array, length );

			  // Since this method is called when serializing a PointValue into the key state, the CRS and number of dimensions
			  // are unknown at this point. Read more about why lazy initialization is required in the class-level javadoc.
			  if ( length == 0 && key.Long1Array == null )
			  {
					// There's this special case where we're initializing an empty geometry array and so the long1Array
					// won't be initialized at all. Therefore we're preemptively making sure it's at least not null.
					key.Long1Array = EMPTY_LONG_ARRAY;
			  }
		 }

		 internal override Value AsValue( GenericKey state )
		 {
			  Point[] points = new Point[state.ArrayLength];
			  if ( points.Length > 0 )
			  {
					assertHasCoordinates( state );
					CoordinateReferenceSystem crs = CoordinateReferenceSystem.get( ( int ) state.Long1, ( int ) state.Long2 );
					int dimensions = dimensions( state );
					for ( int i = 0; i < points.Length; i++ )
					{
						 points[i] = GeometryType.AsValue( state, crs, dimensions * i );
					}
			  }
			  return Values.pointArray( points );
		 }

		 internal override void PutValue( PageCursor cursor, GenericKey state )
		 {
			  putCrs( cursor, state.Long1, state.Long2, state.Long3 );
			  int dimensions = dimensions( state );
			  PutArray( cursor, state, ( c, k, i ) => putPoint( c, k.long0Array[i], k.long3, k.long1Array, i * dimensions ) );
		 }

		 internal override bool ReadValue( PageCursor cursor, int size, GenericKey into )
		 {
			  readCrs( cursor, into );
			  return ReadArray( cursor, Neo4Net.Values.Storable.ValueWriter_ArrayType.Point, GeometryArrayType.readGeometryArrayItem, into );
		 }

		 internal override string ToString( GenericKey state )
		 {
			  string asValueString = hasCoordinates( state ) ? AsValue( state ).ToString() : "NO_COORDINATES";
			  return format( "GeometryArray[tableId:%d, code:%d, rawValues:%s, value:%s]", state.Long1, state.Long2, Arrays.ToString( Arrays.copyOf( state.Long0Array, state.ArrayLength ) ), asValueString );
		 }

		 private static bool ReadGeometryArrayItem( PageCursor cursor, GenericKey into )
		 {
			  into.Long0Array[into.CurrentArrayOffset] = cursor.Long;
			  int dimensions = dimensions( into );
			  if ( into.CurrentArrayOffset == 0 )
			  {
					// Read more about why lazy initialization is required in the class-level javadoc.
					into.Long1Array = EnsureBigEnough( into.Long1Array, dimensions * into.ArrayLength );
			  }
			  for ( int i = 0, offset = into.CurrentArrayOffset * dimensions; i < dimensions; i++ )
			  {
					into.Long1Array[offset + i] = cursor.Long;
			  }
			  into.CurrentArrayOffset++;
			  return true;
		 }

		 internal virtual void Write( GenericKey state, int offset, long derivedSpaceFillingCurveValue, double[] coordinates )
		 {
			  state.Long0Array[offset] = derivedSpaceFillingCurveValue;
			  if ( offset == 0 )
			  {
					// Read more about why lazy initialization is required in the class-level javadoc.
					int dimensions = coordinates.Length;
					state.Long1Array = EnsureBigEnough( state.Long1Array, dimensions * state.ArrayLength );
					state.Long3 = dimensions;
			  }
			  for ( int i = 0, @base = dimensions( state ) * offset; i < coordinates.Length; i++ )
			  {
					state.Long1Array[@base + i] = System.BitConverter.DoubleToInt64Bits( coordinates[i] );
			  }
		 }

		 protected internal override void AddTypeSpecificDetails( StringJoiner joiner, GenericKey state )
		 {
			  joiner.add( "long1=" + state.Long1 );
			  joiner.add( "long2=" + state.Long2 );
			  joiner.add( "long3=" + state.Long3 );
			  joiner.add( "long0Array=" + Arrays.ToString( state.Long0Array ) );
			  joiner.add( "long1Array=" + Arrays.ToString( state.Long1Array ) );
			  base.AddTypeSpecificDetails( joiner, state );
		 }
	}

}