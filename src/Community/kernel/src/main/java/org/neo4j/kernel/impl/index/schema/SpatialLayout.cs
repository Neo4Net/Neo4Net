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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using SpaceFillingCurve = Neo4Net.Gis.Spatial.Index.curves.SpaceFillingCurve;
	using Neo4Net.Index.@internal.gbptree;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;

	/// <summary>
	/// <seealso cref="Layout"/> for PointValues.
	/// </summary>
	internal class SpatialLayout : IndexLayout<SpatialIndexKey, NativeIndexValue>
	{
		 private SpaceFillingCurve _curve;
		 internal CoordinateReferenceSystem Crs;

		 internal SpatialLayout( CoordinateReferenceSystem crs, SpaceFillingCurve curve ) : base( "UPI", 0, 1 )
		 {
			  this.Crs = crs;
			  this._curve = curve;
		 }

		 internal virtual SpaceFillingCurve SpaceFillingCurve
		 {
			 get
			 {
				  return _curve;
			 }
		 }

		 public override SpatialIndexKey NewKey()
		 {
			  return new SpatialIndexKey( Crs, _curve );
		 }

		 public override SpatialIndexKey CopyKey( SpatialIndexKey key, SpatialIndexKey into )
		 {
			  into.RawValueBits = key.RawValueBits;
			  into.EntityId = key.EntityId;
			  into.CompareId = key.CompareId;
			  into.Crs = key.Crs;
			  into.Curve = key.Curve;
			  return into;
		 }

		 public override int KeySize( SpatialIndexKey key )
		 {
			  return SpatialIndexKey.Size;
		 }

		 public override void WriteKey( PageCursor cursor, SpatialIndexKey key )
		 {
			  cursor.PutLong( key.RawValueBits );
			  cursor.PutLong( key.EntityId );
		 }

		 public override void ReadKey( PageCursor cursor, SpatialIndexKey into, int keySize )
		 {
			  into.RawValueBits = cursor.Long;
			  into.EntityId = cursor.Long;
		 }
	}

}