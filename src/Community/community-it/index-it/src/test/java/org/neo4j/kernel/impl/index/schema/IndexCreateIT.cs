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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using Test = org.junit.Test;

	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using SchemaWrite = Neo4Net.@internal.Kernel.Api.SchemaWrite;
	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using SchemaKernelException = Neo4Net.@internal.Kernel.Api.exceptions.schema.SchemaKernelException;
	using LabelSchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using RepeatedLabelInSchemaException = Neo4Net.Kernel.Api.Exceptions.schema.RepeatedLabelInSchemaException;
	using RepeatedPropertyInSchemaException = Neo4Net.Kernel.Api.Exceptions.schema.RepeatedPropertyInSchemaException;
	using RepeatedRelationshipTypeInSchemaException = Neo4Net.Kernel.Api.Exceptions.schema.RepeatedRelationshipTypeInSchemaException;
	using MultiTokenSchemaDescriptor = Neo4Net.Kernel.api.schema.MultiTokenSchemaDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using IndexProviderNotFoundException = Neo4Net.Kernel.Impl.Api.index.IndexProviderNotFoundException;
	using KernelIntegrationTest = Neo4Net.Kernel.Impl.Api.integrationtest.KernelIntegrationTest;
	using EntityType = Neo4Net.Storageengine.Api.EntityType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel;

	public class IndexCreateIT : KernelIntegrationTest
	{

		 private static readonly IndexCreator _indexCreator = ( schemaWrite, descriptor, providerName ) => schemaWrite.indexCreate( descriptor, providerName, null );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
		 private static readonly IndexCreator _uniqueConstraintCreator = SchemaWrite::uniquePropertyConstraintCreate;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateIndexWithSpecificExistingProviderName() throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateIndexWithSpecificExistingProviderName()
		 {
			  ShouldCreateWithSpecificExistingProviderName( _indexCreator );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateUniquePropertyConstraintWithSpecificExistingProviderName() throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateUniquePropertyConstraintWithSpecificExistingProviderName()
		 {
			  ShouldCreateWithSpecificExistingProviderName( _uniqueConstraintCreator );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailCreateIndexWithNonExistentProviderName() throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailCreateIndexWithNonExistentProviderName()
		 {
			  ShouldFailWithNonExistentProviderName( _indexCreator );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailCreateUniquePropertyConstraintWithNonExistentProviderName() throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailCreateUniquePropertyConstraintWithNonExistentProviderName()
		 {
			  ShouldFailWithNonExistentProviderName( _uniqueConstraintCreator );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBePossibleToCreateIndexWithDuplicateLabel() throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBePossibleToCreateIndexWithDuplicateLabel()
		 {
			  // given
			  SchemaWrite schemaWrite = SchemaWriteInNewTransaction();

			  // when
			  try
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.api.schema.MultiTokenSchemaDescriptor descriptor = org.neo4j.kernel.api.schema.SchemaDescriptorFactory.multiToken(new int[]{0, 0}, org.neo4j.storageengine.api.EntityType.NODE, 1);
					MultiTokenSchemaDescriptor descriptor = SchemaDescriptorFactory.multiToken( new int[]{ 0, 0 }, EntityType.NODE, 1 );
					schemaWrite.IndexCreate( descriptor );
					fail( "Should have failed" );
			  }
			  catch ( RepeatedLabelInSchemaException )
			  {
					// then good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBePossibleToCreateIndexWithDuplicateRelationshipTypes() throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBePossibleToCreateIndexWithDuplicateRelationshipTypes()
		 {
			  // given
			  SchemaWrite schemaWrite = SchemaWriteInNewTransaction();

			  // when
			  try
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.api.schema.MultiTokenSchemaDescriptor descriptor = org.neo4j.kernel.api.schema.SchemaDescriptorFactory.multiToken(new int[]{0, 0}, org.neo4j.storageengine.api.EntityType.RELATIONSHIP, 1);
					MultiTokenSchemaDescriptor descriptor = SchemaDescriptorFactory.multiToken( new int[]{ 0, 0 }, EntityType.RELATIONSHIP, 1 );
					schemaWrite.IndexCreate( descriptor );
					fail( "Should have failed" );
			  }
			  catch ( RepeatedRelationshipTypeInSchemaException )
			  {
					// then good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBePossibleToCreateIndexWithDuplicateProperties() throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBePossibleToCreateIndexWithDuplicateProperties()
		 {
			  // given
			  SchemaWrite schemaWrite = SchemaWriteInNewTransaction();

			  // when
			  try
			  {
					LabelSchemaDescriptor descriptor = forLabel( 0, 1, 1 );
					schemaWrite.IndexCreate( descriptor );
					fail( "Should have failed" );
			  }
			  catch ( RepeatedPropertyInSchemaException )
			  {
					// then good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBePossibleToCreateConstraintWithDuplicateProperties() throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBePossibleToCreateConstraintWithDuplicateProperties()
		 {
			  // given
			  SchemaWrite schemaWrite = SchemaWriteInNewTransaction();

			  // when
			  try
			  {
					LabelSchemaDescriptor descriptor = forLabel( 0, 1, 1 );
					schemaWrite.UniquePropertyConstraintCreate( descriptor );
					fail( "Should have failed" );
			  }
			  catch ( RepeatedPropertyInSchemaException )
			  {
					// then good
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void shouldFailWithNonExistentProviderName(IndexCreator creator) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 internal virtual void ShouldFailWithNonExistentProviderName( IndexCreator creator )
		 {
			  // given
			  SchemaWrite schemaWrite = SchemaWriteInNewTransaction();

			  // when
			  try
			  {
					creator.Create( schemaWrite, forLabel( 0, 0 ), "something-completely-different" );
					fail( "Should have failed" );
			  }
			  catch ( IndexProviderNotFoundException )
			  {
					// then good
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void shouldCreateWithSpecificExistingProviderName(IndexCreator creator) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 internal virtual void ShouldCreateWithSpecificExistingProviderName( IndexCreator creator )
		 {
			  int labelId = 0;
			  foreach ( GraphDatabaseSettings.SchemaIndex indexSetting in GraphDatabaseSettings.SchemaIndex.values() )
			  {
					// given
					SchemaWrite schemaWrite = SchemaWriteInNewTransaction();
					string provider = indexSetting.providerName();
					LabelSchemaDescriptor descriptor = forLabel( labelId++, 0 );
					creator.Create( schemaWrite, descriptor, provider );

					// when
					Commit();

					// then
					assertEquals( provider, IndexingService.getIndexProxy( descriptor ).Descriptor.providerDescriptor().name() );
			  }
		 }

		 internal interface IndexCreator
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void create(org.neo4j.internal.kernel.api.SchemaWrite schemaWrite, org.neo4j.internal.kernel.api.schema.LabelSchemaDescriptor descriptor, String providerName) throws org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException;
			  void Create( SchemaWrite schemaWrite, LabelSchemaDescriptor descriptor, string providerName );
		 }
	}

}