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
namespace Neo4Net.causalclustering.core.consensus.outcome
{

	using InFlightCache = Neo4Net.causalclustering.core.consensus.log.cache.InFlightCache;
	using RaftLog = Neo4Net.causalclustering.core.consensus.log.RaftLog;
	using Log = Neo4Net.Logging.Log;

	public class PruneLogCommand : RaftLogCommand
	{
		 private readonly long _pruneIndex;

		 public PruneLogCommand( long pruneIndex )
		 {
			  this._pruneIndex = pruneIndex;
		 }

		 public override void Dispatch( RaftLogCommand_Handler handler )
		 {
			  handler.Prune( _pruneIndex );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void applyTo(org.neo4j.causalclustering.core.consensus.log.RaftLog raftLog, org.neo4j.logging.Log log) throws java.io.IOException
		 public override void ApplyTo( RaftLog raftLog, Log log )
		 {
			  raftLog.Prune( _pruneIndex );
		 }

		 public override void ApplyTo( InFlightCache inFlightCache, Log log )
		 {
			  // only the actual log prunes
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
			  PruneLogCommand that = ( PruneLogCommand ) o;
			  return _pruneIndex == that._pruneIndex;
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _pruneIndex );
		 }

		 public override string ToString()
		 {
			  return "PruneLogCommand{" +
						"pruneIndex=" + _pruneIndex +
						'}';
		 }
	}

}