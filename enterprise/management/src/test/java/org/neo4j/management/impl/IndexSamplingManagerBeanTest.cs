/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.management.impl
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using IndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using NeoStoreDataSource = Org.Neo4j.Kernel.NeoStoreDataSource;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using IndexingService = Org.Neo4j.Kernel.Impl.Api.index.IndexingService;
	using IndexSamplingMode = Org.Neo4j.Kernel.Impl.Api.index.sampling.IndexSamplingMode;
	using TokenHolder = Org.Neo4j.Kernel.impl.core.TokenHolder;
	using TokenHolders = Org.Neo4j.Kernel.impl.core.TokenHolders;
	using StorageEngine = Org.Neo4j.Storageengine.Api.StorageEngine;
	using StorageReader = Org.Neo4j.Storageengine.Api.StorageReader;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.TokenRead_Fields.NO_TOKEN;

	public class IndexSamplingManagerBeanTest
	{
		 private const string EXISTING_LABEL = "label";
		 private const string NON_EXISTING_LABEL = "bogusLabel";
		 private const int LABEL_ID = 42;
		 private const string EXISTING_PROPERTY = "prop";
		 private const string NON_EXISTING_PROPERTY = "bogusProp";
		 private const int PROPERTY_ID = 43;

		 private NeoStoreDataSource _dataSource;
		 private IndexingService _indexingService;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _dataSource = mock( typeof( NeoStoreDataSource ) );
			  StorageEngine storageEngine = mock( typeof( StorageEngine ) );
			  StorageReader storageReader = mock( typeof( StorageReader ) );
			  when( storageEngine.NewReader() ).thenReturn(storageReader);
			  _indexingService = mock( typeof( IndexingService ) );
			  TokenHolders tokenHolders = MockedTokenHolders();
			  when( tokenHolders.LabelTokens().getIdByName(EXISTING_LABEL) ).thenReturn(LABEL_ID);
			  when( tokenHolders.PropertyKeyTokens().getIdByName(EXISTING_PROPERTY) ).thenReturn(PROPERTY_ID);
			  when( tokenHolders.PropertyKeyTokens().getIdByName(NON_EXISTING_PROPERTY) ).thenReturn(-1);
			  when( tokenHolders.LabelTokens().getIdByName(NON_EXISTING_LABEL) ).thenReturn(NO_TOKEN);
			  DependencyResolver resolver = mock( typeof( DependencyResolver ) );
			  when( resolver.ResolveDependency( typeof( IndexingService ) ) ).thenReturn( _indexingService );
			  when( resolver.ResolveDependency( typeof( StorageEngine ) ) ).thenReturn( storageEngine );
			  when( resolver.ResolveDependency( typeof( TokenHolders ) ) ).thenReturn( tokenHolders );
			  when( _dataSource.DependencyResolver ).thenReturn( resolver );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void samplingTriggeredWhenIdsArePresent() throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SamplingTriggeredWhenIdsArePresent()
		 {
			  // Given
			  IndexSamplingManagerBean.StoreAccess storeAccess = new IndexSamplingManagerBean.StoreAccess();
			  storeAccess.Registered( _dataSource );

			  // When
			  storeAccess.TriggerIndexSampling( EXISTING_LABEL, EXISTING_PROPERTY, false );

			  // Then
			  verify( _indexingService, times( 1 ) ).triggerIndexSampling( SchemaDescriptorFactory.forLabel( LABEL_ID, PROPERTY_ID ), IndexSamplingMode.TRIGGER_REBUILD_UPDATED );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void forceSamplingTriggeredWhenIdsArePresent() throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ForceSamplingTriggeredWhenIdsArePresent()
		 {
			  // Given
			  IndexSamplingManagerBean.StoreAccess storeAccess = new IndexSamplingManagerBean.StoreAccess();
			  storeAccess.Registered( _dataSource );

			  // When
			  storeAccess.TriggerIndexSampling( EXISTING_LABEL, EXISTING_PROPERTY, true );

			  // Then
			  verify( _indexingService, times( 1 ) ).triggerIndexSampling( SchemaDescriptorFactory.forLabel( LABEL_ID, PROPERTY_ID ), IndexSamplingMode.TRIGGER_REBUILD_ALL );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void exceptionThrownWhenMissingLabel()
		 public virtual void ExceptionThrownWhenMissingLabel()
		 {
			  // Given
			  IndexSamplingManagerBean.StoreAccess storeAccess = new IndexSamplingManagerBean.StoreAccess();
			  storeAccess.Registered( _dataSource );

			  // When
			  storeAccess.TriggerIndexSampling( NON_EXISTING_LABEL, EXISTING_PROPERTY, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void exceptionThrownWhenMissingProperty()
		 public virtual void ExceptionThrownWhenMissingProperty()
		 {
			  // Given
			  IndexSamplingManagerBean.StoreAccess storeAccess = new IndexSamplingManagerBean.StoreAccess();
			  storeAccess.Registered( _dataSource );

			  // When
			  storeAccess.TriggerIndexSampling( EXISTING_LABEL, NON_EXISTING_PROPERTY, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void exceptionThrownWhenNotRegistered()
		 public virtual void ExceptionThrownWhenNotRegistered()
		 {
			  // Given
			  IndexSamplingManagerBean.StoreAccess storeAccess = new IndexSamplingManagerBean.StoreAccess();

			  // When
			  storeAccess.TriggerIndexSampling( EXISTING_LABEL, EXISTING_PROPERTY, false );
		 }

		 private static TokenHolders MockedTokenHolders()
		 {
			  return new TokenHolders( mock( typeof( TokenHolder ) ), mock( typeof( TokenHolder ) ), mock( typeof( TokenHolder ) ) );
		 }
	}

}