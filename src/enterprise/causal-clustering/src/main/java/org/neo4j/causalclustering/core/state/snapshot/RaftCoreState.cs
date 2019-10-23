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
namespace Neo4Net.causalclustering.core.state.snapshot
{

	using MembershipEntry = Neo4Net.causalclustering.core.consensus.membership.MembershipEntry;
	using Neo4Net.causalclustering.core.state.storage;
	using EndOfStreamException = Neo4Net.causalclustering.messaging.EndOfStreamException;
	using ReadableChannel = Neo4Net.Kernel.Api.StorageEngine.ReadableChannel;
	using WritableChannel = Neo4Net.Kernel.Api.StorageEngine.WritableChannel;

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
//ORIGINAL LINE: public void marshal(RaftCoreState raftCoreState, org.Neo4Net.Kernel.Api.StorageEngine.WritableChannel channel) throws java.io.IOException
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  public override void MarshalConflict( RaftCoreState raftCoreState, WritableChannel channel )
			  {

					MembershipMarshal.marshal( raftCoreState.Committed(), channel );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected RaftCoreState unmarshal0(org.Neo4Net.Kernel.Api.StorageEngine.ReadableChannel channel) throws java.io.IOException, org.Neo4Net.causalclustering.messaging.EndOfStreamException
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