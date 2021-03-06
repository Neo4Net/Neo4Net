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
namespace Org.Neo4j.@internal.Kernel.Api
{
	using Test = org.junit.Test;

	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public abstract class TransactionTestBase<G> : KernelAPIWriteTestBase<G> where G : KernelAPIWriteTestSupport
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackWhenTxIsNotSuccess() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRollbackWhenTxIsNotSuccess()
		 {
			  // GIVEN
			  long nodeId;
			  int labelId;
			  using ( Transaction tx = beginTransaction() )
			  {
					// WHEN
					nodeId = tx.DataWrite().nodeCreate();
					labelId = tx.TokenWrite().labelGetOrCreateForName("labello");
					tx.DataWrite().nodeAddLabel(nodeId, labelId);

					// OBS: not marked as tx.success();
			  }

			  // THEN
			  AssertNoNode( nodeId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackWhenTxIsFailed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRollbackWhenTxIsFailed()
		 {
			  // GIVEN
			  long nodeId;
			  int labelId;
			  using ( Transaction tx = beginTransaction() )
			  {
					// WHEN
					nodeId = tx.DataWrite().nodeCreate();
					labelId = tx.TokenWrite().labelGetOrCreateForName("labello");
					tx.DataWrite().nodeAddLabel(nodeId, labelId);

					tx.Failure();
			  }

			  // THEN
			  AssertNoNode( nodeId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackAndThrowWhenTxIsBothFailedAndSuccess() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRollbackAndThrowWhenTxIsBothFailedAndSuccess()
		 {
			  // GIVEN
			  long nodeId;
			  int labelId;

			  Transaction tx = beginTransaction();
			  nodeId = tx.DataWrite().nodeCreate();
			  labelId = tx.TokenWrite().labelGetOrCreateForName("labello");
			  tx.DataWrite().nodeAddLabel(nodeId, labelId);
			  tx.Failure();
			  tx.Success();

			  // WHEN
			  try
			  {
					tx.Close();
					fail( "Expected TransactionFailureException" );
			  }
			  catch ( TransactionFailureException )
			  {
					// wanted
			  }

			  // THEN
			  AssertNoNode( nodeId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertNoNode(long nodeId) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private void AssertNoNode( long nodeId )
		 {
			  using ( Transaction tx = beginTransaction(), NodeCursor cursor = tx.Cursors().allocateNodeCursor() )
			  {
					tx.DataRead().singleNode(nodeId, cursor);
					assertFalse( cursor.Next() );
			  }
		 }
	}

}