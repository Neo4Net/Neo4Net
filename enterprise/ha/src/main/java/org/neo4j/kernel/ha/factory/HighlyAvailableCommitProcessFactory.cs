using System;

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
namespace Org.Neo4j.Kernel.ha.factory
{
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Org.Neo4j.Kernel.ha;
	using CommitProcessFactory = Org.Neo4j.Kernel.Impl.Api.CommitProcessFactory;
	using ReadOnlyTransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.ReadOnlyTransactionCommitProcess;
	using TransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionAppender = Org.Neo4j.Kernel.impl.transaction.log.TransactionAppender;
	using StorageEngine = Org.Neo4j.Storageengine.Api.StorageEngine;

	internal class HighlyAvailableCommitProcessFactory : CommitProcessFactory
	{
		 private readonly DelegateInvocationHandler<TransactionCommitProcess> _commitProcessDelegate;

		 internal HighlyAvailableCommitProcessFactory( DelegateInvocationHandler<TransactionCommitProcess> commitProcessDelegate )
		 {
			  this._commitProcessDelegate = commitProcessDelegate;
		 }

		 public override TransactionCommitProcess Create( TransactionAppender appender, StorageEngine storageEngine, Config config )
		 {
			  if ( config.Get( GraphDatabaseSettings.read_only ) )
			  {
					return new ReadOnlyTransactionCommitProcess();
			  }
			  return ( TransactionCommitProcess ) newProxyInstance( typeof( TransactionCommitProcess ).ClassLoader, new Type[]{ typeof( TransactionCommitProcess ) }, _commitProcessDelegate );
		 }
	}

}