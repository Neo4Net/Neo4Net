/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Kernel.impl.factory
{
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using CommitProcessFactory = Org.Neo4j.Kernel.Impl.Api.CommitProcessFactory;
	using ReadOnlyTransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.ReadOnlyTransactionCommitProcess;
	using TransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionRepresentationCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionRepresentationCommitProcess;
	using TransactionAppender = Org.Neo4j.Kernel.impl.transaction.log.TransactionAppender;
	using StorageEngine = Org.Neo4j.Storageengine.Api.StorageEngine;

	public class CommunityCommitProcessFactory : CommitProcessFactory
	{
		 public override TransactionCommitProcess Create( TransactionAppender appender, StorageEngine storageEngine, Config config )
		 {
			  if ( config.Get( GraphDatabaseSettings.read_only ) )
			  {
					return new ReadOnlyTransactionCommitProcess();
			  }
			  return new TransactionRepresentationCommitProcess( appender, storageEngine );
		 }
	}

}