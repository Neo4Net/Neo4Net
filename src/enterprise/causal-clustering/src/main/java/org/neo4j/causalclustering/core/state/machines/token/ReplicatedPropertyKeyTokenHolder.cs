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
namespace Neo4Net.causalclustering.core.state.machines.token
{

	using RaftReplicator = Neo4Net.causalclustering.core.replication.RaftReplicator;
	using TransactionState = Neo4Net.Kernel.api.txstate.TransactionState;
	using TokenRegistry = Neo4Net.Kernel.impl.core.TokenRegistry;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using StorageEngine = Neo4Net.Kernel.Api.StorageEngine.StorageEngine;

	public class ReplicatedPropertyKeyTokenHolder : ReplicatedTokenHolder
	{
		 public ReplicatedPropertyKeyTokenHolder( TokenRegistry registry, RaftReplicator replicator, IdGeneratorFactory idGeneratorFactory, System.Func<StorageEngine> storageEngineSupplier ) : base( registry, replicator, idGeneratorFactory, IdType.PROPERTY_KEY_TOKEN, storageEngineSupplier, TokenType.Property, TransactionState.propertyKeyDoCreateForName )
		 {
		 }
	}

}