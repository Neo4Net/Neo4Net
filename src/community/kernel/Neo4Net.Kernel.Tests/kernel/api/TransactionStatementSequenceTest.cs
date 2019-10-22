/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.api
{
	using Test = org.junit.Test;

	using InvalidTransactionTypeKernelException = Neo4Net.Internal.Kernel.Api.exceptions.InvalidTransactionTypeKernelException;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.security.LoginContext.AUTH_DISABLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.KernelTransactionFactory.kernelTransaction;

	public class TransactionStatementSequenceTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowReadStatementAfterReadStatement()
		 public virtual void ShouldAllowReadStatementAfterReadStatement()
		 {
			  // given
			  KernelTransaction tx = kernelTransaction( AnonymousContext.read() );
			  tx.DataRead();

			  // when / then
			  tx.DataRead();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowDataStatementAfterReadStatement() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowDataStatementAfterReadStatement()
		 {
			  // given
			  KernelTransaction tx = kernelTransaction( AnonymousContext.write() );
			  tx.DataRead();

			  // when / then
			  tx.DataWrite();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowSchemaStatementAfterReadStatement() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowSchemaStatementAfterReadStatement()
		 {
			  // given
			  KernelTransaction tx = kernelTransaction( AUTH_DISABLED );
			  tx.DataRead();

			  // when / then
			  tx.SchemaWrite();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectSchemaStatementAfterDataStatement() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRejectSchemaStatementAfterDataStatement()
		 {
			  // given
			  KernelTransaction tx = kernelTransaction( AUTH_DISABLED );
			  tx.DataWrite();

			  // when
			  try
			  {
					tx.SchemaWrite();

					fail( "expected exception" );
			  }
			  // then
			  catch ( InvalidTransactionTypeKernelException e )
			  {
					assertEquals( "Cannot perform schema updates in a transaction that has performed data updates.", e.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectDataStatementAfterSchemaStatement() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRejectDataStatementAfterSchemaStatement()
		 {
			  // given
			  KernelTransaction tx = kernelTransaction( AUTH_DISABLED );
			  tx.SchemaWrite();

			  // when
			  try
			  {
					tx.DataWrite();

					fail( "expected exception" );
			  }
			  // then
			  catch ( InvalidTransactionTypeKernelException e )
			  {
					assertEquals( "Cannot perform data updates in a transaction that has performed schema updates.", e.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowDataStatementAfterDataStatement() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowDataStatementAfterDataStatement()
		 {
			  // given
			  KernelTransaction tx = kernelTransaction( AnonymousContext.write() );
			  tx.DataWrite();

			  // when / then
			  tx.DataWrite();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowSchemaStatementAfterSchemaStatement() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowSchemaStatementAfterSchemaStatement()
		 {
			  // given
			  KernelTransaction tx = kernelTransaction( AUTH_DISABLED );
			  tx.SchemaWrite();

			  // when / then
			  tx.SchemaWrite();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowReadStatementAfterDataStatement() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowReadStatementAfterDataStatement()
		 {
			  // given
			  KernelTransaction tx = kernelTransaction( AnonymousContext.write() );
			  tx.DataWrite();

			  // when / then
			  tx.DataRead();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowReadStatementAfterSchemaStatement() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowReadStatementAfterSchemaStatement()
		 {
			  // given
			  KernelTransaction tx = kernelTransaction( AUTH_DISABLED );
			  tx.SchemaWrite();

			  // when / then
			  tx.DataRead();
		 }
	}

}