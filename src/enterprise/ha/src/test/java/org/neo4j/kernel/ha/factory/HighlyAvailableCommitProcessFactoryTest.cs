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
namespace Neo4Net.Kernel.ha.factory
{
	using Test = org.junit.Test;

	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Neo4Net.Kernel.ha;
	using ReadOnlyTransactionCommitProcess = Neo4Net.Kernel.Impl.Api.ReadOnlyTransactionCommitProcess;
	using TransactionCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionAppender = Neo4Net.Kernel.impl.transaction.log.TransactionAppender;
	using StorageEngine = Neo4Net.Kernel.Api.StorageEngine.StorageEngine;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class HighlyAvailableCommitProcessFactoryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createReadOnlyCommitProcess()
		 public virtual void CreateReadOnlyCommitProcess()
		 {
			  HighlyAvailableCommitProcessFactory factory = new HighlyAvailableCommitProcessFactory( new DelegateInvocationHandler<TransactionCommitProcess>( typeof( TransactionCommitProcess ) ) );

			  Config config = Config.defaults( GraphDatabaseSettings.read_only, "true" );

			  TransactionCommitProcess commitProcess = factory.Create( mock( typeof( TransactionAppender ) ), mock( typeof( StorageEngine ) ), config );

			  assertThat( commitProcess, instanceOf( typeof( ReadOnlyTransactionCommitProcess ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createRegularCommitProcess()
		 public virtual void CreateRegularCommitProcess()
		 {
			  HighlyAvailableCommitProcessFactory factory = new HighlyAvailableCommitProcessFactory( new DelegateInvocationHandler<TransactionCommitProcess>( typeof( TransactionCommitProcess ) ) );

			  TransactionCommitProcess commitProcess = factory.Create( mock( typeof( TransactionAppender ) ), mock( typeof( StorageEngine ) ), Config.defaults() );

			  assertThat( commitProcess, not( instanceOf( typeof( ReadOnlyTransactionCommitProcess ) ) ) );
			  assertThat( Proxy.getInvocationHandler( commitProcess ), instanceOf( typeof( DelegateInvocationHandler ) ) );
		 }
	}

}