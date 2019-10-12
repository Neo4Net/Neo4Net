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
namespace Neo4Net.Kernel.Impl.Api
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using AuthorizationViolationException = Neo4Net.Graphdb.security.AuthorizationViolationException;
	using Read = Neo4Net.@internal.Kernel.Api.Read;
	using SchemaWrite = Neo4Net.@internal.Kernel.Api.SchemaWrite;
	using Write = Neo4Net.@internal.Kernel.Api.Write;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.LoginContext.AUTH_DISABLED;

	public class KernelTransactionSecurityContextTest : KernelTransactionTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowReadsInNoneMode()
		 public virtual void ShouldNotAllowReadsInNoneMode()
		 {
			  // Given
			  KernelTransactionImplementation tx = newTransaction( AnonymousContext.none() );

			  // Expect
			  Exception.expect( typeof( AuthorizationViolationException ) );

			  // When
			  tx.DataRead();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowTokenReadsInNoneMode()
		 public virtual void ShouldNotAllowTokenReadsInNoneMode()
		 {
			  // Given
			  KernelTransactionImplementation tx = newTransaction( AnonymousContext.none() );

			  // Expect
			  Exception.expect( typeof( AuthorizationViolationException ) );

			  // When
			  tx.TokenRead();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowWritesInNoneMode() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowWritesInNoneMode()
		 {
			  // Given
			  KernelTransactionImplementation tx = newTransaction( AnonymousContext.none() );

			  // Expect
			  Exception.expect( typeof( AuthorizationViolationException ) );

			  // When
			  tx.DataWrite();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowSchemaWritesInNoneMode() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowSchemaWritesInNoneMode()
		 {
			  // Given
			  KernelTransactionImplementation tx = newTransaction( AnonymousContext.none() );

			  // Expect
			  Exception.expect( typeof( AuthorizationViolationException ) );

			  // When
			  tx.SchemaWrite();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowReadsInReadMode()
		 public virtual void ShouldAllowReadsInReadMode()
		 {
			  // Given
			  KernelTransactionImplementation tx = newTransaction( AnonymousContext.read() );

			  // When
			  Read reads = tx.DataRead();

			  // Then
			  assertNotNull( reads );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowWriteAccessInReadMode() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowWriteAccessInReadMode()
		 {
			  // Given
			  KernelTransactionImplementation tx = newTransaction( AnonymousContext.read() );

			  // Expect
			  Exception.expect( typeof( AuthorizationViolationException ) );

			  // When
			  tx.DataWrite();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowSchemaWriteAccessInReadMode() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowSchemaWriteAccessInReadMode()
		 {
			  // Given
			  KernelTransactionImplementation tx = newTransaction( AnonymousContext.read() );

			  // Expect
			  Exception.expect( typeof( AuthorizationViolationException ) );

			  // When
			  tx.SchemaWrite();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowReadAccessInWriteOnlyMode()
		 public virtual void ShouldNotAllowReadAccessInWriteOnlyMode()
		 {
			  // Given
			  KernelTransactionImplementation tx = newTransaction( AnonymousContext.writeOnly() );

			  // Expect
			  Exception.expect( typeof( AuthorizationViolationException ) );

			  // When
			  tx.DataRead();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowTokenReadAccessInWriteOnlyMode()
		 public virtual void ShouldNotAllowTokenReadAccessInWriteOnlyMode()
		 {
			  // Given
			  KernelTransactionImplementation tx = newTransaction( AnonymousContext.writeOnly() );

			  // Expect
			  Exception.expect( typeof( AuthorizationViolationException ) );

			  // When
			  tx.TokenRead();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowWriteAccessInWriteOnlyMode() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowWriteAccessInWriteOnlyMode()
		 {
			  // Given
			  KernelTransactionImplementation tx = newTransaction( AnonymousContext.writeOnly() );

			  // When
			  Write writes = tx.DataWrite();

			  // Then
			  assertNotNull( writes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowSchemaWriteAccessInWriteOnlyMode() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowSchemaWriteAccessInWriteOnlyMode()
		 {
			  // Given
			  KernelTransactionImplementation tx = newTransaction( AnonymousContext.writeOnly() );

			  // Expect
			  Exception.expect( typeof( AuthorizationViolationException ) );

			  // When
			  tx.SchemaWrite();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowReadsInWriteMode()
		 public virtual void ShouldAllowReadsInWriteMode()
		 {
			  // Given
			  KernelTransactionImplementation tx = newTransaction( AnonymousContext.write() );

			  // When
			  Read reads = tx.DataRead();

			  // Then
			  assertNotNull( reads );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowWritesInWriteMode() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowWritesInWriteMode()
		 {
			  // Given
			  KernelTransactionImplementation tx = newTransaction( AnonymousContext.write() );

			  // When
			  Write writes = tx.DataWrite();

			  // Then
			  assertNotNull( writes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowSchemaWriteAccessInWriteMode() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowSchemaWriteAccessInWriteMode()
		 {
			  // Given
			  KernelTransactionImplementation tx = newTransaction( AnonymousContext.write() );

			  // Expect
			  Exception.expect( typeof( AuthorizationViolationException ) );

			  // When
			  tx.SchemaWrite();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowReadsInFullMode()
		 public virtual void ShouldAllowReadsInFullMode()
		 {
			  // Given
			  KernelTransactionImplementation tx = newTransaction( AUTH_DISABLED );

			  // When
			  Read reads = tx.DataRead();

			  // Then
			  assertNotNull( reads );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowWritesInFullMode() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowWritesInFullMode()
		 {
			  // Given
			  KernelTransactionImplementation tx = newTransaction( AUTH_DISABLED );

			  // When
			  Write writes = tx.DataWrite();

			  // Then
			  assertNotNull( writes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowSchemaWriteAccessInFullMode() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowSchemaWriteAccessInFullMode()
		 {
			  // Given
			  KernelTransactionImplementation tx = newTransaction( AUTH_DISABLED );

			  // When
			  SchemaWrite writes = tx.SchemaWrite();

			  // Then
			  assertNotNull( writes );
		 }
	}

}