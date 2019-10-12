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

	public class AppendLogEntry : RaftLogCommand
	{
		 public readonly long Index;
		 public readonly RaftLogEntry Entry;

		 public AppendLogEntry( long index, RaftLogEntry entry )
		 {
			  this.Index = index;
			  this.Entry = entry;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void applyTo(org.neo4j.causalclustering.core.consensus.log.RaftLog raftLog, org.neo4j.logging.Log log) throws java.io.IOException
		 public override void ApplyTo( RaftLog raftLog, Log log )
		 {
			  if ( Index <= raftLog.AppendIndex() )
			  {
					throw new System.InvalidOperationException( "Attempted to append over an existing entry at index " + Index );
			  }
			  raftLog.Append( Entry );
		 }

		 public override void ApplyTo( InFlightCache inFlightCache, Log log )
		 {
			  inFlightCache.Put( Index, Entry );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void dispatch(RaftLogCommand_Handler handler) throws java.io.IOException
		 public override void Dispatch( RaftLogCommand_Handler handler )
		 {
			  handler.Append( Index, Entry );
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
			  AppendLogEntry that = ( AppendLogEntry ) o;
			  return Index == that.Index && Objects.Equals( Entry, that.Entry );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( Index, Entry );
		 }

		 public override string ToString()
		 {
			  return "AppendLogEntry{" +
						 "index=" + Index +
						 ", entry=" + Entry +
						 '}';
		 }
	}

}