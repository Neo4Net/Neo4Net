/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.core.consensus.outcome
{

	using RaftLog = Neo4Net.causalclustering.core.consensus.log.RaftLog;
	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using InFlightCache = Neo4Net.causalclustering.core.consensus.log.cache.InFlightCache;
	using Log = Neo4Net.Logging.Log;

	public class BatchAppendLogEntries : RaftLogCommand
	{
		 public readonly long BaseIndex;
		 public readonly int Offset;
		 public readonly RaftLogEntry[] Entries;

		 public BatchAppendLogEntries( long baseIndex, int offset, RaftLogEntry[] entries )
		 {
			  this.BaseIndex = baseIndex;
			  this.Offset = offset;
			  this.Entries = entries;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void dispatch(RaftLogCommand_Handler handler) throws java.io.IOException
		 public override void Dispatch( RaftLogCommand_Handler handler )
		 {
			  handler.Append( BaseIndex + Offset, Arrays.copyOfRange( Entries, Offset, Entries.Length ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void applyTo(org.Neo4Net.causalclustering.core.consensus.log.RaftLog raftLog, org.Neo4Net.logging.Log log) throws java.io.IOException
		 public override void ApplyTo( RaftLog raftLog, Log log )
		 {
			  long lastIndex = BaseIndex + Offset;
			  if ( lastIndex <= raftLog.AppendIndex() )
			  {
					throw new System.InvalidOperationException( "Attempted to append over an existing entry starting at index " + lastIndex );
			  }

			  raftLog.Append( Arrays.copyOfRange( Entries, Offset, Entries.Length ) );
		 }

		 public override void ApplyTo( InFlightCache inFlightCache, Log log )
		 {
			  for ( int i = Offset; i < Entries.Length; i++ )
			  {
					inFlightCache.Put( BaseIndex + i, Entries[i] );
			  }
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
			  BatchAppendLogEntries that = ( BatchAppendLogEntries ) o;
			  return BaseIndex == that.BaseIndex && Offset == that.Offset && Arrays.Equals( Entries, that.Entries );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( BaseIndex, Offset, Arrays.GetHashCode( Entries ) );
		 }

		 public override string ToString()
		 {
			  return format( "BatchAppendLogEntries{baseIndex=%d, offset=%d, entries=%s}", BaseIndex, Offset, Arrays.ToString( Entries ) );
		 }
	}

}