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
namespace Neo4Net.Kernel.Impl.Newapi
{

	using IndexQuery = Neo4Net.Internal.Kernel.Api.IndexQuery;
	using IndexReference = Neo4Net.Internal.Kernel.Api.IndexReference;
	using NodeValueIndexCursor = Neo4Net.Internal.Kernel.Api.NodeValueIndexCursor;
	using IndexNotApplicableKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotApplicableKernelException;
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.locking.ResourceTypes.INDEX_ENTRY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.locking.ResourceTypes.indexEntryResourceId;

	public class LockingNodeUniqueIndexSeek
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <CURSOR extends org.neo4j.internal.kernel.api.NodeValueIndexCursor> long apply(org.neo4j.kernel.impl.locking.Locks_Client locks, org.neo4j.storageengine.api.lock.LockTracer lockTracer, System.Func<CURSOR> cursors, UniqueNodeIndexSeeker<CURSOR> nodeIndexSeeker, Read read, org.neo4j.internal.kernel.api.IndexReference index, org.neo4j.internal.kernel.api.IndexQuery.ExactPredicate... predicates) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException, org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public static long Apply<CURSOR>( Neo4Net.Kernel.impl.locking.Locks_Client locks, LockTracer lockTracer, System.Func<CURSOR> cursors, UniqueNodeIndexSeeker<CURSOR> nodeIndexSeeker, Read read, IndexReference index, params IndexQuery.ExactPredicate[] predicates ) where CURSOR : Neo4Net.Internal.Kernel.Api.NodeValueIndexCursor
		 {
			  int[] entityTokenIds = index.Schema().EntityTokenIds;
			  if ( entityTokenIds.Length != 1 )
			  {
					throw new IndexNotApplicableKernelException( "Multi-token index " + index + " does not support uniqueness." );
			  }
			  long indexEntryId = indexEntryResourceId( entityTokenIds[0], predicates );

			  //First try to find node under a shared lock
			  //if not found upgrade to exclusive and try again
			  locks.AcquireShared( lockTracer, INDEX_ENTRY, indexEntryId );
			  using ( CURSOR cursor = cursors(), IndexReaders readers = new IndexReaders(index, read) )
			  {
					nodeIndexSeeker.NodeIndexSeekWithFreshIndexReader( cursor, readers.CreateReader(), predicates );
					if ( !cursor.next() )
					{
						 locks.ReleaseShared( INDEX_ENTRY, indexEntryId );
						 locks.AcquireExclusive( lockTracer, INDEX_ENTRY, indexEntryId );
						 nodeIndexSeeker.NodeIndexSeekWithFreshIndexReader( cursor, readers.CreateReader(), predicates );
						 if ( cursor.next() ) // we found it under the exclusive lock
						 {
							  // downgrade to a shared lock
							  locks.AcquireShared( lockTracer, INDEX_ENTRY, indexEntryId );
							  locks.ReleaseExclusive( INDEX_ENTRY, indexEntryId );
						 }
					}

					return cursor.nodeReference();
			  }
		 }

		 internal interface UniqueNodeIndexSeeker<CURSOR> where CURSOR : Neo4Net.Internal.Kernel.Api.NodeValueIndexCursor
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void nodeIndexSeekWithFreshIndexReader(CURSOR cursor, org.neo4j.storageengine.api.schema.IndexReader indexReader, org.neo4j.internal.kernel.api.IndexQuery.ExactPredicate... predicates) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException;
			  void NodeIndexSeekWithFreshIndexReader( CURSOR cursor, IndexReader indexReader, params IndexQuery.ExactPredicate[] predicates );
		 }
	}

}