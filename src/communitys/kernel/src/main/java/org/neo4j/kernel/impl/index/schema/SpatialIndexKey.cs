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

	using SpaceFillingCurve = Neo4Net.Gis.Spatial.Index.curves.SpaceFillingCurve;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.NO_VALUE;

	/// <summary>
	/// Includes value and entity id (to be able to handle non-unique values).
	/// A value can be any <seealso cref="PointValue"/> and is represented as a {@code long} to store the 1D mapped version
	/// </summary>
	internal class SpatialIndexKey : NativeIndexSingleValueKey<SpatialIndexKey>
	{
		 internal static readonly int Size = Long.BYTES + ENTITY_ID_SIZE; // entityId

		 internal long RawValueBits;
		 internal CoordinateReferenceSystem Crs;
		 internal SpaceFillingCurve Curve;

		 internal SpatialIndexKey( CoordinateReferenceSystem crs, SpaceFillingCurve curve )
		 {
			  this.Crs = crs;
			  this.Curve = curve;
		 }

		 public override Value AsValue()
		 {
			  return NO_VALUE;
		 }

		 internal override void InitValueAsLowest( ValueGroup valueGroups )
		 {
			  double[] limit = new double[Crs.Dimension];
			  Arrays.fill( limit, double.NegativeInfinity );
			  WritePoint( Crs, limit );
		 }

		 internal override void InitValueAsHighest( ValueGroup valueGroups )
		 {
			  // These coordinates will generate the largest value on the spacial curve
			  double[] limit = new double[Crs.Dimension];
			  Arrays.fill( limit, double.NegativeInfinity );
			  limit[0] = double.PositiveInfinity;
			  WritePoint( Crs, limit );
		 }

		 internal virtual void FromDerivedValue( long entityId, long derivedValue )
		 {
			  RawValueBits = derivedValue;
			  initialize( entityId );
		 }

		 /// <summary>
		 /// This method will compare along the curve, which is not a spatial comparison, but is correct
		 /// for comparison within the space filling index as long as the original spatial range has already
		 /// been decomposed into a collection of 1D curve ranges before calling down into the GPTree.
		 /// </summary>
		 internal override int CompareValueTo( SpatialIndexKey other )
		 {
			  return Long.compare( RawValueBits, other.RawValueBits );
		 }

		 protected internal override Value AssertCorrectType( Value value )
		 {
			  if ( !Values.isGeometryValue( value ) )
			  {
					throw new System.ArgumentException( "Key layout does only support geometries, tried to create key from " + value );
			  }
			  return value;
		 }

		 /// <summary>
		 /// Extracts raw bits from a <seealso cref="PointValue"/> and store as state of this <seealso cref="SpatialIndexKey"/> instance.
		 /// </summary>
		 public override void WritePoint( CoordinateReferenceSystem crs, double[] coordinate )
		 {
			  RawValueBits = Curve.derivedValueFor( coordinate ).Value;
		 }

		 public override string ToString()
		 {
			  return format( "rawValue=%d,value=%s,entityId=%d", RawValueBits, "unknown", EntityId );
		 }
	}

}