using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.transaction.state
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using SchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptor;
	using SchemaUtil = Neo4Net.@internal.Kernel.Api.schema.SchemaUtil;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Neo4Net.Kernel.Api.Index;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DatabaseSchemaState = Neo4Net.Kernel.Impl.Api.DatabaseSchemaState;
	using Neo4Net.Kernel.Impl.Api.index;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using IndexingServiceFactory = Neo4Net.Kernel.Impl.Api.index.IndexingServiceFactory;
	using PropertyPhysicalToLogicalConverter = Neo4Net.Kernel.Impl.Api.index.PropertyPhysicalToLogicalConverter;
	using LockService = Neo4Net.Kernel.impl.locking.LockService;
	using JobSchedulerFactory = Neo4Net.Kernel.impl.scheduler.JobSchedulerFactory;
	using Loaders = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.Loaders;
	using PropertyCreator = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.PropertyCreator;
	using PropertyTraverser = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.PropertyTraverser;
	using CountsComputer = Neo4Net.Kernel.impl.store.CountsComputer;
	using InlineNodeLabels = Neo4Net.Kernel.impl.store.InlineNodeLabels;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using RelationshipStore = Neo4Net.Kernel.impl.store.RelationshipStore;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using PrimitiveRecord = Neo4Net.Kernel.impl.store.record.PrimitiveRecord;
	using PropertyRecord = Neo4Net.Kernel.impl.store.record.PropertyRecord;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using NeoStoreIndexStoreView = Neo4Net.Kernel.impl.transaction.state.storeview.NeoStoreIndexStoreView;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using PageCacheAndDependenciesRule = Neo4Net.Test.rule.PageCacheAndDependenciesRule;
	using Neo4Net.@unsafe.Batchinsert.@internal;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.collection.IsIterableContainingInAnyOrder.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexProvider.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.multiToken;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.Record.NO_LABELS_FIELD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.Record.NO_NEXT_PROPERTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.Record.NO_NEXT_RELATIONSHIP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.EntityType.NODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.EntityType.RELATIONSHIP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.IndexDescriptorFactory.forSchema;

	public class OnlineIndexUpdatesTest
	{
		 private const int ENTITY_TOKEN = 1;
		 private const int OTHER_ENTITY_TOKEN = 2;
		 private static readonly int[] _entityTokens = new int[] { ENTITY_TOKEN };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.PageCacheAndDependenciesRule storage = new org.neo4j.test.rule.PageCacheAndDependenciesRule();
		 public PageCacheAndDependenciesRule Storage = new PageCacheAndDependenciesRule();
		 private NodeStore _nodeStore;
		 private RelationshipStore _relationshipStore;
		 private IndexingService _indexingService;
		 private PropertyPhysicalToLogicalConverter _propertyPhysicalToLogicalConverter;
		 private NeoStores _neoStores;
		 private LifeSupport _life;
		 private PropertyCreator _propertyCreator;
		 private DirectRecordAccess<PropertyRecord, PrimitiveRecord> _recordAccess;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _life = new LifeSupport();
			  PageCache pageCache = Storage.pageCache();
			  DatabaseLayout databaseLayout = Storage.directory().databaseLayout();
			  Config config = Config.defaults( GraphDatabaseSettings.default_schema_provider, EMPTY.ProviderDescriptor.name() );
			  NullLogProvider nullLogProvider = NullLogProvider.Instance;
			  StoreFactory storeFactory = new StoreFactory( databaseLayout, config, new DefaultIdGeneratorFactory( Storage.fileSystem() ), pageCache, Storage.fileSystem(), nullLogProvider, EmptyVersionContextSupplier.EMPTY );

			  _neoStores = storeFactory.OpenAllNeoStores( true );
			  _neoStores.Counts.start();
			  CountsComputer.recomputeCounts( _neoStores, pageCache, databaseLayout );
			  _nodeStore = _neoStores.NodeStore;
			  _relationshipStore = _neoStores.RelationshipStore;
			  PropertyStore propertyStore = _neoStores.PropertyStore;
			  JobScheduler scheduler = JobSchedulerFactory.createScheduler();
			  Dependencies dependencies = new Dependencies();
			  dependencies.SatisfyDependency( EMPTY );
			  DefaultIndexProviderMap providerMap = new DefaultIndexProviderMap( dependencies, config );
			  _life.add( providerMap );
			  _indexingService = IndexingServiceFactory.createIndexingService( config, scheduler, providerMap, new NeoStoreIndexStoreView( LockService.NO_LOCK_SERVICE, _neoStores ), SchemaUtil.idTokenNameLookup, empty(), nullLogProvider, nullLogProvider, IndexingService.NO_MONITOR, new DatabaseSchemaState(nullLogProvider), false );
			  _propertyPhysicalToLogicalConverter = new PropertyPhysicalToLogicalConverter( _neoStores.PropertyStore );
			  _life.add( _indexingService );
			  _life.add( scheduler );
			  _life.init();
			  _life.start();
			  _propertyCreator = new PropertyCreator( _neoStores.PropertyStore, new PropertyTraverser() );
			  _recordAccess = new DirectRecordAccess<PropertyRecord, PrimitiveRecord>( _neoStores.PropertyStore, Loaders.propertyLoader( propertyStore ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _life.shutdown();
			  _neoStores.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldContainFedNodeUpdate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldContainFedNodeUpdate()
		 {
			  OnlineIndexUpdates onlineIndexUpdates = new OnlineIndexUpdates( _nodeStore, _relationshipStore, _indexingService, _propertyPhysicalToLogicalConverter );

			  int nodeId = 0;
			  NodeRecord inUse = GetNode( nodeId, true );
			  Value propertyValue = Values.of( "hej" );
			  long propertyId = CreateNodeProperty( inUse, propertyValue, 1 );
			  NodeRecord notInUse = GetNode( nodeId, false );
			  _nodeStore.updateRecord( inUse );

			  Command.NodeCommand nodeCommand = new Command.NodeCommand( inUse, notInUse );
			  PropertyRecord propertyBlocks = new PropertyRecord( propertyId );
			  propertyBlocks.NodeId = nodeId;
			  Command.PropertyCommand propertyCommand = new Command.PropertyCommand( _recordAccess.getIfLoaded( propertyId ).forReadingData(), propertyBlocks );

			  StoreIndexDescriptor indexDescriptor = forSchema( multiToken( _entityTokens, NODE, 1, 4, 6 ), EMPTY.ProviderDescriptor ).withId( 0 );
			  _indexingService.createIndexes( indexDescriptor );
			  _indexingService.getIndexProxy( indexDescriptor.Schema() ).awaitStoreScanCompleted(0, MILLISECONDS);

			  onlineIndexUpdates.Feed( NodeGroup( nodeCommand, propertyCommand ), RelationshipGroup( null ) );
			  assertTrue( onlineIndexUpdates.HasUpdates() );
			  IEnumerator<IndexEntryUpdate<SchemaDescriptor>> iterator = onlineIndexUpdates.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( iterator.next(), IndexEntryUpdate.remove(nodeId, indexDescriptor, propertyValue, null, null) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( iterator.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldContainFedRelationshipUpdate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldContainFedRelationshipUpdate()
		 {
			  OnlineIndexUpdates onlineIndexUpdates = new OnlineIndexUpdates( _nodeStore, _relationshipStore, _indexingService, _propertyPhysicalToLogicalConverter );

			  long relId = 0;
			  RelationshipRecord inUse = GetRelationship( relId, true, ENTITY_TOKEN );
			  Value propertyValue = Values.of( "hej" );
			  long propertyId = CreateRelationshipProperty( inUse, propertyValue, 1 );
			  RelationshipRecord notInUse = GetRelationship( relId, false, ENTITY_TOKEN );
			  _relationshipStore.updateRecord( inUse );

			  Command.RelationshipCommand relationshipCommand = new Command.RelationshipCommand( inUse, notInUse );
			  PropertyRecord propertyBlocks = new PropertyRecord( propertyId );
			  propertyBlocks.RelId = relId;
			  Command.PropertyCommand propertyCommand = new Command.PropertyCommand( _recordAccess.getIfLoaded( propertyId ).forReadingData(), propertyBlocks );

			  StoreIndexDescriptor indexDescriptor = forSchema( multiToken( _entityTokens, RELATIONSHIP, 1, 4, 6 ), EMPTY.ProviderDescriptor ).withId( 0 );
			  _indexingService.createIndexes( indexDescriptor );
			  _indexingService.getIndexProxy( indexDescriptor.Schema() ).awaitStoreScanCompleted(0, MILLISECONDS);

			  onlineIndexUpdates.Feed( NodeGroup( null ), RelationshipGroup( relationshipCommand, propertyCommand ) );
			  assertTrue( onlineIndexUpdates.HasUpdates() );
			  IEnumerator<IndexEntryUpdate<SchemaDescriptor>> iterator = onlineIndexUpdates.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( iterator.next(), IndexEntryUpdate.remove(relId, indexDescriptor, propertyValue, null, null) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( iterator.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDifferentiateNodesAndRelationships() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDifferentiateNodesAndRelationships()
		 {
			  OnlineIndexUpdates onlineIndexUpdates = new OnlineIndexUpdates( _nodeStore, _relationshipStore, _indexingService, _propertyPhysicalToLogicalConverter );

			  int nodeId = 0;
			  NodeRecord inUseNode = GetNode( nodeId, true );
			  Value nodePropertyValue = Values.of( "hej" );
			  long nodePropertyId = CreateNodeProperty( inUseNode, nodePropertyValue, 1 );
			  NodeRecord notInUseNode = GetNode( nodeId, false );
			  _nodeStore.updateRecord( inUseNode );

			  Command.NodeCommand nodeCommand = new Command.NodeCommand( inUseNode, notInUseNode );
			  PropertyRecord nodePropertyBlocks = new PropertyRecord( nodePropertyId );
			  nodePropertyBlocks.NodeId = nodeId;
			  Command.PropertyCommand nodePropertyCommand = new Command.PropertyCommand( _recordAccess.getIfLoaded( nodePropertyId ).forReadingData(), nodePropertyBlocks );

			  StoreIndexDescriptor nodeIndexDescriptor = forSchema( multiToken( _entityTokens, NODE, 1, 4, 6 ), EMPTY.ProviderDescriptor ).withId( 0 );
			  _indexingService.createIndexes( nodeIndexDescriptor );
			  _indexingService.getIndexProxy( nodeIndexDescriptor.Schema() ).awaitStoreScanCompleted(0, MILLISECONDS);

			  long relId = 0;
			  RelationshipRecord inUse = GetRelationship( relId, true, ENTITY_TOKEN );
			  Value relationshipPropertyValue = Values.of( "da" );
			  long propertyId = CreateRelationshipProperty( inUse, relationshipPropertyValue, 1 );
			  RelationshipRecord notInUse = GetRelationship( relId, false, ENTITY_TOKEN );
			  _relationshipStore.updateRecord( inUse );

			  Command.RelationshipCommand relationshipCommand = new Command.RelationshipCommand( inUse, notInUse );
			  PropertyRecord relationshipPropertyBlocks = new PropertyRecord( propertyId );
			  relationshipPropertyBlocks.RelId = relId;
			  Command.PropertyCommand relationshipPropertyCommand = new Command.PropertyCommand( _recordAccess.getIfLoaded( propertyId ).forReadingData(), relationshipPropertyBlocks );

			  StoreIndexDescriptor relationshipIndexDescriptor = forSchema( multiToken( _entityTokens, RELATIONSHIP, 1, 4, 6 ), EMPTY.ProviderDescriptor ).withId( 1 );
			  _indexingService.createIndexes( relationshipIndexDescriptor );
			  _indexingService.getIndexProxy( relationshipIndexDescriptor.Schema() ).awaitStoreScanCompleted(0, MILLISECONDS);

			  onlineIndexUpdates.Feed( NodeGroup( nodeCommand, nodePropertyCommand ), RelationshipGroup( relationshipCommand, relationshipPropertyCommand ) );
			  assertTrue( onlineIndexUpdates.HasUpdates() );
			  assertThat( onlineIndexUpdates, containsInAnyOrder( IndexEntryUpdate.remove( relId, relationshipIndexDescriptor, relationshipPropertyValue, null, null ), IndexEntryUpdate.remove( nodeId, nodeIndexDescriptor, nodePropertyValue, null, null ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateCorrectIndexes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdateCorrectIndexes()
		 {
			  OnlineIndexUpdates onlineIndexUpdates = new OnlineIndexUpdates( _nodeStore, _relationshipStore, _indexingService, _propertyPhysicalToLogicalConverter );

			  long relId = 0;
			  RelationshipRecord inUse = GetRelationship( relId, true, ENTITY_TOKEN );
			  Value propertyValue = Values.of( "hej" );
			  Value propertyValue2 = Values.of( "da" );
			  long propertyId = CreateRelationshipProperty( inUse, propertyValue, 1 );
			  long propertyId2 = CreateRelationshipProperty( inUse, propertyValue2, 4 );
			  RelationshipRecord notInUse = GetRelationship( relId, false, ENTITY_TOKEN );
			  _relationshipStore.updateRecord( inUse );

			  Command.RelationshipCommand relationshipCommand = new Command.RelationshipCommand( inUse, notInUse );
			  PropertyRecord propertyBlocks = new PropertyRecord( propertyId );
			  propertyBlocks.RelId = relId;
			  Command.PropertyCommand propertyCommand = new Command.PropertyCommand( _recordAccess.getIfLoaded( propertyId ).forReadingData(), propertyBlocks );

			  PropertyRecord propertyBlocks2 = new PropertyRecord( propertyId2 );
			  propertyBlocks2.RelId = relId;
			  Command.PropertyCommand propertyCommand2 = new Command.PropertyCommand( _recordAccess.getIfLoaded( propertyId2 ).forReadingData(), propertyBlocks2 );

			  StoreIndexDescriptor indexDescriptor0 = forSchema( multiToken( _entityTokens, RELATIONSHIP, 1, 4, 6 ), EMPTY.ProviderDescriptor ).withId( 0 );
			  StoreIndexDescriptor indexDescriptor1 = forSchema( multiToken( _entityTokens, RELATIONSHIP, 2, 4, 6 ), EMPTY.ProviderDescriptor ).withId( 1 );
			  StoreIndexDescriptor indexDescriptor2 = forSchema( multiToken( new int[]{ ENTITY_TOKEN, OTHER_ENTITY_TOKEN }, RELATIONSHIP, 1 ), EMPTY.ProviderDescriptor ).withId( 2 );
			  StoreIndexDescriptor indexDescriptor3 = forSchema( multiToken( new int[]{ OTHER_ENTITY_TOKEN }, RELATIONSHIP, 1 ), EMPTY.ProviderDescriptor ).withId( 3 );
			  _indexingService.createIndexes( indexDescriptor0, indexDescriptor1, indexDescriptor2 );
			  _indexingService.getIndexProxy( indexDescriptor0.Schema() ).awaitStoreScanCompleted(0, MILLISECONDS);
			  _indexingService.getIndexProxy( indexDescriptor1.Schema() ).awaitStoreScanCompleted(0, MILLISECONDS);
			  _indexingService.getIndexProxy( indexDescriptor2.Schema() ).awaitStoreScanCompleted(0, MILLISECONDS);

			  onlineIndexUpdates.Feed( NodeGroup( null ), RelationshipGroup( relationshipCommand, propertyCommand, propertyCommand2 ) );
			  assertTrue( onlineIndexUpdates.HasUpdates() );
			  assertThat( onlineIndexUpdates, containsInAnyOrder( IndexEntryUpdate.remove( relId, indexDescriptor0, propertyValue, propertyValue2, null ), IndexEntryUpdate.remove( relId, indexDescriptor1, null, propertyValue2, null ), IndexEntryUpdate.remove( relId, indexDescriptor2, propertyValue ) ) );
			  assertThat( onlineIndexUpdates, not( containsInAnyOrder( indexDescriptor3 ) ) ); // This index is only for a different relationship type.
		 }

		 private EntityCommandGrouper<Command.NodeCommand>.Cursor NodeGroup( Command.NodeCommand nodeCommand, params Command.PropertyCommand[] propertyCommands )
		 {
			  return Group( nodeCommand, typeof( Command.NodeCommand ), propertyCommands );
		 }

		 private EntityCommandGrouper<Command.RelationshipCommand>.Cursor RelationshipGroup( Command.RelationshipCommand relationshipCommand, params Command.PropertyCommand[] propertyCommands )
		 {
			  return Group( relationshipCommand, typeof( Command.RelationshipCommand ), propertyCommands );
		 }

		 private EntityCommandGrouper<ENTITY>.Cursor Group<ENTITY>( ENTITY entityCommand, Type cls, params Command.PropertyCommand[] propertyCommands ) where ENTITY : Neo4Net.Kernel.impl.transaction.command.Command
		 {
				 cls = typeof( ENTITY );
			  EntityCommandGrouper<ENTITY> grouper = new EntityCommandGrouper<ENTITY>( cls, 8 );
			  if ( entityCommand != null )
			  {
					grouper.Add( entityCommand );
			  }
			  foreach ( Command.PropertyCommand propertyCommand in propertyCommands )
			  {
					grouper.Add( propertyCommand );
			  }
			  return grouper.SortAndAccessGroups();
		 }

		 private long CreateRelationshipProperty( RelationshipRecord relRecord, Value propertyValue, int propertyKey )
		 {
			  return _propertyCreator.createPropertyChain( relRecord, singletonList( _propertyCreator.encodePropertyValue( propertyKey, propertyValue ) ).GetEnumerator(), _recordAccess );
		 }

		 private long CreateNodeProperty( NodeRecord inUse, Value value, int propertyKey )
		 {
			  return _propertyCreator.createPropertyChain( inUse, singletonList( _propertyCreator.encodePropertyValue( propertyKey, value ) ).GetEnumerator(), _recordAccess );
		 }

		 private NodeRecord GetNode( int nodeId, bool inUse )
		 {
			  NodeRecord nodeRecord = new NodeRecord( nodeId );
			  nodeRecord = nodeRecord.Initialize( inUse, NO_NEXT_PROPERTY.longValue(), false, NO_NEXT_RELATIONSHIP.longValue(), NO_LABELS_FIELD.longValue() );
			  if ( inUse )
			  {
					InlineNodeLabels labelFieldWriter = new InlineNodeLabels( nodeRecord );
					labelFieldWriter.Put( new long[]{ ENTITY_TOKEN }, null, null );
			  }
			  return nodeRecord;
		 }

		 private RelationshipRecord GetRelationship( long relId, bool inUse, int type )
		 {
			  if ( !inUse )
			  {
					type = -1;
			  }
			  return ( new RelationshipRecord( relId ) ).initialize( inUse, NO_NEXT_PROPERTY.longValue(), 0, 0, type, NO_NEXT_RELATIONSHIP.longValue(), NO_NEXT_RELATIONSHIP.longValue(), NO_NEXT_RELATIONSHIP.longValue(), NO_NEXT_RELATIONSHIP.longValue(), true, false );
		 }
	}

}