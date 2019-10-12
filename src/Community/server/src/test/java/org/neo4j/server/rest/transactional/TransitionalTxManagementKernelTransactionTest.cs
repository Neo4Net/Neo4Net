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
namespace Neo4Net.Server.rest.transactional
{
	using Test = org.junit.Test;

	using LoginContext = Neo4Net.@internal.Kernel.Api.security.LoginContext;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

	public class TransitionalTxManagementKernelTransactionTest
	{

		 private GraphDatabaseFacade _databaseFacade = mock( typeof( GraphDatabaseFacade ) );
		 private ThreadToStatementContextBridge _contextBridge = mock( typeof( ThreadToStatementContextBridge ) );
		 private LoginContext _loginContext = AnonymousContext.read();
		 private KernelTransaction.Type _type = KernelTransaction.Type.@implicit;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void reopenStartTransactionWithCustomTimeoutIfSpecified()
		 public virtual void ReopenStartTransactionWithCustomTimeoutIfSpecified()
		 {
			  TransitionalTxManagementKernelTransaction managementKernelTransaction = new TransitionalTxManagementKernelTransaction( _databaseFacade, _type, _loginContext, 10, _contextBridge );

			  managementKernelTransaction.ReopenAfterPeriodicCommit();

			  verify( _databaseFacade, times( 2 ) ).beginTransaction( _type, _loginContext, 10, TimeUnit.MILLISECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void reopenStartDefaultTransactionIfTimeoutNotSpecified()
		 public virtual void ReopenStartDefaultTransactionIfTimeoutNotSpecified()
		 {
			  TransitionalTxManagementKernelTransaction managementKernelTransaction = new TransitionalTxManagementKernelTransaction( _databaseFacade, _type, _loginContext, -1, _contextBridge );

			  managementKernelTransaction.ReopenAfterPeriodicCommit();

			  verify( _databaseFacade, times( 2 ) ).beginTransaction( _type, _loginContext );
		 }
	}

}