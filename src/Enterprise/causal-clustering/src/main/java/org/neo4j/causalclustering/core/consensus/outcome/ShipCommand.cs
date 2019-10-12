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

	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using RaftLogShipper = Neo4Net.causalclustering.core.consensus.shipping.RaftLogShipper;

	public abstract class ShipCommand
	{
		 public abstract void ApplyTo( RaftLogShipper raftLogShipper, LeaderContext leaderContext );

		 public class Mismatch : ShipCommand
		 {
			  internal readonly long LastRemoteAppendIndex;
			  internal readonly object Target;

			  public Mismatch( long lastRemoteAppendIndex, object target )
			  {
					this.LastRemoteAppendIndex = lastRemoteAppendIndex;
					this.Target = target;
			  }

			  public override void ApplyTo( RaftLogShipper raftLogShipper, LeaderContext leaderContext )
			  {
					if ( raftLogShipper.Identity().Equals(Target) )
					{
						 raftLogShipper.OnMismatch( LastRemoteAppendIndex, leaderContext );
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

					Mismatch mismatch = ( Mismatch ) o;

					if ( LastRemoteAppendIndex != mismatch.LastRemoteAppendIndex )
					{
						 return false;
					}
					return Target.Equals( mismatch.Target );

			  }

			  public override int GetHashCode()
			  {
					int result = ( int )( LastRemoteAppendIndex ^ ( ( long )( ( ulong )LastRemoteAppendIndex >> 32 ) ) );
					result = 31 * result + Target.GetHashCode();
					return result;
			  }

			  public override string ToString()
			  {
					return format( "Mismatch{lastRemoteAppendIndex=%d, target=%s}", LastRemoteAppendIndex, Target );
			  }
		 }

		 public class Match : ShipCommand
		 {
			  internal readonly long NewMatchIndex;
			  internal readonly object Target;

			  public Match( long newMatchIndex, object target )
			  {
					this.NewMatchIndex = newMatchIndex;
					this.Target = target;
			  }

			  public override void ApplyTo( RaftLogShipper raftLogShipper, LeaderContext leaderContext )
			  {
					if ( raftLogShipper.Identity().Equals(Target) )
					{
						 raftLogShipper.OnMatch( NewMatchIndex, leaderContext );
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

					Match match = ( Match ) o;

					if ( NewMatchIndex != match.NewMatchIndex )
					{
						 return false;
					}
					return Target.Equals( match.Target );

			  }

			  public override int GetHashCode()
			  {
					int result = ( int )( NewMatchIndex ^ ( ( long )( ( ulong )NewMatchIndex >> 32 ) ) );
					result = 31 * result + Target.GetHashCode();
					return result;
			  }

			  public override string ToString()
			  {
					return format( "Match{newMatchIndex=%d, target=%s}", NewMatchIndex, Target );
			  }
		 }

		 public class NewEntries : ShipCommand
		 {
			  internal readonly long PrevLogIndex;
			  internal readonly long PrevLogTerm;
			  internal readonly RaftLogEntry[] NewLogEntries;

			  public NewEntries( long prevLogIndex, long prevLogTerm, RaftLogEntry[] newLogEntries )
			  {
					this.PrevLogIndex = prevLogIndex;
					this.PrevLogTerm = prevLogTerm;
					this.NewLogEntries = newLogEntries;
			  }

			  public override void ApplyTo( RaftLogShipper raftLogShipper, LeaderContext leaderContext )
			  {
					raftLogShipper.OnNewEntries( PrevLogIndex, PrevLogTerm, NewLogEntries, leaderContext );
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

					NewEntries newEntries = ( NewEntries ) o;

					if ( PrevLogIndex != newEntries.PrevLogIndex )
					{
						 return false;
					}
					if ( PrevLogTerm != newEntries.PrevLogTerm )
					{
						 return false;
					}
					return Arrays.Equals( NewLogEntries, newEntries.NewLogEntries );

			  }

			  public override int GetHashCode()
			  {
					int result = ( int )( PrevLogIndex ^ ( ( long )( ( ulong )PrevLogIndex >> 32 ) ) );
					result = 31 * result + ( int )( PrevLogTerm ^ ( ( long )( ( ulong )PrevLogTerm >> 32 ) ) );
					result = 31 * result + Arrays.GetHashCode( NewLogEntries );
					return result;
			  }

			  public override string ToString()
			  {
					return format( "NewEntry{prevLogIndex=%d, prevLogTerm=%d, newLogEntry=%s}", PrevLogIndex, PrevLogTerm, Arrays.ToString( NewLogEntries ) );
			  }
		 }

		 public class CommitUpdate : ShipCommand
		 {
			  public override void ApplyTo( RaftLogShipper raftLogShipper, LeaderContext leaderContext )
			  {
					raftLogShipper.OnCommitUpdate( leaderContext );
			  }

			  public override string ToString()
			  {
					return "CommitUpdate{}";
			  }
		 }
	}

}