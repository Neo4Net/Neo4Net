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
namespace Org.Neo4j.causalclustering.core.state.machines.token
{

	using Replicator = Org.Neo4j.causalclustering.core.replication.Replicator;
	using TransactionState = Org.Neo4j.Kernel.api.txstate.TransactionState;
	using TokenRegistry = Org.Neo4j.Kernel.impl.core.TokenRegistry;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;
	using StorageEngine = Org.Neo4j.Storageengine.Api.StorageEngine;

	public class ReplicatedLabelTokenHolder : ReplicatedTokenHolder
	{
		 public ReplicatedLabelTokenHolder( TokenRegistry registry, Replicator replicator, IdGeneratorFactory idGeneratorFactory, System.Func<StorageEngine> storageEngineSupplier ) : base( registry, replicator, idGeneratorFactory, IdType.LABEL_TOKEN, storageEngineSupplier, TokenType.Label, TransactionState.labelDoCreateForName )
		 {
		 }
	}

}