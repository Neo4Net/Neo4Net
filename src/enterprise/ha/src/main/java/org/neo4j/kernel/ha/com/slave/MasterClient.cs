﻿/*
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
namespace Neo4Net.Kernel.ha.com.slave
{
	using ComExceptionHandler = Neo4Net.com.ComExceptionHandler;
	using Neo4Net.com;
	using Neo4Net.com;
	using ProtocolVersion = Neo4Net.com.ProtocolVersion;
	using RequestContext = Neo4Net.com.RequestContext;
	using Neo4Net.com;
	using ResponseUnpacker_TxHandler = Neo4Net.com.storecopy.ResponseUnpacker_TxHandler;
	using StoreWriter = Neo4Net.com.storecopy.StoreWriter;
	using Master = Neo4Net.Kernel.ha.com.master.Master;
	using LockResult = Neo4Net.Kernel.ha.@lock.LockResult;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;

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