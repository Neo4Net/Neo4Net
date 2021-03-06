﻿/*
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
	using Test = org.junit.Test;

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ReadOnlyTransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.ReadOnlyTransactionCommitProcess;
	using TransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionRepresentationCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionRepresentationCommitProcess;
	using TransactionAppender = Org.Neo4j.Kernel.impl.transaction.log.TransactionAppender;
	using StorageEngine = Org.Neo4j.Storageengine.Api.StorageEngine;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class CommunityCommitProcessFactoryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createReadOnlyCommitProcess()
		 public virtual void CreateReadOnlyCommitProcess()
		 {
			  CommunityCommitProcessFactory factory = new CommunityCommitProcessFactory();

			  Config config = Config.defaults( GraphDatabaseSettings.read_only, "true" );

			  TransactionCommitProcess commitProcess = factory.Create( mock( typeof( TransactionAppender ) ), mock( typeof( StorageEngine ) ), config );

			  assertThat( commitProcess, instanceOf( typeof( ReadOnlyTransactionCommitProcess ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createRegularCommitProcess()
		 public virtual void CreateRegularCommitProcess()
		 {
			  CommunityCommitProcessFactory factory = new CommunityCommitProcessFactory();

			  TransactionCommitProcess commitProcess = factory.Create( mock( typeof( TransactionAppender ) ), mock( typeof( StorageEngine ) ), Config.defaults() );

			  assertThat( commitProcess, instanceOf( typeof( TransactionRepresentationCommitProcess ) ) );
		 }
	}

}