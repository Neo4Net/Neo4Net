using System;

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
namespace Org.Neo4j.Kernel
{
	using Test = org.junit.Test;

	using TransactionTerminatedException = Org.Neo4j.Graphdb.TransactionTerminatedException;
	using TransientDatabaseFailureException = Org.Neo4j.Graphdb.TransientDatabaseFailureException;
	using TransientFailureException = Org.Neo4j.Graphdb.TransientFailureException;
	using TransientTransactionFailureException = Org.Neo4j.Graphdb.TransientTransactionFailureException;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using TopLevelTransaction = Org.Neo4j.Kernel.impl.coreapi.TopLevelTransaction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doReturn;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class TopLevelTransactionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowTransientExceptionOnTransientKernelException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowTransientExceptionOnTransientKernelException()
		 {
			  // GIVEN
			  KernelTransaction kernelTransaction = mock( typeof( KernelTransaction ) );
			  when( kernelTransaction.Open ).thenReturn( true );
			  doThrow( new TransactionFailureException( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.ConstraintsChanged, "Proving that TopLevelTransaction does the right thing" ) ).when( kernelTransaction ).close();
			  TopLevelTransaction transaction = new TopLevelTransaction( kernelTransaction );

			  // WHEN
			  transaction.Success();
			  try
			  {
					transaction.Close();
					fail( "Should have failed" );
			  }
			  catch ( TransientTransactionFailureException )
			  { // THEN Good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowTransactionExceptionOnTransientKernelException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowTransactionExceptionOnTransientKernelException()
		 {
			  // GIVEN
			  KernelTransaction kernelTransaction = mock( typeof( KernelTransaction ) );
			  when( kernelTransaction.Open ).thenReturn( true );
			  doThrow( new Exception( "Just a random failure" ) ).when( kernelTransaction ).close();
			  TopLevelTransaction transaction = new TopLevelTransaction( kernelTransaction );

			  // WHEN
			  transaction.Success();
			  try
			  {
					transaction.Close();
					fail( "Should have failed" );
			  }
			  catch ( Org.Neo4j.Graphdb.TransactionFailureException )
			  { // THEN Good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLetThroughTransientFailureException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLetThroughTransientFailureException()
		 {
			  // GIVEN
			  KernelTransaction kernelTransaction = mock( typeof( KernelTransaction ) );
			  when( kernelTransaction.Open ).thenReturn( true );
			  doThrow( new TransientDatabaseFailureException( "Just a random failure" ) ).when( kernelTransaction ).close();
			  TopLevelTransaction transaction = new TopLevelTransaction( kernelTransaction );

			  // WHEN
			  transaction.Success();
			  try
			  {
					transaction.Close();
					fail( "Should have failed" );
			  }
			  catch ( TransientFailureException )
			  { // THEN Good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShowTransactionTerminatedExceptionAsTransient() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldShowTransactionTerminatedExceptionAsTransient()
		 {
			  KernelTransaction kernelTransaction = mock( typeof( KernelTransaction ) );
			  doReturn( true ).when( kernelTransaction ).Open;
			  Exception error = new TransactionTerminatedException( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.Terminated );
			  doThrow( error ).when( kernelTransaction ).close();
			  TopLevelTransaction transaction = new TopLevelTransaction( kernelTransaction );

			  transaction.Success();
			  try
			  {
					transaction.Close();
					fail( "Should have failed" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( TransientTransactionFailureException ) ) );
					assertSame( error, e.InnerException );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnTerminationReason()
		 public virtual void ShouldReturnTerminationReason()
		 {
			  KernelTransaction kernelTransaction = mock( typeof( KernelTransaction ) );
			  when( kernelTransaction.ReasonIfTerminated ).thenReturn( null ).thenReturn( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.Terminated );

			  TopLevelTransaction tx = new TopLevelTransaction( kernelTransaction );

			  Optional<Status> terminationReason1 = tx.TerminationReason();
			  Optional<Status> terminationReason2 = tx.TerminationReason();

			  assertFalse( terminationReason1.Present );
			  assertTrue( terminationReason2.Present );
			  assertEquals( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.Terminated, terminationReason2.get() );
		 }
	}

}