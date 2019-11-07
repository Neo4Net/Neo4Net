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
namespace Neo4Net.Index.Internal.gbptree
{
	using MutableBoolean = org.apache.commons.lang3.mutable.MutableBoolean;
	using ImmutableLongList = org.eclipse.collections.api.list.primitive.ImmutableLongList;
	using LongList = org.eclipse.collections.api.list.primitive.LongList;
	using Assert = org.junit.Assert;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using RandomValues = Neo4Net.Values.Storable.RandomValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.rules.RuleChain.outerRule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.GenerationSafePointerPair.pointer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.rule.PageCacheRule.config;

	public abstract class GBPTreeConsistencyCheckerTestBase<KEY, VALUE>
	{
		private bool InstanceFieldsInitialized = false;

		public GBPTreeConsistencyCheckerTestBase()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_directory = TestDirectory.testDirectory( this.GetType(), _fs.get() );
			Rules = outerRule( _fs ).around( _directory ).around( _pageCacheRule ).around( _random );
		}

		 private const int PAGE_SIZE = 256;
		 private readonly DefaultFileSystemRule _fs = new DefaultFileSystemRule();
		 private TestDirectory _directory;
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule( config().withAccessChecks(true) );
		 private readonly RandomRule _random = new RandomRule();
		 private RandomValues _randomValues;
		 private TestLayout<KEY, VALUE> _layout;
		 private TreeNode<KEY, VALUE> _node;
		 private File _indexFile;
		 private PageCache _pageCache;
		 private bool _isDynamic;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain rules = outerRule(fs).around(directory).around(pageCacheRule).around(random);
		 public RuleChain Rules;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _indexFile = _directory.file( "index" );
			  _pageCache = CreatePageCache();
			  _layout = Layout;
			  _node = TreeNodeSelector.SelectByLayout( _layout ).create( PAGE_SIZE, _layout );
			  _randomValues = _random.randomValues();
			  _isDynamic = _node is TreeNodeDynamicSize;
		 }

		 protected internal abstract TestLayout<KEY, VALUE> Layout { get; }

		 private PageCache CreatePageCache()
		 {
			  return _pageCacheRule.getPageCache( _fs.get(), PageCacheRule.config().withPageSize(PAGE_SIZE) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectNotATreeNodeRoot() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectNotATreeNodeRoot()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long rootNode = inspection.RootNode;

					index.Unsafe( Page( rootNode, GBPTreeCorruption.NotATreeNode() ) );

					AssertReportNotATreeNode( index, rootNode );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectNotATreeNodeInternal() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectNotATreeNodeInternal()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.list.primitive.ImmutableLongList internalNodes = inspection.getInternalNodes();
					ImmutableLongList internalNodes = inspection.InternalNodes;
					long internalNode = RandomAmong( internalNodes );

					index.Unsafe( Page( internalNode, GBPTreeCorruption.NotATreeNode() ) );

					AssertReportNotATreeNode( index, internalNode );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectNotATreeNodeLeaf() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectNotATreeNodeLeaf()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long leafNode = RandomAmong( inspection.LeafNodes );

					index.Unsafe( Page( leafNode, GBPTreeCorruption.NotATreeNode() ) );

					AssertReportNotATreeNode( index, leafNode );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectUnknownTreeNodeTypeRoot() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectUnknownTreeNodeTypeRoot()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long rootNode = inspection.RootNode;

					index.Unsafe( Page( rootNode, GBPTreeCorruption.UnknownTreeNodeType() ) );

					AssertReportUnknownTreeNodeType( index, rootNode );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectUnknownTreeNodeTypeInternal() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectUnknownTreeNodeTypeInternal()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long internalNode = RandomAmong( inspection.InternalNodes );

					index.Unsafe( Page( internalNode, GBPTreeCorruption.UnknownTreeNodeType() ) );

					AssertReportUnknownTreeNodeType( index, internalNode );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectUnknownTreeNodeTypeLeaf() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectUnknownTreeNodeTypeLeaf()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long leafNode = RandomAmong( inspection.LeafNodes );

					index.Unsafe( Page( leafNode, GBPTreeCorruption.UnknownTreeNodeType() ) );

					AssertReportUnknownTreeNodeType( index, leafNode );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectRightSiblingNotPointingToCorrectSibling() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectRightSiblingNotPointingToCorrectSibling()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long targetNode = RandomAmong( inspection.LeafNodes );

					index.Unsafe( Page( targetNode, GBPTreeCorruption.RightSiblingPointToNonExisting() ) );

					AssertReportMisalignedSiblingPointers( index );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectLeftSiblingNotPointingToCorrectSibling() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectLeftSiblingNotPointingToCorrectSibling()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long targetNode = RandomAmong( inspection.LeafNodes );

					index.Unsafe( Page( targetNode, GBPTreeCorruption.LeftSiblingPointToNonExisting() ) );

					AssertReportMisalignedSiblingPointers( index );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectIfAnyNodeInTreeHasSuccessor() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectIfAnyNodeInTreeHasSuccessor()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long targetNode = RandomAmong( inspection.AllNodes );

					index.Unsafe( Page( targetNode, GBPTreeCorruption.HasSuccessor() ) );

					AssertReportPointerToOldVersionOfTreeNode( index, targetNode );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectRightSiblingPointerWithTooLowGeneration() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectRightSiblingPointerWithTooLowGeneration()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long targetNode = NodeWithRightSibling( inspection );

					index.Unsafe( Page( targetNode, GBPTreeCorruption.RightSiblingPointerHasTooLowGeneration() ) );

					AssertReportPointerGenerationLowerThanNodeGeneration( index, targetNode, GBPTreePointerType.rightSibling() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectLeftSiblingPointerWithTooLowGeneration() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectLeftSiblingPointerWithTooLowGeneration()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long targetNode = NodeWithLeftSibling( inspection );

					index.Unsafe( Page( targetNode, GBPTreeCorruption.LeftSiblingPointerHasTooLowGeneration() ) );

					AssertReportPointerGenerationLowerThanNodeGeneration( index, targetNode, GBPTreePointerType.leftSibling() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectChildPointerWithTooLowGeneration() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectChildPointerWithTooLowGeneration()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long targetNode = RandomAmong( inspection.InternalNodes );
					int keyCount = inspection.KeyCounts[targetNode];
					int childPos = _randomValues.Next( keyCount + 1 );

					index.Unsafe( Page( targetNode, GBPTreeCorruption.ChildPointerHasTooLowGeneration( childPos ) ) );

					AssertReportPointerGenerationLowerThanNodeGeneration( index, targetNode, GBPTreePointerType.child( childPos ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectKeysOutOfOrderInIsolatedNode() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectKeysOutOfOrderInIsolatedNode()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long targetNode = NodeWithMultipleKeys( inspection );
					int keyCount = inspection.KeyCounts[targetNode];
					int firstKey = _randomValues.Next( keyCount );
					int secondKey = NextRandomIntExcluding( keyCount, firstKey );
					bool isLeaf = inspection.LeafNodes.contains( targetNode );

					GBPTreeCorruption.PageCorruption<KEY, VALUE> swapKeyOrder = isLeaf ? GBPTreeCorruption.SwapKeyOrderLeaf( firstKey, secondKey, keyCount ) : GBPTreeCorruption.SwapKeyOrderInternal( firstKey, secondKey, keyCount );
					index.Unsafe( Page( targetNode, swapKeyOrder ) );

					AssertReportKeysOutOfOrderInNode( index, targetNode );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectKeysLocatedInWrongNodeLowKey() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectKeysLocatedInWrongNodeLowKey()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long targetNode = NodeWithLeftSibling( inspection );
					int keyCount = inspection.KeyCounts[targetNode];
					int keyPos = _randomValues.Next( keyCount );
					KEY key = _layout.key( long.MinValue );
					bool isLeaf = inspection.LeafNodes.contains( targetNode );

					GBPTreeCorruption.PageCorruption<KEY, VALUE> swapKeyOrder = isLeaf ? GBPTreeCorruption.OverwriteKeyAtPosLeaf( key, keyPos, keyCount ) : GBPTreeCorruption.OverwriteKeyAtPosInternal( key, keyPos, keyCount );
					index.Unsafe( Page( targetNode, swapKeyOrder ) );

					AssertReportKeysLocatedInWrongNode( index, targetNode );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectKeysLocatedInWrongNodeHighKey() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectKeysLocatedInWrongNodeHighKey()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long targetNode = NodeWithRightSibling( inspection );
					int keyCount = inspection.KeyCounts[targetNode];
					int keyPos = _randomValues.Next( keyCount );
					KEY key = _layout.key( long.MaxValue );
					bool isLeaf = inspection.LeafNodes.contains( targetNode );

					GBPTreeCorruption.PageCorruption<KEY, VALUE> swapKeyOrder = isLeaf ? GBPTreeCorruption.OverwriteKeyAtPosLeaf( key, keyPos, keyCount ) : GBPTreeCorruption.OverwriteKeyAtPosInternal( key, keyPos, keyCount );
					index.Unsafe( Page( targetNode, swapKeyOrder ) );

					AssertReportKeysLocatedInWrongNode( index, targetNode );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectNodeMetaInconsistencyDynamicNodeAllocSpaceOverlapActiveKeys() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectNodeMetaInconsistencyDynamicNodeAllocSpaceOverlapActiveKeys()
		 {
			  assumeTrue( "Only relevant for dynamic layout", _isDynamic );
			  using ( GBPTree<KEY, VALUE> index = index( _layout ).build() )
			  {
					TreeWithHeight( index, _layout, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long targetNode = RandomAmong( inspection.AllNodes );

					GBPTreeCorruption.PageCorruption<KEY, VALUE> corruption = GBPTreeCorruption.MaximizeAllocOffsetInDynamicNode();
					index.Unsafe( Page( targetNode, corruption ) );

					AssertReportAllocSpaceOverlapActiveKeys( index, targetNode );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectNodeMetaInconsistencyDynamicNodeOverlapBetweenOffsetArrayAndAllocSpace() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectNodeMetaInconsistencyDynamicNodeOverlapBetweenOffsetArrayAndAllocSpace()
		 {
			  assumeTrue( "Only relevant for dynamic layout", _isDynamic );
			  using ( GBPTree<KEY, VALUE> index = index( _layout ).build() )
			  {
					TreeWithHeight( index, _layout, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long targetNode = RandomAmong( inspection.AllNodes );

					GBPTreeCorruption.PageCorruption<KEY, VALUE> corruption = GBPTreeCorruption.MinimizeAllocOffsetInDynamicNode();
					index.Unsafe( Page( targetNode, corruption ) );

					AssertReportAllocSpaceOverlapOffsetArray( index, targetNode );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectNodeMetaInconsistencyDynamicNodeSpaceAreasNotSummingToTotalSpace() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectNodeMetaInconsistencyDynamicNodeSpaceAreasNotSummingToTotalSpace()
		 {
			  assumeTrue( "Only relevant for dynamic layout", _isDynamic );
			  using ( GBPTree<KEY, VALUE> index = index( _layout ).build() )
			  {
					TreeWithHeight( index, _layout, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long targetNode = RandomAmong( inspection.AllNodes );

					GBPTreeCorruption.PageCorruption<KEY, VALUE> corruption = GBPTreeCorruption.IncrementDeadSpaceInDynamicNode();
					index.Unsafe( Page( targetNode, corruption ) );

					AssertReportSpaceAreasNotSummingToTotalSpace( index, targetNode );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectNodeMetaInconsistencyDynamicNodeAllocOffsetMisplaced() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectNodeMetaInconsistencyDynamicNodeAllocOffsetMisplaced()
		 {
			  assumeTrue( "Only relevant for dynamic layout", _isDynamic );
			  using ( GBPTree<KEY, VALUE> index = index( _layout ).build() )
			  {
					TreeWithHeight( index, _layout, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long targetNode = RandomAmong( inspection.AllNodes );

					GBPTreeCorruption.PageCorruption<KEY, VALUE> corruption = GBPTreeCorruption.DecrementAllocOffsetInDynamicNode();
					index.Unsafe( Page( targetNode, corruption ) );

					AssertReportAllocOffsetMisplaced( index, targetNode );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectPageMissingFreelistEntry() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectPageMissingFreelistEntry()
		 {
			  long targetMissingId;
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					// Add and remove a bunch of keys to fill freelist
					using ( Writer<KEY, VALUE> writer = index.Writer() )
					{
						 int keyCount = 0;
						 while ( GetHeight( index ) < 2 )
						 {
							  writer.Put( _layout.key( keyCount ), _layout.value( keyCount ) );
							  keyCount++;
						 }

						 for ( int i = 0; i < keyCount; i++ )
						 {
							  writer.Remove( _layout.key( i ) );
						 }
					}
					index.Checkpoint( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
			  }

			  // When tree is closed we will overwrite treeState with in memory state so we need to open tree in read only mode for our state corruption to persist.
			  using ( GBPTree<KEY, VALUE> index = index().withReadOnly(true).Build() )
			  {
					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					int lastIndex = inspection.AllFreelistEntries.Count - 1;
					InspectingVisitor.FreelistEntry lastFreelistEntry = inspection.AllFreelistEntries[lastIndex];
					targetMissingId = lastFreelistEntry.Id;

					GBPTreeCorruption.IndexCorruption<KEY, VALUE> corruption = GBPTreeCorruption.DecrementFreelistWritePos();
					index.Unsafe( corruption );
			  }

			  // Need to restart tree to reload corrupted freelist
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					AssertReportUnusedPage( index, targetMissingId );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectExtraFreelistEntry() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectExtraFreelistEntry()
		 {
			  long targetNode;
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					// Add and remove a bunch of keys to fill freelist
					using ( Writer<KEY, VALUE> writer = index.Writer() )
					{
						 int keyCount = 0;
						 while ( GetHeight( index ) < 2 )
						 {
							  writer.Put( _layout.key( keyCount ), _layout.value( keyCount ) );
							  keyCount++;
						 }
					}
					index.Checkpoint( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
			  }

			  // When tree is closed we will overwrite treeState with in memory state so we need to open tree in read only mode for our state corruption to persist.
			  using ( GBPTree<KEY, VALUE> index = index().withReadOnly(true).Build() )
			  {
					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					targetNode = RandomAmong( inspection.AllNodes );

					GBPTreeCorruption.IndexCorruption<KEY, VALUE> corruption = GBPTreeCorruption.AddFreelistEntry( targetNode );
					index.Unsafe( corruption );
			  }

			  // Need to restart tree to reload corrupted freelist
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					AssertReportActiveTreeNodeInFreelist( index, targetNode );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectExtraEmptyPageInFile() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectExtraEmptyPageInFile()
		 {
			  long lastId;
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					// Add and remove a bunch of keys to fill freelist
					using ( Writer<KEY, VALUE> writer = index.Writer() )
					{
						 int keyCount = 0;
						 while ( GetHeight( index ) < 2 )
						 {
							  writer.Put( _layout.key( keyCount ), _layout.value( keyCount ) );
							  keyCount++;
						 }
					}
					index.Checkpoint( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
			  }

			  // When tree is closed we will overwrite treeState with in memory state so we need to open tree in read only mode for our state corruption to persist.
			  using ( GBPTree<KEY, VALUE> index = index().withReadOnly(true).Build() )
			  {
					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					TreeState treeState = inspection.TreeState;
					lastId = treeState.LastId() + 1;
					TreeState newTreeState = TreeStateWithLastId( lastId, treeState );

					GBPTreeCorruption.IndexCorruption<KEY, VALUE> corruption = GBPTreeCorruption.setTreeState( newTreeState );
					index.Unsafe( corruption );
			  }

			  // Need to restart tree to reload corrupted freelist
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					AssertReportUnusedPage( index, lastId );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectIdLargerThanFreelistLastId() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectIdLargerThanFreelistLastId()
		 {
			  long targetLastId;
			  long targetPageId;
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					// Add and remove a bunch of keys to fill freelist
					using ( Writer<KEY, VALUE> writer = index.Writer() )
					{
						 int keyCount = 0;
						 while ( GetHeight( index ) < 2 )
						 {
							  writer.Put( _layout.key( keyCount ), _layout.value( keyCount ) );
							  keyCount++;
						 }
					}
					index.Checkpoint( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
			  }

			  // When tree is closed we will overwrite treeState with in memory state so we need to open tree in read only mode for our state corruption to persist.
			  using ( GBPTree<KEY, VALUE> index = index().withReadOnly(true).Build() )
			  {
					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					TreeState treeState = inspection.TreeState;
					targetPageId = treeState.LastId();
					targetLastId = treeState.LastId() - 1;
					TreeState newTreeState = TreeStateWithLastId( targetLastId, treeState );

					GBPTreeCorruption.IndexCorruption<KEY, VALUE> corruption = GBPTreeCorruption.setTreeState( newTreeState );
					index.Unsafe( corruption );
			  }

			  // Need to restart tree to reload corrupted freelist
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					AssertReportIdExceedLastId( index, targetLastId, targetPageId );
			  }
		 }

		 private static TreeState TreeStateWithLastId( long lastId, TreeState treeState )
		 {
			  return new TreeState( treeState.PageId(), treeState.StableGeneration(), treeState.UnstableGeneration(), treeState.RootId(), treeState.RootGeneration(), lastId, treeState.FreeListWritePageId(), treeState.FreeListReadPageId(), treeState.FreeListWritePos(), treeState.FreeListReadPos(), treeState.Clean, treeState.Valid );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectCrashedGSPP() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectCrashedGSPP()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long targetNode = RandomAmong( inspection.AllNodes );
					bool isLeaf = inspection.LeafNodes.contains( targetNode );
					int keyCount = inspection.KeyCounts[targetNode];
					GBPTreePointerType pointerType = RandomPointerType( keyCount, isLeaf );

					GBPTreeCorruption.PageCorruption<KEY, VALUE> corruption = GBPTreeCorruption.Crashed( pointerType );
					index.Unsafe( Page( targetNode, corruption ) );

					AssertReportCrashedGSPP( index, targetNode, pointerType );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectBrokenGSPP() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectBrokenGSPP()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long targetNode = RandomAmong( inspection.AllNodes );
					bool isLeaf = inspection.LeafNodes.contains( targetNode );
					int keyCount = inspection.KeyCounts[targetNode];
					GBPTreePointerType pointerType = RandomPointerType( keyCount, isLeaf );

					GBPTreeCorruption.PageCorruption<KEY, VALUE> corruption = GBPTreeCorruption.Broken( pointerType );
					index.Unsafe( Page( targetNode, corruption ) );

					AssertReportBrokenGSPP( index, targetNode, pointerType );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectUnreasonableKeyCount() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectUnreasonableKeyCount()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long targetNode = RandomAmong( inspection.AllNodes );
					int unreasonableKeyCount = PAGE_SIZE;

					GBPTreeCorruption.PageCorruption<KEY, VALUE> corruption = GBPTreeCorruption.setKeyCount( unreasonableKeyCount );
					index.Unsafe( Page( targetNode, corruption ) );

					AssertReportUnreasonableKeyCount( index, targetNode, unreasonableKeyCount );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectChildPointerPointingTwoLevelsDown() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectChildPointerPointingTwoLevelsDown()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long rootNode = inspection.RootNode;
					int childPos = RandomChildPos( inspection, rootNode );
					long targetChildNode = RandomAmong( inspection.LeafNodes );

					GBPTreeCorruption.PageCorruption<KEY, VALUE> corruption = GBPTreeCorruption.SetChild( childPos, targetChildNode );
					index.Unsafe( Page( rootNode, corruption ) );

					AssertReportAnyStructuralInconsistency( index );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectChildPointerPointingToUpperLevelSameStack() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectChildPointerPointingToUpperLevelSameStack()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long rootNode = inspection.RootNode;
					long? internalNode = RandomAmong( inspection.NodesPerLevel[1] );
					int childPos = RandomChildPos( inspection, internalNode.Value );

					GBPTreeCorruption.PageCorruption<KEY, VALUE> corruption = GBPTreeCorruption.SetChild( childPos, rootNode );
					index.Unsafe( Page( internalNode.Value, corruption ) );

					AssertReportCircularChildPointer( index, rootNode );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectChildPointerPointingToSameLevel() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectChildPointerPointingToSameLevel()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					ImmutableLongList internalNodesWithSiblings = inspection.NodesPerLevel[1];
					long internalNode = RandomAmong( internalNodesWithSiblings );
					long otherInternalNode = RandomFromExcluding( internalNodesWithSiblings, internalNode );
					int childPos = RandomChildPos( inspection, internalNode );

					GBPTreeCorruption.PageCorruption<KEY, VALUE> corruption = GBPTreeCorruption.SetChild( childPos, otherInternalNode );
					index.Unsafe( Page( internalNode, corruption ) );

					AssertReportAnyStructuralInconsistency( index );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectChildPointerPointingToUpperLevelNotSameStack() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectChildPointerPointingToUpperLevelNotSameStack()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 3 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long upperInternalNode = RandomAmong( inspection.NodesPerLevel[1] );
					long lowerInternalNode = RandomAmong( inspection.NodesPerLevel[2] );
					int childPos = RandomChildPos( inspection, lowerInternalNode );

					GBPTreeCorruption.PageCorruption<KEY, VALUE> corruption = GBPTreeCorruption.SetChild( childPos, upperInternalNode );
					index.Unsafe( Page( lowerInternalNode, corruption ) );

					AssertReportAnyStructuralInconsistency( index );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectChildPointerPointingToChildOwnedByOtherNode() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectChildPointerPointingToChildOwnedByOtherNode()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					LongList internalNodesWithSiblings = inspection.NodesPerLevel[1];
					long internalNode = RandomAmong( internalNodesWithSiblings );
					long otherInternalNode = RandomFromExcluding( internalNodesWithSiblings, internalNode );
					int otherChildPos = RandomChildPos( inspection, otherInternalNode );
					long childInOtherInternal = ChildAt( otherInternalNode, otherChildPos, inspection.TreeState );
					int childPos = RandomChildPos( inspection, internalNode );

					GBPTreeCorruption.PageCorruption<KEY, VALUE> corruption = GBPTreeCorruption.SetChild( childPos, childInOtherInternal );
					index.Unsafe( Page( internalNode, corruption ) );

					AssertReportAnyStructuralInconsistency( index );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectExceptionDuringConsistencyCheck() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectExceptionDuringConsistencyCheck()
		 {
			  assumeTrue( "This trick to make GBPTreeConsistencyChecker throw exception only work for dynamic layout", _isDynamic );
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long leaf = randomAmong(inspection.getLeafNodes());
					long leaf = RandomAmong( inspection.LeafNodes );

					GBPTreeCorruption.PageCorruption<KEY, VALUE> corruption = GBPTreeCorruption.SetHighestReasonableKeyCount();
					index.Unsafe( Page( leaf, corruption ) );

					AssertReportException( index );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectSiblingPointerPointingToLowerLevel() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectSiblingPointerPointingToLowerLevel()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long internalNode = RandomAmong( inspection.InternalNodes );
					long leafNode = RandomAmong( inspection.LeafNodes );
					GBPTreePointerType siblingPointer = RandomSiblingPointerType();

					GBPTreeCorruption.PageCorruption<KEY, VALUE> corruption = GBPTreeCorruption.SetPointer( siblingPointer, leafNode );
					index.Unsafe( Page( internalNode, corruption ) );

					AssertReportAnyStructuralInconsistency( index );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectSiblingPointerPointingToUpperLevel() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectSiblingPointerPointingToUpperLevel()
		 {
			  using ( GBPTree<KEY, VALUE> index = index().build() )
			  {
					TreeWithHeight( index, 2 );

					GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
					long internalNode = RandomAmong( inspection.InternalNodes );
					long leafNode = RandomAmong( inspection.LeafNodes );
					GBPTreePointerType siblingPointer = RandomSiblingPointerType();

					GBPTreeCorruption.PageCorruption<KEY, VALUE> corruption = GBPTreeCorruption.SetPointer( siblingPointer, internalNode );
					index.Unsafe( Page( leafNode, corruption ) );

					AssertReportAnyStructuralInconsistency( index );
			  }
		 }

		 private static GBPTreeCorruption.IndexCorruption<KEY, VALUE> Page<KEY, VALUE>( long targetNode, GBPTreeCorruption.PageCorruption<KEY, VALUE> corruption )
		 {
			  return GBPTreeCorruption.PageSpecificCorruption( targetNode, corruption );
		 }

		 private long NodeWithLeftSibling( GBPTreeInspection<KEY, VALUE> visitor )
		 {
			  IList<ImmutableLongList> nodesPerLevel = visitor.NodesPerLevel;
			  long targetNode = -1;
			  bool foundNodeWithLeftSibling;
			  do
			  {
					ImmutableLongList level = _randomValues.among( nodesPerLevel );
					if ( level.size() < 2 )
					{
						 foundNodeWithLeftSibling = false;
					}
					else
					{
						 int index = _random.Next( level.size() - 1 ) + 1;
						 targetNode = level.get( index );
						 foundNodeWithLeftSibling = true;
					}
			  } while ( !foundNodeWithLeftSibling );
			  return targetNode;
		 }

		 private long NodeWithRightSibling( GBPTreeInspection<KEY, VALUE> inspection )
		 {
			  IList<ImmutableLongList> nodesPerLevel = inspection.NodesPerLevel;
			  long targetNode = -1;
			  bool foundNodeWithRightSibling;
			  do
			  {
					ImmutableLongList level = _randomValues.among( nodesPerLevel );
					if ( level.size() < 2 )
					{
						 foundNodeWithRightSibling = false;
					}
					else
					{
						 int index = _random.Next( level.size() - 1 );
						 targetNode = level.get( index );
						 foundNodeWithRightSibling = true;
					}
			  } while ( !foundNodeWithRightSibling );
			  return targetNode;
		 }

		 private long NodeWithMultipleKeys( GBPTreeInspection<KEY, VALUE> inspection )
		 {
			  long targetNode;
			  int keyCount;
			  do
			  {
					targetNode = RandomAmong( inspection.AllNodes );
					keyCount = inspection.KeyCounts[targetNode];
			  } while ( keyCount < 2 );
			  return targetNode;
		 }

		 private int NextRandomIntExcluding( int bound, int excluding )
		 {
			  int result;
			  do
			  {
					result = _randomValues.Next( bound );
			  } while ( result == excluding );
			  return result;
		 }

		 private long RandomFromExcluding( LongList from, long excluding )
		 {
			  long other;
			  do
			  {
					other = RandomAmong( from );
			  } while ( other == excluding );
			  return other;
		 }

		 private int RandomChildPos( GBPTreeInspection<KEY, VALUE> inspection, long internalNode )
		 {
			  int childCount = inspection.KeyCounts[internalNode] + 1;
			  return _randomValues.Next( childCount );
		 }

		 private GBPTreePointerType RandomSiblingPointerType()
		 {
			  return _randomValues.among( Arrays.asList( GBPTreePointerType.leftSibling(), GBPTreePointerType.rightSibling() ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long childAt(long internalNode, int childPos, TreeState treeState) throws java.io.IOException
		 private long ChildAt( long internalNode, int childPos, TreeState treeState )
		 {
			  using ( PagedFile pagedFile = _pageCache.map( _indexFile, _pageCache.pageSize() ), PageCursor cursor = pagedFile.Io(0, Neo4Net.Io.pagecache.PagedFile_Fields.PfSharedWriteLock) )
			  {
					PageCursorUtil.GoTo( cursor, "", internalNode );
					return pointer( _node.childAt( cursor, childPos, treeState.StableGeneration(), treeState.UnstableGeneration() ) );
			  }
		 }

		 private GBPTreeBuilder<KEY, VALUE> Index()
		 {
			  return Index( _layout );
		 }

		 private GBPTreeBuilder<KEY, VALUE> Index( Layout<KEY, VALUE> layout )
		 {
			  return new GBPTreeBuilder<KEY, VALUE>( _pageCache, _indexFile, layout );
		 }

		 private GBPTreePointerType RandomPointerType( int keyCount, bool isLeaf )
		 {
			  int bound = isLeaf ? 3 : 4;
			  switch ( _randomValues.Next( bound ) )
			  {
			  case 0:
					return GBPTreePointerType.leftSibling();
			  case 1:
					return GBPTreePointerType.rightSibling();
			  case 2:
					return GBPTreePointerType.successor();
			  case 3:
					return GBPTreePointerType.child( _randomValues.Next( keyCount + 1 ) );
			  default:
					throw new System.InvalidOperationException( "Unrecognized option" );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void treeWithHeight(GBPTree<KEY,VALUE> index, int height) throws java.io.IOException
		 private void TreeWithHeight( GBPTree<KEY, VALUE> index, int height )
		 {
			  TreeWithHeight( index, _layout, height );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY, VALUE> void treeWithHeight(GBPTree<KEY,VALUE> index, TestLayout<KEY,VALUE> layout, int height) throws java.io.IOException
		 private static void TreeWithHeight<KEY, VALUE>( GBPTree<KEY, VALUE> index, TestLayout<KEY, VALUE> layout, int height )
		 {
			  using ( Writer<KEY, VALUE> writer = index.Writer() )
			  {
					int keyCount = 0;
					while ( GetHeight( index ) < height )
					{
						 writer.Put( layout.Key( keyCount ), layout.Value( keyCount ) );
						 keyCount++;
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY,VALUE> int getHeight(GBPTree<KEY,VALUE> index) throws java.io.IOException
		 private static int GetHeight<KEY, VALUE>( GBPTree<KEY, VALUE> index )
		 {
			  GBPTreeInspection<KEY, VALUE> inspection = Inspect( index );
			  return inspection.LastLevel;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY, VALUE> GBPTreeInspection<KEY,VALUE> inspect(GBPTree<KEY,VALUE> index) throws java.io.IOException
		 private static GBPTreeInspection<KEY, VALUE> Inspect<KEY, VALUE>( GBPTree<KEY, VALUE> index )
		 {
			  return index.Visit( new InspectingVisitor<>() ).get();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY,VALUE> void assertReportNotATreeNode(GBPTree<KEY,VALUE> index, long targetNode) throws java.io.IOException
		 private static void AssertReportNotATreeNode<KEY, VALUE>( GBPTree<KEY, VALUE> index, long targetNode )
		 {
			  MutableBoolean called = new MutableBoolean();
			  index.ConsistencyCheck( new GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass( targetNode, called ) );
			  AssertCalled( called );
		 }

		 private class GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass : GBPTreeConsistencyCheckVisitor_Adaptor<KEY>
		 {
			 private long _targetNode;
			 private MutableBoolean _called;

			 public GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass( long targetNode, MutableBoolean called )
			 {
				 this._targetNode = targetNode;
				 this._called = called;
			 }

			 public override void notATreeNode( long pageId, File file )
			 {
				  _called.setTrue();
				  assertEquals( _targetNode, pageId );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY,VALUE> void assertReportUnknownTreeNodeType(GBPTree<KEY,VALUE> index, long targetNode) throws java.io.IOException
		 private static void AssertReportUnknownTreeNodeType<KEY, VALUE>( GBPTree<KEY, VALUE> index, long targetNode )
		 {
			  MutableBoolean called = new MutableBoolean();
			  index.ConsistencyCheck( new GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass2( targetNode, called ) );
			  AssertCalled( called );
		 }

		 private class GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass2 : GBPTreeConsistencyCheckVisitor_Adaptor<KEY>
		 {
			 private long _targetNode;
			 private MutableBoolean _called;

			 public GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass2( long targetNode, MutableBoolean called )
			 {
				 this._targetNode = targetNode;
				 this._called = called;
			 }

			 public override void unknownTreeNodeType( long pageId, sbyte treeNodeType, File file )
			 {
				  _called.setTrue();
				  assertEquals( _targetNode, pageId );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY,VALUE> void assertReportMisalignedSiblingPointers(GBPTree<KEY,VALUE> index) throws java.io.IOException
		 private static void AssertReportMisalignedSiblingPointers<KEY, VALUE>( GBPTree<KEY, VALUE> index )
		 {
			  MutableBoolean corruptedSiblingPointerCalled = new MutableBoolean();
			  MutableBoolean rightmostNodeHasRightSiblingCalled = new MutableBoolean();
			  index.ConsistencyCheck( new GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass3( corruptedSiblingPointerCalled, rightmostNodeHasRightSiblingCalled ) );
			  assertTrue( corruptedSiblingPointerCalled.Value || rightmostNodeHasRightSiblingCalled.Value );
		 }

		 private class GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass3 : GBPTreeConsistencyCheckVisitor_Adaptor<KEY>
		 {
			 private MutableBoolean _corruptedSiblingPointerCalled;
			 private MutableBoolean _rightmostNodeHasRightSiblingCalled;

			 public GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass3( MutableBoolean corruptedSiblingPointerCalled, MutableBoolean rightmostNodeHasRightSiblingCalled )
			 {
				 this._corruptedSiblingPointerCalled = corruptedSiblingPointerCalled;
				 this._rightmostNodeHasRightSiblingCalled = rightmostNodeHasRightSiblingCalled;
			 }

			 public override void siblingsDontPointToEachOther( long leftNode, long leftNodeGeneration, long leftRightSiblingPointerGeneration, long leftRightSiblingPointer, long rightLeftSiblingPointer, long rightLeftSiblingPointerGeneration, long rightNode, long rightNodeGeneration, File file )
			 {
				  _corruptedSiblingPointerCalled.setTrue();
			 }

			 public override void rightmostNodeHasRightSibling( long rightSiblingPointer, long rightmostNode, File file )
			 {
				  _rightmostNodeHasRightSiblingCalled.setTrue();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY,VALUE> void assertReportPointerToOldVersionOfTreeNode(GBPTree<KEY,VALUE> index, long targetNode) throws java.io.IOException
		 private static void AssertReportPointerToOldVersionOfTreeNode<KEY, VALUE>( GBPTree<KEY, VALUE> index, long targetNode )
		 {
			  MutableBoolean called = new MutableBoolean();
			  index.ConsistencyCheck( new GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass4( targetNode, called ) );
			  AssertCalled( called );
		 }

		 private class GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass4 : GBPTreeConsistencyCheckVisitor_Adaptor<KEY>
		 {
			 private long _targetNode;
			 private MutableBoolean _called;

			 public GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass4( long targetNode, MutableBoolean called )
			 {
				 this._targetNode = targetNode;
				 this._called = called;
			 }

			 public override void pointerToOldVersionOfTreeNode( long pageId, long successorPointer, File file )
			 {
				  _called.setTrue();
				  assertEquals( _targetNode, pageId );
				  assertEquals( GenerationSafePointer.MAX_POINTER, successorPointer );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY,VALUE> void assertReportPointerGenerationLowerThanNodeGeneration(GBPTree<KEY,VALUE> index, long targetNode, GBPTreePointerType expectedPointerType) throws java.io.IOException
		 private static void AssertReportPointerGenerationLowerThanNodeGeneration<KEY, VALUE>( GBPTree<KEY, VALUE> index, long targetNode, GBPTreePointerType expectedPointerType )
		 {
			  MutableBoolean called = new MutableBoolean();
			  index.ConsistencyCheck( new GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass5( targetNode, expectedPointerType, called ) );
			  AssertCalled( called );
		 }

		 private class GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass5 : GBPTreeConsistencyCheckVisitor_Adaptor<KEY>
		 {
			 private long _targetNode;
			 private Neo4Net.Index.Internal.gbptree.GBPTreePointerType _expectedPointerType;
			 private MutableBoolean _called;

			 public GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass5( long targetNode, Neo4Net.Index.Internal.gbptree.GBPTreePointerType expectedPointerType, MutableBoolean called )
			 {
				 this._targetNode = targetNode;
				 this._expectedPointerType = expectedPointerType;
				 this._called = called;
			 }

			 public override void pointerHasLowerGenerationThanNode( GBPTreePointerType pointerType, long sourceNode, long pointerGeneration, long pointer, long targetNodeGeneration, File file )
			 {
				  _called.setTrue();
				  assertEquals( _targetNode, sourceNode );
				  assertEquals( _expectedPointerType, pointerType );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY,VALUE> void assertReportKeysOutOfOrderInNode(GBPTree<KEY,VALUE> index, long targetNode) throws java.io.IOException
		 private static void AssertReportKeysOutOfOrderInNode<KEY, VALUE>( GBPTree<KEY, VALUE> index, long targetNode )
		 {
			  MutableBoolean called = new MutableBoolean();
			  index.ConsistencyCheck( new GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass6( targetNode, called ) );
			  AssertCalled( called );
		 }

		 private class GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass6 : GBPTreeConsistencyCheckVisitor_Adaptor<KEY>
		 {
			 private long _targetNode;
			 private MutableBoolean _called;

			 public GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass6( long targetNode, MutableBoolean called )
			 {
				 this._targetNode = targetNode;
				 this._called = called;
			 }

			 public override void keysOutOfOrderInNode( long pageId, File file )
			 {
				  _called.setTrue();
				  assertEquals( _targetNode, pageId );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY,VALUE> void assertReportKeysLocatedInWrongNode(GBPTree<KEY,VALUE> index, long targetNode) throws java.io.IOException
		 private static void AssertReportKeysLocatedInWrongNode<KEY, VALUE>( GBPTree<KEY, VALUE> index, long targetNode )
		 {
			  ISet<long> allNodesWithKeysLocatedInWrongNode = new HashSet<long>();
			  MutableBoolean called = new MutableBoolean();
			  index.ConsistencyCheck( new GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass7( allNodesWithKeysLocatedInWrongNode, called ) );
			  AssertCalled( called );
			  assertTrue( allNodesWithKeysLocatedInWrongNode.Contains( targetNode ) );
		 }

		 private class GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass7 : GBPTreeConsistencyCheckVisitor_Adaptor<KEY>
		 {
			 private ISet<long> _allNodesWithKeysLocatedInWrongNode;
			 private MutableBoolean _called;

			 public GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass7( ISet<long> allNodesWithKeysLocatedInWrongNode, MutableBoolean called )
			 {
				 this._allNodesWithKeysLocatedInWrongNode = allNodesWithKeysLocatedInWrongNode;
				 this._called = called;
			 }

			 public override void keysLocatedInWrongNode( KeyRange<KEY> range, KEY key, int pos, int keyCount, long pageId, File file )
			 {
				  _called.setTrue();
				  _allNodesWithKeysLocatedInWrongNode.Add( pageId );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY,VALUE> void assertReportAllocSpaceOverlapActiveKeys(GBPTree<KEY,VALUE> index, long targetNode) throws java.io.IOException
		 private static void AssertReportAllocSpaceOverlapActiveKeys<KEY, VALUE>( GBPTree<KEY, VALUE> index, long targetNode )
		 {
			  MutableBoolean called = new MutableBoolean();
			  index.ConsistencyCheck( new GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass8( targetNode, called ) );
			  AssertCalled( called );
		 }

		 private class GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass8 : GBPTreeConsistencyCheckVisitor_Adaptor<KEY>
		 {
			 private long _targetNode;
			 private MutableBoolean _called;

			 public GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass8( long targetNode, MutableBoolean called )
			 {
				 this._targetNode = targetNode;
				 this._called = called;
			 }

			 public override void nodeMetaInconsistency( long pageId, string message, File file )
			 {
				  _called.setTrue();
				  assertEquals( _targetNode, pageId );
				  Assert.assertThat( message, containsString( "Overlap between allocSpace and active keys" ) );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY,VALUE> void assertReportAllocSpaceOverlapOffsetArray(GBPTree<KEY,VALUE> index, long targetNode) throws java.io.IOException
		 private static void AssertReportAllocSpaceOverlapOffsetArray<KEY, VALUE>( GBPTree<KEY, VALUE> index, long targetNode )
		 {
			  MutableBoolean called = new MutableBoolean();
			  index.ConsistencyCheck( new GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass9( targetNode, called ) );
			  AssertCalled( called );
		 }

		 private class GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass9 : GBPTreeConsistencyCheckVisitor_Adaptor<KEY>
		 {
			 private long _targetNode;
			 private MutableBoolean _called;

			 public GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass9( long targetNode, MutableBoolean called )
			 {
				 this._targetNode = targetNode;
				 this._called = called;
			 }

			 public override void nodeMetaInconsistency( long pageId, string message, File file )
			 {
				  _called.setTrue();
				  assertEquals( _targetNode, pageId );
				  Assert.assertThat( message, containsString( "Overlap between offsetArray and allocSpace" ) );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY,VALUE> void assertReportSpaceAreasNotSummingToTotalSpace(GBPTree<KEY,VALUE> index, long targetNode) throws java.io.IOException
		 private static void AssertReportSpaceAreasNotSummingToTotalSpace<KEY, VALUE>( GBPTree<KEY, VALUE> index, long targetNode )
		 {
			  MutableBoolean called = new MutableBoolean();
			  index.ConsistencyCheck( new GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass10( targetNode, called ) );
			  AssertCalled( called );
		 }

		 private class GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass10 : GBPTreeConsistencyCheckVisitor_Adaptor<KEY>
		 {
			 private long _targetNode;
			 private MutableBoolean _called;

			 public GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass10( long targetNode, MutableBoolean called )
			 {
				 this._targetNode = targetNode;
				 this._called = called;
			 }

			 public override void nodeMetaInconsistency( long pageId, string message, File file )
			 {
				  _called.setTrue();
				  assertEquals( _targetNode, pageId );
				  Assert.assertThat( message, containsString( "Space areas did not sum to total space" ) );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY,VALUE> void assertReportAllocOffsetMisplaced(GBPTree<KEY,VALUE> index, long targetNode) throws java.io.IOException
		 private static void AssertReportAllocOffsetMisplaced<KEY, VALUE>( GBPTree<KEY, VALUE> index, long targetNode )
		 {
			  MutableBoolean called = new MutableBoolean();
			  index.ConsistencyCheck( new GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass11( targetNode, called ) );
			  AssertCalled( called );
		 }

		 private class GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass11 : GBPTreeConsistencyCheckVisitor_Adaptor<KEY>
		 {
			 private long _targetNode;
			 private MutableBoolean _called;

			 public GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass11( long targetNode, MutableBoolean called )
			 {
				 this._targetNode = targetNode;
				 this._called = called;
			 }

			 public override void nodeMetaInconsistency( long pageId, string message, File file )
			 {
				  _called.setTrue();
				  assertEquals( _targetNode, pageId );
				  Assert.assertThat( message, containsString( "Pointer to allocSpace is misplaced, it should point to start of key" ) );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY,VALUE> void assertReportUnusedPage(GBPTree<KEY,VALUE> index, long targetNode) throws java.io.IOException
		 private static void AssertReportUnusedPage<KEY, VALUE>( GBPTree<KEY, VALUE> index, long targetNode )
		 {
			  MutableBoolean called = new MutableBoolean();
			  index.ConsistencyCheck( new GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass12( targetNode, called ) );
			  AssertCalled( called );
		 }

		 private class GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass12 : GBPTreeConsistencyCheckVisitor_Adaptor<KEY>
		 {
			 private long _targetNode;
			 private MutableBoolean _called;

			 public GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass12( long targetNode, MutableBoolean called )
			 {
				 this._targetNode = targetNode;
				 this._called = called;
			 }

			 public override void unusedPage( long pageId, File file )
			 {
				  _called.setTrue();
				  assertEquals( _targetNode, pageId );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY,VALUE> void assertReportActiveTreeNodeInFreelist(GBPTree<KEY,VALUE> index, long targetNode) throws java.io.IOException
		 private static void AssertReportActiveTreeNodeInFreelist<KEY, VALUE>( GBPTree<KEY, VALUE> index, long targetNode )
		 {
			  MutableBoolean called = new MutableBoolean();
			  index.ConsistencyCheck( new GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass13( targetNode, called ) );
			  AssertCalled( called );
		 }

		 private class GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass13 : GBPTreeConsistencyCheckVisitor_Adaptor<KEY>
		 {
			 private long _targetNode;
			 private MutableBoolean _called;

			 public GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass13( long targetNode, MutableBoolean called )
			 {
				 this._targetNode = targetNode;
				 this._called = called;
			 }

			 public override void pageIdSeenMultipleTimes( long pageId, File file )
			 {
				  _called.setTrue();
				  assertEquals( _targetNode, pageId );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY,VALUE> void assertReportIdExceedLastId(GBPTree<KEY,VALUE> index, long targetLastId, long targetPageId) throws java.io.IOException
		 private static void AssertReportIdExceedLastId<KEY, VALUE>( GBPTree<KEY, VALUE> index, long targetLastId, long targetPageId )
		 {
			  MutableBoolean called = new MutableBoolean();
			  index.ConsistencyCheck( new GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass14( targetLastId, targetPageId, called ) );
			  AssertCalled( called );
		 }

		 private class GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass14 : GBPTreeConsistencyCheckVisitor_Adaptor<KEY>
		 {
			 private long _targetLastId;
			 private long _targetPageId;
			 private MutableBoolean _called;

			 public GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass14( long targetLastId, long targetPageId, MutableBoolean called )
			 {
				 this._targetLastId = targetLastId;
				 this._targetPageId = targetPageId;
				 this._called = called;
			 }

			 public override void pageIdExceedLastId( long lastId, long pageId, File file )
			 {
				  _called.setTrue();
				  assertEquals( _targetLastId, lastId );
				  assertEquals( _targetPageId, pageId );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY,VALUE> void assertReportCrashedGSPP(GBPTree<KEY,VALUE> index, long targetNode, GBPTreePointerType targetPointerType) throws java.io.IOException
		 private static void AssertReportCrashedGSPP<KEY, VALUE>( GBPTree<KEY, VALUE> index, long targetNode, GBPTreePointerType targetPointerType )
		 {
			  MutableBoolean called = new MutableBoolean();
			  index.ConsistencyCheck( new GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass15( targetNode, targetPointerType, called ) );
			  AssertCalled( called );
		 }

		 private class GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass15 : GBPTreeConsistencyCheckVisitor_Adaptor<KEY>
		 {
			 private long _targetNode;
			 private Neo4Net.Index.Internal.gbptree.GBPTreePointerType _targetPointerType;
			 private MutableBoolean _called;

			 public GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass15( long targetNode, Neo4Net.Index.Internal.gbptree.GBPTreePointerType targetPointerType, MutableBoolean called )
			 {
				 this._targetNode = targetNode;
				 this._targetPointerType = targetPointerType;
				 this._called = called;
			 }

			 public override void crashedPointer( long pageId, GBPTreePointerType pointerType, long generationA, long readPointerA, long pointerA, sbyte stateA, long generationB, long readPointerB, long pointerB, sbyte stateB, File file )
			 {
				  _called.setTrue();
				  assertEquals( _targetNode, pageId );
				  assertEquals( _targetPointerType, pointerType );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY,VALUE> void assertReportBrokenGSPP(GBPTree<KEY,VALUE> index, long targetNode, GBPTreePointerType targetPointerType) throws java.io.IOException
		 private static void AssertReportBrokenGSPP<KEY, VALUE>( GBPTree<KEY, VALUE> index, long targetNode, GBPTreePointerType targetPointerType )
		 {
			  MutableBoolean called = new MutableBoolean();
			  index.ConsistencyCheck( new GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass16( targetNode, targetPointerType, called ) );
			  AssertCalled( called );
		 }

		 private class GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass16 : GBPTreeConsistencyCheckVisitor_Adaptor<KEY>
		 {
			 private long _targetNode;
			 private Neo4Net.Index.Internal.gbptree.GBPTreePointerType _targetPointerType;
			 private MutableBoolean _called;

			 public GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass16( long targetNode, Neo4Net.Index.Internal.gbptree.GBPTreePointerType targetPointerType, MutableBoolean called )
			 {
				 this._targetNode = targetNode;
				 this._targetPointerType = targetPointerType;
				 this._called = called;
			 }

			 public override void brokenPointer( long pageId, GBPTreePointerType pointerType, long generationA, long readPointerA, long pointerA, sbyte stateA, long generationB, long readPointerB, long pointerB, sbyte stateB, File file )
			 {
				  _called.setTrue();
				  assertEquals( _targetNode, pageId );
				  assertEquals( _targetPointerType, pointerType );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY,VALUE> void assertReportUnreasonableKeyCount(GBPTree<KEY,VALUE> index, long targetNode, int targetKeyCount) throws java.io.IOException
		 private static void AssertReportUnreasonableKeyCount<KEY, VALUE>( GBPTree<KEY, VALUE> index, long targetNode, int targetKeyCount )
		 {
			  MutableBoolean called = new MutableBoolean();
			  index.ConsistencyCheck( new GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass17( targetNode, targetKeyCount, called ) );
			  AssertCalled( called );
		 }

		 private class GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass17 : GBPTreeConsistencyCheckVisitor_Adaptor<KEY>
		 {
			 private long _targetNode;
			 private int _targetKeyCount;
			 private MutableBoolean _called;

			 public GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass17( long targetNode, int targetKeyCount, MutableBoolean called )
			 {
				 this._targetNode = targetNode;
				 this._targetKeyCount = targetKeyCount;
				 this._called = called;
			 }

			 public override void unreasonableKeyCount( long pageId, int keyCount, File file )
			 {
				  _called.setTrue();
				  assertEquals( _targetNode, pageId );
				  assertEquals( _targetKeyCount, keyCount );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY,VALUE> void assertReportAnyStructuralInconsistency(GBPTree<KEY,VALUE> index) throws java.io.IOException
		 private static void AssertReportAnyStructuralInconsistency<KEY, VALUE>( GBPTree<KEY, VALUE> index )
		 {
			  MutableBoolean called = new MutableBoolean();
			  index.ConsistencyCheck( new GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass18( called ) );
			  AssertCalled( called );
		 }

		 private class GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass18 : GBPTreeConsistencyCheckVisitor_Adaptor<KEY>
		 {
			 private MutableBoolean _called;

			 public GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass18( MutableBoolean called )
			 {
				 this._called = called;
			 }

			 public override void rightmostNodeHasRightSibling( long rightSiblingPointer, long rightmostNode, File file )
			 {
				  _called.setTrue();
			 }

			 public override void siblingsDontPointToEachOther( long leftNode, long leftNodeGeneration, long leftRightSiblingPointerGeneration, long leftRightSiblingPointer, long rightLeftSiblingPointer, long rightLeftSiblingPointerGeneration, long rightNode, long rightNodeGeneration, File file )
			 {
				  _called.setTrue();
			 }

			 public override void keysLocatedInWrongNode( KeyRange<KEY> range, KEY key, int pos, int keyCount, long pageId, File file )
			 {
				  _called.setTrue();
			 }

			 public override void pageIdSeenMultipleTimes( long pageId, File file )
			 {
				  _called.setTrue();
			 }

			 public override void childNodeFoundAmongParentNodes( KeyRange<KEY> superRange, int level, long pageId, File file )
			 {
				  _called.setTrue();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY,VALUE> void assertReportCircularChildPointer(GBPTree<KEY,VALUE> index, long targetNode) throws java.io.IOException
		 private static void AssertReportCircularChildPointer<KEY, VALUE>( GBPTree<KEY, VALUE> index, long targetNode )
		 {
			  MutableBoolean called = new MutableBoolean();
			  index.ConsistencyCheck( new GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass19( targetNode, called ) );
			  AssertCalled( called );
		 }

		 private class GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass19 : GBPTreeConsistencyCheckVisitor_Adaptor<KEY>
		 {
			 private long _targetNode;
			 private MutableBoolean _called;

			 public GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass19( long targetNode, MutableBoolean called )
			 {
				 this._targetNode = targetNode;
				 this._called = called;
			 }

			 public override void childNodeFoundAmongParentNodes( KeyRange<KEY> superRange, int level, long pageId, File file )
			 {
				  _called.setTrue();
				  assertEquals( _targetNode, pageId );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY,VALUE> void assertReportException(GBPTree<KEY,VALUE> index) throws java.io.IOException
		 private static void AssertReportException<KEY, VALUE>( GBPTree<KEY, VALUE> index )
		 {
			  MutableBoolean called = new MutableBoolean();
			  index.ConsistencyCheck( new GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass20( called ) );
			  AssertCalled( called );
		 }

		 private class GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass20 : GBPTreeConsistencyCheckVisitor_Adaptor<KEY>
		 {
			 private MutableBoolean _called;

			 public GBPTreeConsistencyCheckVisitor_AdaptorAnonymousInnerClass20( MutableBoolean called )
			 {
				 this._called = called;
			 }

			 public override void exception( Exception e )
			 {
				  _called.setTrue();
			 }
		 }

		 private static void AssertCalled( MutableBoolean called )
		 {
			  assertTrue( "Expected to receive call to correct consistency report method.", called.Value );
		 }

		 private long RandomAmong( LongList list )
		 {
			  return list.get( _random.Next( list.size() ) );
		 }
	}

}