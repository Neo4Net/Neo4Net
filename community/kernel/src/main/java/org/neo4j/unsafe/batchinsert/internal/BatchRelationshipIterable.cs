using System.Collections.Generic;

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
namespace Org.Neo4j.@unsafe.Batchinsert.@internal
{

	using NotFoundException = Org.Neo4j.Graphdb.NotFoundException;
	using Org.Neo4j.Helpers.Collection;
	using RecordNodeCursor = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RecordNodeCursor;
	using RecordStorageReader = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RecordStorageReader;
	using StorageRelationshipTraversalCursor = Org.Neo4j.Storageengine.Api.StorageRelationshipTraversalCursor;

	internal abstract class BatchRelationshipIterable<T> : IEnumerable<T>
	{
		 private readonly StorageRelationshipTraversalCursor _relationshipCursor;

		 internal BatchRelationshipIterable( RecordStorageReader storageReader, long nodeId )
		 {
			  _relationshipCursor = storageReader.AllocateRelationshipTraversalCursor();
			  RecordNodeCursor nodeCursor = storageReader.AllocateNodeCursor();
			  nodeCursor.Single( nodeId );
			  if ( !nodeCursor.Next() )
			  {
					throw new NotFoundException( "Node " + nodeId + " not found" );
			  }
			  _relationshipCursor.init( nodeId, nodeCursor.AllRelationshipsReference() );
		 }

		 public override IEnumerator<T> Iterator()
		 {
			  return new PrefetchingIteratorAnonymousInnerClass( this );
		 }

		 private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<T>
		 {
			 private readonly BatchRelationshipIterable<T> _outerInstance;

			 public PrefetchingIteratorAnonymousInnerClass( BatchRelationshipIterable<T> outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override T fetchNextOrNull()
			 {
				  if ( !_outerInstance.relationshipCursor.next() )
				  {
						return default( T );
				  }

				  return _outerInstance.nextFrom( _outerInstance.relationshipCursor.entityReference(), _outerInstance.relationshipCursor.type(), _outerInstance.relationshipCursor.sourceNodeReference(), _outerInstance.relationshipCursor.targetNodeReference() );
			 }
		 }

		 protected internal abstract T NextFrom( long relId, int type, long startNode, long endNode );
	}

}