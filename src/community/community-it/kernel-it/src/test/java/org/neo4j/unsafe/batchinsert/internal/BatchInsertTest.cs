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
namespace Neo4Net.@unsafe.Batchinsert.Internal
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using After = org.junit.After;
	using AfterClass = org.junit.AfterClass;
	using BeforeClass = org.junit.BeforeClass;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using ConstraintViolationException = Neo4Net.GraphDb.ConstraintViolationException;
	using Direction = Neo4Net.GraphDb.Direction;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using ConstraintDefinition = Neo4Net.GraphDb.Schema.ConstraintDefinition;
	using ConstraintType = Neo4Net.GraphDb.Schema.ConstraintType;
	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using MapUtil = Neo4Net.Collections.Helpers.MapUtil;
	using Neo4Net.Collections.Helpers;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.Internal.gbptree.RecoveryCleanupWorkCollector;
	using IndexProviderDescriptor = Neo4Net.Kernel.Api.Internal.Schema.IndexProviderDescriptor;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using LabelScanStore = Neo4Net.Kernel.Api.LabelScan.LabelScanStore;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.Api.schema.index.TestIndexDescriptorFactory;
	using Neo4Net.Kernel.extension;
	using MyRelTypes = Neo4Net.Kernel.impl.MyRelTypes;
	using TestIndexProviderDescriptor = Neo4Net.Kernel.Impl.Api.index.TestIndexProviderDescriptor;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using FullStoreChangeStream = Neo4Net.Kernel.Impl.Api.scan.FullStoreChangeStream;
	using NativeLabelScanStore = Neo4Net.Kernel.impl.index.labelscan.NativeLabelScanStore;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using NodeLabels = Neo4Net.Kernel.impl.store.NodeLabels;
	using NodeLabelsField = Neo4Net.Kernel.impl.store.NodeLabelsField;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using SchemaStorage = Neo4Net.Kernel.impl.store.SchemaStorage;
	using SchemaStore = Neo4Net.Kernel.impl.store.SchemaStore;
	using ConstraintRule = Neo4Net.Kernel.Impl.Store.Records.ConstraintRule;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using RecordLoad = Neo4Net.Kernel.Impl.Store.Records.RecordLoad;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using NodePropertyAccessor = Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using IndexSample = Neo4Net.Kernel.Api.StorageEngine.schema.IndexSample;
	using LabelScanReader = Neo4Net.Kernel.Api.StorageEngine.schema.LabelScanReader;
	using SchemaRule = Neo4Net.Kernel.Api.StorageEngine.schema.SchemaRule;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.parseInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.arrayContaining;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.emptyArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.hamcrest.MockitoHamcrest.argThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterables.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterables.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.asCollection;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.iterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.index.IndexEntryUpdate.add;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.api.index.SchemaIndexTestHelper.singleInstanceIndexProviderFactory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.RecordStore.getRecord;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.record.RecordLoad.NORMAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.mockito.matcher.CollectionMatcher.matchesCollection;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.mockito.matcher.Neo4NetMatchers.hasProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.mockito.matcher.Neo4NetMatchers.inTx;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class BatchInsertTest
	public class BatchInsertTest
	{
		 private static readonly IndexProviderDescriptor _descriptor = TestIndexProviderDescriptor.PROVIDER_DESCRIPTOR;
		 private static readonly string _key = _descriptor.Key;
		 private const string INTERNAL_LOG_FILE = "debug.log";
		 private readonly int _denseNodeThreshold;
		 // This is the assumed internal index descriptor based on knowledge of what ids get assigned
		 private static readonly IndexDescriptor _internalIndex = TestIndexDescriptorFactory.forLabel( 0, 0 );
		 private static readonly IndexDescriptor _internalUniqueIndex = TestIndexDescriptorFactory.uniqueForLabel( 0, 0 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{index} denseNodeThreshold={0}") public static java.util.Collection<int> data()
		 public static ICollection<int> Data()
		 {
			  return Arrays.asList( 5, parseInt( GraphDatabaseSettings.dense_node_threshold.DefaultValue ) );
		 }

		 public BatchInsertTest( int denseNodeThreshold )
		 {
			  this._denseNodeThreshold = denseNodeThreshold;
		 }

		 private static IDictionary<string, object> _properties = new Dictionary<string, object>();

		 private enum RelTypes
		 {
			  BatchTest,
			  RelType1,
			  RelType2,
			  RelType3,
			  RelType4,
			  RelType5
		 }

		 private static RelationshipType[] _relTypeArray = new RelationshipType[] { RelTypes.RelType1, RelTypes.RelType2, RelTypes.RelType3, RelTypes.RelType4, RelTypes.RelType5 };

		 static BatchInsertTest()
		 {
			  _properties["key0"] = "SDSDASSDLKSDSAKLSLDAKSLKDLSDAKLDSLA";
			  _properties["key1"] = 1;
			  _properties["key2"] = ( short ) 2;
			  _properties["key3"] = 3L;
			  _properties["key4"] = 4.0f;
			  _properties["key5"] = 5.0d;
			  _properties["key6"] = ( sbyte ) 6;
			  _properties["key7"] = true;
			  _properties["key8"] = ( char ) 8;
			  _properties["key10"] = new string[] { "SDSDASSDLKSDSAKLSLDAKSLKDLSDAKLDSLA", "dsasda", "dssadsad" };
			  _properties["key11"] = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
			  _properties["key12"] = new short[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
			  _properties["key13"] = new long[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
			  _properties["key14"] = new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
			  _properties["key15"] = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
			  _properties["key16"] = new sbyte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
			  _properties["key17"] = new bool[] { true, false, true, false };
			  _properties["key18"] = new char[] { ( char )1, ( char )2, ( char )3, ( char )4, ( char )5, ( char )6, ( char )7, ( char )8, ( char )9 };
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static Neo4Net.test.rule.TestDirectory globalTestDirectory = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public static TestDirectory GlobalTestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static Neo4Net.test.rule.fs.DefaultFileSystemRule fileSystemRule = new Neo4Net.test.rule.fs.DefaultFileSystemRule();
		 public static DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.TestDirectory localTestDirectory = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory LocalTestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.PageCacheRule pageCacheRule = new Neo4Net.test.rule.PageCacheRule();
		 public readonly PageCacheRule PageCacheRule = new PageCacheRule();

		 private static BatchInserter _globalInserter;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void startGlobalInserter() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void StartGlobalInserter()
		 {
			  // Global inserter can be used in tests which simply want to verify "local" behaviour,
			  // e.g. create a node with some properties and read them back.
			  _globalInserter = BatchInserters.inserter( GlobalTestDirectory.directory( "global" ), FileSystemRule.get(), stringMap() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void flushGlobalInserter()
		 public virtual void FlushGlobalInserter()
		 {
			  ForceFlush( _globalInserter );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void shutDownGlobalInserter()
		 public static void ShutDownGlobalInserter()
		 {
			  _globalInserter.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateStringArrayPropertiesOnNodesUsingBatchInserter1()
		 public virtual void ShouldUpdateStringArrayPropertiesOnNodesUsingBatchInserter1()
		 {
			  // Given
			  BatchInserter inserter = _globalInserter;

			  string[] array1 = new string[] { "1" };
			  string[] array2 = new string[] { "a" };

			  long id1 = inserter.createNode( map( "array", array1 ) );
			  long id2 = inserter.createNode( map() );

			  // When
			  inserter.GetNodeProperties( id1 )["array"];
			  inserter.SetNodeProperty( id1, "array", array1 );
			  inserter.SetNodeProperty( id2, "array", array2 );

			  inserter.GetNodeProperties( id1 )["array"];
			  inserter.SetNodeProperty( id1, "array", array1 );
			  inserter.SetNodeProperty( id2, "array", array2 );

			  // Then
			  assertThat( inserter.GetNodeProperties( id1 )["array"], equalTo( array1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSimple()
		 public virtual void TestSimple()
		 {
			  BatchInserter graphDb = _globalInserter;
			  long node1 = graphDb.CreateNode( null );
			  long node2 = graphDb.CreateNode( null );
			  long rel1 = graphDb.CreateRelationship( node1, node2, RelTypes.BatchTest, null );
			  BatchRelationship rel = graphDb.GetRelationshipById( rel1 );
			  assertEquals( rel.StartNode, node1 );
			  assertEquals( rel.EndNode, node2 );
			  assertEquals( RelTypes.BatchTest.name(), rel.Type.name() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSetAndAddNodeProperties()
		 public virtual void TestSetAndAddNodeProperties()
		 {
			  BatchInserter inserter = _globalInserter;

			  long tehNode = inserter.createNode( MapUtil.map( "one", "one","two","two","three","three" ) );
			  inserter.SetNodeProperty( tehNode, "four", "four" );
			  inserter.SetNodeProperty( tehNode, "five", "five" );
			  IDictionary<string, object> props = GetNodeProperties( inserter, tehNode );
			  assertEquals( 5, props.Count );
			  assertEquals( "one", props["one"] );
			  assertEquals( "five", props["five"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void setSingleProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetSingleProperty()
		 {
			  BatchInserter inserter = NewBatchInserter();
			  long node = inserter.CreateNode( null );

			  string value = "Something";
			  string key = "name";
			  inserter.SetNodeProperty( node, key, value );

			  IGraphDatabaseService db = SwitchToEmbeddedGraphDatabaseService( inserter );
			  assertThat( GetNodeInTx( node, db ), inTx( db, hasProperty( key ).withValue( value ) ) );
			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSetAndKeepNodeProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestSetAndKeepNodeProperty()
		 {
			  BatchInserter inserter = NewBatchInserter();

			  long tehNode = inserter.CreateNode( MapUtil.map( "foo", "bar" ) );
			  inserter.SetNodeProperty( tehNode, "foo2", "bar2" );
			  IDictionary<string, object> props = GetNodeProperties( inserter, tehNode );
			  assertEquals( 2, props.Count );
			  assertEquals( "bar", props["foo"] );
			  assertEquals( "bar2", props["foo2"] );

			  inserter.Shutdown();

			  inserter = NewBatchInserter();

			  props = GetNodeProperties( inserter, tehNode );
			  assertEquals( 2, props.Count );
			  assertEquals( "bar", props["foo"] );
			  assertEquals( "bar2", props["foo2"] );

			  inserter.SetNodeProperty( tehNode, "foo", "bar3" );

			  props = GetNodeProperties( inserter, tehNode );
			  assertEquals( "bar3", props["foo"] );
			  assertEquals( 2, props.Count );
			  assertEquals( "bar3", props["foo"] );
			  assertEquals( "bar2", props["foo2"] );

			  inserter.Shutdown();
			  inserter = NewBatchInserter();

			  props = GetNodeProperties( inserter, tehNode );
			  assertEquals( "bar3", props["foo"] );
			  assertEquals( 2, props.Count );
			  assertEquals( "bar3", props["foo"] );
			  assertEquals( "bar2", props["foo2"] );

			  inserter.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSetAndKeepRelationshipProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestSetAndKeepRelationshipProperty()
		 {
			  BatchInserter inserter = NewBatchInserter();

			  long from = inserter.createNode( Collections.emptyMap() );
			  long to = inserter.createNode( Collections.emptyMap() );
			  long theRel = inserter.CreateRelationship( from, to, RelationshipType.withName( "TestingPropsHere" ), MapUtil.map( "foo", "bar" ) );
			  inserter.SetRelationshipProperty( theRel, "foo2", "bar2" );
			  IDictionary<string, object> props = GetRelationshipProperties( inserter, theRel );
			  assertEquals( 2, props.Count );
			  assertEquals( "bar", props["foo"] );
			  assertEquals( "bar2", props["foo2"] );

			  inserter.Shutdown();

			  inserter = NewBatchInserter();

			  props = GetRelationshipProperties( inserter, theRel );
			  assertEquals( 2, props.Count );
			  assertEquals( "bar", props["foo"] );
			  assertEquals( "bar2", props["foo2"] );

			  inserter.SetRelationshipProperty( theRel, "foo", "bar3" );

			  props = GetRelationshipProperties( inserter, theRel );
			  assertEquals( "bar3", props["foo"] );
			  assertEquals( 2, props.Count );
			  assertEquals( "bar3", props["foo"] );
			  assertEquals( "bar2", props["foo2"] );

			  inserter.Shutdown();
			  inserter = NewBatchInserter();

			  props = GetRelationshipProperties( inserter, theRel );
			  assertEquals( "bar3", props["foo"] );
			  assertEquals( 2, props.Count );
			  assertEquals( "bar3", props["foo"] );
			  assertEquals( "bar2", props["foo2"] );

			  inserter.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeHasProperty()
		 public virtual void TestNodeHasProperty()
		 {
			  BatchInserter inserter = _globalInserter;

			  long theNode = inserter.CreateNode( _properties );
			  long anotherNode = inserter.createNode( Collections.emptyMap() );
			  long relationship = inserter.CreateRelationship( theNode, anotherNode, RelationshipType.withName( "foo" ), _properties );
			  foreach ( string key in _properties.Keys )
			  {
					assertTrue( inserter.NodeHasProperty( theNode, key ) );
					assertFalse( inserter.NodeHasProperty( theNode, key + "-" ) );
					assertTrue( inserter.RelationshipHasProperty( relationship, key ) );
					assertFalse( inserter.RelationshipHasProperty( relationship, key + "-" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRemoveProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestRemoveProperties()
		 {
			  BatchInserter inserter = NewBatchInserter();

			  long theNode = inserter.CreateNode( _properties );
			  long anotherNode = inserter.createNode( Collections.emptyMap() );
			  long relationship = inserter.CreateRelationship( theNode, anotherNode, RelationshipType.withName( "foo" ), _properties );

			  inserter.RemoveNodeProperty( theNode, "key0" );
			  inserter.RemoveRelationshipProperty( relationship, "key1" );

			  foreach ( string key in _properties.Keys )
			  {
					switch ( key )
					{
						 case "key0":
							  assertFalse( inserter.NodeHasProperty( theNode, key ) );
							  assertTrue( inserter.RelationshipHasProperty( relationship, key ) );
							  break;
						 case "key1":
							  assertTrue( inserter.NodeHasProperty( theNode, key ) );
							  assertFalse( inserter.RelationshipHasProperty( relationship, key ) );
							  break;
						 default:
							  assertTrue( inserter.NodeHasProperty( theNode, key ) );
							  assertTrue( inserter.RelationshipHasProperty( relationship, key ) );
							  break;
					}
			  }
			  inserter.Shutdown();
			  inserter = NewBatchInserter();

			  foreach ( string key in _properties.Keys )
			  {
					switch ( key )
					{
						 case "key0":
							  assertFalse( inserter.NodeHasProperty( theNode, key ) );
							  assertTrue( inserter.RelationshipHasProperty( relationship, key ) );
							  break;
						 case "key1":
							  assertTrue( inserter.NodeHasProperty( theNode, key ) );
							  assertFalse( inserter.RelationshipHasProperty( relationship, key ) );
							  break;
						 default:
							  assertTrue( inserter.NodeHasProperty( theNode, key ) );
							  assertTrue( inserter.RelationshipHasProperty( relationship, key ) );
							  break;
					}
			  }
			  inserter.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRemoveDynamicProperty()
		 public virtual void ShouldBeAbleToRemoveDynamicProperty()
		 {
			  // Only triggered if assertions are enabled

			  // GIVEN
			  BatchInserter batchInserter = _globalInserter;
			  string key = "tags";
			  long nodeId = batchInserter.CreateNode( MapUtil.map( key, new string[] { "one", "two", "three" } ) );

			  // WHEN
			  batchInserter.RemoveNodeProperty( nodeId, key );

			  // THEN
			  assertFalse( batchInserter.GetNodeProperties( nodeId ).ContainsKey( key ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToOverwriteDynamicProperty()
		 public virtual void ShouldBeAbleToOverwriteDynamicProperty()
		 {
			  // Only triggered if assertions are enabled

			  // GIVEN
			  BatchInserter batchInserter = _globalInserter;
			  string key = "tags";
			  long nodeId = batchInserter.CreateNode( MapUtil.map( key, new string[] { "one", "two", "three" } ) );

			  // WHEN
			  string[] secondValue = new string[] { "four", "five", "six" };
			  batchInserter.SetNodeProperty( nodeId, key, secondValue );

			  // THEN
			  assertTrue( Arrays.Equals( secondValue, ( string[] ) GetNodeProperties( batchInserter, nodeId )[key] ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMore()
		 public virtual void TestMore()
		 {
			  BatchInserter graphDb = _globalInserter;
			  long startNode = graphDb.CreateNode( _properties );
			  long[] endNodes = new long[25];
			  ISet<long> rels = new HashSet<long>();
			  for ( int i = 0; i < 25; i++ )
			  {
					endNodes[i] = graphDb.CreateNode( _properties );
					rels.Add( graphDb.CreateRelationship( startNode, endNodes[i], _relTypeArray[i % 5], _properties ) );
			  }
			  foreach ( BatchRelationship rel in graphDb.GetRelationships( startNode ) )
			  {
					assertTrue( rels.Contains( rel.Id ) );
					assertEquals( rel.StartNode, startNode );
			  }
			  graphDb.SetNodeProperties( startNode, _properties );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureLoopsCanBeCreated() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MakeSureLoopsCanBeCreated()
		 {
			  BatchInserter graphDb = NewBatchInserter();
			  long startNode = graphDb.CreateNode( _properties );
			  long otherNode = graphDb.CreateNode( _properties );
			  long selfRelationship = graphDb.CreateRelationship( startNode, startNode, _relTypeArray[0], _properties );
			  long relationship = graphDb.CreateRelationship( startNode, otherNode, _relTypeArray[0], _properties );
			  foreach ( BatchRelationship rel in graphDb.GetRelationships( startNode ) )
			  {
					if ( rel.Id == selfRelationship )
					{
						 assertEquals( startNode, rel.StartNode );
						 assertEquals( startNode, rel.EndNode );
					}
					else if ( rel.Id == relationship )
					{
						 assertEquals( startNode, rel.StartNode );
						 assertEquals( otherNode, rel.EndNode );
					}
					else
					{
						 fail( "Unexpected relationship " + rel.Id );
					}
			  }

			  IGraphDatabaseService db = SwitchToEmbeddedGraphDatabaseService( graphDb );

			  try
			  {
					  using ( Transaction ignored = Db.beginTx() )
					  {
						Node realStartNode = Db.getNodeById( startNode );
						Relationship realSelfRelationship = Db.getRelationshipById( selfRelationship );
						Relationship realRelationship = Db.getRelationshipById( relationship );
						assertEquals( realSelfRelationship, realStartNode.GetSingleRelationship( RelTypes.RelType1, Direction.INCOMING ) );
						assertEquals( asSet( realSelfRelationship, realRelationship ), Iterables.asSet( realStartNode.GetRelationships( Direction.OUTGOING ) ) );
						assertEquals( asSet( realSelfRelationship, realRelationship ), Iterables.asSet( realStartNode.Relationships ) );
					  }
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createBatchNodeAndRelationshipsDeleteAllInEmbedded() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateBatchNodeAndRelationshipsDeleteAllInEmbedded()
		 {
			  /*
			   *    ()--[REL_TYPE1]-->(node)--[BATCH_TEST]->()
			   */

			  BatchInserter inserter = NewBatchInserter();
			  long nodeId = inserter.CreateNode( null );
			  inserter.CreateRelationship( nodeId, inserter.CreateNode( null ), RelTypes.BatchTest, null );
			  inserter.CreateRelationship( inserter.CreateNode( null ), nodeId, RelTypes.RelType1, null );

			  // Delete node and all its relationships
			  IGraphDatabaseService db = SwitchToEmbeddedGraphDatabaseService( inserter );

			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.getNodeById( nodeId );
					foreach ( Relationship relationship in node.Relationships )
					{
						 relationship.Delete();
					}
					node.Delete();
					tx.Success();
			  }

			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void messagesLogGetsClosed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MessagesLogGetsClosed()
		 {
			  File storeDir = LocalTestDirectory.databaseDir();
			  BatchInserter inserter = BatchInserters.inserter( storeDir, FileSystemRule.get(), stringMap() );
			  inserter.Shutdown();
			  assertTrue( ( new File( storeDir, INTERNAL_LOG_FILE ) ).delete() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createEntitiesWithEmptyPropertiesMap()
		 public virtual void CreateEntitiesWithEmptyPropertiesMap()
		 {
			  BatchInserter inserter = _globalInserter;

			  // Assert for node
			  long nodeId = inserter.createNode( map() );
			  GetNodeProperties( inserter, nodeId );
			  //cp=N U http://www.w3.org/1999/02/22-rdf-syntax-ns#type, c=N

			  // Assert for relationship
			  long anotherNodeId = inserter.CreateNode( null );
			  long relId = inserter.CreateRelationship( nodeId, anotherNodeId, RelTypes.BatchTest, map() );
			  inserter.GetRelationshipProperties( relId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createEntitiesWithDynamicPropertiesMap()
		 public virtual void CreateEntitiesWithDynamicPropertiesMap()
		 {
			  BatchInserter inserter = _globalInserter;

			  SetAndGet( inserter, "http://www.w3.org/1999/02/22-rdf-syntax-ns#type" );
			  SetAndGet( inserter, IntArray() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddInitialLabelsToCreatedNode()
		 public virtual void ShouldAddInitialLabelsToCreatedNode()
		 {
			  // GIVEN
			  BatchInserter inserter = _globalInserter;

			  // WHEN
			  long node = inserter.createNode( map(), Labels.First, Labels.Second );

			  // THEN
			  assertTrue( inserter.NodeHasLabel( node, Labels.First ) );
			  assertTrue( inserter.NodeHasLabel( node, Labels.Second ) );
			  assertFalse( inserter.NodeHasLabel( node, Labels.Third ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetNodeLabels()
		 public virtual void ShouldGetNodeLabels()
		 {
			  // GIVEN
			  BatchInserter inserter = _globalInserter;
			  long node = inserter.createNode( map(), Labels.First, Labels.Third );

			  // WHEN
			  IEnumerable<string> labelNames = AsNames( inserter.GetNodeLabels( node ) );

			  // THEN
			  assertEquals( asSet( Labels.First.name(), Labels.Third.name() ), Iterables.asSet(labelNames) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddManyInitialLabelsAsDynamicRecords()
		 public virtual void ShouldAddManyInitialLabelsAsDynamicRecords()
		 {
			  // GIVEN
			  BatchInserter inserter = _globalInserter;
			  Pair<Label[], ISet<string>> labels = ManyLabels( 200 );
			  long node = inserter.createNode( map(), labels.First() );
			  ForceFlush( inserter );

			  // WHEN
			  IEnumerable<string> labelNames = AsNames( inserter.GetNodeLabels( node ) );

			  // THEN
			  assertEquals( labels.Other(), Iterables.asSet(labelNames) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReplaceExistingInlinedLabelsWithDynamic()
		 public virtual void ShouldReplaceExistingInlinedLabelsWithDynamic()
		 {
			  // GIVEN
			  BatchInserter inserter = _globalInserter;
			  long node = inserter.createNode( map(), Labels.First );

			  // WHEN
			  Pair<Label[], ISet<string>> labels = ManyLabels( 100 );
			  inserter.SetNodeLabels( node, labels.First() );

			  // THEN
			  IEnumerable<string> labelNames = AsNames( inserter.GetNodeLabels( node ) );
			  assertEquals( labels.Other(), Iterables.asSet(labelNames) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReplaceExistingDynamicLabelsWithInlined()
		 public virtual void ShouldReplaceExistingDynamicLabelsWithInlined()
		 {
			  // GIVEN
			  BatchInserter inserter = _globalInserter;
			  long node = inserter.createNode( map(), ManyLabels(150).first() );

			  // WHEN
			  inserter.SetNodeLabels( node, Labels.First );

			  // THEN
			  IEnumerable<string> labelNames = AsNames( inserter.GetNodeLabels( node ) );
			  assertEquals( asSet( Labels.First.name() ), Iterables.asSet(labelNames) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateDeferredSchemaIndexesInEmptyDatabase() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateDeferredSchemaIndexesInEmptyDatabase()
		 {
			  // GIVEN
			  BatchInserter inserter = NewBatchInserter();

			  // WHEN
			  IndexDefinition definition = inserter.CreateDeferredSchemaIndex( label( "Hacker" ) ).on( "handle" ).create();

			  // THEN
			  assertEquals( "Hacker", single( definition.Labels ).name() );
			  assertEquals( asCollection( iterator( "handle" ) ), Iterables.asCollection( definition.PropertyKeys ) );
			  inserter.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateDeferredUniquenessConstraintInEmptyDatabase() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateDeferredUniquenessConstraintInEmptyDatabase()
		 {
			  // GIVEN
			  BatchInserter inserter = NewBatchInserter();

			  // WHEN
			  ConstraintDefinition definition = inserter.CreateDeferredConstraint( label( "Hacker" ) ).assertPropertyIsUnique( "handle" ).create();

			  // THEN
			  assertEquals( "Hacker", definition.Label.name() );
			  assertEquals( ConstraintType.UNIQUENESS, definition.ConstraintType );
			  assertEquals( asSet( "handle" ), Iterables.asSet( definition.PropertyKeys ) );
			  inserter.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateConsistentUniquenessConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateConsistentUniquenessConstraint()
		 {
			  // given
			  BatchInserter inserter = NewBatchInserter();

			  // when
			  inserter.CreateDeferredConstraint( label( "Hacker" ) ).assertPropertyIsUnique( "handle" ).create();

			  // then
			  GraphDatabaseAPI graphdb = ( GraphDatabaseAPI ) SwitchToEmbeddedGraphDatabaseService( inserter );
			  try
			  {
					NeoStores neoStores = graphdb.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores();
					SchemaStore store = neoStores.SchemaStore;
					SchemaStorage storage = new SchemaStorage( store );
					IList<long> inUse = new List<long>();
					DynamicRecord record = store.NextRecord();
					for ( long i = 1, high = store.HighestPossibleIdInUse; i <= high; i++ )
					{
						 store.GetRecord( i, record, RecordLoad.FORCE );
						 if ( record.InUse() && record.StartRecord )
						 {
							  inUse.Add( i );
						 }
					}
					assertEquals( "records in use", 2, inUse.Count );
					SchemaRule rule0 = storage.LoadSingleSchemaRule( inUse[0] );
					SchemaRule rule1 = storage.LoadSingleSchemaRule( inUse[1] );
					StoreIndexDescriptor indexRule;
					ConstraintRule constraintRule;
					if ( rule0 is StoreIndexDescriptor )
					{
						 indexRule = ( StoreIndexDescriptor ) rule0;
						 constraintRule = ( ConstraintRule ) rule1;
					}
					else
					{
						 constraintRule = ( ConstraintRule ) rule0;
						 indexRule = ( StoreIndexDescriptor ) rule1;
					}
					assertEquals( "index should reference constraint", constraintRule.Id, indexRule.OwningConstraint.Value );
					assertEquals( "constraint should reference index", indexRule.Id, constraintRule.OwnedIndex );
			  }
			  finally
			  {
					graphdb.Shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowCreationOfDuplicateIndex()
		 public virtual void ShouldNotAllowCreationOfDuplicateIndex()
		 {
			  // GIVEN
			  BatchInserter inserter = _globalInserter;
			  string labelName = "Hacker1-" + _denseNodeThreshold;

			  // WHEN
			  inserter.CreateDeferredSchemaIndex( label( labelName ) ).on( "handle" ).create();

			  try
			  {
					inserter.CreateDeferredSchemaIndex( label( labelName ) ).on( "handle" ).create();
					fail( "Should have thrown exception." );
			  }
			  catch ( ConstraintViolationException )
			  {
					// THEN Good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowCreationOfDuplicateConstraint()
		 public virtual void ShouldNotAllowCreationOfDuplicateConstraint()
		 {
			  // GIVEN
			  BatchInserter inserter = _globalInserter;
			  string labelName = "Hacker2-" + _denseNodeThreshold;

			  // WHEN
			  inserter.CreateDeferredConstraint( label( labelName ) ).assertPropertyIsUnique( "handle" ).create();

			  try
			  {
					inserter.CreateDeferredConstraint( label( labelName ) ).assertPropertyIsUnique( "handle" ).create();
					fail( "Should have thrown exception." );
			  }
			  catch ( ConstraintViolationException )
			  {
					// THEN Good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowCreationOfDeferredSchemaConstraintAfterIndexOnSameKeys()
		 public virtual void ShouldNotAllowCreationOfDeferredSchemaConstraintAfterIndexOnSameKeys()
		 {
			  // GIVEN
			  BatchInserter inserter = _globalInserter;
			  string labelName = "Hacker3-" + _denseNodeThreshold;

			  // WHEN
			  inserter.CreateDeferredSchemaIndex( label( labelName ) ).on( "handle" ).create();

			  try
			  {
					inserter.CreateDeferredConstraint( label( labelName ) ).assertPropertyIsUnique( "handle" ).create();
					fail( "Should have thrown exception." );
			  }
			  catch ( ConstraintViolationException )
			  {
					// THEN Good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowCreationOfDeferredSchemaIndexAfterConstraintOnSameKeys()
		 public virtual void ShouldNotAllowCreationOfDeferredSchemaIndexAfterConstraintOnSameKeys()
		 {
			  // GIVEN
			  BatchInserter inserter = _globalInserter;
			  string labelName = "Hacker4-" + _denseNodeThreshold;

			  // WHEN
			  inserter.CreateDeferredConstraint( label( labelName ) ).assertPropertyIsUnique( "handle" ).create();

			  try
			  {
					inserter.CreateDeferredSchemaIndex( label( labelName ) ).on( "handle" ).create();
					fail( "Should have thrown exception." );
			  }
			  catch ( ConstraintViolationException )
			  {
					// THEN Good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRunIndexPopulationJobAtShutdown() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRunIndexPopulationJobAtShutdown()
		 {
			  // GIVEN
			  IndexPopulator populator = mock( typeof( IndexPopulator ) );
			  IndexProvider provider = mock( typeof( IndexProvider ) );
			  IndexAccessor accessor = mock( typeof( IndexAccessor ) );

			  when( provider.ProviderDescriptor ).thenReturn( _descriptor );
			  when( provider.GetPopulator( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ), any() ) ).thenReturn(populator);
			  when( populator.SampleResult() ).thenReturn(new IndexSample());
			  when( provider.GetOnlineAccessor( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ) ) ).thenReturn( accessor );
			  when( provider.Bless( any( typeof( IndexDescriptor ) ) ) ).thenCallRealMethod();

			  BatchInserter inserter = NewBatchInserterWithIndexProvider( singleInstanceIndexProviderFactory( _key, provider ), provider.ProviderDescriptor );

			  inserter.CreateDeferredSchemaIndex( label( "Hacker" ) ).on( "handle" ).create();

			  long nodeId = inserter.createNode( map( "handle", "Jakewins" ), label( "Hacker" ) );

			  // WHEN
			  inserter.Shutdown();

			  // THEN
			  verify( provider ).init();
			  verify( provider ).start();
			  verify( provider ).getPopulator( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ), any() );
			  verify( populator ).create();
			  verify( populator ).add( argThat( matchesCollection( add( nodeId, _internalIndex.schema(), Values.of("Jakewins") ) ) ) );
			  verify( populator ).verifyDeferredConstraints( any( typeof( NodePropertyAccessor ) ) );
			  verify( populator ).close( true );
			  verify( provider ).stop();
			  verify( provider ).shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRunConstraintPopulationJobAtShutdown() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRunConstraintPopulationJobAtShutdown()
		 {
			  // GIVEN
			  IndexPopulator populator = mock( typeof( IndexPopulator ) );
			  IndexProvider provider = mock( typeof( IndexProvider ) );
			  IndexAccessor accessor = mock( typeof( IndexAccessor ) );

			  when( provider.ProviderDescriptor ).thenReturn( _descriptor );
			  when( provider.GetPopulator( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ), any() ) ).thenReturn(populator);
			  when( populator.SampleResult() ).thenReturn(new IndexSample());
			  when( provider.GetOnlineAccessor( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ) ) ).thenReturn( accessor );
			  when( provider.Bless( any( typeof( IndexDescriptor ) ) ) ).thenCallRealMethod();

			  BatchInserter inserter = NewBatchInserterWithIndexProvider( singleInstanceIndexProviderFactory( _key, provider ), provider.ProviderDescriptor );

			  inserter.CreateDeferredConstraint( label( "Hacker" ) ).assertPropertyIsUnique( "handle" ).create();

			  long nodeId = inserter.createNode( map( "handle", "Jakewins" ), label( "Hacker" ) );

			  // WHEN
			  inserter.Shutdown();

			  // THEN
			  verify( provider ).init();
			  verify( provider ).start();
			  verify( provider ).getPopulator( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ), any() );
			  verify( populator ).create();
			  verify( populator ).add( argThat( matchesCollection( add( nodeId, _internalUniqueIndex.schema(), Values.of("Jakewins") ) ) ) );
			  verify( populator ).verifyDeferredConstraints( any( typeof( NodePropertyAccessor ) ) );
			  verify( populator ).close( true );
			  verify( provider ).stop();
			  verify( provider ).shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRepopulatePreexistingIndexed() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRepopulatePreexistingIndexed()
		 {
			  // GIVEN
			  long jakewins = DbWithIndexAndSingleIndexedNode();

			  IndexPopulator populator = mock( typeof( IndexPopulator ) );
			  IndexProvider provider = mock( typeof( IndexProvider ) );
			  IndexAccessor accessor = mock( typeof( IndexAccessor ) );

			  when( provider.ProviderDescriptor ).thenReturn( _descriptor );
			  when( provider.GetPopulator( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ), any() ) ).thenReturn(populator);
			  when( populator.SampleResult() ).thenReturn(new IndexSample());
			  when( provider.GetOnlineAccessor( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ) ) ).thenReturn( accessor );

			  BatchInserter inserter = NewBatchInserterWithIndexProvider( singleInstanceIndexProviderFactory( _key, provider ), provider.ProviderDescriptor );

			  long boggle = inserter.createNode( map( "handle", "b0ggl3" ), label( "Hacker" ) );

			  // WHEN
			  inserter.Shutdown();

			  // THEN
			  verify( provider ).init();
			  verify( provider ).start();
			  verify( provider ).getPopulator( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ), any() );
			  verify( populator ).create();
			  verify( populator ).add( argThat( matchesCollection( add( jakewins, _internalIndex.schema(), Values.of("Jakewins") ), add(boggle, _internalIndex.schema(), Values.of("b0ggl3")) ) ) );
			  verify( populator ).verifyDeferredConstraints( any( typeof( NodePropertyAccessor ) ) );
			  verify( populator ).close( true );
			  verify( provider ).stop();
			  verify( provider ).shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPopulateLabelScanStoreOnShutdown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPopulateLabelScanStoreOnShutdown()
		 {
			  // GIVEN
			  // -- a database and a mocked label scan store
			  BatchInserter inserter = NewBatchInserter();

			  // -- and some data that we insert
			  long node1 = inserter.createNode( null, Labels.First );
			  long node2 = inserter.createNode( null, Labels.Second );
			  long node3 = inserter.createNode( null, Labels.Third );
			  long node4 = inserter.createNode( null, Labels.First, Labels.Second );
			  long node5 = inserter.createNode( null, Labels.First, Labels.Third );

			  // WHEN we shut down the batch inserter
			  LabelScanStore labelScanStore = LabelScanStore;
			  inserter.Shutdown();

			  labelScanStore.Init();
			  labelScanStore.Start();

			  // THEN the label scan store should receive all the updates.
			  // of course, we don't know the label ids at this point, but we're assuming 0..2 (bad boy)
			  AssertLabelScanStoreContains( labelScanStore, 0, node1, node4, node5 );
			  AssertLabelScanStoreContains( labelScanStore, 1, node2, node4 );
			  AssertLabelScanStoreContains( labelScanStore, 2, node3, node5 );

			  labelScanStore.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void propertiesCanBeReSetUsingBatchInserter()
		 public virtual void PropertiesCanBeReSetUsingBatchInserter()
		 {
			  // GIVEN
			  BatchInserter batchInserter = _globalInserter;
			  IDictionary<string, object> props = new Dictionary<string, object>();
			  props["name"] = "One";
			  props["count"] = 1;
			  props["tags"] = new string[] { "one", "two" };
			  props["something"] = "something";
			  long nodeId = batchInserter.CreateNode( props );
			  batchInserter.SetNodeProperty( nodeId, "name", "NewOne" );
			  batchInserter.RemoveNodeProperty( nodeId, "count" );
			  batchInserter.RemoveNodeProperty( nodeId, "something" );

			  // WHEN setting new properties
			  batchInserter.SetNodeProperty( nodeId, "name", "YetAnotherOne" );
			  batchInserter.SetNodeProperty( nodeId, "additional", "something" );

			  // THEN there should be no problems doing so
			  assertEquals( "YetAnotherOne", batchInserter.GetNodeProperties( nodeId )["name"] );
			  assertEquals( "something", batchInserter.GetNodeProperties( nodeId )["additional"] );
		 }

		 /// <summary>
		 /// Test checks that during node property set we will cleanup not used property records
		 /// During initial node creation properties will occupy 5 property records.
		 /// Last property record will have only empty array for email.
		 /// During first update email property will be migrated to dynamic property and last property record will become
		 /// empty. That record should be deleted form property chain or otherwise on next node load user will get an
		 /// property record not in use exception.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCleanupEmptyPropertyRecords() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestCleanupEmptyPropertyRecords()
		 {
			  BatchInserter inserter = _globalInserter;

			  IDictionary<string, object> properties = new Dictionary<string, object>();
			  properties["id"] = 1099511659993L;
			  properties["firstName"] = "Edward";
			  properties["lastName"] = "Shevchenko";
			  properties["gender"] = "male";
			  properties["birthday"] = ( new SimpleDateFormat( "yyyy-MM-dd" ) ).parse( "1987-11-08" ).Time;
			  properties["birthday_month"] = 11;
			  properties["birthday_day"] = 8;
			  long time = ( new SimpleDateFormat( "yyyy-MM-dd'T'HH:mm:ss.SSSZ" ) ).parse( "2010-04-22T18:05:40.912+0000" ).Time;
			  properties["creationDate"] = time;
			  properties["locationIP"] = "46.151.255.205";
			  properties["browserUsed"] = "Firefox";
			  properties["email"] = new string[0];
			  properties["languages"] = new string[0];
			  long personNodeId = inserter.CreateNode( properties );

			  assertEquals( "Shevchenko", GetNodeProperties( inserter, personNodeId )["lastName"] );
			  assertThat( ( string[] ) GetNodeProperties( inserter, personNodeId )["email"], @is( emptyArray() ) );

			  inserter.SetNodeProperty( personNodeId, "email", new string[]{ "Edward1099511659993@gmail.com" } );
			  assertThat( ( string[] ) GetNodeProperties( inserter, personNodeId )["email"], arrayContaining( "Edward1099511659993@gmail.com" ) );

			  inserter.SetNodeProperty( personNodeId, "email", new string[]{ "Edward1099511659993@gmail.com", "backup@gmail.com" } );

			  assertThat( ( string[] ) GetNodeProperties( inserter, personNodeId )["email"], arrayContaining( "Edward1099511659993@gmail.com", "backup@gmail.com" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void propertiesCanBeReSetUsingBatchInserter2()
		 public virtual void PropertiesCanBeReSetUsingBatchInserter2()
		 {
			  // GIVEN
			  BatchInserter batchInserter = _globalInserter;
			  long id = batchInserter.createNode( new Dictionary<>() );

			  // WHEN
			  batchInserter.SetNodeProperty( id, "test", "looooooooooong test" );
			  batchInserter.SetNodeProperty( id, "test", "small test" );

			  // THEN
			  assertEquals( "small test", batchInserter.GetNodeProperties( id )["test"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void replaceWithBiggerPropertySpillsOverIntoNewPropertyRecord()
		 public virtual void ReplaceWithBiggerPropertySpillsOverIntoNewPropertyRecord()
		 {
			  // GIVEN
			  BatchInserter batchInserter = _globalInserter;
			  IDictionary<string, object> props = new Dictionary<string, object>();
			  props["name"] = "One";
			  props["count"] = 1;
			  props["tags"] = new string[] { "one", "two" };
			  long id = batchInserter.CreateNode( props );
			  batchInserter.SetNodeProperty( id, "name", "NewOne" );

			  // WHEN
			  batchInserter.SetNodeProperty( id, "count", "something" );

			  // THEN
			  assertEquals( "something", batchInserter.GetNodeProperties( id )["count"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustSplitUpRelationshipChainsWhenCreatingDenseNodes()
		 public virtual void MustSplitUpRelationshipChainsWhenCreatingDenseNodes()
		 {
			  BatchInserter inserter = _globalInserter;

			  long node1 = inserter.CreateNode( null );
			  long node2 = inserter.CreateNode( null );

			  for ( int i = 0; i < 1000; i++ )
			  {
					foreach ( MyRelTypes relType in Enum.GetValues( typeof( MyRelTypes ) ) )
					{
						 inserter.CreateRelationship( node1, node2, relType, null );
					}
			  }

			  NeoStores neoStores = GetFlushedNeoStores( inserter );
			  NodeRecord record = getRecord( neoStores.NodeStore, node1 );
			  assertTrue( "Node " + record + " should have been dense", record.Dense );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetRelationships()
		 public virtual void ShouldGetRelationships()
		 {
			  // GIVEN
			  BatchInserter inserter = _globalInserter;
			  long node = inserter.CreateNode( null );
			  CreateRelationships( inserter, node, RelTypes.RelType1, 3 );
			  CreateRelationships( inserter, node, RelTypes.RelType2, 4 );

			  // WHEN
			  ISet<long> gottenRelationships = Iterables.asSet( inserter.GetRelationshipIds( node ) );

			  // THEN
			  assertEquals( 21, gottenRelationships.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCreateSameLabelTwiceOnSameNode()
		 public virtual void ShouldNotCreateSameLabelTwiceOnSameNode()
		 {
			  // GIVEN
			  BatchInserter inserter = _globalInserter;

			  // WHEN
			  long nodeId = inserter.createNode( map( "itemId", 1000L ), label( "Item" ), label( "Item" ) );

			  // THEN
			  NodeStore nodeStore = GetFlushedNeoStores( inserter ).NodeStore;
			  NodeRecord node = nodeStore.GetRecord( nodeId, nodeStore.NewRecord(), NORMAL );
			  NodeLabels labels = NodeLabelsField.parseLabelsField( node );
			  long[] labelIds = labels.Get( nodeStore );
			  assertEquals( 1, labelIds.Length );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSortLabelIdsWhenGetOrCreate()
		 public virtual void ShouldSortLabelIdsWhenGetOrCreate()
		 {
			  // GIVEN
			  BatchInserter inserter = _globalInserter;

			  // WHEN
			  long nodeId = inserter.createNode( map( "Item", 123456789123L ), label( "AA" ), label( "BB" ), label( "CC" ), label( "DD" ) );
			  inserter.SetNodeLabels( nodeId, label( "CC" ), label( "AA" ), label( "DD" ), label( "EE" ), label( "FF" ) );

			  // THEN
			  NodeStore nodeStore = GetFlushedNeoStores( inserter ).NodeStore;
			  NodeRecord node = nodeStore.GetRecord( nodeId, nodeStore.NewRecord(), RecordLoad.NORMAL );
			  NodeLabels labels = NodeLabelsField.parseLabelsField( node );

			  long[] labelIds = labels.Get( nodeStore );
			  long[] sortedLabelIds = labelIds.Clone();
			  Arrays.sort( sortedLabelIds );
			  assertArrayEquals( sortedLabelIds, labelIds );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateUniquenessConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateUniquenessConstraint()
		 {
			  // Given
			  Label label = label( "Person" );
			  string propertyKey = "name";
			  string duplicatedValue = "Tom";

			  BatchInserter inserter = NewBatchInserter();

			  // When
			  inserter.CreateDeferredConstraint( label ).assertPropertyIsUnique( propertyKey ).create();

			  // Then
			  IGraphDatabaseService db = SwitchToEmbeddedGraphDatabaseService( inserter );
			  try
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 IList<ConstraintDefinition> constraints = Iterables.asList( Db.schema().Constraints );
						 assertEquals( 1, constraints.Count );
						 ConstraintDefinition constraint = constraints[0];
						 assertEquals( label.Name(), constraint.Label.name() );
						 assertEquals( propertyKey, single( constraint.PropertyKeys ) );

						 Db.createNode( label ).setProperty( propertyKey, duplicatedValue );

						 tx.Success();
					}

					using ( Transaction tx = Db.beginTx() )
					{
						 Db.createNode( label ).setProperty( propertyKey, duplicatedValue );
						 tx.Success();
					}
					fail( "Uniqueness property constraint was violated, exception expected" );
			  }
			  catch ( ConstraintViolationException e )
			  {
					assertEquals( format( "Node(0) already exists with label `%s` and property `%s` = '%s'", label.Name(), propertyKey, duplicatedValue ), e.Message );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowCreationOfUniquenessConstraintAndIndexOnSameLabelAndProperty()
		 public virtual void ShouldNotAllowCreationOfUniquenessConstraintAndIndexOnSameLabelAndProperty()
		 {
			  // Given
			  Label label = label( "Person1-" + _denseNodeThreshold );
			  string property = "name";

			  BatchInserter inserter = _globalInserter;

			  // When
			  inserter.CreateDeferredConstraint( label ).assertPropertyIsUnique( property ).create();
			  try
			  {
					inserter.CreateDeferredSchemaIndex( label ).on( property ).create();
					fail( "Exception expected" );
			  }
			  catch ( ConstraintViolationException e )
			  {
					// Then
					assertEquals( "Index for given {label;property} already exists", e.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowDuplicatedUniquenessConstraints()
		 public virtual void ShouldNotAllowDuplicatedUniquenessConstraints()
		 {
			  // Given
			  Label label = label( "Person2-" + _denseNodeThreshold );
			  string property = "name";

			  BatchInserter inserter = _globalInserter;

			  // When
			  inserter.CreateDeferredConstraint( label ).assertPropertyIsUnique( property ).create();
			  try
			  {
					inserter.CreateDeferredConstraint( label ).assertPropertyIsUnique( property ).create();
					fail( "Exception expected" );
			  }
			  catch ( ConstraintViolationException e )
			  {
					// Then
					assertEquals( "It is not allowed to create node keys, uniqueness constraints or indexes on the same {label;property}", e.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowDuplicatedIndexes()
		 public virtual void ShouldNotAllowDuplicatedIndexes()
		 {
			  // Given
			  Label label = label( "Person3-" + _denseNodeThreshold );
			  string property = "name";

			  BatchInserter inserter = _globalInserter;

			  // When
			  inserter.CreateDeferredSchemaIndex( label ).on( property ).create();
			  try
			  {
					inserter.CreateDeferredSchemaIndex( label ).on( property ).create();
					fail( "Exception expected" );
			  }
			  catch ( ConstraintViolationException e )
			  {
					// Then
					assertEquals( "Index for given {label;property} already exists", e.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void uniquenessConstraintShouldBeCheckedOnBatchInserterShutdownAndFailIfViolated() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UniquenessConstraintShouldBeCheckedOnBatchInserterShutdownAndFailIfViolated()
		 {
			  // Given
			  Label label = label( "Foo" );
			  string property = "Bar";
			  string value = "Baz";

			  BatchInserter inserter = NewBatchInserter();

			  // When
			  inserter.CreateDeferredConstraint( label ).assertPropertyIsUnique( property ).create();

			  inserter.createNode( Collections.singletonMap( property, value ), label );
			  inserter.createNode( Collections.singletonMap( property, value ), label );

			  // Then
			  IGraphDatabaseService db = SwitchToEmbeddedGraphDatabaseService( inserter );
			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						IndexDefinition index = Db.schema().getIndexes(label).GetEnumerator().next();
						string indexFailure = Db.schema().getIndexFailure(index);
						assertThat( indexFailure, containsString( "IndexEntryConflictException" ) );
						assertThat( indexFailure, containsString( value ) );
						tx.Success();
					  }
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldChangePropertiesInCurrentBatch()
		 public virtual void ShouldChangePropertiesInCurrentBatch()
		 {
			  // GIVEN
			  BatchInserter inserter = _globalInserter;
			  IDictionary<string, object> properties = map( "key1", "value1" );
			  long node = inserter.CreateNode( properties );

			  // WHEN
			  properties["additionalKey"] = "Additional value";
			  inserter.SetNodeProperties( node, properties );

			  // THEN
			  assertEquals( properties, GetNodeProperties( inserter, node ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreRemovingNonExistentNodeProperty()
		 public virtual void ShouldIgnoreRemovingNonExistentNodeProperty()
		 {
			  // given
			  BatchInserter inserter = _globalInserter;
			  long id = inserter.createNode( Collections.emptyMap() );

			  // when
			  inserter.RemoveNodeProperty( id, "non-existent" );

			  // then no exception should be thrown, this mimics IGraphDatabaseService behaviour
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreRemovingNonExistentRelationshipProperty()
		 public virtual void ShouldIgnoreRemovingNonExistentRelationshipProperty()
		 {
			  // given
			  BatchInserter inserter = _globalInserter;
			  IDictionary<string, object> noProperties = Collections.emptyMap();
			  long nodeId1 = inserter.CreateNode( noProperties );
			  long nodeId2 = inserter.CreateNode( noProperties );
			  long id = inserter.CreateRelationship( nodeId1, nodeId2, MyRelTypes.TEST, noProperties );

			  // when
			  inserter.RemoveRelationshipProperty( id, "non-existent" );

			  // then no exception should be thrown, this mimics IGraphDatabaseService behaviour
		 }

		 private IDictionary<string, string> Configuration()
		 {
			  return stringMap( GraphDatabaseSettings.dense_node_threshold.name(), _denseNodeThreshold.ToString() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Neo4Net.unsafe.batchinsert.BatchInserter newBatchInserter() throws Exception
		 private BatchInserter NewBatchInserter()
		 {
			  return BatchInserters.inserter( LocalTestDirectory.databaseDir(), FileSystemRule.get(), Configuration() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Neo4Net.unsafe.batchinsert.BatchInserter newBatchInserterWithIndexProvider(Neo4Net.kernel.extension.KernelExtensionFactory<?> provider, Neo4Net.Kernel.Api.Internal.Schema.IndexProviderDescriptor providerDescriptor) throws Exception
		 private BatchInserter NewBatchInserterWithIndexProvider<T1>( KernelExtensionFactory<T1> provider, IndexProviderDescriptor providerDescriptor )
		 {
			  IDictionary<string, string> configuration = configuration();
			  configuration[GraphDatabaseSettings.default_schema_provider.name()] = providerDescriptor.Name();
			  return BatchInserters.inserter( LocalTestDirectory.databaseDir(), FileSystemRule.get(), configuration, singletonList(provider) );
		 }

		 private IGraphDatabaseService SwitchToEmbeddedGraphDatabaseService( BatchInserter inserter )
		 {
			  inserter.Shutdown();
			  TestGraphDatabaseFactory factory = new TestGraphDatabaseFactory();
			  factory.FileSystem = FileSystemRule.get();
			  return factory.NewImpermanentDatabaseBuilder( LocalTestDirectory.databaseDir() ).setConfig(Configuration()).newGraphDatabase();
		 }

		 private LabelScanStore LabelScanStore
		 {
			 get
			 {
				  DefaultFileSystemAbstraction fs = FileSystemRule.get();
				  return new NativeLabelScanStore( PageCacheRule.getPageCache( fs ), LocalTestDirectory.databaseLayout(), fs, Neo4Net.Kernel.Impl.Api.scan.FullStoreChangeStream_Fields.Empty, true, new Monitors(), RecoveryCleanupWorkCollector.immediate() );
			 }
		 }

		 private void AssertLabelScanStoreContains( LabelScanStore labelScanStore, int labelId, params long[] nodes )
		 {
			  using ( LabelScanReader labelScanReader = labelScanStore.NewReader() )
			  {
					IList<long> actualNodeIds = ExtractPrimitiveLongIteratorAsList( labelScanReader.NodesWithLabel( labelId ) );
					IList<long> expectedNodeIds = Arrays.stream( nodes ).boxed().collect(Collectors.toList());
					assertEquals( expectedNodeIds, actualNodeIds );
			  }
		 }

		 private IList<long> ExtractPrimitiveLongIteratorAsList( LongIterator longIterator )
		 {
			  IList<long> actualNodeIds = new List<long>();
			  while ( longIterator.hasNext() )
			  {
					actualNodeIds.Add( longIterator.next() );
			  }
			  return actualNodeIds;
		 }

		 private void CreateRelationships( BatchInserter inserter, long node, RelationshipType relType, int @out )
		 {
			  for ( int i = 0; i < @out; i++ )
			  {
					inserter.CreateRelationship( node, inserter.CreateNode( null ), relType, null );
			  }
			  for ( int i = 0; i < @out; i++ )
			  {
					inserter.CreateRelationship( inserter.CreateNode( null ), node, relType, null );
			  }
			  for ( int i = 0; i < @out; i++ )
			  {
					inserter.CreateRelationship( node, node, relType, null );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long dbWithIndexAndSingleIndexedNode() throws Exception
		 private long DbWithIndexAndSingleIndexedNode()
		 {
			  IndexPopulator populator = mock( typeof( IndexPopulator ) );
			  IndexProvider provider = mock( typeof( IndexProvider ) );

			  when( provider.ProviderDescriptor ).thenReturn( _descriptor );
			  when( provider.GetPopulator( any( typeof( StoreIndexDescriptor ) ), any( typeof( IndexSamplingConfig ) ), any() ) ).thenReturn(populator);
			  when( provider.Bless( any( typeof( IndexDescriptor ) ) ) ).thenCallRealMethod();

			  BatchInserter inserter = NewBatchInserterWithIndexProvider( singleInstanceIndexProviderFactory( _key, provider ), provider.ProviderDescriptor );

			  inserter.CreateDeferredSchemaIndex( label( "Hacker" ) ).on( "handle" ).create();
			  long nodeId = inserter.createNode( map( "handle", "Jakewins" ), label( "Hacker" ) );
			  inserter.Shutdown();
			  return nodeId;
		 }

		 private void SetAndGet( BatchInserter inserter, object value )
		 {
			  long nodeId = inserter.createNode( map( "key", value ) );
			  object readValue = inserter.GetNodeProperties( nodeId )["key"];
			  if ( readValue.GetType().IsArray )
			  {
					assertTrue( Arrays.Equals( ( int[] )value, ( int[] )readValue ) );
			  }
			  else
			  {
					assertEquals( value, readValue );
			  }
		 }

		 private int[] IntArray()
		 {
			  int length = 20;
			  int[] array = new int[length];
			  for ( int i = 0, startValue = ( int )Math.Pow( 2, 30 ); i < length; i++ )
			  {
					array[i] = startValue + i;
			  }
			  return array;
		 }

		 private Node GetNodeInTx( long nodeId, IGraphDatabaseService db )
		 {
			  using ( Transaction ignored = Db.beginTx() )
			  {
					return Db.getNodeById( nodeId );
			  }
		 }

		 private void ForceFlush( BatchInserter inserter )
		 {
			  ( ( BatchInserterImpl )inserter ).ForceFlushChanges();
		 }

		 private NeoStores GetFlushedNeoStores( BatchInserter inserter )
		 {
			  ForceFlush( inserter );
			  return ( ( BatchInserterImpl ) inserter ).NeoStores;
		 }

		 private enum Labels
		 {
			  First,
			  Second,
			  Third
		 }

		 private IEnumerable<string> AsNames( IEnumerable<Label> nodeLabels )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return map( Label::name, nodeLabels );
		 }

		 private Pair<Label[], ISet<string>> ManyLabels( int count )
		 {
			  Label[] labels = new Label[count];
			  ISet<string> expectedLabelNames = new HashSet<string>();
			  for ( int i = 0; i < labels.Length; i++ )
			  {
					string labelName = "bach label " + i;
					labels[i] = label( labelName );
					expectedLabelNames.Add( labelName );
			  }
			  return Pair.of( labels, expectedLabelNames );
		 }

		 private IDictionary<string, object> GetNodeProperties( BatchInserter inserter, long nodeId )
		 {
			  return inserter.GetNodeProperties( nodeId );
		 }

		 private IDictionary<string, object> GetRelationshipProperties( BatchInserter inserter, long relId )
		 {
			  return inserter.GetRelationshipProperties( relId );
		 }
	}

}