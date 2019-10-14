/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.core.consensus.log
{

	public class DelegatingRaftLog : RaftLog
	{
		 private readonly RaftLog _inner;

		 public DelegatingRaftLog( RaftLog inner )
		 {
			  this._inner = inner;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long append(RaftLogEntry... entry) throws java.io.IOException
		 public override long Append( params RaftLogEntry[] entry )
		 {
			  return _inner.append( entry );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void truncate(long fromIndex) throws java.io.IOException
		 public override void Truncate( long fromIndex )
		 {
			  _inner.truncate( fromIndex );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long prune(long safeIndex) throws java.io.IOException
		 public override long Prune( long safeIndex )
		 {
			  return _inner.prune( safeIndex );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long skip(long index, long term) throws java.io.IOException
		 public override long Skip( long index, long term )
		 {
			  return _inner.skip( index, term );
		 }

		 public override long AppendIndex()
		 {
			  return _inner.appendIndex();
		 }

		 public override long PrevIndex()
		 {
			  return _inner.prevIndex();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long readEntryTerm(long logIndex) throws java.io.IOException
		 public override long ReadEntryTerm( long logIndex )
		 {
			  return _inner.readEntryTerm( logIndex );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public RaftLogCursor getEntryCursor(long fromIndex) throws java.io.IOException
		 public override RaftLogCursor GetEntryCursor( long fromIndex )
		 {
			  return _inner.getEntryCursor( fromIndex );
		 }
	}

}