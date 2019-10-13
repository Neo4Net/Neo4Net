﻿/*
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
namespace Neo4Net.causalclustering.core.consensus.explorer
{

	using InMemoryRaftLog = Neo4Net.causalclustering.core.consensus.log.InMemoryRaftLog;
	using RaftLogCursor = Neo4Net.causalclustering.core.consensus.log.RaftLogCursor;
	using ReadableRaftLog = Neo4Net.causalclustering.core.consensus.log.ReadableRaftLog;

	public class ComparableRaftLog : InMemoryRaftLog
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ComparableRaftLog(org.neo4j.causalclustering.core.consensus.log.ReadableRaftLog raftLog) throws java.io.IOException
		 public ComparableRaftLog( ReadableRaftLog raftLog )
		 {
			  using ( RaftLogCursor cursor = raftLog.GetEntryCursor( 0 ) )
			  {
					while ( cursor.Next() )
					{
						 Append( cursor.get() );
					}
			  }
		 }
	}

}