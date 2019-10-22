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
namespace Neo4Net.Kernel.builtinprocs
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using IndexReference = Neo4Net.Internal.Kernel.Api.IndexReference;
	using SchemaRead = Neo4Net.Internal.Kernel.Api.SchemaRead;
	using TokenRead = Neo4Net.Internal.Kernel.Api.TokenRead;
	using ProcedureException = Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException;
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using SchemaRuleNotFoundException = Neo4Net.Kernel.Api.Exceptions.schema.SchemaRuleNotFoundException;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using IndexSamplingMode = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingMode;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
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

	public class ResampleIndexProcedureTest
	{
		 private IndexingService _indexingService;
		 private IndexProcedures _procedure;
		 private TokenRead _tokenRead;
		 private SchemaRead _schemaRead;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {

			  KernelTransaction transaction = mock( typeof( KernelTransaction ) );
			  _tokenRead = mock( typeof( TokenRead ) );
			  _schemaRead = mock( typeof( SchemaRead ) );
			  _procedure = new IndexProcedures( transaction, null );

			  when( transaction.TokenRead() ).thenReturn(_tokenRead);
			  when( transaction.SchemaRead() ).thenReturn(_schemaRead);
			  _indexingService = mock( typeof( IndexingService ) );
			  _procedure = new IndexProcedures( transaction, _indexingService );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowAnExceptionIfTheLabelDoesntExist()
		 public virtual void ShouldThrowAnExceptionIfTheLabelDoesntExist()
		 {
			  when( _tokenRead.nodeLabel( "NonExistentLabel" ) ).thenReturn( -1 );

			  try
			  {
					_procedure.resampleIndex( ":NonExistentLabel(prop)" );
					fail( "Expected an exception" );
			  }
			  catch ( ProcedureException e )
			  {
					assertThat( e.Status(), @is(Neo4Net.Kernel.Api.Exceptions.Status_Schema.LabelAccessFailed) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowAnExceptionIfThePropertyKeyDoesntExist()
		 public virtual void ShouldThrowAnExceptionIfThePropertyKeyDoesntExist()
		 {
			  when( _tokenRead.propertyKey( "nonExistentProperty" ) ).thenReturn( -1 );

			  try
			  {
					_procedure.resampleIndex( ":Label(nonExistentProperty)" );
					fail( "Expected an exception" );
			  }
			  catch ( ProcedureException e )
			  {
					assertThat( e.Status(), @is(Neo4Net.Kernel.Api.Exceptions.Status_Schema.PropertyKeyAccessFailed) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLookUpTheIndexByLabelIdAndPropertyKeyId() throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException, org.Neo4Net.kernel.api.exceptions.schema.SchemaRuleNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLookUpTheIndexByLabelIdAndPropertyKeyId()
		 {
			  IndexDescriptor index = TestIndexDescriptorFactory.forLabel( 0, 0 );
			  when( _tokenRead.nodeLabel( anyString() ) ).thenReturn(123);
			  when( _tokenRead.propertyKey( anyString() ) ).thenReturn(456);
			  when( _schemaRead.index( anyInt(), any() ) ).thenReturn(index);

			  _procedure.resampleIndex( ":Person(name)" );

			  verify( _schemaRead ).index( 123, 456 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLookUpTheCompositeIndexByLabelIdAndPropertyKeyId() throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException, org.Neo4Net.kernel.api.exceptions.schema.SchemaRuleNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLookUpTheCompositeIndexByLabelIdAndPropertyKeyId()
		 {
			  IndexDescriptor index = TestIndexDescriptorFactory.forLabel( 0, 0, 1 );
			  when( _tokenRead.nodeLabel( anyString() ) ).thenReturn(123);
			  when( _tokenRead.propertyKey( "name" ) ).thenReturn( 0 );
			  when( _tokenRead.propertyKey( "lastName" ) ).thenReturn( 1 );
			  when( _schemaRead.index( 123, 0, 1 ) ).thenReturn( index );

			  _procedure.resampleIndex( ":Person(name, lastName)" );

			  verify( _schemaRead ).index( 123, 0, 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowAnExceptionIfTheIndexDoesNotExist() throws org.Neo4Net.kernel.api.exceptions.schema.SchemaRuleNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowAnExceptionIfTheIndexDoesNotExist()

		 {
			  when( _tokenRead.nodeLabel( anyString() ) ).thenReturn(0);
			  when( _tokenRead.propertyKey( anyString() ) ).thenReturn(0);
			  when( _schemaRead.index( anyInt(), any() ) ).thenReturn(IndexReference.NO_INDEX);

			  try
			  {
					_procedure.resampleIndex( ":Person(name)" );
					fail( "Expected an exception" );
			  }
			  catch ( ProcedureException e )
			  {
					assertThat( e.Status(), @is(Neo4Net.Kernel.Api.Exceptions.Status_Schema.IndexNotFound) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTriggerResampling() throws org.Neo4Net.kernel.api.exceptions.schema.SchemaRuleNotFoundException, org.Neo4Net.internal.kernel.api.exceptions.ProcedureException, org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTriggerResampling()
		 {
			  IndexDescriptor index = TestIndexDescriptorFactory.forLabel( 123, 456 );
			  when( _schemaRead.index( anyInt(), any() ) ).thenReturn(index);

			  _procedure.resampleIndex( ":Person(name)" );

			  verify( _indexingService ).triggerIndexSampling( index.Schema(), IndexSamplingMode.TRIGGER_REBUILD_ALL );
		 }
	}

}