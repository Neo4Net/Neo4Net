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
namespace Neo4Net.causalclustering.core.state.snapshot
{

	using MembershipEntry = Neo4Net.causalclustering.core.consensus.membership.MembershipEntry;
	using Neo4Net.causalclustering.core.state.storage;
	using EndOfStreamException = Neo4Net.causalclustering.messaging.EndOfStreamException;
	using ReadableChannel = Neo4Net.Storageengine.Api.ReadableChannel;
	using WritableChannel = Neo4Net.Storageengine.Api.WritableChannel;

	public class RaftCoreState
	{
		 private MembershipEntry _committed;

		 public RaftCoreState( MembershipEntry committed )
		 {
			  this._committed = committed;
		 }

		 public virtual MembershipEntry Committed()
		 {
			  return _committed;
		 }

		 public class Marshal : SafeStateMarshal<RaftCoreState>
		 {
			  internal static MembershipEntry.Marshal MembershipMarshal = new MembershipEntry.Marshal();

			  public override RaftCoreState StartState()
			  {
					return null;
			  }

			  public override long Ordinal( RaftCoreState raftCoreState )
			  {
					return 0;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(RaftCoreState raftCoreState, org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  public override void MarshalConflict( RaftCoreState raftCoreState, WritableChannel channel )
			  {

					MembershipMarshal.marshal( raftCoreState.Committed(), channel );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected RaftCoreState unmarshal0(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException, org.neo4j.causalclustering.messaging.EndOfStreamException
			  protected internal override RaftCoreState Unmarshal0( ReadableChannel channel )
			  {
					return new RaftCoreState( MembershipMarshal.unmarshal( channel ) );
			  }
		 }

		 public override string ToString()
		 {
			  return "RaftCoreState{" +
						"committed=" + _committed +
						'}';
		 }
	}

}