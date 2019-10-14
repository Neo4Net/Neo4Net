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
namespace Neo4Net.Consistency
{
	using MutableObject = org.apache.commons.lang3.mutable.MutableObject;
	using Arrays = org.bouncycastle.util.Arrays;
	using ImmutableLongList = org.eclipse.collections.api.list.primitive.ImmutableLongList;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using ConsistencyCheckIncompleteException = Neo4Net.Consistency.checking.full.ConsistencyCheckIncompleteException;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseFactory = Neo4Net.Graphdb.factory.GraphDatabaseFactory;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using Neo4Net.Index.@internal.gbptree;
	using GBPTreeBootstrapper = Neo4Net.Index.@internal.gbptree.GBPTreeBootstrapper;
	using GBPTreeCorruption = Neo4Net.Index.@internal.gbptree.GBPTreeCorruption;
	using Neo4Net.Index.@internal.gbptree;
	using GBPTreePointerType = Neo4Net.Index.@internal.gbptree.GBPTreePointerType;
	using Neo4Net.Index.@internal.gbptree;
	using FileHandle = Neo4Net.Io.fs.FileHandle;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseFile = Neo4Net.Io.layout.DatabaseFile;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using SchemaLayouts = Neo4Net.Kernel.Impl.Index.Schema.SchemaLayouts;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.impl.muninn.StandalonePageCacheFactory.createPageCache;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitializedScheduler;

	public class ConsistencyCheckWithCorruptGBPTreeIT
	{
		private bool InstanceFieldsInitialized = false;

