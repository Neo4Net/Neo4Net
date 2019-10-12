using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Impl.Api.index
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using TokenRead = Org.Neo4j.@internal.Kernel.Api.TokenRead;
	using MisconfiguredIndexException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.MisconfiguredIndexException;
	using LabelSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using IndexAccessor = Org.Neo4j.Kernel.Api.Index.IndexAccessor;
	using Org.Neo4j.Kernel.Api.Index;
	using IndexPopulator = Org.Neo4j.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using IndexUpdater = Org.Neo4j.Kernel.Api.Index.IndexUpdater;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using Org.Neo4j.Kernel.extension;
	using IndexSamplingConfig = Org.Neo4j.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using CollectingIndexUpdater = Org.Neo4j.Kernel.Impl.Index.Schema.CollectingIndexUpdater;
	using StoreMigrationParticipant = Org.Neo4j.Kernel.impl.storemigration.StoreMigrationParticipant;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using IndexSample = Org.Neo4j.Storageengine.Api.schema.IndexSample;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;
	using Values = Org.Neo4j.Values.Storable.Values;

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
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.default_schema_provider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.SchemaIndexTestHelper.singleInstanceIndexProviderFactory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.TestIndexProviderDescriptor.PROVIDER_DESCRIPTOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.createIndex;

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
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.fs.EphemeralFileSystemRule fs = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public EphemeralFileSystemRule Fs = new EphemeralFileSystemRule();
		 private readonly IndexProvider _mockedIndexProvider = mock( typeof( IndexProvider ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.neo4j.kernel.extension.KernelExtensionFactory<?> mockedIndexProviderFactory = singleInstanceIndexProviderFactory("none", mockedIndexProvider);
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
//ORIGINAL LINE: @Before public void before() throws org.neo4j.internal.kernel.api.exceptions.schema.MisconfiguredIndexException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Before()
		 {
			  when( _mockedIndexProvider.ProviderDescriptor ).thenReturn( PROVIDER_DESCRIPTOR );
			  when( _mockedIndexProvider.storeMigrationParticipant( any( typeof( FileSystemAbstraction ) ), any( typeof( PageCache ) ) ) ).thenReturn( Org.Neo4j.Kernel.impl.storemigration.StoreMigrationParticipant_Fields.NotParticipating );
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

		 private class GatheringIndexWriter : Org.Neo4j.Kernel.Api.Index.IndexAccessor_Adapter, IndexPopulator
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Set<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> updatesCommitted = new java.util.HashSet<>();
			  internal readonly ISet<IndexEntryUpdate<object>> UpdatesCommitted = new HashSet<IndexEntryUpdate<object>>();
			  internal readonly IDictionary<object, ISet<long>> IndexSamples = new Dictionary<object, ISet<long>>();

			  public override void Create()
			  {
			  }

			  public override void Add<T1>( ICollection<T1> updates ) where T1 : Org.Neo4j.Kernel.Api.Index.IndexEntryUpdate<T1>
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
//ORIGINAL LINE: public org.neo4j.kernel.api.index.IndexUpdater newUpdater(final IndexUpdateMode mode)
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