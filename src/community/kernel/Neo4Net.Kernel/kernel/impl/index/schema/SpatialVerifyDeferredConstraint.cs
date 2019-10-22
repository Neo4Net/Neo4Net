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
	using MutableLongIterator = org.eclipse.collections.api.iterator.MutableLongIterator;
	using MutableMap = org.eclipse.collections.api.map.MutableMap;
	using Maps = org.eclipse.collections.impl.factory.Maps;
	using LongArrayList = org.eclipse.collections.impl.list.mutable.primitive.LongArrayList;


	using Neo4Net.Cursors;
	using Neo4Net.Index.Internal.gbptree;
	using Neo4Net.Index.Internal.gbptree;
	using IEntityNotFoundException = Neo4Net.Internal.Kernel.Api.exceptions.EntityNotFoundException;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;

	internal class SpatialVerifyDeferredConstraint
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void verify(org.Neo4Net.storageengine.api.NodePropertyAccessor nodePropertyAccessor, IndexLayout<SpatialIndexKey,NativeIndexValue> layout, org.Neo4Net.index.internal.gbptree.GBPTree<SpatialIndexKey,NativeIndexValue> tree, org.Neo4Net.storageengine.api.schema.StoreIndexDescriptor descriptor) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 internal static void Verify( NodePropertyAccessor nodePropertyAccessor, IndexLayout<SpatialIndexKey, NativeIndexValue> layout, GBPTree<SpatialIndexKey, NativeIndexValue> tree, StoreIndexDescriptor descriptor )
		 {
			  SpatialIndexKey from = layout.newKey();
			  SpatialIndexKey to = layout.newKey();
			  InitializeKeys( from, to );
			  try
			  {
					  using ( IRawCursor<Hit<SpatialIndexKey, NativeIndexValue>, IOException> seek = tree.Seek( from, to ) )
					  {
						ScanAndVerifyDuplicates( nodePropertyAccessor, descriptor, seek );
					  }
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void scanAndVerifyDuplicates(org.Neo4Net.storageengine.api.NodePropertyAccessor nodePropertyAccessor, org.Neo4Net.storageengine.api.schema.StoreIndexDescriptor descriptor, org.Neo4Net.cursor.RawCursor<org.Neo4Net.index.internal.gbptree.Hit<SpatialIndexKey,NativeIndexValue>,java.io.IOException> seek) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 private static void ScanAndVerifyDuplicates( NodePropertyAccessor nodePropertyAccessor, StoreIndexDescriptor descriptor, IRawCursor<Hit<SpatialIndexKey, NativeIndexValue>, IOException> seek )
		 {
			  LongArrayList nodesWithCollidingPoints = new LongArrayList();
			  long prevRawBits = long.MinValue;

			  // Bootstrap starting state
			  if ( seek.Next() )
			  {
					Hit<SpatialIndexKey, NativeIndexValue> hit = seek.get();
					prevRawBits = hit.Key().RawValueBits;
					nodesWithCollidingPoints.add( hit.Key().EntityId );
			  }

			  while ( seek.Next() )
			  {
					Hit<SpatialIndexKey, NativeIndexValue> hit = seek.get();
					SpatialIndexKey key = hit.Key();
					long currentRawBits = key.RawValueBits;
					long currentNodeId = key.EntityId;
					if ( prevRawBits != currentRawBits )
					{
						 if ( nodesWithCollidingPoints.size() > 1 )
						 {
							  VerifyConstraintOn( nodesWithCollidingPoints, nodePropertyAccessor, descriptor );
						 }
						 nodesWithCollidingPoints.clear();
					}
					nodesWithCollidingPoints.add( currentNodeId );
					prevRawBits = currentRawBits;
			  }

			  // Verify the last batch if needed
			  if ( nodesWithCollidingPoints.size() > 1 )
			  {
					VerifyConstraintOn( nodesWithCollidingPoints, nodePropertyAccessor, descriptor );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void verifyConstraintOn(org.eclipse.collections.impl.list.mutable.primitive.LongArrayList nodeIds, org.Neo4Net.storageengine.api.NodePropertyAccessor nodePropertyAccessor, org.Neo4Net.storageengine.api.schema.StoreIndexDescriptor descriptor) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 private static void VerifyConstraintOn( LongArrayList nodeIds, NodePropertyAccessor nodePropertyAccessor, StoreIndexDescriptor descriptor )
		 {
			  MutableMap<Value, long> points = Maps.mutable.empty();
			  MutableLongIterator iter = nodeIds.longIterator();
			  try
			  {
					while ( iter.hasNext() )
					{
						 long id = iter.next();
						 Value value = nodePropertyAccessor.GetNodePropertyValue( id, descriptor.Schema().PropertyId );
						 long? other = points.getIfAbsentPut( value, id );
						 if ( other.Value != id )
						 {
							  throw new IndexEntryConflictException( other.Value, id, value );
						 }
					}
			  }
			  catch ( IEntityNotFoundException e )
			  {
					throw new Exception( "Failed to validate uniqueness constraint", e );
			  }
		 }

		 private static void InitializeKeys( SpatialIndexKey from, SpatialIndexKey to )
		 {
			  from.initialize( long.MinValue );
			  to.initialize( long.MaxValue );
			  from.InitValueAsLowest( ValueGroup.GEOMETRY );
			  to.InitValueAsHighest( ValueGroup.GEOMETRY );
		 }
	}

}