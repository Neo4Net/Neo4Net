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
namespace Neo4Net.Kernel.impl.transaction.state.storeview
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;


	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using Neo4Net.Helpers.Collections;
	using TokenWrite = Neo4Net.Kernel.Api.Internal.TokenWrite;
	using IEntityNotFoundException = Neo4Net.Kernel.Api.Internal.Exceptions.EntityNotFoundException;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using LabelSchemaDescriptor = Neo4Net.Kernel.Api.Internal.schema.LabelSchemaDescriptor;
	using Neo4Net.Kernel.Api.Index;
	using NodeLabelUpdate = Neo4Net.Kernel.api.labelscan.NodeLabelUpdate;
	using RelationTypeSchemaDescriptor = Neo4Net.Kernel.api.schema.RelationTypeSchemaDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using IEntityUpdates = Neo4Net.Kernel.Impl.Api.index.EntityUpdates;
	using Neo4Net.Kernel.Impl.Api.index;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using Lock = Neo4Net.Kernel.impl.locking.Lock;
	using LockService = Neo4Net.Kernel.impl.locking.LockService;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using RecordStorageReader = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageReader;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using StorageNodeCursor = Neo4Net.Kernel.Api.StorageEngine.StorageNodeCursor;
	using StorageReader = Neo4Net.Kernel.Api.StorageEngine.StorageReader;
	using StorageRelationshipScanCursor = Neo4Net.Kernel.Api.StorageEngine.StorageRelationshipScanCursor;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;

	public class NeoStoreIndexStoreViewTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.EmbeddedDatabaseRule dbRule = new org.Neo4Net.test.rule.EmbeddedDatabaseRule();
		 public EmbeddedDatabaseRule DbRule = new EmbeddedDatabaseRule();

		 private readonly IDictionary<long, Lock> _lockMocks = new Dictionary<long, Lock>();
		 private readonly Label _label = Label.label( "Person" );
		 private readonly RelationshipType _relationshipType = RelationshipType.withName( "Knows" );

		 private GraphDatabaseAPI _graphDb;
		 private NeoStoreIndexStoreView _storeView;

		 private int _labelId;
		 private int _relTypeId;
		 private int _propertyKeyId;
		 private int _relPropertyKeyId;

		 private Node _alistair;
		 private Node _stefan;
		 private LockService _locks;
		 private NeoStores _neoStores;
		 private Relationship _aKnowsS;
		 private Relationship _sKnowsA;
		 private StorageReader _reader;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Before()
		 {
			  _graphDb = DbRule.GraphDatabaseAPI;

			  CreateAlistairAndStefanNodes();
			  OrCreateIds;

			  _neoStores = _graphDb.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores();

			  _locks = mock( typeof( LockService ) );
			  when( _locks.acquireNodeLock( anyLong(), any() ) ).thenAnswer(invocation =>
			  {
						  long? nodeId = invocation.getArgument( 0 );
						  return _lockMocks.computeIfAbsent( nodeId, k => mock( typeof( Lock ) ) );
			  });
			  when( _locks.acquireRelationshipLock( anyLong(), any() ) ).thenAnswer(invocation =>
			  {
				long? nodeId = invocation.getArgument( 0 );
				return _lockMocks.computeIfAbsent( nodeId, k => mock( typeof( Lock ) ) );
			  });
			  _storeView = new NeoStoreIndexStoreView( _locks, _neoStores );
			  _reader = new RecordStorageReader( _neoStores );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  _reader.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldScanExistingNodesForALabel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldScanExistingNodesForALabel()
		 {
			  // given
			  IEntityUpdateCollectingVisitor visitor = new IEntityUpdateCollectingVisitor( this );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.Neo4Net.helpers.collection.Visitor<org.Neo4Net.kernel.api.labelscan.NodeLabelUpdate,Exception> labelVisitor = mock(org.Neo4Net.helpers.collection.Visitor.class);
			  Visitor<NodeLabelUpdate, Exception> labelVisitor = mock( typeof( Visitor ) );
			  StoreScan<Exception> storeScan = _storeView.visitNodes( new int[]{ _labelId }, id => id == _propertyKeyId, visitor, labelVisitor, false );

			  // when
			  storeScan.Run();

			  // then
			  assertEquals( asSet( Add( _alistair.Id, _propertyKeyId, "Alistair", new long[] { _labelId } ), Add( _stefan.Id, _propertyKeyId, "Stefan", new long[] { _labelId } ) ), visitor.Updates );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldScanExistingRelationshipsForARelationshiptype() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldScanExistingRelationshipsForARelationshiptype()
		 {
			  // given
			  IEntityUpdateCollectingVisitor visitor = new IEntityUpdateCollectingVisitor( this );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.Neo4Net.kernel.impl.api.index.StoreScan<Exception> storeScan = storeView.visitRelationships(new int[]{relTypeId}, id -> id == relPropertyKeyId, visitor);
			  StoreScan<Exception> storeScan = _storeView.visitRelationships( new int[]{ _relTypeId }, id => id == _relPropertyKeyId, visitor );

			  // when
			  storeScan.Run();

			  // then
			  assertEquals( asSet( Add( _aKnowsS.Id, _relPropertyKeyId, "long", new long[]{ _relTypeId } ), Add( _sKnowsA.Id, _relPropertyKeyId, "lengthy", new long[]{ _relTypeId } ) ), visitor.Updates );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreDeletedNodesDuringScan() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreDeletedNodesDuringScan()
		 {
			  // given
			  DeleteAlistairAndStefanNodes();

			  IEntityUpdateCollectingVisitor visitor = new IEntityUpdateCollectingVisitor( this );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.Neo4Net.helpers.collection.Visitor<org.Neo4Net.kernel.api.labelscan.NodeLabelUpdate,Exception> labelVisitor = mock(org.Neo4Net.helpers.collection.Visitor.class);
			  Visitor<NodeLabelUpdate, Exception> labelVisitor = mock( typeof( Visitor ) );
			  StoreScan<Exception> storeScan = _storeView.visitNodes( new int[]{ _labelId }, id => id == _propertyKeyId, visitor, labelVisitor, false );

			  // when
			  storeScan.Run();

			  // then
			  assertEquals( emptySet(), visitor.Updates );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreDeletedRelationshipsDuringScan() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreDeletedRelationshipsDuringScan()
		 {
			  // given
			  DeleteAlistairAndStefanNodes();

			  IEntityUpdateCollectingVisitor visitor = new IEntityUpdateCollectingVisitor( this );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.Neo4Net.kernel.impl.api.index.StoreScan<Exception> storeScan = storeView.visitRelationships(new int[]{relTypeId}, id -> id == relPropertyKeyId, visitor);
			  StoreScan<Exception> storeScan = _storeView.visitRelationships( new int[]{ _relTypeId }, id => id == _relPropertyKeyId, visitor );

			  // when
			  storeScan.Run();

			  // then
			  assertEquals( emptySet(), visitor.Updates );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLockNodesWhileReadingThem() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLockNodesWhileReadingThem()
		 {
			  // given
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.Neo4Net.helpers.collection.Visitor<org.Neo4Net.kernel.impl.api.index.EntityUpdates,Exception> visitor = mock(org.Neo4Net.helpers.collection.Visitor.class);
			  Visitor<EntityUpdates, Exception> visitor = mock( typeof( Visitor ) );
			  StoreScan<Exception> storeScan = _storeView.visitNodes( new int[]{ _labelId }, id => id == _propertyKeyId, visitor, null, false );

			  // when
			  storeScan.Run();

			  // then
			  assertThat( "allocated locks: " + _lockMocks.Keys, _lockMocks.Count, greaterThanOrEqualTo( 2 ) );
			  Lock lock0 = _lockMocks[0L];
			  Lock lock1 = _lockMocks[1L];
			  assertNotNull( "Lock[node=0] never acquired", lock0 );
			  assertNotNull( "Lock[node=1] never acquired", lock1 );
			  InOrder order = inOrder( _locks, lock0, lock1 );
			  order.verify( _locks ).acquireNodeLock( 0, Neo4Net.Kernel.impl.locking.LockService_LockType.ReadLock );
			  order.verify( lock0 ).release();
			  order.verify( _locks ).acquireNodeLock( 1, Neo4Net.Kernel.impl.locking.LockService_LockType.ReadLock );
			  order.verify( lock1 ).release();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLockRelationshipsWhileReadingThem() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLockRelationshipsWhileReadingThem()
		 {
			  // given
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.Neo4Net.helpers.collection.Visitor<org.Neo4Net.kernel.impl.api.index.EntityUpdates,Exception> visitor = mock(org.Neo4Net.helpers.collection.Visitor.class);
			  Visitor<EntityUpdates, Exception> visitor = mock( typeof( Visitor ) );
			  StoreScan<Exception> storeScan = _storeView.visitRelationships( new int[]{ _relTypeId }, id => id == _relPropertyKeyId, visitor );

			  // when
			  storeScan.Run();

			  // then
			  assertThat( "allocated locks: " + _lockMocks.Keys, _lockMocks.Count, greaterThanOrEqualTo( 2 ) );
			  Lock lock0 = _lockMocks[0L];
			  Lock lock1 = _lockMocks[1L];
			  assertNotNull( "Lock[relationship=0] never acquired", lock0 );
			  assertNotNull( "Lock[relationship=1] never acquired", lock1 );
			  InOrder order = inOrder( _locks, lock0, lock1 );
			  order.verify( _locks ).acquireRelationshipLock( 0, Neo4Net.Kernel.impl.locking.LockService_LockType.ReadLock );
			  order.verify( lock0 ).release();
			  order.verify( _locks ).acquireRelationshipLock( 1, Neo4Net.Kernel.impl.locking.LockService_LockType.ReadLock );
			  order.verify( lock1 ).release();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadProperties() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.EntityNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadProperties()
		 {
			  Value value = _storeView.getNodePropertyValue( _alistair.Id, _propertyKeyId );
			  assertTrue( value.Equals( Values.of( "Alistair" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void processAllNodeProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ProcessAllNodeProperties()
		 {
			  CopyUpdateVisitor propertyUpdateVisitor = new CopyUpdateVisitor();
			  StoreViewNodeStoreScan storeViewNodeStoreScan = new StoreViewNodeStoreScan( new RecordStorageReader( _neoStores ), _locks, null, propertyUpdateVisitor, new int[]{ _labelId }, id => true );

			  using ( StorageNodeCursor nodeCursor = _reader.allocateNodeCursor() )
			  {
					nodeCursor.Single( 1 );
					nodeCursor.Next();

					storeViewNodeStoreScan.process( nodeCursor );
			  }

			  IEntityUpdates propertyUpdates = propertyUpdateVisitor.PropertyUpdates;
			  assertNotNull( "Visitor should contain container with updates.", propertyUpdates );

			  LabelSchemaDescriptor index1 = SchemaDescriptorFactory.forLabel( 0, 0 );
			  LabelSchemaDescriptor index2 = SchemaDescriptorFactory.forLabel( 0, 1 );
			  LabelSchemaDescriptor index3 = SchemaDescriptorFactory.forLabel( 0, 0, 1 );
			  LabelSchemaDescriptor index4 = SchemaDescriptorFactory.forLabel( 1, 1 );
			  IList<LabelSchemaDescriptor> indexes = Arrays.asList( index1, index2, index3, index4 );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( Iterables.map( IndexEntryUpdate::indexKey, propertyUpdates.ForIndexKeys( indexes ) ), containsInAnyOrder( index1, index2, index3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void processAllRelationshipProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ProcessAllRelationshipProperties()
		 {
			  CreateAlistairAndStefanNodes();
			  CopyUpdateVisitor propertyUpdateVisitor = new CopyUpdateVisitor();
			  RelationshipStoreScan relationshipStoreScan = new RelationshipStoreScan( new RecordStorageReader( _neoStores ), _locks, propertyUpdateVisitor, new int[]{ _relTypeId }, id => true );

			  using ( StorageRelationshipScanCursor relationshipScanCursor = _reader.allocateRelationshipScanCursor() )
			  {
					relationshipScanCursor.Single( 1 );
					relationshipScanCursor.Next();

					relationshipStoreScan.process( relationshipScanCursor );
			  }

			  IEntityUpdates propertyUpdates = propertyUpdateVisitor.PropertyUpdates;
			  assertNotNull( "Visitor should contain container with updates.", propertyUpdates );

			  RelationTypeSchemaDescriptor index1 = SchemaDescriptorFactory.forRelType( 0, 2 );
			  RelationTypeSchemaDescriptor index2 = SchemaDescriptorFactory.forRelType( 0, 3 );
			  RelationTypeSchemaDescriptor index3 = SchemaDescriptorFactory.forRelType( 0, 2, 3 );
			  RelationTypeSchemaDescriptor index4 = SchemaDescriptorFactory.forRelType( 1, 3 );
			  IList<RelationTypeSchemaDescriptor> indexes = Arrays.asList( index1, index2, index3, index4 );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( Iterables.map( IndexEntryUpdate::indexKey, propertyUpdates.ForIndexKeys( indexes ) ), containsInAnyOrder( index1, index2, index3 ) );
		 }

		 internal virtual IEntityUpdates Add( long nodeId, int propertyKeyId, object value, long[] labels )
		 {
			  return IEntityUpdates.forEntity( nodeId, true ).withTokens( labels ).added( propertyKeyId, Values.of( value ) ).build();
		 }

		 private void CreateAlistairAndStefanNodes()
		 {
			  using ( Transaction tx = _graphDb.beginTx() )
			  {
					_alistair = _graphDb.createNode( _label );
					_alistair.setProperty( "name", "Alistair" );
					_alistair.setProperty( "country", "UK" );
					_stefan = _graphDb.createNode( _label );
					_stefan.setProperty( "name", "Stefan" );
					_stefan.setProperty( "country", "Deutschland" );
					_aKnowsS = _alistair.createRelationshipTo( _stefan, _relationshipType );
					_aKnowsS.setProperty( "duration", "long" );
					_aKnowsS.setProperty( "irrelevant", "prop" );
					_sKnowsA = _stefan.createRelationshipTo( _alistair, _relationshipType );
					_sKnowsA.setProperty( "duration", "lengthy" );
					_sKnowsA.setProperty( "irrelevant", "prop" );
					tx.Success();
			  }
		 }

		 private void DeleteAlistairAndStefanNodes()
		 {
			  using ( Transaction tx = _graphDb.beginTx() )
			  {
					_aKnowsS.delete();
					_sKnowsA.delete();
					_alistair.delete();
					_stefan.delete();
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void getOrCreateIds() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 private void getOrCreateIds()
		 {
			  using ( Transaction tx = _graphDb.beginTx() )
			  {
					ThreadToStatementContextBridge bridge = _graphDb.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) );

					TokenWrite tokenWrite = bridge.GetKernelTransactionBoundToThisThread( true ).tokenWrite();
					_labelId = tokenWrite.LabelGetOrCreateForName( "Person" );
					_relTypeId = tokenWrite.RelationshipTypeGetOrCreateForName( "Knows" );
					_propertyKeyId = tokenWrite.PropertyKeyGetOrCreateForName( "name" );
					_relPropertyKeyId = tokenWrite.PropertyKeyGetOrCreateForName( "duration" );
					tx.Success();
			  }
		 }

		 private class CopyUpdateVisitor : Visitor<EntityUpdates, Exception>
		 {

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal IEntityUpdates PropertyUpdatesConflict;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visit(org.Neo4Net.kernel.impl.api.index.EntityUpdates element) throws RuntimeException
			  public override bool Visit( IEntityUpdates element )
			  {
					PropertyUpdatesConflict = element;
					return true;
			  }

			  public virtual IEntityUpdates PropertyUpdates
			  {
				  get
				  {
						return PropertyUpdatesConflict;
				  }
			  }
		 }

		 internal class IEntityUpdateCollectingVisitor : Visitor<EntityUpdates, Exception>
		 {
			 private readonly NeoStoreIndexStoreViewTest _outerInstance;

			 public IEntityUpdateCollectingVisitor( NeoStoreIndexStoreViewTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly ISet<EntityUpdates> UpdatesConflict = new HashSet<EntityUpdates>();

			  public override bool Visit( IEntityUpdates propertyUpdates )
			  {
					UpdatesConflict.Add( propertyUpdates );
					return false;
			  }

			  internal virtual ISet<EntityUpdates> Updates
			  {
				  get
				  {
						return UpdatesConflict;
				  }
			  }
		 }
	}

}