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

	using IndexQuery = Neo4Net.Kernel.Api.Internal.IndexQuery;
	using IndexReference = Neo4Net.Kernel.Api.Internal.IndexReference;
	using NodeValueIndexCursor = Neo4Net.Kernel.Api.Internal.NodeValueIndexCursor;
	using IndexNotApplicableKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotApplicableKernelException;
	using IndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using LockTracer = Neo4Net.Kernel.Api.StorageEngine.@lock.LockTracer;
	using IndexReader = Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.locking.ResourceTypes.INDEX_ENTRY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.locking.ResourceTypes.indexEntryResourceId;

	public class LockingNodeUniqueIndexSeek
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <CURSOR extends Neo4Net.Kernel.Api.Internal.NodeValueIndexCursor> long apply(Neo4Net.kernel.impl.locking.Locks_Client locks, Neo4Net.Kernel.Api.StorageEngine.lock.LockTracer lockTracer, System.Func<CURSOR> cursors, UniqueNodeIndexSeeker<CURSOR> nodeIndexSeeker, Read read, Neo4Net.Kernel.Api.Internal.IndexReference index, Neo4Net.Kernel.Api.Internal.IndexQuery.ExactPredicate... predicates) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotApplicableKernelException, Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException
		 public static long Apply<CURSOR>( Neo4Net.Kernel.impl.locking.Locks_Client locks, LockTracer lockTracer, System.Func<CURSOR> cursors, UniqueNodeIndexSeeker<CURSOR> nodeIndexSeeker, Read read, IndexReference index, params IndexQuery.ExactPredicate[] predicates ) where CURSOR : Neo4Net.Kernel.Api.Internal.NodeValueIndexCursor
		 {
			  int[] IEntityTokenIds = index.Schema().EntityTokenIds;
			  if ( IEntityTokenIds.Length != 1 )
			  {
					throw new IndexNotApplicableKernelException( "Multi-token index " + index + " does not support uniqueness." );
			  }
			  long indexEntryId = indexEntryResourceId( IEntityTokenIds[0], predicates );

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

		 internal interface UniqueNodeIndexSeeker<CURSOR> where CURSOR : Neo4Net.Kernel.Api.Internal.NodeValueIndexCursor
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void nodeIndexSeekWithFreshIndexReader(CURSOR cursor, Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader indexReader, Neo4Net.Kernel.Api.Internal.IndexQuery.ExactPredicate... predicates) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotApplicableKernelException;
			  void NodeIndexSeekWithFreshIndexReader( CURSOR cursor, IndexReader indexReader, params IndexQuery.ExactPredicate[] predicates );
		 }
	}

}