using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Kernel.impl.store
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RuleChain = org.junit.rules.RuleChain;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using UncloseableDelegatingFileSystemAbstraction = Neo4Net.GraphDb.mockfs.UncloseableDelegatingFileSystemAbstraction;
	using Neo4Net.Helpers.Collections;
	using NamedToken = Neo4Net.Internal.Kernel.Api.NamedToken;
	using EntityNotFoundException = Neo4Net.Internal.Kernel.Api.exceptions.EntityNotFoundException;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using PropertyKeyValue = Neo4Net.Kernel.api.properties.PropertyKeyValue;
	using TransactionState = Neo4Net.Kernel.api.txstate.TransactionState;
	using Config = Neo4Net.Kernel.configuration.Config;
	using KernelStatement = Neo4Net.Kernel.Impl.Api.KernelStatement;
	using KernelTransactionImplementation = Neo4Net.Kernel.Impl.Api.KernelTransactionImplementation;
	using DelegatingTokenHolder = Neo4Net.Kernel.impl.core.DelegatingTokenHolder;
	using TokenHolder = Neo4Net.Kernel.impl.core.TokenHolder;
	using TokenNotFoundException = Neo4Net.Kernel.impl.core.TokenNotFoundException;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using Neo4Net.Kernel.impl.storageengine.impl.recordstorage.TransactionRecordState;
	using Position = Neo4Net.Kernel.impl.store.MetaDataStore.Position;
	using CountsTracker = Neo4Net.Kernel.impl.store.counts.CountsTracker;
	using RecordFormatSelector = Neo4Net.Kernel.impl.store.format.RecordFormatSelector;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using DynamicRecordFormat = Neo4Net.Kernel.impl.store.format.standard.DynamicRecordFormat;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyBlock = Neo4Net.Kernel.Impl.Store.Records.PropertyBlock;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using LogHeader = Neo4Net.Kernel.impl.transaction.log.entry.LogHeader;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using Neo4Net.Storageengine.Api;
	using StorageEngine = Neo4Net.Storageengine.Api.StorageEngine;
	using StorageNodeCursor = Neo4Net.Storageengine.Api.StorageNodeCursor;
	using StorageProperty = Neo4Net.Storageengine.Api.StorageProperty;
	using StoragePropertyCursor = Neo4Net.Storageengine.Api.StoragePropertyCursor;
	using StorageReader = Neo4Net.Storageengine.Api.StorageReader;
	using StorageRelationshipScanCursor = Neo4Net.Storageengine.Api.StorageRelationshipScanCursor;
	using StorageRelationshipTraversalCursor = Neo4Net.Storageengine.Api.StorageRelationshipTraversalCursor;
	using UTF8 = Neo4Net.Strings.UTF8;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using ThreadTestUtils = Neo4Net.Test.ThreadTestUtils;
	using ConfigurablePageCacheRule = Neo4Net.Test.rule.ConfigurablePageCacheRule;
	using NeoStoreDataSourceRule = Neo4Net.Test.rule.NeoStoreDataSourceRule;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.counts_store_rotation_timeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.security.LoginContext.AUTH_DISABLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.RecordStore.getRecord;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.format.standard.MetaDataRecordFormat.FIELD_NOT_PRESENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.record.RecordLoad.NORMAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP;

	public class NeoStoresTest
	{
		private bool InstanceFieldsInitialized = false;

		public NeoStoresTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_dir = TestDirectory.testDirectory( _fs.get() );
			RuleChain = RuleChain.outerRule( _exception ).around( _pageCacheRule ).around( _fs ).around( _dir ).around( _dsRule );
		}

		 private static readonly NullLogProvider _logProvider = NullLogProvider.Instance;
		 private readonly PageCacheRule _pageCacheRule = new ConfigurablePageCacheRule();
		 private readonly ExpectedException _exception = ExpectedException.none();
		 private readonly EphemeralFileSystemRule _fs = new EphemeralFileSystemRule();
		 private TestDirectory _dir;
		 private readonly NeoStoreDataSourceRule _dsRule = new NeoStoreDataSourceRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(exception).around(pageCacheRule).around(fs).around(dir).around(dsRule);
		 public RuleChain RuleChain;

		 private PageCache _pageCache;
		 private DatabaseLayout _databaseLayout;
		 private PropertyStore _pStore;
		 private RelationshipTypeTokenStore _rtStore;
		 private RelationshipStore _relStore;
		 private NodeStore _nodeStore;
		 private NeoStoreDataSource _ds;
		 private KernelTransaction _tx;
		 private TransactionState _transaction;
		 private StorageReader _storageReader;
		 private TokenHolder _propertyKeyTokenHolder;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUpNeoStores()
		 public virtual void SetUpNeoStores()
		 {
			  _databaseLayout = _dir.databaseLayout();
			  Config config = Config.defaults();
			  _pageCache = _pageCacheRule.getPageCache( _fs.get() );
			  StoreFactory sf = GetStoreFactory( config, _databaseLayout, _fs.get(), NullLogProvider.Instance );
			  sf.OpenAllNeoStores( true ).close();
			  _propertyKeyTokenHolder = new DelegatingTokenHolder( this.createPropertyKeyToken, Neo4Net.Kernel.impl.core.TokenHolder_Fields.TYPE_PROPERTY_KEY );
		 }

		 private int CreatePropertyKeyToken( string name )
		 {
			  return ( int ) NextId( typeof( PropertyKeyTokenRecord ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void impossibleToGetStoreFromClosedNeoStoresContainer()
		 public virtual void ImpossibleToGetStoreFromClosedNeoStoresContainer()
		 {
			  Config config = Config.defaults();
			  StoreFactory sf = GetStoreFactory( config, _databaseLayout, _fs.get(), NullLogProvider.Instance );
			  NeoStores neoStores = sf.OpenAllNeoStores( true );

			  assertNotNull( neoStores.MetaDataStore );

			  neoStores.Close();

			  _exception.expect( typeof( System.InvalidOperationException ) );
			  _exception.expectMessage( "Specified store was already closed." );
			  neoStores.MetaDataStore;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void notAllowCreateDynamicStoreWithNegativeBlockSize()
		 public virtual void NotAllowCreateDynamicStoreWithNegativeBlockSize()
		 {
			  Config config = Config.defaults();
			  StoreFactory sf = GetStoreFactory( config, _databaseLayout, _fs.get(), NullLogProvider.Instance );

			  _exception.expect( typeof( System.ArgumentException ) );
			  _exception.expectMessage( "Block size of dynamic array store should be positive integer." );

			  using ( NeoStores neoStores = sf.OpenNeoStores( true ) )
			  {
					neoStores.CreateDynamicArrayStore( new File( "someStore" ), new File( "someIdFile" ), IdType.ARRAY_BLOCK, -2 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void impossibleToGetNotRequestedStore()
		 public virtual void ImpossibleToGetNotRequestedStore()
		 {
			  Config config = Config.defaults();
			  StoreFactory sf = GetStoreFactory( config, _databaseLayout, _fs.get(), NullLogProvider.Instance );

			  _exception.expect( typeof( System.InvalidOperationException ) );
			  _exception.expectMessage( "Specified store was not initialized. Please specify " + StoreType.MetaData.name() + " as one of the stores types that should be open to be able to use it." );
			  using ( NeoStores neoStores = sf.OpenNeoStores( true, StoreType.NodeLabel ) )
			  {
					neoStores.MetaDataStore;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCreateStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestCreateStore()
		 {
			  InitializeStores( _databaseLayout, stringMap() );
			  StartTx();
			  // setup test population
			  long node1 = NextId( typeof( Node ) );
			  _transaction.nodeDoCreate( node1 );
			  long node2 = NextId( typeof( Node ) );
			  _transaction.nodeDoCreate( node2 );
			  StorageProperty n1prop1 = NodeAddProperty( node1, Index( "prop1" ), "string1" );
			  StorageProperty n1prop2 = NodeAddProperty( node1, Index( "prop2" ), 1 );
			  StorageProperty n1prop3 = NodeAddProperty( node1, Index( "prop3" ), true );

			  StorageProperty n2prop1 = NodeAddProperty( node2, Index( "prop1" ), "string2" );
			  StorageProperty n2prop2 = NodeAddProperty( node2, Index( "prop2" ), 2 );
			  StorageProperty n2prop3 = NodeAddProperty( node2, Index( "prop3" ), false );

			  int relType1 = ( int ) NextId( typeof( RelationshipType ) );
			  string typeName1 = "relationshiptype1";
			  _transaction.relationshipTypeDoCreateForName( typeName1, relType1 );
			  int relType2 = ( int ) NextId( typeof( RelationshipType ) );
			  string typeName2 = "relationshiptype2";
			  _transaction.relationshipTypeDoCreateForName( typeName2, relType2 );
			  long rel1 = NextId( typeof( Relationship ) );
			  _transaction.relationshipDoCreate( rel1, relType1, node1, node2 );
			  long rel2 = NextId( typeof( Relationship ) );
			  _transaction.relationshipDoCreate( rel2, relType2, node2, node1 );

			  StorageProperty r1prop1 = RelAddProperty( rel1, Index( "prop1" ), "string1" );
			  StorageProperty r1prop2 = RelAddProperty( rel1, Index( "prop2" ), 1 );
			  StorageProperty r1prop3 = RelAddProperty( rel1, Index( "prop3" ), true );

			  StorageProperty r2prop1 = RelAddProperty( rel2, Index( "prop1" ), "string2" );
			  StorageProperty r2prop2 = RelAddProperty( rel2, Index( "prop2" ), 2 );
			  StorageProperty r2prop3 = RelAddProperty( rel2, Index( "prop3" ), false );
			  CommitTx();
			  _ds.stop();

			  InitializeStores( _databaseLayout, stringMap() );
			  StartTx();
			  // validate node
			  ValidateNodeRel1( node1, n1prop1, n1prop2, n1prop3, rel1, rel2, relType1, relType2 );
			  ValidateNodeRel2( node2, n2prop1, n2prop2, n2prop3, rel1, rel2, relType1, relType2 );
			  // validate rels
			  ValidateRel1( rel1, r1prop1, r1prop2, r1prop3, node1, node2, relType1 );
			  ValidateRel2( rel2, r2prop1, r2prop2, r2prop3, node2, node1, relType2 );
			  ValidateRelTypes( relType1, relType2 );
			  // validate reltypes
			  ValidateRelTypes( relType1, relType2 );
			  CommitTx();
			  _ds.stop();

			  InitializeStores( _databaseLayout, stringMap() );
			  StartTx();
			  // validate and delete rels
			  DeleteRel1( rel1, r1prop1, r1prop2, r1prop3, node1, node2, relType1 );
			  DeleteRel2( rel2, r2prop1, r2prop2, r2prop3, node2, node1, relType2 );
			  // validate and delete nodes
			  DeleteNode1( node1, n1prop1, n1prop2, n1prop3 );
			  DeleteNode2( node2, n2prop1, n2prop2, n2prop3 );
			  CommitTx();
			  _ds.stop();

			  InitializeStores( _databaseLayout, stringMap() );
			  StartTx();
			  assertFalse( NodeExists( node1 ) );
			  assertFalse( NodeExists( node2 ) );
			  TestGetRels( new long[]{ rel1, rel2 } );
			  long[] nodeIds = new long[10];
			  for ( int i = 0; i < 3; i++ )
			  {
					nodeIds[i] = NextId( typeof( Node ) );
					_transaction.nodeDoCreate( nodeIds[i] );
					NodeAddProperty( nodeIds[i], Index( "nisse" ), 10 - i );
			  }
			  for ( int i = 0; i < 2; i++ )
			  {
					long id = NextId( typeof( Relationship ) );
					_transaction.relationshipDoCreate( id, relType1, nodeIds[i], nodeIds[i + 1] );
					_transaction.relationshipDoDelete( id, relType1, nodeIds[i], nodeIds[i + 1] );
			  }
			  for ( int i = 0; i < 3; i++ )
			  {
					_transaction.nodeDoDelete( nodeIds[i] );
			  }
			  CommitTx();
			  _ds.stop();
		 }

		 private StorageProperty NodeAddProperty( long nodeId, int key, object value )
		 {
			  StorageProperty property = new PropertyKeyValue( key, Values.of( value ) );
			  StorageProperty oldProperty = null;
			  using ( StorageNodeCursor nodeCursor = _storageReader.allocateNodeCursor() )
			  {
					nodeCursor.Single( nodeId );
					if ( nodeCursor.Next() )
					{
						 StorageProperty fetched = GetProperty( key, nodeCursor.PropertiesReference() );
						 if ( fetched != null )
						 {
							  oldProperty = fetched;
						 }
					}
			  }

			  if ( oldProperty == null )
			  {
					_transaction.nodeDoAddProperty( nodeId, key, property.Value() );
			  }
			  else
			  {
					_transaction.nodeDoChangeProperty( nodeId, key, property.Value() );
			  }
			  return property;
		 }

		 private StorageProperty RelAddProperty( long relationshipId, int key, object value )
		 {
			  StorageProperty property = new PropertyKeyValue( key, Values.of( value ) );
			  Value oldValue = Values.NO_VALUE;
			  using ( StorageRelationshipScanCursor cursor = _storageReader.allocateRelationshipScanCursor() )
			  {
					cursor.Single( relationshipId );
					if ( cursor.Next() )
					{
						 StorageProperty fetched = GetProperty( key, cursor.PropertiesReference() );
						 if ( fetched != null )
						 {
							  oldValue = fetched.Value();
						 }
					}
			  }

			  _transaction.relationshipDoReplaceProperty( relationshipId, key, oldValue, Values.of( value ) );
			  return property;
		 }

		 private StorageProperty GetProperty( int key, long propertyId )
		 {
			  using ( StoragePropertyCursor propertyCursor = _storageReader.allocatePropertyCursor() )
			  {
					propertyCursor.Init( propertyId );
					if ( propertyCursor.Next() )
					{
						 Value oldValue = propertyCursor.PropertyValue();
						 if ( oldValue != null )
						 {
							  return new PropertyKeyValue( key, oldValue );
						 }
					}
			  }
			  return null;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRels1() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestRels1()
		 {
			  InitializeStores( _databaseLayout, stringMap() );
			  StartTx();
			  int relType1 = ( int ) NextId( typeof( RelationshipType ) );
			  string typeName = "relationshiptype1";
			  _transaction.relationshipTypeDoCreateForName( typeName, relType1 );
			  long[] nodeIds = new long[3];
			  for ( int i = 0; i < 3; i++ )
			  {
					nodeIds[i] = NextId( typeof( Node ) );
					_transaction.nodeDoCreate( nodeIds[i] );
					NodeAddProperty( nodeIds[i], Index( "nisse" ), 10 - i );
			  }
			  for ( int i = 0; i < 2; i++ )
			  {
					_transaction.relationshipDoCreate( NextId( typeof( Relationship ) ), relType1, nodeIds[i], nodeIds[i + 1] );
			  }
			  CommitTx();
			  StartTx();
			  for ( int i = 0; i < 3; i += 2 )
			  {
					DeleteRelationships( nodeIds[i] );
					_transaction.nodeDoDelete( nodeIds[i] );
			  }
			  CommitTx();
			  _ds.stop();
		 }

		 private void RelDelete( long id )
		 {
			  RelationshipVisitor<Exception> visitor = ( relId, type, startNode, endNode ) => _transaction.relationshipDoDelete( relId, type, startNode, endNode );
			  if ( !_transaction.relationshipVisit( id, visitor ) )
			  {
					try
					{
						 _storageReader.relationshipVisit( id, visitor );
					}
					catch ( EntityNotFoundException e )
					{
						 throw new Exception( e );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRels2() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestRels2()
		 {
			  InitializeStores( _databaseLayout, stringMap() );
			  StartTx();
			  int relType1 = ( int ) NextId( typeof( RelationshipType ) );
			  string typeName = "relationshiptype1";
			  _transaction.relationshipTypeDoCreateForName( typeName, relType1 );
			  long[] nodeIds = new long[3];
			  for ( int i = 0; i < 3; i++ )
			  {
					nodeIds[i] = NextId( typeof( Node ) );
					_transaction.nodeDoCreate( nodeIds[i] );
					NodeAddProperty( nodeIds[i], Index( "nisse" ), 10 - i );
			  }
			  for ( int i = 0; i < 2; i++ )
			  {
					_transaction.relationshipDoCreate( NextId( typeof( Relationship ) ), relType1, nodeIds[i], nodeIds[i + 1] );
			  }
			  _transaction.relationshipDoCreate( NextId( typeof( Relationship ) ), relType1, nodeIds[0], nodeIds[2] );
			  CommitTx();
			  StartTx();
			  for ( int i = 0; i < 3; i++ )
			  {
					DeleteRelationships( nodeIds[i] );
					_transaction.nodeDoDelete( nodeIds[i] );
			  }
			  CommitTx();
			  _ds.stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRels3() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestRels3()
		 {
			  // test linked list stuff during relationship delete
			  InitializeStores( _databaseLayout, stringMap() );
			  StartTx();
			  int relType1 = ( int ) NextId( typeof( RelationshipType ) );
			  _transaction.relationshipTypeDoCreateForName( "relationshiptype1", relType1 );
			  long[] nodeIds = new long[8];
			  for ( int i = 0; i < nodeIds.Length; i++ )
			  {
					nodeIds[i] = NextId( typeof( Node ) );
					_transaction.nodeDoCreate( nodeIds[i] );
			  }
			  for ( int i = 0; i < nodeIds.Length / 2; i++ )
			  {
					_transaction.relationshipDoCreate( NextId( typeof( Relationship ) ), relType1, nodeIds[i], nodeIds[i * 2] );
			  }
			  long rel5 = NextId( typeof( Relationship ) );
			  _transaction.relationshipDoCreate( rel5, relType1, nodeIds[0], nodeIds[5] );
			  long rel2 = NextId( typeof( Relationship ) );
			  _transaction.relationshipDoCreate( rel2, relType1, nodeIds[1], nodeIds[2] );
			  long rel3 = NextId( typeof( Relationship ) );
			  _transaction.relationshipDoCreate( rel3, relType1, nodeIds[1], nodeIds[3] );
			  long rel6 = NextId( typeof( Relationship ) );
			  _transaction.relationshipDoCreate( rel6, relType1, nodeIds[1], nodeIds[6] );
			  long rel1 = NextId( typeof( Relationship ) );
			  _transaction.relationshipDoCreate( rel1, relType1, nodeIds[0], nodeIds[1] );
			  long rel4 = NextId( typeof( Relationship ) );
			  _transaction.relationshipDoCreate( rel4, relType1, nodeIds[0], nodeIds[4] );
			  long rel7 = NextId( typeof( Relationship ) );
			  _transaction.relationshipDoCreate( rel7, relType1, nodeIds[0], nodeIds[7] );
			  CommitTx();
			  StartTx();
			  RelDelete( rel7 );
			  RelDelete( rel4 );
			  RelDelete( rel1 );
			  RelDelete( rel6 );
			  RelDelete( rel3 );
			  RelDelete( rel2 );
			  RelDelete( rel5 );
			  CommitTx();
			  _ds.stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testProps1() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestProps1()
		 {
			  InitializeStores( _databaseLayout, stringMap() );
			  StartTx();
			  long nodeId = NextId( typeof( Node ) );
			  _transaction.nodeDoCreate( nodeId );
			  _pStore.nextId();
			  StorageProperty prop = NodeAddProperty( nodeId, Index( "nisse" ), 10 );
			  CommitTx();
			  _ds.stop();
			  InitializeStores( _databaseLayout, stringMap() );
			  StartTx();
			  StorageProperty prop2 = NodeAddProperty( nodeId, prop.PropertyKeyId(), 5 );
			  _transaction.nodeDoRemoveProperty( nodeId, prop2.PropertyKeyId() );
			  _transaction.nodeDoDelete( nodeId );
			  CommitTx();
			  _ds.stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSetBlockSize() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestSetBlockSize()
		 {
			  DatabaseLayout databaseLayout = _dir.databaseLayout( "small_store" );
			  InitializeStores( databaseLayout, stringMap( "unsupported.dbms.block_size.strings", "62", "unsupported.dbms.block_size.array_properties", "302" ) );
			  assertEquals( 62 + DynamicRecordFormat.RECORD_HEADER_SIZE, _pStore.StringStore.RecordSize );
			  assertEquals( 302 + DynamicRecordFormat.RECORD_HEADER_SIZE, _pStore.ArrayStore.RecordSize );
			  _ds.stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void setVersion() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetVersion()
		 {
			  FileSystemAbstraction fileSystem = _fs.get();
			  File storeDir = _dir.directory();
			  CreateTestDatabase( fileSystem, storeDir ).shutdown();
			  DatabaseLayout databaseLayout = _dir.databaseLayout();
			  assertEquals( 0, MetaDataStore.SetRecord( _pageCache, databaseLayout.MetadataStore(), Position.LOG_VERSION, 10 ) );
			  assertEquals( 10, MetaDataStore.SetRecord( _pageCache, databaseLayout.MetadataStore(), Position.LOG_VERSION, 12 ) );

			  Config config = Config.defaults();
			  StoreFactory sf = GetStoreFactory( config, databaseLayout, fileSystem, _logProvider );

			  NeoStores neoStores = sf.OpenAllNeoStores();
			  assertEquals( 12, neoStores.MetaDataStore.CurrentLogVersion );
			  neoStores.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReadNonRecordDataAsRecord() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotReadNonRecordDataAsRecord()
		 {
			  FileSystemAbstraction fileSystem = _fs.get();
			  StoreFactory factory = NewStoreFactory( _databaseLayout, _pageCache, fileSystem );
			  long recordVersion = DefaultStoreVersion();
			  using ( NeoStores neoStores = factory.OpenAllNeoStores( true ) )
			  {
					MetaDataStore metaDataStore = neoStores.MetaDataStore;
					metaDataStore.CreationTime = 3;
					metaDataStore.RandomNumber = 4;
					metaDataStore.CurrentLogVersion = 5;
					metaDataStore.SetLastCommittedAndClosedTransactionId( 6, 0, 0, 43, 44 );
					metaDataStore.StoreVersion = recordVersion;

					metaDataStore.GraphNextProp = 8;
					metaDataStore.LatestConstraintIntroducingTx = 9;
			  }

			  File file = _databaseLayout.metadataStore();
			  using ( StoreChannel channel = fileSystem.Open( file, OpenMode.READ_WRITE ) )
			  {
					channel.Position( 0 );
					channel.write( ByteBuffer.wrap( UTF8.encode( "This is some data that is not a record." ) ) );
			  }

			  MetaDataStore.SetRecord( _pageCache, file, Position.STORE_VERSION, recordVersion );

			  using ( NeoStores neoStores = factory.OpenAllNeoStores() )
			  {
					MetaDataStore metaDataStore = neoStores.MetaDataStore;
					assertEquals( FIELD_NOT_PRESENT, metaDataStore.CreationTime );
					assertEquals( FIELD_NOT_PRESENT, metaDataStore.RandomNumber );
					assertEquals( FIELD_NOT_PRESENT, metaDataStore.CurrentLogVersion );
					assertEquals( FIELD_NOT_PRESENT, metaDataStore.LastCommittedTransactionId );
					assertEquals( FIELD_NOT_PRESENT, metaDataStore.LastClosedTransactionId );
					assertEquals( recordVersion, metaDataStore.StoreVersion );
					assertEquals( 8, metaDataStore.GraphNextProp );
					assertEquals( 9, metaDataStore.LatestConstraintIntroducingTx );
					assertArrayEquals( metaDataStore.LastClosedTransaction, new long[]{ FIELD_NOT_PRESENT, 44, 43 } );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSetLatestConstraintTx()
		 public virtual void TestSetLatestConstraintTx()
		 {
			  // given
			  Config config = Config.defaults();
			  StoreFactory sf = new StoreFactory( _dir.databaseLayout(), config, new DefaultIdGeneratorFactory(_fs.get()), _pageCacheRule.getPageCache(_fs.get()), _fs.get(), _logProvider, EmptyVersionContextSupplier.EMPTY );

			  // when
			  NeoStores neoStores = sf.OpenAllNeoStores( true );
			  MetaDataStore metaDataStore = neoStores.MetaDataStore;

			  // then the default is 0
			  assertEquals( 0L, metaDataStore.LatestConstraintIntroducingTx );

			  // when
			  metaDataStore.LatestConstraintIntroducingTx = 10L;

			  // then
			  assertEquals( 10L, metaDataStore.LatestConstraintIntroducingTx );

			  // when
			  neoStores.Flush( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
			  neoStores.Close();
			  neoStores = sf.OpenAllNeoStores();

			  // then the value should have been stored
			  assertEquals( 10L, neoStores.MetaDataStore.LatestConstraintIntroducingTx );
			  neoStores.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInitializeTheTxIdToOne()
		 public virtual void ShouldInitializeTheTxIdToOne()
		 {
			  StoreFactory factory = GetStoreFactory( Config.defaults(), _dir.databaseLayout(), _fs.get(), _logProvider );
			  using ( NeoStores neoStores = factory.OpenAllNeoStores( true ) )
			  {
					neoStores.MetaDataStore;
			  }

			  using ( NeoStores neoStores = factory.OpenAllNeoStores() )
			  {
					long lastCommittedTransactionId = neoStores.MetaDataStore.LastCommittedTransactionId;
					assertEquals( Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID, lastCommittedTransactionId );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowUnderlyingStorageExceptionWhenFailingToLoadStorage()
		 public virtual void ShouldThrowUnderlyingStorageExceptionWhenFailingToLoadStorage()
		 {
			  FileSystemAbstraction fileSystem = _fs.get();
			  DatabaseLayout databaseLayout = _dir.databaseLayout();
			  StoreFactory factory = GetStoreFactory( Config.defaults(), databaseLayout, fileSystem, _logProvider );

			  using ( NeoStores neoStores = factory.OpenAllNeoStores( true ) )
			  {
					neoStores.MetaDataStore;
			  }
			  File file = databaseLayout.MetadataStore();
			  fileSystem.DeleteFile( file );

			  _exception.expect( typeof( StoreNotFoundException ) );
			  using ( NeoStores neoStores = factory.OpenAllNeoStores() )
			  {
					neoStores.MetaDataStore;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddUpgradeFieldsToTheNeoStoreIfNotPresent() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddUpgradeFieldsToTheNeoStoreIfNotPresent()
		 {
			  FileSystemAbstraction fileSystem = _fs.get();
			  StoreFactory factory = NewStoreFactory( _databaseLayout, _pageCache, fileSystem );
			  long recordVersion = DefaultStoreVersion();
			  using ( NeoStores neoStores = factory.OpenAllNeoStores( true ) )
			  {
					MetaDataStore metaDataStore = neoStores.MetaDataStore;
					metaDataStore.CreationTime = 3;
					metaDataStore.RandomNumber = 4;
					metaDataStore.CurrentLogVersion = 5;
					metaDataStore.SetLastCommittedAndClosedTransactionId( 6, 42, BASE_TX_COMMIT_TIMESTAMP, 43, 44 );
					metaDataStore.StoreVersion = recordVersion;

					metaDataStore.GraphNextProp = 8;
					metaDataStore.LatestConstraintIntroducingTx = 9;
			  }

			  File file = _databaseLayout.metadataStore();

			  assertNotEquals( 10, MetaDataStore.GetRecord( _pageCache, file, Position.UPGRADE_TRANSACTION_ID ) );
			  assertNotEquals( 11, MetaDataStore.GetRecord( _pageCache, file, Position.UPGRADE_TRANSACTION_CHECKSUM ) );

			  MetaDataStore.SetRecord( _pageCache, file, Position.UPGRADE_TRANSACTION_ID, 10 );
			  MetaDataStore.SetRecord( _pageCache, file, Position.UPGRADE_TRANSACTION_CHECKSUM, 11 );
			  MetaDataStore.SetRecord( _pageCache, file, Position.UPGRADE_TIME, 12 );

			  using ( NeoStores neoStores = factory.OpenAllNeoStores() )
			  {
					MetaDataStore metaDataStore = neoStores.MetaDataStore;
					assertEquals( 3, metaDataStore.CreationTime );
					assertEquals( 4, metaDataStore.RandomNumber );
					assertEquals( 5, metaDataStore.CurrentLogVersion );
					assertEquals( 6, metaDataStore.LastCommittedTransactionId );
					assertEquals( recordVersion, metaDataStore.StoreVersion );
					assertEquals( 8, metaDataStore.GraphNextProp );
					assertEquals( 9, metaDataStore.LatestConstraintIntroducingTx );
					assertEquals( new TransactionId( 10, 11, BASE_TX_COMMIT_TIMESTAMP ), metaDataStore.UpgradeTransaction );
					assertEquals( 12, metaDataStore.UpgradeTime );
					assertArrayEquals( metaDataStore.LastClosedTransaction, new long[]{ 6, 44, 43 } );
			  }

			  MetaDataStore.SetRecord( _pageCache, file, Position.UPGRADE_TRANSACTION_COMMIT_TIMESTAMP, 13 );

			  using ( NeoStores neoStores = factory.OpenAllNeoStores() )
			  {
					MetaDataStore metaDataStore = neoStores.MetaDataStore;
					assertEquals( new TransactionId( 10, 11, 13 ), metaDataStore.UpgradeTransaction );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetHighestTransactionIdWhenNeeded()
		 public virtual void ShouldSetHighestTransactionIdWhenNeeded()
		 {
			  // GIVEN
			  FileSystemAbstraction fileSystem = _fs.get();
			  StoreFactory factory = GetStoreFactory( Config.defaults(), _databaseLayout, fileSystem, _logProvider );

			  using ( NeoStores neoStore = factory.OpenAllNeoStores( true ) )
			  {
					MetaDataStore store = neoStore.MetaDataStore;
					store.SetLastCommittedAndClosedTransactionId( 40, 4444, BASE_TX_COMMIT_TIMESTAMP, LogHeader.LOG_HEADER_SIZE, 0 );

					// WHEN
					store.TransactionCommitted( 42, 6666, BASE_TX_COMMIT_TIMESTAMP );

					// THEN
					assertEquals( new TransactionId( 42, 6666, BASE_TX_COMMIT_TIMESTAMP ), store.LastCommittedTransaction );
					assertArrayEquals( store.LastClosedTransaction, new long[]{ 40, 0, LogHeader.LOG_HEADER_SIZE } );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSetHighestTransactionIdWhenNeeded()
		 public virtual void ShouldNotSetHighestTransactionIdWhenNeeded()
		 {
			  // GIVEN
			  FileSystemAbstraction fileSystem = _fs.get();
			  StoreFactory factory = GetStoreFactory( Config.defaults(), _databaseLayout, fileSystem, _logProvider );

			  using ( NeoStores neoStore = factory.OpenAllNeoStores( true ) )
			  {
					MetaDataStore store = neoStore.MetaDataStore;
					store.SetLastCommittedAndClosedTransactionId( 40, 4444, BASE_TX_COMMIT_TIMESTAMP, LogHeader.LOG_HEADER_SIZE, 0 );

					// WHEN
					store.TransactionCommitted( 39, 3333, BASE_TX_COMMIT_TIMESTAMP );

					// THEN
					assertEquals( new TransactionId( 40, 4444, BASE_TX_COMMIT_TIMESTAMP ), store.LastCommittedTransaction );
					assertArrayEquals( store.LastClosedTransaction, new long[]{ 40, 0, LogHeader.LOG_HEADER_SIZE } );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseAllTheStoreEvenIfExceptionsAreThrown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseAllTheStoreEvenIfExceptionsAreThrown()
		 {
			  // given
			  FileSystemAbstraction fileSystem = _fs.get();
			  Config defaults = Config.defaults( counts_store_rotation_timeout, "60m" );
			  StoreFactory factory = GetStoreFactory( defaults, _databaseLayout, fileSystem, _logProvider );
			  NeoStores neoStore = factory.OpenAllNeoStores( true );

			  // let's hack the counts store so it fails to rotate and hence it fails to close as well...
			  CountsTracker counts = neoStore.Counts;
			  Counts.start();
			  long nextTxId = neoStore.MetaDataStore.LastCommittedTransactionId + 1;
			  AtomicReference<Exception> exRef = new AtomicReference<Exception>();
			  Thread thread = new Thread(() =>
			  {
				try
				{
					 Counts.rotate( nextTxId );
				}
				catch ( InterruptedIOException )
				{
					 // expected due to the interrupted below
				}
				catch ( Exception e )
				{
					 exRef.set( e );
					 throw new Exception( e );
				}
			  });
			  thread.Start();

			  // let's wait for the thread to start waiting for the next transaction id
			  ThreadTestUtils.awaitThreadState( thread, TimeUnit.SECONDS.toMillis( 5 ), Thread.State.TIMED_WAITING, Thread.State.WAITING );

			  try
			  {
					// when we close the stores...
					neoStore.Close();
					fail( "should have thrown2" );
			  }
			  catch ( System.InvalidOperationException ex )
			  {
					// then
					assertEquals( "Cannot stop in state: rotating", ex.Message );
			  }

			  thread.Interrupt();
			  thread.Join();

			  // and the page cache closes with no errors
			  _pageCache.close();
			  // and only InterruptedIOException is thrown in the other thread
			  assertNull( exRef.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void isPresentAfterCreatingAllStores() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void isPresentAfterCreatingAllStores()
		 {
			  // given
			  FileSystemAbstraction fileSystem = _fs.get();
			  fileSystem.DeleteRecursively( _databaseLayout.databaseDirectory() );
			  DefaultIdGeneratorFactory idFactory = new DefaultIdGeneratorFactory( fileSystem );
			  StoreFactory factory = new StoreFactory( _databaseLayout, Config.defaults(), idFactory, _pageCache, fileSystem, _logProvider, EmptyVersionContextSupplier.EMPTY );

			  // when
			  using ( NeoStores ignore = factory.OpenAllNeoStores( true ) )
			  {
					// then
					assertTrue( NeoStores.IsStorePresent( _pageCache, _databaseLayout ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void isPresentFalseAfterCreatingAllButLastStoreType() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void isPresentFalseAfterCreatingAllButLastStoreType()
		 {
			  // given
			  FileSystemAbstraction fileSystem = _fs.get();
			  fileSystem.DeleteRecursively( _databaseLayout.databaseDirectory() );
			  DefaultIdGeneratorFactory idFactory = new DefaultIdGeneratorFactory( fileSystem );
			  StoreFactory factory = new StoreFactory( _databaseLayout, Config.defaults(), idFactory, _pageCache, fileSystem, _logProvider, EmptyVersionContextSupplier.EMPTY );
			  StoreType[] allStoreTypes = StoreType.values();
			  StoreType[] allButLastStoreTypes = Arrays.copyOf( allStoreTypes, allStoreTypes.Length - 1 );

			  // when
			  using ( NeoStores ignore = factory.OpenNeoStores( true, allButLastStoreTypes ) )
			  {
					// then
					assertFalse( NeoStores.IsStorePresent( _pageCache, _databaseLayout ) );
			  }
		 }

		 private static long DefaultStoreVersion()
		 {
			  return MetaDataStore.VersionStringToLong( RecordFormatSelector.defaultFormat().storeVersion() );
		 }

		 private static StoreFactory NewStoreFactory( DatabaseLayout databaseLayout, PageCache pageCache, FileSystemAbstraction fs )
		 {
			  RecordFormats recordFormats = RecordFormatSelector.defaultFormat();
			  Config config = Config.defaults();
			  IdGeneratorFactory idGeneratorFactory = new DefaultIdGeneratorFactory( fs );
			  return new StoreFactory( databaseLayout, config, idGeneratorFactory, pageCache, fs, recordFormats, _logProvider, EmptyVersionContextSupplier.EMPTY );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void initializeStores(org.Neo4Net.io.layout.DatabaseLayout databaseLayout, java.util.Map<String,String> additionalConfig) throws java.io.IOException
		 private void InitializeStores( DatabaseLayout databaseLayout, IDictionary<string, string> additionalConfig )
		 {
			  Dependencies dependencies = new Dependencies();
			  dependencies.SatisfyDependency( Config.defaults( additionalConfig ) );
			  _ds = _dsRule.getDataSource( databaseLayout, _fs.get(), _pageCache, dependencies );
			  _ds.start();

			  NeoStores neoStores = _ds.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores();
			  _pStore = neoStores.PropertyStore;
			  _rtStore = neoStores.RelationshipTypeTokenStore;
			  _relStore = neoStores.RelationshipStore;
			  _nodeStore = neoStores.NodeStore;
			  _storageReader = _ds.DependencyResolver.resolveDependency( typeof( StorageEngine ) ).newReader();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void startTx() throws org.Neo4Net.internal.kernel.api.exceptions.TransactionFailureException
		 private void StartTx()
		 {
			  _tx = _ds.Kernel.beginTransaction( KernelTransaction.Type.@implicit, AUTH_DISABLED );
			  _transaction = ( ( KernelTransactionImplementation ) _tx ).txState();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void commitTx() throws org.Neo4Net.internal.kernel.api.exceptions.TransactionFailureException
		 private void CommitTx()
		 {
			  _tx.success();
			  _tx.close();
		 }

		 private int Index( string key )
		 {
			  return _propertyKeyTokenHolder.getOrCreateId( key );
		 }

		 private long NextId( Type clazz )
		 {
			  NeoStores neoStores = _ds.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores();
			  if ( clazz.Equals( typeof( PropertyKeyTokenRecord ) ) )
			  {
					return neoStores.PropertyKeyTokenStore.nextId();
			  }
			  if ( clazz.Equals( typeof( RelationshipType ) ) )
			  {
					return neoStores.RelationshipTypeTokenStore.nextId();
			  }
			  if ( clazz.Equals( typeof( Node ) ) )
			  {
					return neoStores.NodeStore.nextId();
			  }
			  if ( clazz.Equals( typeof( Relationship ) ) )
			  {
					return neoStores.RelationshipStore.nextId();
			  }
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  throw new System.ArgumentException( clazz.FullName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void validateNodeRel1(long node, org.Neo4Net.storageengine.api.StorageProperty prop1, org.Neo4Net.storageengine.api.StorageProperty prop2, org.Neo4Net.storageengine.api.StorageProperty prop3, long rel1, long rel2, int relType1, int relType2) throws java.io.IOException, org.Neo4Net.kernel.impl.core.TokenNotFoundException
		 private void ValidateNodeRel1( long node, StorageProperty prop1, StorageProperty prop2, StorageProperty prop3, long rel1, long rel2, int relType1, int relType2 )
		 {
			  assertTrue( NodeExists( node ) );
			  IDictionary<int, Pair<StorageProperty, long>> props = new Dictionary<int, Pair<StorageProperty, long>>();
			  PropertyReceiver<StorageProperty> receiver = NewPropertyReceiver( props );
			  NodeLoadProperties( node, receiver );
			  int count = 0;
			  foreach ( int keyId in props.Keys )
			  {
					long id = props[keyId].Other();
					PropertyRecord record = getRecord( _pStore, id );
					PropertyBlock block = record.GetPropertyBlock( props[keyId].First().propertyKeyId() );
					StorageProperty data = block.NewPropertyKeyValue( _pStore );
					if ( data.PropertyKeyId() == prop1.PropertyKeyId() )
					{
						 assertEquals( "prop1", _propertyKeyTokenHolder.getTokenById( keyId ).name() );
						 assertEquals( "string1", data.Value().asObject() );
						 NodeAddProperty( node, prop1.PropertyKeyId(), "-string1" );
					}
					else if ( data.PropertyKeyId() == prop2.PropertyKeyId() )
					{
						 assertEquals( "prop2", _propertyKeyTokenHolder.getTokenById( keyId ).name() );
						 assertEquals( 1, data.Value().asObject() );
						 NodeAddProperty( node, prop2.PropertyKeyId(), -1 );
					}
					else if ( data.PropertyKeyId() == prop3.PropertyKeyId() )
					{
						 assertEquals( "prop3", _propertyKeyTokenHolder.getTokenById( keyId ).name() );
						 assertEquals( true, data.Value().asObject() );
						 NodeAddProperty( node, prop3.PropertyKeyId(), false );
					}
					else
					{
						 throw new IOException();
					}
					count++;
			  }
			  assertEquals( 3, count );

			  count = ValidateAndCountRelationships( node, rel1, rel2, relType1, relType2 );
			  assertEquals( 2, count );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int validateAndCountRelationships(long node, long rel1, long rel2, int relType1, int relType2) throws java.io.IOException
		 private int ValidateAndCountRelationships( long node, long rel1, long rel2, int relType1, int relType2 )
		 {
			  int count = 0;
			  using ( KernelStatement statement = ( KernelStatement ) _tx.acquireStatement(), StorageNodeCursor nodeCursor = AllocateNodeCursor(node) )
			  {
					assertTrue( nodeCursor.Next() );
					using ( StorageRelationshipTraversalCursor relationships = AllocateRelationshipTraversalCursor( nodeCursor ) )
					{
						 while ( relationships.Next() )
						 {
							  long rel = relationships.EntityReference();
							  if ( rel == rel1 )
							  {
									assertEquals( node, relationships.SourceNodeReference() );
									assertEquals( relType1, relationships.Type() );
							  }
							  else if ( rel == rel2 )
							  {
									assertEquals( node, relationships.TargetNodeReference() );
									assertEquals( relType2, relationships.Type() );
							  }
							  else
							  {
									throw new IOException();
							  }
							  count++;

						 }
					}
			  }
			  return count;
		 }

		 private StorageRelationshipTraversalCursor AllocateRelationshipTraversalCursor( StorageNodeCursor node )
		 {
			  StorageRelationshipTraversalCursor relationships = _storageReader.allocateRelationshipTraversalCursor();
			  relationships.Init( node.EntityReference(), node.AllRelationshipsReference() );
			  return relationships;
		 }

		 private StorageNodeCursor AllocateNodeCursor( long nodeId )
		 {
			  StorageNodeCursor nodeCursor = _storageReader.allocateNodeCursor();
			  nodeCursor.Single( nodeId );
			  return nodeCursor;
		 }

		 private PropertyReceiver<StorageProperty> NewPropertyReceiver( IDictionary<int, Pair<StorageProperty, long>> props )
		 {
			  return ( property, propertyRecordId ) => props[property.propertyKeyId()] = Pair.of(property, propertyRecordId);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void validateNodeRel2(long node, org.Neo4Net.storageengine.api.StorageProperty prop1, org.Neo4Net.storageengine.api.StorageProperty prop2, org.Neo4Net.storageengine.api.StorageProperty prop3, long rel1, long rel2, int relType1, int relType2) throws java.io.IOException, RuntimeException, org.Neo4Net.kernel.impl.core.TokenNotFoundException
		 private void ValidateNodeRel2( long node, StorageProperty prop1, StorageProperty prop2, StorageProperty prop3, long rel1, long rel2, int relType1, int relType2 )
		 {
			  assertTrue( NodeExists( node ) );
			  IDictionary<int, Pair<StorageProperty, long>> props = new Dictionary<int, Pair<StorageProperty, long>>();
			  NodeLoadProperties( node, NewPropertyReceiver( props ) );
			  int count = 0;
			  foreach ( int keyId in props.Keys )
			  {
					long id = props[keyId].Other();
					PropertyRecord record = getRecord( _pStore, id );
					PropertyBlock block = record.GetPropertyBlock( props[keyId].First().propertyKeyId() );
					StorageProperty data = block.NewPropertyKeyValue( _pStore );
					if ( data.PropertyKeyId() == prop1.PropertyKeyId() )
					{
						 assertEquals( "prop1", _propertyKeyTokenHolder.getTokenById( keyId ).name() );
						 assertEquals( "string2", data.Value().asObject() );
						 NodeAddProperty( node, prop1.PropertyKeyId(), "-string2" );
					}
					else if ( data.PropertyKeyId() == prop2.PropertyKeyId() )
					{
						 assertEquals( "prop2", _propertyKeyTokenHolder.getTokenById( keyId ).name() );
						 assertEquals( 2, data.Value().asObject() );
						 NodeAddProperty( node, prop2.PropertyKeyId(), -2 );
					}
					else if ( data.PropertyKeyId() == prop3.PropertyKeyId() )
					{
						 assertEquals( "prop3", _propertyKeyTokenHolder.getTokenById( keyId ).name() );
						 assertEquals( false, data.Value().asObject() );
						 NodeAddProperty( node, prop3.PropertyKeyId(), true );
					}
					else
					{
						 throw new IOException();
					}
					count++;
			  }
			  assertEquals( 3, count );
			  count = 0;

			  using ( KernelStatement statement = ( KernelStatement ) _tx.acquireStatement(), StorageNodeCursor nodeCursor = AllocateNodeCursor(node) )
			  {
					assertTrue( nodeCursor.Next() );
					using ( StorageRelationshipTraversalCursor relationships = AllocateRelationshipTraversalCursor( nodeCursor ) )
					{
						 while ( relationships.Next() )
						 {
							  long rel = relationships.EntityReference();
							  if ( rel == rel1 )
							  {
									assertEquals( node, relationships.TargetNodeReference() );
									assertEquals( relType1, relationships.Type() );
							  }
							  else if ( rel == rel2 )
							  {
									assertEquals( node, relationships.SourceNodeReference() );
									assertEquals( relType2, relationships.Type() );
							  }
							  else
							  {
									throw new IOException();
							  }
							  count++;

						 }
					}
			  }
			  assertEquals( 2, count );
		 }

		 private bool NodeExists( long nodeId )
		 {
			  using ( StorageNodeCursor node = AllocateNodeCursor( nodeId ) )
			  {
					return node.Next();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void validateRel1(long rel, org.Neo4Net.storageengine.api.StorageProperty prop1, org.Neo4Net.storageengine.api.StorageProperty prop2, org.Neo4Net.storageengine.api.StorageProperty prop3, long firstNode, long secondNode, int relType) throws java.io.IOException, org.Neo4Net.kernel.impl.core.TokenNotFoundException
		 private void ValidateRel1( long rel, StorageProperty prop1, StorageProperty prop2, StorageProperty prop3, long firstNode, long secondNode, int relType )
		 {
			  IDictionary<int, Pair<StorageProperty, long>> props = new Dictionary<int, Pair<StorageProperty, long>>();
			  RelLoadProperties( rel, NewPropertyReceiver( props ) );
			  int count = 0;
			  foreach ( int keyId in props.Keys )
			  {
					long id = props[keyId].Other();
					PropertyRecord record = getRecord( _pStore, id );
					PropertyBlock block = record.GetPropertyBlock( props[keyId].First().propertyKeyId() );
					StorageProperty data = block.NewPropertyKeyValue( _pStore );
					if ( data.PropertyKeyId() == prop1.PropertyKeyId() )
					{
						 assertEquals( "prop1", _propertyKeyTokenHolder.getTokenById( keyId ).name() );
						 assertEquals( "string1", data.Value().asObject() );
						 RelAddProperty( rel, prop1.PropertyKeyId(), "-string1" );
					}
					else if ( data.PropertyKeyId() == prop2.PropertyKeyId() )
					{
						 assertEquals( "prop2", _propertyKeyTokenHolder.getTokenById( keyId ).name() );
						 assertEquals( 1, data.Value().asObject() );
						 RelAddProperty( rel, prop2.PropertyKeyId(), -1 );
					}
					else if ( data.PropertyKeyId() == prop3.PropertyKeyId() )
					{
						 assertEquals( "prop3", _propertyKeyTokenHolder.getTokenById( keyId ).name() );
						 assertEquals( true, data.Value().asObject() );
						 RelAddProperty( rel, prop3.PropertyKeyId(), false );
					}
					else
					{
						 throw new IOException();
					}
					count++;
			  }
			  assertEquals( 3, count );
			  AssertRelationshipData( rel, firstNode, secondNode, relType );
		 }

		 private void AssertRelationshipData( long rel, long firstNode, long secondNode, int relType )
		 {
			  try
			  {
					_storageReader.relationshipVisit(rel, (relId, type, startNode, endNode) =>
					{
					 assertEquals( firstNode, startNode );
					 assertEquals( secondNode, endNode );
					 assertEquals( relType, type );
					});
			  }
			  catch ( EntityNotFoundException e )
			  {
					throw new Exception( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void validateRel2(long rel, org.Neo4Net.storageengine.api.StorageProperty prop1, org.Neo4Net.storageengine.api.StorageProperty prop2, org.Neo4Net.storageengine.api.StorageProperty prop3, long firstNode, long secondNode, int relType) throws java.io.IOException, org.Neo4Net.kernel.impl.core.TokenNotFoundException
		 private void ValidateRel2( long rel, StorageProperty prop1, StorageProperty prop2, StorageProperty prop3, long firstNode, long secondNode, int relType )
		 {
			  IDictionary<int, Pair<StorageProperty, long>> props = new Dictionary<int, Pair<StorageProperty, long>>();
			  RelLoadProperties( rel, NewPropertyReceiver( props ) );
			  int count = 0;
			  foreach ( int keyId in props.Keys )
			  {
					long id = props[keyId].Other();
					PropertyRecord record = getRecord( _pStore, id );
					PropertyBlock block = record.GetPropertyBlock( props[keyId].First().propertyKeyId() );
					StorageProperty data = block.NewPropertyKeyValue( _pStore );
					if ( data.PropertyKeyId() == prop1.PropertyKeyId() )
					{
						 assertEquals( "prop1", _propertyKeyTokenHolder.getTokenById( keyId ).name() );
						 assertEquals( "string2", data.Value().asObject() );
						 RelAddProperty( rel, prop1.PropertyKeyId(), "-string2" );
					}
					else if ( data.PropertyKeyId() == prop2.PropertyKeyId() )
					{
						 assertEquals( "prop2", _propertyKeyTokenHolder.getTokenById( keyId ).name() );
						 assertEquals( 2, data.Value().asObject() );
						 RelAddProperty( rel, prop2.PropertyKeyId(), -2 );
					}
					else if ( data.PropertyKeyId() == prop3.PropertyKeyId() )
					{
						 assertEquals( "prop3", _propertyKeyTokenHolder.getTokenById( keyId ).name() );
						 assertEquals( false, data.Value().asObject() );
						 RelAddProperty( rel, prop3.PropertyKeyId(), true );
					}
					else
					{
						 throw new IOException();
					}
					count++;
			  }
			  assertEquals( 3, count );
			  AssertRelationshipData( rel, firstNode, secondNode, relType );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void validateRelTypes(int relType1, int relType2) throws java.io.IOException
		 private void ValidateRelTypes( int relType1, int relType2 )
		 {
			  NamedToken data = _rtStore.getToken( relType1 );
			  assertEquals( relType1, data.Id() );
			  assertEquals( "relationshiptype1", data.Name() );
			  data = _rtStore.getToken( relType2 );
			  assertEquals( relType2, data.Id() );
			  assertEquals( "relationshiptype2", data.Name() );
			  IList<NamedToken> allData = _rtStore.Tokens;
			  assertEquals( 2, allData.Count );
			  for ( int i = 0; i < 2; i++ )
			  {
					if ( allData[i].Id() == relType1 )
					{
						 assertEquals( relType1, allData[i].Id() );
						 assertEquals( "relationshiptype1", allData[i].Name() );
					}
					else if ( allData[i].Id() == relType2 )
					{
						 assertEquals( relType2, allData[i].Id() );
						 assertEquals( "relationshiptype2", allData[i].Name() );
					}
					else
					{
						 throw new IOException();
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void deleteRel1(long rel, org.Neo4Net.storageengine.api.StorageProperty prop1, org.Neo4Net.storageengine.api.StorageProperty prop2, org.Neo4Net.storageengine.api.StorageProperty prop3, long firstNode, long secondNode, int relType) throws Exception
		 private void DeleteRel1( long rel, StorageProperty prop1, StorageProperty prop2, StorageProperty prop3, long firstNode, long secondNode, int relType )
		 {
			  IDictionary<int, Pair<StorageProperty, long>> props = new Dictionary<int, Pair<StorageProperty, long>>();
			  RelLoadProperties( rel, NewPropertyReceiver( props ) );
			  int count = 0;
			  foreach ( int keyId in props.Keys )
			  {
					long id = props[keyId].Other();
					PropertyRecord record = _pStore.getRecord( id, _pStore.newRecord(), NORMAL );
					PropertyBlock block = record.GetPropertyBlock( props[keyId].First().propertyKeyId() );
					StorageProperty data = block.NewPropertyKeyValue( _pStore );
					if ( data.PropertyKeyId() == prop1.PropertyKeyId() )
					{
						 assertEquals( "prop1", _propertyKeyTokenHolder.getTokenById( keyId ).name() );
						 assertEquals( "-string1", data.Value().asObject() );
					}
					else if ( data.PropertyKeyId() == prop2.PropertyKeyId() )
					{
						 assertEquals( "prop2", _propertyKeyTokenHolder.getTokenById( keyId ).name() );
						 assertEquals( -1, data.Value().asObject() );
					}
					else if ( data.PropertyKeyId() == prop3.PropertyKeyId() )
					{
						 assertEquals( "prop3", _propertyKeyTokenHolder.getTokenById( keyId ).name() );
						 assertEquals( false, data.Value().asObject() );
						 _transaction.relationshipDoRemoveProperty( rel, prop3.PropertyKeyId() );
					}
					else
					{
						 throw new IOException();
					}
					count++;
			  }
			  assertEquals( 3, count );
			  CountingPropertyReceiver propertyCounter = new CountingPropertyReceiver();
			  RelLoadProperties( rel, propertyCounter );
			  assertEquals( 3, propertyCounter.Count );
			  AssertRelationshipData( rel, firstNode, secondNode, relType );
			  RelDelete( rel );

			  AssertHasRelationships( firstNode );

			  AssertHasRelationships( secondNode );
		 }

		 private class CountingPropertyReceiver : PropertyReceiver<StorageProperty>
		 {
			  internal int Count;

			  public override void Receive( StorageProperty property, long propertyRecordId )
			  {
					Count++;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void deleteRel2(long rel, org.Neo4Net.storageengine.api.StorageProperty prop1, org.Neo4Net.storageengine.api.StorageProperty prop2, org.Neo4Net.storageengine.api.StorageProperty prop3, long firstNode, long secondNode, int relType) throws Exception
		 private void DeleteRel2( long rel, StorageProperty prop1, StorageProperty prop2, StorageProperty prop3, long firstNode, long secondNode, int relType )
		 {
			  IDictionary<int, Pair<StorageProperty, long>> props = new Dictionary<int, Pair<StorageProperty, long>>();
			  RelLoadProperties( rel, NewPropertyReceiver( props ) );
			  int count = 0;
			  foreach ( int keyId in props.Keys )
			  {
					long id = props[keyId].Other();
					PropertyRecord record = _pStore.getRecord( id, _pStore.newRecord(), NORMAL );
					PropertyBlock block = record.GetPropertyBlock( props[keyId].First().propertyKeyId() );
					StorageProperty data = block.NewPropertyKeyValue( _pStore );
					if ( data.PropertyKeyId() == prop1.PropertyKeyId() )
					{
						 assertEquals( "prop1", _propertyKeyTokenHolder.getTokenById( keyId ).name() );
						 assertEquals( "-string2", data.Value().asObject() );
					}
					else if ( data.PropertyKeyId() == prop2.PropertyKeyId() )
					{
						 assertEquals( "prop2", _propertyKeyTokenHolder.getTokenById( keyId ).name() );
						 assertEquals( -2, data.Value().asObject() );
					}
					else if ( data.PropertyKeyId() == prop3.PropertyKeyId() )
					{
						 assertEquals( "prop3", _propertyKeyTokenHolder.getTokenById( keyId ).name() );
						 assertEquals( true, data.Value().asObject() );
						 _transaction.relationshipDoRemoveProperty( rel, prop3.PropertyKeyId() );
					}
					else
					{
						 throw new IOException();
					}
					count++;
			  }
			  assertEquals( 3, count );
			  CountingPropertyReceiver propertyCounter = new CountingPropertyReceiver();
			  RelLoadProperties( rel, propertyCounter );
			  assertEquals( 3, propertyCounter.Count );
			  AssertRelationshipData( rel, firstNode, secondNode, relType );
			  RelDelete( rel );

			  AssertHasRelationships( firstNode );

			  AssertHasRelationships( secondNode );

		 }

		 private void AssertHasRelationships( long node )
		 {
			  using ( KernelStatement statement = ( KernelStatement ) _tx.acquireStatement(), StorageNodeCursor nodeCursor = AllocateNodeCursor(node) )
			  {
					assertTrue( nodeCursor.Next() );
					using ( StorageRelationshipTraversalCursor relationships = AllocateRelationshipTraversalCursor( nodeCursor ) )
					{
						 assertTrue( relationships.Next() );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void deleteNode1(long node, org.Neo4Net.storageengine.api.StorageProperty prop1, org.Neo4Net.storageengine.api.StorageProperty prop2, org.Neo4Net.storageengine.api.StorageProperty prop3) throws java.io.IOException, org.Neo4Net.kernel.impl.core.TokenNotFoundException
		 private void DeleteNode1( long node, StorageProperty prop1, StorageProperty prop2, StorageProperty prop3 )
		 {
			  IDictionary<int, Pair<StorageProperty, long>> props = new Dictionary<int, Pair<StorageProperty, long>>();
			  NodeLoadProperties( node, NewPropertyReceiver( props ) );
			  int count = 0;
			  foreach ( int keyId in props.Keys )
			  {
					long id = props[keyId].Other();
					PropertyRecord record = _pStore.getRecord( id, _pStore.newRecord(), NORMAL );
					PropertyBlock block = record.GetPropertyBlock( props[keyId].First().propertyKeyId() );
					StorageProperty data = block.NewPropertyKeyValue( _pStore );
					if ( data.PropertyKeyId() == prop1.PropertyKeyId() )
					{
						 assertEquals( "prop1", _propertyKeyTokenHolder.getTokenById( keyId ).name() );
						 assertEquals( "-string1", data.Value().asObject() );
					}
					else if ( data.PropertyKeyId() == prop2.PropertyKeyId() )
					{
						 assertEquals( "prop2", _propertyKeyTokenHolder.getTokenById( keyId ).name() );
						 assertEquals( -1, data.Value().asObject() );
					}
					else if ( data.PropertyKeyId() == prop3.PropertyKeyId() )
					{
						 assertEquals( "prop3", _propertyKeyTokenHolder.getTokenById( keyId ).name() );
						 assertEquals( false, data.Value().asObject() );
						 _transaction.nodeDoRemoveProperty( node, prop3.PropertyKeyId() );
					}
					else
					{
						 throw new IOException();
					}
					count++;
			  }
			  assertEquals( 3, count );
			  CountingPropertyReceiver propertyCounter = new CountingPropertyReceiver();
			  NodeLoadProperties( node, propertyCounter );
			  assertEquals( 3, propertyCounter.Count );
			  AssertHasRelationships( node );
			  _transaction.nodeDoDelete( node );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void deleteNode2(long node, org.Neo4Net.storageengine.api.StorageProperty prop1, org.Neo4Net.storageengine.api.StorageProperty prop2, org.Neo4Net.storageengine.api.StorageProperty prop3) throws java.io.IOException, org.Neo4Net.kernel.impl.core.TokenNotFoundException
		 private void DeleteNode2( long node, StorageProperty prop1, StorageProperty prop2, StorageProperty prop3 )
		 {
			  IDictionary<int, Pair<StorageProperty, long>> props = new Dictionary<int, Pair<StorageProperty, long>>();
			  NodeLoadProperties( node, NewPropertyReceiver( props ) );
			  int count = 0;
			  foreach ( int keyId in props.Keys )
			  {
					long id = props[keyId].Other();
					PropertyRecord record = _pStore.getRecord( id, _pStore.newRecord(), NORMAL );
					PropertyBlock block = record.GetPropertyBlock( props[keyId].First().propertyKeyId() );
					StorageProperty data = block.NewPropertyKeyValue( _pStore );
					if ( data.PropertyKeyId() == prop1.PropertyKeyId() )
					{
						 assertEquals( "prop1", _propertyKeyTokenHolder.getTokenById( keyId ).name() );
						 assertEquals( "-string2", data.Value().asObject() );
					}
					else if ( data.PropertyKeyId() == prop2.PropertyKeyId() )
					{
						 assertEquals( "prop2", _propertyKeyTokenHolder.getTokenById( keyId ).name() );
						 assertEquals( -2, data.Value().asObject() );
					}
					else if ( data.PropertyKeyId() == prop3.PropertyKeyId() )
					{
						 assertEquals( "prop3", _propertyKeyTokenHolder.getTokenById( keyId ).name() );
						 assertEquals( true, data.Value().asObject() );
						 _transaction.nodeDoRemoveProperty( node, prop3.PropertyKeyId() );
					}
					else
					{
						 throw new IOException();
					}
					count++;
			  }
			  assertEquals( 3, count );
			  CountingPropertyReceiver propertyCounter = new CountingPropertyReceiver();
			  NodeLoadProperties( node, propertyCounter );
			  assertEquals( 3, propertyCounter.Count );

			  AssertHasRelationships( node );

			  _transaction.nodeDoDelete( node );
		 }

		 private void TestGetRels( long[] relIds )
		 {
			  foreach ( long relId in relIds )
			  {
					using ( StorageRelationshipScanCursor relationship = _storageReader.allocateRelationshipScanCursor() )
					{
						 relationship.Single( relId );
						 assertFalse( relationship.Next() );
					}
			  }
		 }

		 private void DeleteRelationships( long nodeId )
		 {
			  using ( KernelStatement statement = ( KernelStatement ) _tx.acquireStatement(), StorageNodeCursor nodeCursor = AllocateNodeCursor(nodeId) )
			  {
					assertTrue( nodeCursor.Next() );
					using ( StorageRelationshipTraversalCursor relationships = AllocateRelationshipTraversalCursor( nodeCursor ) )
					{
						 while ( relationships.Next() )
						 {
							  RelDelete( relationships.EntityReference() );
						 }
					}
			  }
		 }

		 private IGraphDatabaseService CreateTestDatabase( FileSystemAbstraction fileSystem, File storeDir )
		 {
			  return ( new TestGraphDatabaseFactory() ).setFileSystem(new UncloseableDelegatingFileSystemAbstraction(fileSystem)).newImpermanentDatabase(storeDir);
		 }

		 private void NodeLoadProperties<RECEIVER>( long nodeId, RECEIVER receiver ) where RECEIVER : Neo4Net.Kernel.impl.storageengine.impl.recordstorage.TransactionRecordState.PropertyReceiver
		 {
			  NodeRecord nodeRecord = _nodeStore.getRecord( nodeId, _nodeStore.newRecord(), NORMAL );
			  LoadProperties( nodeRecord.NextProp, receiver );
		 }

		 private void RelLoadProperties<RECEIVER>( long relId, RECEIVER receiver ) where RECEIVER : Neo4Net.Kernel.impl.storageengine.impl.recordstorage.TransactionRecordState.PropertyReceiver
		 {
			  RelationshipRecord relRecord = _relStore.getRecord( relId, _relStore.newRecord(), NORMAL );
			  LoadProperties( relRecord.NextProp, receiver );
		 }

		 private void LoadProperties<RECEIVER>( long nextProp, RECEIVER receiver ) where RECEIVER : Neo4Net.Kernel.impl.storageengine.impl.recordstorage.TransactionRecordState.PropertyReceiver
		 {
			  ICollection<PropertyRecord> chain = _pStore.getPropertyRecordChain( nextProp );
			  ReceivePropertyChain( receiver, chain );
		 }

		 private void ReceivePropertyChain<RECEIVER>( RECEIVER receiver, ICollection<PropertyRecord> chain ) where RECEIVER : Neo4Net.Kernel.impl.storageengine.impl.recordstorage.TransactionRecordState.PropertyReceiver
		 {
			  if ( chain != null )
			  {
					foreach ( PropertyRecord propRecord in chain )
					{
						 foreach ( PropertyBlock propBlock in propRecord )
						 {
							  receiver.receive( propBlock.NewPropertyKeyValue( _pStore ), propRecord.Id );
						 }
					}
			  }
		 }

		 private StoreFactory GetStoreFactory( Config config, DatabaseLayout databaseLayout, FileSystemAbstraction ephemeralFileSystemAbstraction, NullLogProvider instance )
		 {
			  return new StoreFactory( databaseLayout, config, new DefaultIdGeneratorFactory( ephemeralFileSystemAbstraction ), _pageCache, ephemeralFileSystemAbstraction, instance, EmptyVersionContextSupplier.EMPTY );
		 }
	}

}