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
namespace Org.Neo4j.Kernel.impl.coreapi.schema
{
	using Matchers = org.hamcrest.Matchers;
	using Test = org.junit.jupiter.api.Test;

	using Exceptions = Org.Neo4j.Helpers.Exceptions;
	using IndexReference = Org.Neo4j.@internal.Kernel.Api.IndexReference;
	using InternalIndexState = Org.Neo4j.@internal.Kernel.Api.InternalIndexState;
	using SchemaRead = Org.Neo4j.@internal.Kernel.Api.SchemaRead;
	using TokenRead = Org.Neo4j.@internal.Kernel.Api.TokenRead;
	using IndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using Statement = Org.Neo4j.Kernel.api.Statement;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class SchemaImplMockTest
	{
		 private static readonly Exception _cause = new Exception( "Kilroy made it" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void includeCauseOfFailure() throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void IncludeCauseOfFailure()
		 {
			  // given
			  IndexDefinitionImpl indexDefinition = MockIndexDefinition();
			  when( indexDefinition.ToString() ).thenReturn("IndexDefinition( of-some-sort )");
			  KernelTransaction kernelTransaction = MockKernelTransaction();
			  SchemaImpl schema = new SchemaImpl( () => kernelTransaction );

			  // when
			  System.InvalidOperationException e = assertThrows( typeof( System.InvalidOperationException ), () => Schema.awaitIndexOnline(indexDefinition, 1, TimeUnit.MINUTES) );

			  // then
			  assertThat( e.Message, Matchers.containsString( indexDefinition.ToString() ) );
			  assertThat( e.Message, Matchers.containsString( Exceptions.stringify( _cause ) ) );
		 }

		 private static IndexDefinitionImpl MockIndexDefinition()
		 {
			  IndexDefinitionImpl indexDefinition = mock( typeof( IndexDefinitionImpl ) );
			  when( indexDefinition.IndexReference ).thenReturn( IndexReference.NO_INDEX );
			  return indexDefinition;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.kernel.api.KernelTransaction mockKernelTransaction() throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 private static KernelTransaction MockKernelTransaction()
		 {
			  SchemaRead schemaRead = mock( typeof( SchemaRead ) );
			  when( schemaRead.IndexGetState( any( typeof( IndexReference ) ) ) ).thenReturn( InternalIndexState.FAILED );
			  when( schemaRead.IndexGetFailure( any( typeof( IndexReference ) ) ) ).thenReturn( Exceptions.stringify( _cause ) );

			  KernelTransaction kt = mock( typeof( KernelTransaction ) );
			  when( kt.TokenRead() ).thenReturn(mock(typeof(TokenRead)));
			  when( kt.SchemaRead() ).thenReturn(schemaRead);
			  when( kt.Terminated ).thenReturn( false );
			  when( kt.AcquireStatement() ).thenReturn(mock(typeof(Statement)));
			  return kt;
		 }
	}

}