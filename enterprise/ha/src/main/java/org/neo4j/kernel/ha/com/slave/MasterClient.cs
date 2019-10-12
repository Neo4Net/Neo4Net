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
namespace Org.Neo4j.Kernel.ha.com.slave
{
	using ComExceptionHandler = Org.Neo4j.com.ComExceptionHandler;
	using Org.Neo4j.com;
	using Org.Neo4j.com;
	using ProtocolVersion = Org.Neo4j.com.ProtocolVersion;
	using RequestContext = Org.Neo4j.com.RequestContext;
	using Org.Neo4j.com;
	using ResponseUnpacker_TxHandler = Org.Neo4j.com.storecopy.ResponseUnpacker_TxHandler;
	using StoreWriter = Org.Neo4j.com.storecopy.StoreWriter;
	using Master = Org.Neo4j.Kernel.ha.com.master.Master;
	using LockResult = Org.Neo4j.Kernel.ha.@lock.LockResult;
	using TransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.TransactionRepresentation;

	public interface MasterClient : Master
	{

		 Response<int> CreateRelationshipType( RequestContext context, string name );

		 Response<Void> NewLockSession( RequestContext context );

		 Response<long> Commit( RequestContext context, TransactionRepresentation channel );

		 Response<Void> PullUpdates( RequestContext context );

		 Response<Void> PullUpdates( RequestContext context, ResponseUnpacker_TxHandler txHandler );

		 Response<Void> CopyStore( RequestContext context, StoreWriter writer );

		 ComExceptionHandler ComExceptionHandler { set; }

		 ProtocolVersion ProtocolVersion { get; }

		 ObjectSerializer<LockResult> CreateLockResultSerializer();

		 Deserializer<LockResult> CreateLockResultDeserializer();
	}

	public static class MasterClient_Fields
	{
		 public static readonly ProtocolVersion Current = MasterClient320.PROTOCOL_VERSION;
	}

}