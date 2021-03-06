﻿using System;
using System.Threading;

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
namespace Org.Neo4j.Kernel.builtinprocs
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using Exceptions = Org.Neo4j.Helpers.Exceptions;
	using IndexReference = Org.Neo4j.@internal.Kernel.Api.IndexReference;
	using InternalIndexState = Org.Neo4j.@internal.Kernel.Api.InternalIndexState;
	using SchemaRead = Org.Neo4j.@internal.Kernel.Api.SchemaRead;
	using TokenRead = Org.Neo4j.@internal.Kernel.Api.TokenRead;
	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using IndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using LabelSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.InternalIndexState.FAILED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.InternalIndexState.ONLINE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.InternalIndexState.POPULATING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.IndexDescriptorFactory.forSchema;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	public class AwaitIndexProcedureTest
	{
		 private const int TIMEOUT = 10;
		 private static readonly TimeUnit _timeUnit = TimeUnit.SECONDS;
		 private KernelTransaction _transaction;
		 private TokenRead _tokenRead;
		 private SchemaRead _schemaRead;
		 private IndexProcedures _procedure;
		 private LabelSchemaDescriptor _descriptor;
		 private LabelSchemaDescriptor _anyDescriptor;
		 private IndexReference _anyIndex;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _transaction = mock( typeof( KernelTransaction ) );
			  _tokenRead = mock( typeof( TokenRead ) );
			  _schemaRead = mock( typeof( SchemaRead ) );
			  _procedure = new IndexProcedures( _transaction, null );
			  _descriptor = SchemaDescriptorFactory.forLabel( 123, 456 );
			  _anyDescriptor = SchemaDescriptorFactory.forLabel( 0, 0 );
			  _anyIndex = forSchema( _anyDescriptor );
			  when( _transaction.tokenRead() ).thenReturn(_tokenRead);
			  when( _transaction.schemaRead() ).thenReturn(_schemaRead);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowAnExceptionIfTheLabelDoesntExist()
		 public virtual void ShouldThrowAnExceptionIfTheLabelDoesntExist()
		 {
			  when( _tokenRead.nodeLabel( "NonExistentLabel" ) ).thenReturn( -1 );

			  try
			  {
					_procedure.awaitIndexByPattern( ":NonExistentLabel(prop)", TIMEOUT, _timeUnit );
					fail( "Expected an exception" );
			  }
			  catch ( ProcedureException e )
			  {
					assertThat( e.Status(), @is(Org.Neo4j.Kernel.Api.Exceptions.Status_Schema.LabelAccessFailed) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowAnExceptionIfThePropertyKeyDoesntExist()
		 public virtual void ShouldThrowAnExceptionIfThePropertyKeyDoesntExist()
		 {
			  when( _tokenRead.propertyKey( "nonExistentProperty" ) ).thenReturn( -1 );

			  try
			  {
					_procedure.awaitIndexByPattern( ":Label(nonExistentProperty)", TIMEOUT, _timeUnit );
					fail( "Expected an exception" );
			  }
			  catch ( ProcedureException e )
			  {
					assertThat( e.Status(), @is(Org.Neo4j.Kernel.Api.Exceptions.Status_Schema.PropertyKeyAccessFailed) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLookUpTheIndexByLabelIdAndPropertyKeyId() throws org.neo4j.internal.kernel.api.exceptions.ProcedureException, org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLookUpTheIndexByLabelIdAndPropertyKeyId()
		 {
			  when( _tokenRead.nodeLabel( anyString() ) ).thenReturn(_descriptor.LabelId);
			  when( _tokenRead.propertyKey( anyString() ) ).thenReturn(_descriptor.PropertyId);
			  when( _schemaRead.index( anyInt(), any() ) ).thenReturn(_anyIndex);
			  when( _schemaRead.indexGetState( any( typeof( IndexReference ) ) ) ).thenReturn( ONLINE );

			  _procedure.awaitIndexByPattern( ":Person(name)", TIMEOUT, _timeUnit );

			  verify( _schemaRead ).index( _descriptor.LabelId, _descriptor.PropertyId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLookUpTheIndexByIndexName() throws org.neo4j.internal.kernel.api.exceptions.ProcedureException, org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLookUpTheIndexByIndexName()
		 {
			  when( _tokenRead.nodeLabel( anyString() ) ).thenReturn(_descriptor.LabelId);
			  when( _tokenRead.propertyKey( anyString() ) ).thenReturn(_descriptor.PropertyId);
			  when( _schemaRead.indexGetForName( "my index" ) ).thenReturn( _anyIndex );
			  when( _schemaRead.indexGetState( any( typeof( IndexReference ) ) ) ).thenReturn( ONLINE );

			  _procedure.awaitIndexByName( "`my index`", TIMEOUT, _timeUnit );

			  verify( _schemaRead ).indexGetForName( "my index" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowAnExceptionIfTheIndexHasFailed() throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowAnExceptionIfTheIndexHasFailed()
		 {
			  when( _tokenRead.nodeLabel( anyString() ) ).thenReturn(0);
			  when( _tokenRead.propertyKey( anyString() ) ).thenReturn(0);
			  when( _schemaRead.index( anyInt(), any() ) ).thenReturn(_anyIndex);
			  when( _schemaRead.indexGetState( any( typeof( IndexReference ) ) ) ).thenReturn( FAILED );
			  when( _schemaRead.indexGetFailure( any( typeof( IndexReference ) ) ) ).thenReturn( Exceptions.stringify( new Exception( "Kilroy was here" ) ) );

			  try
			  {
					_procedure.awaitIndexByPattern( ":Person(name)", TIMEOUT, _timeUnit );
					fail( "Expected an exception" );
			  }
			  catch ( ProcedureException e )
			  {
					assertThat( e.Status(), @is(Org.Neo4j.Kernel.Api.Exceptions.Status_Schema.IndexCreationFailed) );
					assertThat( e.Message, containsString( ":Person(name)" ) );
					assertThat( e.Message, containsString( "Kilroy was here" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowAnExceptionIfTheIndexDoesNotExist()
		 public virtual void ShouldThrowAnExceptionIfTheIndexDoesNotExist()
		 {
			  when( _tokenRead.propertyKey( anyString() ) ).thenReturn(0);
			  when( _tokenRead.nodeLabel( anyString() ) ).thenReturn(0);
			  when( _schemaRead.index( anyInt(), any() ) ).thenReturn(IndexReference.NO_INDEX);

			  try
			  {
					_procedure.awaitIndexByPattern( ":Person(name)", TIMEOUT, _timeUnit );
					fail( "Expected an exception" );
			  }
			  catch ( ProcedureException e )
			  {
					assertThat( e.Status(), @is(Org.Neo4j.Kernel.Api.Exceptions.Status_Schema.IndexNotFound) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void shouldThrowAnExceptionIfGivenAnIndexName() throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowAnExceptionIfGivenAnIndexName()
		 {
			  _procedure.awaitIndexByPattern( "`some index`", TIMEOUT, _timeUnit );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowAnExceptionIfTheIndexWithGivenNameDoesNotExist()
		 public virtual void ShouldThrowAnExceptionIfTheIndexWithGivenNameDoesNotExist()
		 {
			  when( _tokenRead.propertyKey( anyString() ) ).thenReturn(0);
			  when( _tokenRead.nodeLabel( anyString() ) ).thenReturn(0);
			  when( _schemaRead.indexGetForName( "some index" ) ).thenReturn( IndexReference.NO_INDEX );

			  try
			  {
					_procedure.awaitIndexByName( "`some index`", TIMEOUT, _timeUnit );
					fail( "Expected an exception" );
			  }
			  catch ( ProcedureException e )
			  {
					assertThat( e.Status(), @is(Org.Neo4j.Kernel.Api.Exceptions.Status_Schema.IndexNotFound) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBlockUntilTheIndexIsOnline() throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBlockUntilTheIndexIsOnline()
		 {
			  when( _tokenRead.nodeLabel( anyString() ) ).thenReturn(0);
			  when( _tokenRead.propertyKey( anyString() ) ).thenReturn(0);
			  when( _schemaRead.index( anyInt(), any() ) ).thenReturn(_anyIndex);

			  AtomicReference<InternalIndexState> state = new AtomicReference<InternalIndexState>( POPULATING );
			  when( _schemaRead.indexGetState( any( typeof( IndexReference ) ) ) ).then( invocationOnMock => state.get() );

			  AtomicBoolean done = new AtomicBoolean( false );
			  (new Thread(() =>
			  {
				try
				{
					 _procedure.awaitIndexByPattern( ":Person(name)", TIMEOUT, _timeUnit );
				}
				catch ( ProcedureException e )
				{
					 throw new Exception( e );
				}
				done.set( true );
			  })).Start();

			  assertThat( done.get(), @is(false) );

			  state.set( ONLINE );
			  assertEventually( "Procedure did not return after index was online", done.get, @is( true ), TIMEOUT, _timeUnit );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTimeoutIfTheIndexTakesTooLongToComeOnline() throws InterruptedException, org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTimeoutIfTheIndexTakesTooLongToComeOnline()
		 {
			  when( _tokenRead.nodeLabel( anyString() ) ).thenReturn(0);
			  when( _tokenRead.propertyKey( anyString() ) ).thenReturn(0);
			  when( _schemaRead.index( anyInt(), anyInt() ) ).thenReturn(_anyIndex);
			  when( _schemaRead.indexGetState( any( typeof( IndexReference ) ) ) ).thenReturn( POPULATING );

			  AtomicReference<ProcedureException> exception = new AtomicReference<ProcedureException>();
			  (new Thread(() =>
			  {
				try
				{
					 // We wait here, because we expect timeout
					 _procedure.awaitIndexByPattern( ":Person(name)", 0, _timeUnit );
				}
				catch ( ProcedureException e )
				{
					 exception.set( e );
				}
			  })).Start();

			  assertEventually( "Procedure did not time out", exception.get, not( nullValue() ), TIMEOUT, _timeUnit );
			  //noinspection ThrowableResultOfMethodCallIgnored
			  assertThat( exception.get().status(), @is(Org.Neo4j.Kernel.Api.Exceptions.Status_Procedure.ProcedureTimedOut) );
		 }
	}

}