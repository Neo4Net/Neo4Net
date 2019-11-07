﻿using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using TokenRead = Neo4Net.Kernel.Api.Internal.TokenRead;
	using MisconfiguredIndexException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.MisconfiguredIndexException;
	using LabelSchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.LabelSchemaDescriptor;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using KernelTransaction = Neo4Net.Kernel.Api.KernelTransaction;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using Neo4Net.Kernel.Api.Index;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using NodePropertyAccessor = Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.Api.schema.SchemaDescriptorFactory;
	using Neo4Net.Kernel.extension;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using CollectingIndexUpdater = Neo4Net.Kernel.Impl.Index.Schema.CollectingIndexUpdater;
	using StoreMigrationParticipant = Neo4Net.Kernel.impl.storemigration.StoreMigrationParticipant;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using IndexSample = Neo4Net.Kernel.Api.StorageEngine.schema.IndexSample;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.default_schema_provider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.api.index.SchemaIndexTestHelper.singleInstanceIndexProviderFactory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.api.index.TestIndexProviderDescriptor.PROVIDER_DESCRIPTOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.mockito.matcher.Neo4NetMatchers.createIndex;

	public class IndexCRUDIT
	{
		private bool InstanceFieldsInitialized = false;

		public IndexCRUDIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_mockedIndexProviderFactory = singleInstanceIndexProviderFactory( "none", _mockedIndexProvider );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addingANodeWithPropertyShouldGetIndexed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AddingANodeWithPropertyShouldGetIndexed()
		 {
			  // Given
			  string indexProperty = "indexProperty";
			  GatheringIndexWriter writer = NewWriter();
			  createIndex( _db, _myLabel, indexProperty );

			  // When
			  int value1 = 12;
			  string otherProperty = "otherProperty";
			  int otherValue = 17;
			  Node node = CreateNode( map( indexProperty, value1, otherProperty, otherValue ), _myLabel );

			  // Then, for now, this should trigger two NodePropertyUpdates
			  using ( Transaction tx = _db.beginTx() )
			  {
					KernelTransaction ktx = _ctxSupplier.getKernelTransactionBoundToThisThread( true );
					TokenRead tokenRead = ktx.TokenRead();
					int propertyKey1 = tokenRead.PropertyKey( indexProperty );
					int label = tokenRead.NodeLabel( _myLabel.name() );
					LabelSchemaDescriptor descriptor = SchemaDescriptorFactory.forLabel( label, propertyKey1 );
					assertThat( writer.UpdatesCommitted, equalTo( asSet( IndexEntryUpdate.add( node.Id, descriptor, Values.of( value1 ) ) ) ) );
					tx.Success();
			  }
			  // We get two updates because we both add a label and a property to be indexed
			  // in the same transaction, in the future, we should optimize this down to
			  // one NodePropertyUpdate.
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addingALabelToPreExistingNodeShouldGetIndexed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AddingALabelToPreExistingNodeShouldGetIndexed()
		 {
			  // GIVEN
			  string indexProperty = "indexProperty";
			  GatheringIndexWriter writer = NewWriter();
			  createIndex( _db, _myLabel, indexProperty );

			  // WHEN
			  string otherProperty = "otherProperty";
			  int value = 12;
			  int otherValue = 17;
			  Node node = CreateNode( map( indexProperty, value, otherProperty, otherValue ) );

			  // THEN
			  assertThat( writer.UpdatesCommitted.Count, equalTo( 0 ) );

			  // AND WHEN
			  using ( Transaction tx = _db.beginTx() )
			  {
					node.AddLabel( _myLabel );
					tx.Success();
			  }

			  // THEN
			  using ( Transaction tx = _db.beginTx() )
			  {
					KernelTransaction ktx = _ctxSupplier.getKernelTransactionBoundToThisThread( true );
					TokenRead tokenRead = ktx.TokenRead();
					int propertyKey1 = tokenRead.PropertyKey( indexProperty );
					int label = tokenRead.NodeLabel( _myLabel.name() );
					LabelSchemaDescriptor descriptor = SchemaDescriptorFactory.forLabel( label, propertyKey1 );
					assertThat( writer.UpdatesCommitted, equalTo( asSet( IndexEntryUpdate.add( node.Id, descriptor, Values.of( value ) ) ) ) );
					tx.Success();
			  }
		 }

		 private GraphDatabaseAPI _db;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.fs.EphemeralFileSystemRule fs = new Neo4Net.test.rule.fs.EphemeralFileSystemRule();
		 public EphemeralFileSystemRule Fs = new EphemeralFileSystemRule();
		 private readonly IndexProvider _mockedIndexProvider = mock( typeof( IndexProvider ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final Neo4Net.kernel.extension.KernelExtensionFactory<?> mockedIndexProviderFactory = singleInstanceIndexProviderFactory("none", mockedIndexProvider);
		 private KernelExtensionFactory<object> _mockedIndexProviderFactory;
		 private ThreadToStatementContextBridge _ctxSupplier;
		 private readonly Label _myLabel = Label.label( "MYLABEL" );

		 private Node CreateNode( IDictionary<string, object> properties, params Label[] labels )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode( labels );
					foreach ( KeyValuePair<string, object> prop in properties.SetOfKeyValuePairs() )
					{
						 node.SetProperty( prop.Key, prop.Value );
					}
					tx.Success();
					return node;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before() throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.MisconfiguredIndexException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Before()
		 {
			  when( _mockedIndexProvider.ProviderDescriptor ).thenReturn( PROVIDER_DESCRIPTOR );
			  when( _mockedIndexProvider.storeMigrationParticipant( any( typeof( FileSystemAbstraction ) ), any( typeof( PageCache ) ) ) ).thenReturn( Neo4Net.Kernel.impl.storemigration.StoreMigrationParticipant_Fields.NotParticipating );
			  when( _mockedIndexProvider.bless( any( typeof( IndexDescriptor ) ) ) ).thenCallRealMethod();
			  TestGraphDatabaseFactory factory = new TestGraphDatabaseFactory();
			  factory.FileSystem = Fs.get();
			  factory.KernelExtensions = Collections.singletonList( _mockedIndexProviderFactory );
			  _db = ( GraphDatabaseAPI ) factory.NewImpermanentDatabaseBuilder().setConfig(default_schema_provider, PROVIDER_DESCRIPTOR.name()).newGraphDatabase();
			  _ctxSupplier = _db.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private GatheringIndexWriter newWriter() throws java.io.IOException
		 private GatheringIndexWriter NewWriter()
		 {
			  GatheringIndexWriter writer = new GatheringIndexWriter();
			  when( _mockedIndexProvider.getPopulator( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ), any() ) ).thenReturn(writer);
			  when( _mockedIndexProvider.getOnlineAccessor( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ) ) ).thenReturn( writer );
			  return writer;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  _db.shutdown();
		 }

		 private class GatheringIndexWriter : Neo4Net.Kernel.Api.Index.IndexAccessor_Adapter, IndexPopulator
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Set<Neo4Net.kernel.api.index.IndexEntryUpdate<?>> updatesCommitted = new java.util.HashSet<>();
			  internal readonly ISet<IndexEntryUpdate<object>> UpdatesCommitted = new HashSet<IndexEntryUpdate<object>>();
			  internal readonly IDictionary<object, ISet<long>> IndexSamples = new Dictionary<object, ISet<long>>();

			  public override void Create()
			  {
			  }

			  public override void Add<T1>( ICollection<T1> updates ) where T1 : Neo4Net.Kernel.Api.Index.IndexEntryUpdate<T1>
			  {
					UpdatesCommitted.addAll( updates );
			  }

			  public override void VerifyDeferredConstraints( NodePropertyAccessor nodePropertyAccessor )
			  {
			  }

			  public override IndexUpdater NewPopulatingUpdater( NodePropertyAccessor nodePropertyAccessor )
			  {
					return NewUpdater( IndexUpdateMode.Online );
			  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Neo4Net.kernel.api.index.IndexUpdater newUpdater(final IndexUpdateMode mode)
			  public override IndexUpdater NewUpdater( IndexUpdateMode mode )
			  {
					return new CollectingIndexUpdater( UpdatesCommitted.addAll );
			  }

			  public override void Close( bool populationCompletedSuccessfully )
			  {
			  }

			  public override void MarkAsFailed( string failure )
			  {
			  }

			  public override void IncludeSample<T1>( IndexEntryUpdate<T1> update )
			  {
					AddValueToSample( update.EntityId, update.Values()[0] );
			  }

			  public override IndexSample SampleResult()
			  {
					long indexSize = 0;
					foreach ( ISet<long> nodeIds in IndexSamples.Values )
					{
						 indexSize += nodeIds.Count;
					}
					return new IndexSample( indexSize, IndexSamples.Count, indexSize );
			  }

			  internal virtual void AddValueToSample( long nodeId, object propertyValue )
			  {
					ISet<long> nodeIds = IndexSamples.computeIfAbsent( propertyValue, k => new HashSet<long>() );
					nodeIds.Add( nodeId );
			  }
		 }
	}

}