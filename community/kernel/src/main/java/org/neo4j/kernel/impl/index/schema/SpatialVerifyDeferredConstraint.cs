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
	using MutableLongIterator = org.eclipse.collections.api.iterator.MutableLongIterator;
	using MutableMap = org.eclipse.collections.api.map.MutableMap;
	using Maps = org.eclipse.collections.impl.factory.Maps;
	using LongArrayList = org.eclipse.collections.impl.list.mutable.primitive.LongArrayList;


	using Org.Neo4j.Cursor;
	using Org.Neo4j.Index.@internal.gbptree;
	using Org.Neo4j.Index.@internal.gbptree;
	using EntityNotFoundException = Org.Neo4j.@internal.Kernel.Api.exceptions.EntityNotFoundException;
	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using Value = Org.Neo4j.Values.Storable.Value;
	using ValueGroup = Org.Neo4j.Values.Storable.ValueGroup;

	internal class SpatialVerifyDeferredConstraint
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void verify(org.neo4j.storageengine.api.NodePropertyAccessor nodePropertyAccessor, IndexLayout<SpatialIndexKey,NativeIndexValue> layout, org.neo4j.index.internal.gbptree.GBPTree<SpatialIndexKey,NativeIndexValue> tree, org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 internal static void Verify( NodePropertyAccessor nodePropertyAccessor, IndexLayout<SpatialIndexKey, NativeIndexValue> layout, GBPTree<SpatialIndexKey, NativeIndexValue> tree, StoreIndexDescriptor descriptor )
		 {
			  SpatialIndexKey from = layout.newKey();
			  SpatialIndexKey to = layout.newKey();
			  InitializeKeys( from, to );
			  try
			  {
					  using ( RawCursor<Hit<SpatialIndexKey, NativeIndexValue>, IOException> seek = tree.Seek( from, to ) )
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
//ORIGINAL LINE: private static void scanAndVerifyDuplicates(org.neo4j.storageengine.api.NodePropertyAccessor nodePropertyAccessor, org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor, org.neo4j.cursor.RawCursor<org.neo4j.index.internal.gbptree.Hit<SpatialIndexKey,NativeIndexValue>,java.io.IOException> seek) throws java.io.IOException, org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 private static void ScanAndVerifyDuplicates( NodePropertyAccessor nodePropertyAccessor, StoreIndexDescriptor descriptor, RawCursor<Hit<SpatialIndexKey, NativeIndexValue>, IOException> seek )
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
//ORIGINAL LINE: private static void verifyConstraintOn(org.eclipse.collections.impl.list.mutable.primitive.LongArrayList nodeIds, org.neo4j.storageengine.api.NodePropertyAccessor nodePropertyAccessor, org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
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
			  catch ( EntityNotFoundException e )
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