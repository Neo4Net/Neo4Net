﻿using System.Threading;

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
namespace Org.Neo4j.Server.rest.transactional
{
	using Test = org.junit.Test;


	using LoginContext = Org.Neo4j.@internal.Kernel.Api.security.LoginContext;
	using GraphDatabaseQueryService = Org.Neo4j.Kernel.GraphDatabaseQueryService;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using InvalidConcurrentTransactionAccess = Org.Neo4j.Server.rest.transactional.error.InvalidConcurrentTransactionAccess;
	using TransactionUriScheme = Org.Neo4j.Server.rest.web.TransactionUriScheme;
	using DoubleLatch = Org.Neo4j.Test.DoubleLatch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.parseLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ConcurrentTransactionAccessTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowSpecificExceptionOnConcurrentTransactionAccess() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowSpecificExceptionOnConcurrentTransactionAccess()
		 {
			  // given
			  TransactionRegistry registry = new TransactionHandleRegistry( mock( typeof( Clock ) ), 0, NullLogProvider.Instance );
			  TransitionalPeriodTransactionMessContainer kernel = mock( typeof( TransitionalPeriodTransactionMessContainer ) );
			  GraphDatabaseQueryService queryService = mock( typeof( GraphDatabaseQueryService ) );
			  TransitionalTxManagementKernelTransaction kernelTransaction = mock( typeof( TransitionalTxManagementKernelTransaction ) );
			  when( kernel.NewTransaction( any( typeof( KernelTransaction.Type ) ), any( typeof( LoginContext ) ), anyLong() ) ).thenReturn(kernelTransaction);
			  TransactionFacade actions = new TransactionFacade( kernel, null, queryService, registry, NullLogProvider.Instance );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final TransactionHandle transactionHandle = actions.newTransactionHandle(new DisgustingUriScheme(), true, org.neo4j.internal.kernel.api.security.LoginContext.AUTH_DISABLED, -1);
			  TransactionHandle transactionHandle = actions.NewTransactionHandle( new DisgustingUriScheme(), true, LoginContext.AUTH_DISABLED, -1 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.DoubleLatch latch = new org.neo4j.test.DoubleLatch();
			  DoubleLatch latch = new DoubleLatch();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final StatementDeserializer statements = mock(StatementDeserializer.class);
			  StatementDeserializer statements = mock( typeof( StatementDeserializer ) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  when( statements.HasNext() ).thenAnswer(invocation =>
			  {
				latch.StartAndWaitForAllToStartAndFinish();
				return false;
			  });

			  (new Thread(() =>
			  {
				// start and block until finish
				transactionHandle.Execute( statements, mock( typeof( ExecutionResultSerializer ) ), mock( typeof( HttpServletRequest ) ) );
			  })).Start();

			  latch.WaitForAllToStart();

			  try
			  {
					// when
					actions.FindTransactionHandle( DisgustingUriScheme.ParseTxId( transactionHandle.Uri() ) );
					fail( "should have thrown exception" );
			  }
			  catch ( InvalidConcurrentTransactionAccess )
			  {
					// then we get here
			  }
			  finally
			  {
					latch.Finish();
			  }
		 }

		 private class DisgustingUriScheme : TransactionUriScheme
		 {
			  internal static long ParseTxId( URI txUri )
			  {
					return parseLong( txUri.ToString() );
			  }

			  public override URI TxUri( long id )
			  {
					return URI.create( id.ToString() );
			  }

			  public override URI TxCommitUri( long id )
			  {
					return TxUri( id );
			  }
		 }
	}

}