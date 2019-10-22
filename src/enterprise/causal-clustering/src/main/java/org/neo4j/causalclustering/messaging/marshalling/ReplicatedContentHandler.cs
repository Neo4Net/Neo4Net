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
namespace Neo4Net.causalclustering.messaging.marshalling
{

	using NewLeaderBarrier = Neo4Net.causalclustering.core.consensus.NewLeaderBarrier;
	using MemberIdSet = Neo4Net.causalclustering.core.consensus.membership.MemberIdSet;
	using DistributedOperation = Neo4Net.causalclustering.core.replication.DistributedOperation;
	using DummyRequest = Neo4Net.causalclustering.core.state.machines.dummy.DummyRequest;
	using ReplicatedIdAllocationRequest = Neo4Net.causalclustering.core.state.machines.id.ReplicatedIdAllocationRequest;
	using ReplicatedLockTokenRequest = Neo4Net.causalclustering.core.state.machines.locks.ReplicatedLockTokenRequest;
	using ReplicatedTokenRequest = Neo4Net.causalclustering.core.state.machines.token.ReplicatedTokenRequest;
	using ReplicatedTransaction = Neo4Net.causalclustering.core.state.machines.tx.ReplicatedTransaction;

	public interface ReplicatedContentHandler
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void handle(org.Neo4Net.causalclustering.core.state.machines.tx.ReplicatedTransaction replicatedTransaction) throws java.io.IOException;
		 void Handle( ReplicatedTransaction replicatedTransaction );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void handle(org.Neo4Net.causalclustering.core.consensus.membership.MemberIdSet memberIdSet) throws java.io.IOException;
		 void Handle( MemberIdSet memberIdSet );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void handle(org.Neo4Net.causalclustering.core.state.machines.id.ReplicatedIdAllocationRequest replicatedIdAllocationRequest) throws java.io.IOException;
		 void Handle( ReplicatedIdAllocationRequest replicatedIdAllocationRequest );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void handle(org.Neo4Net.causalclustering.core.state.machines.token.ReplicatedTokenRequest replicatedTokenRequest) throws java.io.IOException;
		 void Handle( ReplicatedTokenRequest replicatedTokenRequest );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void handle(org.Neo4Net.causalclustering.core.consensus.NewLeaderBarrier newLeaderBarrier) throws java.io.IOException;
		 void Handle( NewLeaderBarrier newLeaderBarrier );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void handle(org.Neo4Net.causalclustering.core.state.machines.locks.ReplicatedLockTokenRequest replicatedLockTokenRequest) throws java.io.IOException;
		 void Handle( ReplicatedLockTokenRequest replicatedLockTokenRequest );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void handle(org.Neo4Net.causalclustering.core.replication.DistributedOperation distributedOperation) throws java.io.IOException;
		 void Handle( DistributedOperation distributedOperation );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void handle(org.Neo4Net.causalclustering.core.state.machines.dummy.DummyRequest dummyRequest) throws java.io.IOException;
		 void Handle( DummyRequest dummyRequest );
	}

}