		public ConsistencyCheckWithCorruptGBPTreeIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _testDirectory ).around( _pageCacheRule ).around( _random );
		}

		 private static readonly Label _label = Label.label( "label" );
		 private const string PROP_KEY1 = "key1";
		 private PageCacheRule _pageCacheRule = new PageCacheRule();
		 private TestDirectory _testDirectory = TestDirectory.testDirectory();
		 private RandomRule _random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(testDirectory).around(pageCacheRule).around(random);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void assertTreeHeightIsAsExpected() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AssertTreeHeightIsAsExpected()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );
			  MutableObject<int> heightRef = new MutableObject<int>();
			  File[] indexFiles = SchemaIndexFiles();
			  CorruptIndexes( true, ( tree, inspection ) => heightRef.setValue( inspection.LastLevel ), indexFiles );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int height = heightRef.getValue();
			  int height = heightRef.Value;
			  assertEquals( "This test assumes height of index tree is 2 but height for this index was " + height + ". This is most easily regulated by changing number of nodes in setup.", 2, height );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCheckIndexesIfConfiguredNotTo() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCheckIndexesIfConfiguredNotTo()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );
			  MutableObject<long> targetNode = new MutableObject<long>();
			  File[] indexFiles = SchemaIndexFiles();
			  CorruptIndexes(true, (tree, inspection) =>
			  {
			  targetNode.Value = inspection.RootNode;
			  tree.@unsafe( GBPTreeCorruption.pageSpecificCorruption( targetNode.Value, GBPTreeCorruption.notATreeNode() ) );
			  }, indexFiles);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.configuration.Config config = config(org.neo4j.kernel.configuration.Settings.FALSE, org.neo4j.kernel.configuration.Settings.FALSE);
			  Config config = config( Settings.FALSE, Settings.FALSE );
			  ConsistencyCheckService.Result result = RunConsistencyCheck( config );

			  assertTrue( "Expected store to be consistent when not checking indexes.", result.Successful );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCheckIndexStructureEvenIfNotCheckingIndexes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCheckIndexStructureEvenIfNotCheckingIndexes()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );
			  MutableObject<long> targetNode = new MutableObject<long>();
			  File[] indexFiles = SchemaIndexFiles();
			  CorruptIndexes(true, (tree, inspection) =>
			  {
			  targetNode.Value = inspection.RootNode;
			  tree.@unsafe( GBPTreeCorruption.pageSpecificCorruption( targetNode.Value, GBPTreeCorruption.notATreeNode() ) );
			  }, indexFiles);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.configuration.Config config = config(org.neo4j.kernel.configuration.Settings.TRUE, org.neo4j.kernel.configuration.Settings.FALSE);
			  Config config = config( Settings.TRUE, Settings.FALSE );
			  ConsistencyCheckService.Result result = RunConsistencyCheck( config );

			  assertFalse( "Expected store to be inconsistent when checking index structure.", result.Successful );
			  AssertResultContainsMessage( result, "Page: " + targetNode.Value + " is not a tree node page" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCheckIndexStructureIfConfiguredNotToEvenIfCheckingIndexes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCheckIndexStructureIfConfiguredNotToEvenIfCheckingIndexes()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );
			  MutableObject<long> targetNode = new MutableObject<long>();
			  File[] indexFiles = SchemaIndexFiles();
			  CorruptIndexes(true, (tree, inspection) =>
			  {
			  targetNode.Value = inspection.RootNode;
			  tree.@unsafe( GBPTreeCorruption.addFreelistEntry( 5 ) );
			  }, indexFiles);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.configuration.Config config = config(org.neo4j.kernel.configuration.Settings.FALSE, org.neo4j.kernel.configuration.Settings.TRUE);
			  Config config = config( Settings.FALSE, Settings.TRUE );
			  ConsistencyCheckService.Result result = RunConsistencyCheck( config );

			  assertTrue( "Expected store to be consistent when not checking indexes.", result.Successful );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportProgress() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportProgress()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );

			  Writer writer = new StringWriter();
			  ProgressMonitorFactory factory = ProgressMonitorFactory.textual( writer );
			  ConsistencyCheckService.Result result = RunConsistencyCheck( factory );

			  assertTrue( "Expected new database to be clean.", result.Successful );
			  assertTrue( writer.ToString().Contains("Index structure consistency check") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void notATreeNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NotATreeNode()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );
			  MutableObject<long> targetNode = new MutableObject<long>();
			  File[] indexFiles = SchemaIndexFiles();
			  CorruptIndexes(true, (tree, inspection) =>
			  {
			  targetNode.Value = inspection.RootNode;
			  tree.@unsafe( GBPTreeCorruption.pageSpecificCorruption( targetNode.Value, GBPTreeCorruption.notATreeNode() ) );
			  }, indexFiles);

			  ConsistencyCheckService.Result result = RunConsistencyCheck();

			  assertFalse( "Expected store to be considered inconsistent.", result.Successful );
			  AssertResultContainsMessage( result, "Page: " + targetNode.Value + " is not a tree node page." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unknownTreeNodeType() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UnknownTreeNodeType()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );
			  MutableObject<long> targetNode = new MutableObject<long>();
			  File[] indexFiles = SchemaIndexFiles();
			  CorruptIndexes(true, (tree, inspection) =>
			  {
			  targetNode.Value = inspection.RootNode;
			  tree.@unsafe( GBPTreeCorruption.pageSpecificCorruption( targetNode.Value, GBPTreeCorruption.unknownTreeNodeType() ) );
			  }, indexFiles);

			  ConsistencyCheckService.Result result = RunConsistencyCheck();

			  assertFalse( "Expected store to be considered inconsistent.", result.Successful );
			  AssertResultContainsMessage( result, "Page: " + targetNode.Value + " has an unknown tree node type:" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void siblingsDontPointToEachOther() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SiblingsDontPointToEachOther()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );
			  MutableObject<long> targetNode = new MutableObject<long>();
			  File[] indexFiles = SchemaIndexFiles();
			  CorruptIndexes(true, (tree, inspection) =>
			  {
			  targetNode.Value = inspection.LeafNodes.get( 0 );
			  tree.@unsafe( GBPTreeCorruption.pageSpecificCorruption( targetNode.Value, GBPTreeCorruption.rightSiblingPointToNonExisting() ) );
			  }, indexFiles);

			  ConsistencyCheckService.Result result = RunConsistencyCheck();

			  assertFalse( "Expected store to be considered inconsistent.", result.Successful );
			  AssertResultContainsMessage( result, "Sibling pointers misaligned." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rightmostNodeHasRightSibling() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RightmostNodeHasRightSibling()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );
			  File[] indexFiles = SchemaIndexFiles();
			  CorruptIndexes(true, (tree, inspection) =>
			  {
			  long root = inspection.RootNode;
			  tree.@unsafe( GBPTreeCorruption.pageSpecificCorruption( root, GBPTreeCorruption.setPointer( GBPTreePointerType.rightSibling(), 10 ) ) );
			  }, indexFiles);

			  ConsistencyCheckService.Result result = RunConsistencyCheck();

			  assertFalse( "Expected store to be considered inconsistent.", result.Successful );
			  AssertResultContainsMessage( result, "Expected rightmost node to have no right sibling but was 10" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pointerToOldVersionOfTreeNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PointerToOldVersionOfTreeNode()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );
			  MutableObject<long> targetNode = new MutableObject<long>();
			  File[] indexFiles = SchemaIndexFiles();
			  CorruptIndexes(true, (tree, inspection) =>
			  {
			  targetNode.Value = inspection.RootNode;
			  tree.@unsafe( GBPTreeCorruption.pageSpecificCorruption( targetNode.Value, GBPTreeCorruption.setPointer( GBPTreePointerType.successor(), 6 ) ) );
			  }, indexFiles);

			  ConsistencyCheckService.Result result = RunConsistencyCheck();

			  assertFalse( "Expected store to be considered inconsistent.", result.Successful );
			  AssertResultContainsMessage( result, "We ended up on tree node " + targetNode.Value + " which has a newer generation, successor is: 6" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pointerHasLowerGenerationThanNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PointerHasLowerGenerationThanNode()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );
			  MutableObject<long> targetNode = new MutableObject<long>();
			  MutableObject<long> rightSibling = new MutableObject<long>();
			  File[] indexFiles = SchemaIndexFiles();
			  CorruptIndexes(true, (tree, inspection) =>
			  {
			  ImmutableLongList leafNodes = inspection.LeafNodes;
			  targetNode.Value = leafNodes.get( 0 );
			  rightSibling.Value = leafNodes.get( 1 );
			  tree.@unsafe( GBPTreeCorruption.pageSpecificCorruption( targetNode.Value, GBPTreeCorruption.rightSiblingPointerHasTooLowGeneration() ) );
			  }, indexFiles);

			  ConsistencyCheckService.Result result = RunConsistencyCheck();

			  assertFalse( "Expected store to be considered inconsistent.", result.Successful );
			  AssertResultContainsMessage( result, string.Format( "Pointer ({0}) in tree node {1:D} has pointer generation {2:D}, but target node {3:D} has a higher generation {4:D}.", GBPTreePointerType.rightSibling(), targetNode.Value, 1, rightSibling.Value, 4 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void keysOutOfOrderInNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void KeysOutOfOrderInNode()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );
			  MutableObject<long> targetNode = new MutableObject<long>();
			  File[] indexFiles = SchemaIndexFiles();
			  CorruptIndexes(true, (tree, inspection) =>
			  {
			  targetNode.Value = inspection.LeafNodes.get( 0 );
			  int keyCount = inspection.KeyCounts.get( targetNode.Value );
			  tree.@unsafe( GBPTreeCorruption.pageSpecificCorruption( targetNode.Value, GBPTreeCorruption.swapKeyOrderLeaf( 0, 1, keyCount ) ) );
			  }, indexFiles);

			  ConsistencyCheckService.Result result = RunConsistencyCheck();

			  assertFalse( "Expected store to be considered inconsistent.", result.Successful );
			  AssertResultContainsMessage( result, string.Format( "Keys in tree node {0:D} are out of order.", targetNode.Value ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void keysLocatedInWrongNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void KeysLocatedInWrongNode()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );
			  File[] indexFiles = SchemaIndexFiles();
			  CorruptIndexes(true, (tree, inspection) =>
			  {
			  long internalNode = inspection.NodesPerLevel.get( 1 ).get( 0 );
			  int keyCount = inspection.KeyCounts.get( internalNode );
			  tree.@unsafe( GBPTreeCorruption.pageSpecificCorruption( internalNode, GBPTreeCorruption.swapChildOrder( 0, 1, keyCount ) ) );
			  }, indexFiles);

			  ConsistencyCheckService.Result result = RunConsistencyCheck();

			  assertFalse( "Expected store to be considered inconsistent.", result.Successful );
			  AssertResultContainsMessage( result, "Expected range for this tree node is" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unusedPage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UnusedPage()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );
			  File[] indexFiles = SchemaIndexFiles();
			  CorruptIndexes(true, (tree, inspection) =>
			  {
			  long? internalNode = inspection.NodesPerLevel.get( 1 ).get( 0 );
			  int keyCount = inspection.KeyCounts.get( internalNode );
			  tree.@unsafe( GBPTreeCorruption.pageSpecificCorruption( internalNode,GBPTreeCorruption.setKeyCount( keyCount - 1 ) ) );
			  }, indexFiles);

			  ConsistencyCheckService.Result result = RunConsistencyCheck();

			  assertFalse( "Expected store to be considered inconsistent.", result.Successful );
			  AssertResultContainsMessage( result, "Index has a leaked page that will never be reclaimed, pageId=" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pageIdExceedLastId() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PageIdExceedLastId()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );
			  File[] indexFiles = SchemaIndexFiles();
			  CorruptIndexes(true, (tree, inspection) =>
			  {
			  tree.@unsafe( GBPTreeCorruption.decrementFreelistWritePos() );
			  }, indexFiles);

			  ConsistencyCheckService.Result result = RunConsistencyCheck();

			  assertFalse( "Expected store to be considered inconsistent.", result.Successful );
			  AssertResultContainsMessage( result, "Index has a leaked page that will never be reclaimed, pageId=" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nodeMetaInconsistency() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NodeMetaInconsistency()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );
			  File[] indexFiles = SchemaIndexFiles();
			  CorruptIndexes(true, (tree, inspection) =>
			  {
			  tree.@unsafe( GBPTreeCorruption.pageSpecificCorruption( inspection.RootNode, GBPTreeCorruption.decrementAllocOffsetInDynamicNode() ) );
			  }, indexFiles);

			  ConsistencyCheckService.Result result = RunConsistencyCheck();

			  assertFalse( "Expected store to be considered inconsistent.", result.Successful );
			  AssertResultContainsMessage( result, "has inconsistent meta data: Meta data for tree node is inconsistent" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pageIdSeenMultipleTimes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PageIdSeenMultipleTimes()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );
			  MutableObject<long> targetNode = new MutableObject<long>();
			  File[] indexFiles = SchemaIndexFiles();
			  CorruptIndexes(true, (tree, inspection) =>
			  {
			  targetNode.Value = inspection.RootNode;
			  tree.@unsafe( GBPTreeCorruption.addFreelistEntry( targetNode.Value ) );
			  }, indexFiles);

			  ConsistencyCheckService.Result result = RunConsistencyCheck();

			  assertFalse( "Expected store to be considered inconsistent.", result.Successful );
			  AssertResultContainsMessage( result, "Page id seen multiple times, this means either active tree node is present in freelist or pointers in tree create a loop, pageId=" + targetNode.Value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void crashPointer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CrashPointer()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );
			  MutableObject<long> targetNode = new MutableObject<long>();
			  File[] indexFiles = SchemaIndexFiles();
			  CorruptIndexes(false, (tree, inspection) =>
			  {
			  targetNode.Value = inspection.RootNode;
			  tree.@unsafe( GBPTreeCorruption.pageSpecificCorruption( targetNode.Value, GBPTreeCorruption.crashed( GBPTreePointerType.rightSibling() ) ) );
			  }, indexFiles);

			  ConsistencyCheckService.Result result = RunConsistencyCheck();

			  assertFalse( "Expected store to be considered inconsistent.", result.Successful );
			  AssertResultContainsMessage( result, "Crashed pointer found in tree node " + targetNode.Value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void brokenPointer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BrokenPointer()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );
			  MutableObject<long> targetNode = new MutableObject<long>();
			  File[] indexFiles = SchemaIndexFiles();
			  CorruptIndexes(true, (tree, inspection) =>
			  {
			  targetNode.Value = inspection.RootNode;
			  tree.@unsafe( GBPTreeCorruption.pageSpecificCorruption( targetNode.Value, GBPTreeCorruption.broken( GBPTreePointerType.leftSibling() ) ) );
			  }, indexFiles);

			  ConsistencyCheckService.Result result = RunConsistencyCheck();

			  assertFalse( "Expected store to be considered inconsistent.", result.Successful );
			  AssertResultContainsMessage( result, "Broken pointer found in tree node " + targetNode.Value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unreasonableKeyCount() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UnreasonableKeyCount()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );
			  MutableObject<long> targetNode = new MutableObject<long>();
			  File[] indexFiles = SchemaIndexFiles();
			  CorruptIndexes(true, (tree, inspection) =>
			  {
			  targetNode.Value = inspection.RootNode;
			  tree.@unsafe( GBPTreeCorruption.pageSpecificCorruption( targetNode.Value, GBPTreeCorruption.setKeyCount( int.MaxValue ) ) );
			  }, indexFiles);

			  ConsistencyCheckService.Result result = RunConsistencyCheck();

			  assertFalse( "Expected store to be considered inconsistent.", result.Successful );
			  AssertResultContainsMessage( result, "Unexpected keyCount on pageId " + targetNode.Value + ", keyCount=" + int.MaxValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void childNodeFoundAmongParentNodes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ChildNodeFoundAmongParentNodes()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );
			  File[] indexFiles = SchemaIndexFiles();
			  CorruptIndexes(true, (tree, inspection) =>
			  {
			  long rootNode = inspection.RootNode;
			  tree.@unsafe( GBPTreeCorruption.pageSpecificCorruption( rootNode, GBPTreeCorruption.setChild( 0, rootNode ) ) );
			  }, indexFiles);

			  ConsistencyCheckService.Result result = RunConsistencyCheck();

			  assertFalse( "Expected store to be considered inconsistent.", result.Successful );
			  AssertResultContainsMessage( result, "Circular reference, child tree node found among parent nodes. Parents:" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exception() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Exception()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );
			  File[] indexFiles = SchemaIndexFiles();
			  CorruptIndexes(true, (tree, inspection) =>
			  {
			  long rootNode = inspection.RootNode;
			  tree.@unsafe( GBPTreeCorruption.pageSpecificCorruption( rootNode, GBPTreeCorruption.setHighestReasonableKeyCount() ) );
			  }, indexFiles);

			  ConsistencyCheckService.Result result = RunConsistencyCheck();

			  assertFalse( "Expected store to be considered inconsistent.", result.Successful );
			  AssertResultContainsMessage( result, "Caught exception during consistency check: org.neo4j.index.internal.gbptree.TreeInconsistencyException: Some internal problem causing out of" + " bounds: pageId:" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeIndexFileInConsistencyReport() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIncludeIndexFileInConsistencyReport()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );
			  File[] indexFiles = SchemaIndexFiles();
			  IList<File> corruptedFiles = CorruptIndexes(true, (tree, inspection) =>
			  {
			  long rootNode = inspection.RootNode;
			  tree.@unsafe( GBPTreeCorruption.pageSpecificCorruption( rootNode, GBPTreeCorruption.notATreeNode() ) );
			  }, indexFiles);

			  ConsistencyCheckService.Result result = RunConsistencyCheck();

			  assertFalse( "Expected store to be considered inconsistent.", result.Successful );
			  AssertResultContainsMessage( result, "Index file: " + corruptedFiles[0].AbsolutePath );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void multipleCorruptions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MultipleCorruptions()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );
			  MutableObject<long> internalNode = new MutableObject<long>();
			  File[] indexFiles = SchemaIndexFiles();
			  CorruptIndexes(true, (tree, inspection) =>
			  {
			  long leafNode = inspection.LeafNodes.get( 0 );
			  internalNode.Value = inspection.NodesPerLevel.get( 1 ).get( 1 );
			  int? internalNodeKeyCount = inspection.KeyCounts.get( internalNode.Value );
			  tree.@unsafe( GBPTreeCorruption.pageSpecificCorruption( leafNode, GBPTreeCorruption.rightSiblingPointToNonExisting() ) );
			  tree.@unsafe( GBPTreeCorruption.pageSpecificCorruption( internalNode.Value, GBPTreeCorruption.swapChildOrder( 0, 1, internalNodeKeyCount ) ) );
			  tree.@unsafe( GBPTreeCorruption.pageSpecificCorruption( internalNode.Value, GBPTreeCorruption.broken( GBPTreePointerType.leftSibling() ) ) );
			  }, indexFiles);

			  ConsistencyCheckService.Result result = RunConsistencyCheck();
			  AssertResultContainsMessage( result, "Index inconsistency: Sibling pointers misaligned." );
			  AssertResultContainsMessage( result, "Index inconsistency: Expected range for this tree node is" );
			  AssertResultContainsMessage( result, "Index inconsistency: Broken pointer found in tree node " + internalNode.Value + ", pointerType='left sibling'" );
			  AssertResultContainsMessage( result, "Index inconsistency: Pointer (left sibling) in tree node " + internalNode.Value + " has pointer generation 0, but target node 0 has a higher generation 4." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void multipleCorruptionsInLabelScanStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MultipleCorruptionsInLabelScanStore()
		 {
			  Setup( GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10 );
			  MutableObject<long> rootNode = new MutableObject<long>();
			  File labelScanStoreFile = labelScanStoreFile();
			  CorruptIndexes(true, (tree, inspection) =>
			  {
			  rootNode.Value = inspection.RootNode;
			  tree.@unsafe( GBPTreeCorruption.pageSpecificCorruption( rootNode.Value, GBPTreeCorruption.broken( GBPTreePointerType.leftSibling() ) ) );
			  }, labelScanStoreFile);

			  ConsistencyCheckService.Result result = RunConsistencyCheck();
			  assertFalse( result.Successful );
			  AssertResultContainsMessage( result, "Index inconsistency: Broken pointer found in tree node " + rootNode.Value + ", pointerType='left sibling'" );
			  AssertResultContainsMessage( result, "Number of inconsistent LABEL_SCAN_DOCUMENT records: 1" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void multipleCorruptionsInFusionIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MultipleCorruptionsInFusionIndex()
		 {
			  Setup(GraphDatabaseSettings.SchemaIndex.NATIVE20, db =>
			  {
						  using ( Transaction tx = Db.beginTx() )
						  {
								// Also make sure we have some numbers
								for ( int i = 0; i < 1000; i++ )
								{
									 Node node = Db.createNode( _label );
									 node.setProperty( PROP_KEY1, i );
									 Node secondNode = Db.createNode( _label );
									 secondNode.setProperty( PROP_KEY1, LocalDate.ofEpochDay( i ) );
								}
								tx.success();
						  }
			  });

			  File[] indexFiles = SchemaIndexFiles();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<java.io.File> files = corruptIndexes(true, (tree, inspection) ->
			  IList<File> files = CorruptIndexes(true, (tree, inspection) =>
			  {
			  long leafNode = inspection.LeafNodes.get( 1 );
			  long internalNode = inspection.InternalNodes.get( 0 );
			  tree.@unsafe( GBPTreeCorruption.pageSpecificCorruption( leafNode, GBPTreeCorruption.rightSiblingPointToNonExisting() ) );
			  tree.@unsafe( GBPTreeCorruption.pageSpecificCorruption( internalNode, GBPTreeCorruption.setChild( 0, internalNode ) ) );
			  }, indexFiles);

			  ConsistencyCheckService.Result result = RunConsistencyCheck();
			  foreach ( File file in files )
			  {
					AssertResultContainsMessage( result, "Index file: " + file.AbsolutePath );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertResultContainsMessage(ConsistencyCheckService.Result result, String expectedMessage) throws java.io.IOException
		 private void AssertResultContainsMessage( ConsistencyCheckService.Result result, string expectedMessage )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<String> lines = java.nio.file.Files.readAllLines(result.reportFile().toPath());
			  IList<string> lines = Files.readAllLines( result.ReportFile().toPath() );
			  bool reportContainExpectedMessage = false;
			  foreach ( string line in lines )
			  {
					if ( line.Contains( expectedMessage ) )
					{
						 reportContainExpectedMessage = true;
						 break;
					}
			  }
			  string errorMessage = format( "Expected consistency report to contain message `%s'. Real result was: %s%n", expectedMessage, string.join( Environment.NewLine, lines ) );
			  assertTrue( errorMessage, reportContainExpectedMessage );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private ConsistencyCheckService.Result runConsistencyCheck() throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
		 private ConsistencyCheckService.Result RunConsistencyCheck()
		 {
			  return RunConsistencyCheck( Config( Settings.TRUE, Settings.TRUE ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private ConsistencyCheckService.Result runConsistencyCheck(org.neo4j.kernel.configuration.Config config) throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
		 private ConsistencyCheckService.Result RunConsistencyCheck( Config config )
		 {
			  return RunConsistencyCheck( ProgressMonitorFactory.NONE, config );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private ConsistencyCheckService.Result runConsistencyCheck(org.neo4j.helpers.progress.ProgressMonitorFactory progressFactory) throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
		 private ConsistencyCheckService.Result RunConsistencyCheck( ProgressMonitorFactory progressFactory )
		 {
			  return RunConsistencyCheck( progressFactory, Config( Settings.TRUE, Settings.TRUE ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private ConsistencyCheckService.Result runConsistencyCheck(org.neo4j.helpers.progress.ProgressMonitorFactory progressFactory, org.neo4j.kernel.configuration.Config config) throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
		 private ConsistencyCheckService.Result RunConsistencyCheck( ProgressMonitorFactory progressFactory, Config config )
		 {
			  ConsistencyCheckService consistencyCheckService = new ConsistencyCheckService();
			  DatabaseLayout databaseLayout = DatabaseLayout.of( _testDirectory.storeDir() );
			  LogProvider logProvider = NullLogProvider.Instance;
			  return consistencyCheckService.RunFullConsistencyCheck( databaseLayout, config, progressFactory, logProvider, false );
		 }

		 private static Config Config( string checkStructure, string checkIndex )
		 {
			  return Config.defaults( MapUtil.stringMap( ConsistencyCheckSettings.ConsistencyCheckIndexStructure.name(), checkStructure, ConsistencyCheckSettings.ConsistencyCheckIndexes.name(), checkIndex ) );
		 }

		 private void Setup( GraphDatabaseSettings.SchemaIndex schemaIndex )
		 {
			  Setup(schemaIndex, db =>
			  {
			  });
		 }

		 private void Setup( GraphDatabaseSettings.SchemaIndex schemaIndex, System.Action<GraphDatabaseService> additionalSetup )
		 {
			  File dataDir = _testDirectory.storeLayout().storeDirectory();
			  GraphDatabaseService db = ( new GraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(dataDir).setConfig(GraphDatabaseSettings.default_schema_provider, schemaIndex.providerName()).newGraphDatabase();
			  try
			  {
					CreateAnIndex( db );
					additionalSetup( db );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

		 private File LabelScanStoreFile()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File dataDir = testDirectory.storeLayout().storeDirectory();
			  File dataDir = _testDirectory.storeLayout().storeDirectory();
			  return new File( dataDir, DatabaseFile.LABEL_SCAN_STORE.Name );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File[] schemaIndexFiles() throws java.io.IOException
		 private File[] SchemaIndexFiles()
		 {
			  FileSystemAbstraction fs = _testDirectory.FileSystem;
			  File indexDir = new File( _testDirectory.storeDir(), "schema/index/" );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return fs.StreamFilesRecursive( indexDir ).map( FileHandle::getFile ).toArray( File[]::new );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<java.io.File> corruptIndexes(boolean readOnly, CorruptionInject corruptionInject, java.io.File... targetFiles) throws Exception
		 private IList<File> CorruptIndexes( bool readOnly, CorruptionInject corruptionInject, params File[] targetFiles )
		 {
			  IList<File> treeFiles = new List<File>();
			  using ( JobScheduler jobScheduler = createInitializedScheduler(), PageCache pageCache = createPageCache(_testDirectory.FileSystem, jobScheduler) )
			  {
					SchemaLayouts schemaLayouts = new SchemaLayouts();
					GBPTreeBootstrapper bootstrapper = new GBPTreeBootstrapper( pageCache, schemaLayouts, readOnly );
					foreach ( File file in targetFiles )
					{
						 GBPTreeBootstrapper.Bootstrap bootstrap = bootstrapper.BootstrapTree( file, "generic1" );
						 if ( bootstrap.Tree )
						 {
							  treeFiles.Add( file );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: try (org.neo4j.index.internal.gbptree.GBPTree<?,?> gbpTree = bootstrap.getTree())
							  using ( GBPTree<object, ?> gbpTree = bootstrap.Tree )
							  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.index.internal.gbptree.InspectingVisitor<?,?> visitor = gbpTree.visit(new org.neo4j.index.internal.gbptree.InspectingVisitor<>());
									InspectingVisitor<object, ?> visitor = gbpTree.Visit( new InspectingVisitor<object, ?>() );
									corruptionInject.Corrupt( gbpTree, visitor.Get() );
							  }
						 }
					}
			  }
			  return treeFiles;
		 }

		 private void CreateAnIndex( GraphDatabaseService db )
		 {
			  string longString = longString();

			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 0; i < 40; i++ )
					{
						 Node node = Db.createNode( _label );
						 // Using long string that only differ in the end make sure index tree will be higher which we need to mess up internal pointers
						 string value = longString + i;
						 node.SetProperty( PROP_KEY1, value );
					}
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(_label).on(PROP_KEY1).create();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.HOURS);
					tx.Success();
			  }
		 }

		 private string LongString()
		 {
			  char[] chars = new char[1000];
			  Arrays.fill( chars, 'a' );
			  return new string( chars );
		 }

		 private interface CorruptionInject
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void corrupt(org.neo4j.index.internal.gbptree.GBPTree<?,?> tree, org.neo4j.index.internal.gbptree.GBPTreeInspection<?,?> inspection) throws java.io.IOException;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  void corrupt<T1, T2>( GBPTree<T1> tree, GBPTreeInspection<T2> inspection );
		 }
	}

}