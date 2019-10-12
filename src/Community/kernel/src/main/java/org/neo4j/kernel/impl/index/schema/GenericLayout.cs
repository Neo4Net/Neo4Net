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
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using IndexSpecificSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;

	internal class GenericLayout : IndexLayout<GenericKey, NativeIndexValue>
	{
		 private readonly int _numberOfSlots;
		 private readonly IndexSpecificSpaceFillingCurveSettingsCache _spatialSettings;

		 internal GenericLayout( int numberOfSlots, IndexSpecificSpaceFillingCurveSettingsCache spatialSettings ) : base( "NSIL", 0, 5 )
		 {
			  this._numberOfSlots = numberOfSlots;
			  this._spatialSettings = spatialSettings;
		 }

		 public override GenericKey NewKey()
		 {
			  return _numberOfSlots == 1 ? new GenericKey( _spatialSettings ) : new CompositeGenericKey( _numberOfSlots, _spatialSettings );
		 }

		 public override GenericKey CopyKey( GenericKey key, GenericKey into )
		 {
			  into.CopyFrom( key );
			  return into;
		 }

		 public override int KeySize( GenericKey key )
		 {
			  return key.Size();
		 }

		 public override void WriteKey( PageCursor cursor, GenericKey key )
		 {
			  key.Put( cursor );
		 }

		 public override void ReadKey( PageCursor cursor, GenericKey into, int keySize )
		 {
			  into.Get( cursor, keySize );
		 }

		 public override bool FixedSize()
		 {
			  return false;
		 }

		 public override void MinimalSplitter( GenericKey left, GenericKey right, GenericKey into )
		 {
			  right.MinimalSplitter( left, right, into );
		 }

		 internal virtual IndexSpecificSpaceFillingCurveSettingsCache SpaceFillingCurveSettings
		 {
			 get
			 {
				  return _spatialSettings;
			 }
		 }
	}

}