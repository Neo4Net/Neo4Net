/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.causalclustering.core.consensus.outcome
{

	using InFlightCache = Org.Neo4j.causalclustering.core.consensus.log.cache.InFlightCache;
	using RaftLog = Org.Neo4j.causalclustering.core.consensus.log.RaftLog;
	using RaftLogEntry = Org.Neo4j.causalclustering.core.consensus.log.RaftLogEntry;
	using Log = Org.Neo4j.Logging.Log;

	public interface RaftLogCommand
	{

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void dispatch(RaftLogCommand_Handler handler) throws java.io.IOException;
		 void Dispatch( RaftLogCommand_Handler handler );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void applyTo(org.neo4j.causalclustering.core.consensus.log.RaftLog raftLog, org.neo4j.logging.Log log) throws java.io.IOException;
		 void ApplyTo( RaftLog raftLog, Log log );

		 void ApplyTo( InFlightCache inFlightCache, Log log );
	}

	 public interface RaftLogCommand_Handler
	 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void append(long baseIndex, org.neo4j.causalclustering.core.consensus.log.RaftLogEntry... entries) throws java.io.IOException;
		  void Append( long baseIndex, params RaftLogEntry[] entries );
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void truncate(long fromIndex) throws java.io.IOException;
		  void Truncate( long fromIndex );
		  void Prune( long pruneIndex );
	 }

}