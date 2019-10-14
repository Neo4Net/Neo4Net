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
namespace Neo4Net.Kernel.ha.com.master
{
	using RequestContext = Neo4Net.com.RequestContext;
	using Neo4Net.com;
	using StoreWriter = Neo4Net.com.storecopy.StoreWriter;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using IdAllocation = Neo4Net.Kernel.ha.id.IdAllocation;
	using LockResult = Neo4Net.Kernel.ha.@lock.LockResult;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;
	using ResourceType = Neo4Net.Storageengine.Api.@lock.ResourceType;

	/// <summary>
	/// Represents the master-side of the HA communication between master and slave.
	/// A master will receive calls to these methods from slaves when they do stuff.
	/// </summary>
	public interface Master
	{
		 Response<IdAllocation> AllocateIds( RequestContext context, IdType idType );

		 Response<int> CreateRelationshipType( RequestContext context, string name );

		 Response<int> CreatePropertyKey( RequestContext context, string name );

		 Response<int> CreateLabel( RequestContext context, string name );

		 /// <summary>
		 /// Calling this method will validate, persist to log and apply changes to stores on
		 /// the master.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.com.Response<long> commit(org.neo4j.com.RequestContext context, org.neo4j.kernel.impl.transaction.TransactionRepresentation channel) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException;
		 Response<long> Commit( RequestContext context, TransactionRepresentation channel );

		 /// <summary>
		 /// Calling this method will create a new session with the cluster lock manager and associate that
		 /// session with the provided <seealso cref="RequestContext"/>.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.com.Response<Void> newLockSession(org.neo4j.com.RequestContext context) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException;
		 Response<Void> NewLockSession( RequestContext context );

		 /// <summary>
		 /// Calling this will end the current lock session (identified by the <seealso cref="RequestContext"/>),
		 /// releasing all cluster-global locks held.
		 /// </summary>
		 Response<Void> EndLockSession( RequestContext context, bool success );

		 /// <summary>
		 /// Gets the master id for a given txId, also a checksum for that tx.
		 /// </summary>
		 /// <param name="txId">      the transaction id to get the data for. </param>
		 /// <param name="myStoreId"> clients store id. </param>
		 /// <returns> the master id for a given txId, also a checksum for that tx. </returns>
		 Response<HandshakeResult> Handshake( long txId, StoreId myStoreId );

		 Response<Void> PullUpdates( RequestContext context );

		 Response<Void> CopyStore( RequestContext context, StoreWriter writer );

		 Response<LockResult> AcquireExclusiveLock( RequestContext context, ResourceType type, params long[] resourceIds );

		 Response<LockResult> AcquireSharedLock( RequestContext context, ResourceType type, params long[] resourceIds );

	}

}