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
namespace Org.Neo4j.Index.@internal.gbptree
{
	using MutableInt = org.apache.commons.lang3.mutable.MutableInt;
	using MutableLong = org.apache.commons.lang3.mutable.MutableLong;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using PagedFile = Org.Neo4j.Io.pagecache.PagedFile;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using RandomRule = Org.Neo4j.Test.rule.RandomRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;
	using Org.Neo4j.Test.rule.fs;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.SimpleLongLayout.longLayout;
	using static Org.Neo4j.Index.@internal.gbptree.TreeNode.Overflow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.TreeNode.setKeyCount;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.PageCacheRule.config;

	public class CrashGenerationCleanerTest
	{
		private bool InstanceFieldsInitialized = false;

		public CrashGenerationCleanerTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_testDirectory = TestDirectory.testDirectory( this.GetType(), _fileSystemRule.get() );
			RuleChain = RuleChain.outerRule( _fileSystemRule ).around( _testDirectory ).around( _pageCacheRule ).around( _randomRule );
			_treeNode = new TreeNodeFixedSize<MutableLong, MutableLong>( PAGE_SIZE, _layout );
		}

		 private readonly FileSystemRule _fileSystemRule = new DefaultFileSystemRule();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private TestDirectory _testDirectory;
		 private readonly RandomRule _randomRule = new RandomRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fileSystemRule).around(testDirectory).around(pageCacheRule).around(randomRule);
		 public RuleChain RuleChain;

		 private const string FILE_NAME = "index";
		 private const int PAGE_SIZE = 256;

		 private PagedFile _pagedFile;
		 private readonly Layout<MutableLong, MutableLong> _layout = longLayout().build();
		 private TreeNode<MutableLong, MutableLong> _treeNode;
		 private readonly ExecutorService _executor = Executors.newFixedThreadPool( Runtime.Runtime.availableProcessors() );
		 private readonly TreeState _checkpointedTreeState = new TreeState( 0, 9, 10, 0, 0, 0, 0, 0, 0, 0, true, true );
		 private readonly TreeState _unstableTreeState = new TreeState( 0, 10, 12, 0, 0, 0, 0, 0, 0, 0, true, true );
		 private readonly IList<GBPTreeCorruption.PageCorruption> _possibleCorruptionsInInternal = Arrays.asList( GBPTreeCorruption.Crashed( GBPTreePointerType.leftSibling() ), GBPTreeCorruption.Crashed(GBPTreePointerType.rightSibling()), GBPTreeCorruption.Crashed(GBPTreePointerType.successor()), GBPTreeCorruption.Crashed(GBPTreePointerType.child(0)) );
		 private readonly IList<GBPTreeCorruption.PageCorruption> _possibleCorruptionsInLeaf = Arrays.asList( GBPTreeCorruption.Crashed( GBPTreePointerType.leftSibling() ), GBPTreeCorruption.Crashed(GBPTreePointerType.rightSibling()), GBPTreeCorruption.Crashed(GBPTreePointerType.successor()) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupPagedFile() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetupPagedFile()
		 {
			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystemRule.get(), config().withPageSize(PAGE_SIZE).withAccessChecks(true) );
			  _pagedFile = pageCache.Map( _testDirectory.file( FILE_NAME ), PAGE_SIZE, CREATE, DELETE_ON_CLOSE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void teardownPagedFile() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TeardownPagedFile()
		 {
			  _pagedFile.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCrashOnEmptyFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCrashOnEmptyFile()
		 {
			  // GIVEN
			  Page[] pages = With();
			  InitializeFile( _pagedFile, pages );

			  // WHEN
			  SimpleCleanupMonitor monitor = new SimpleCleanupMonitor();
			  CrashGenerationCleaner( _pagedFile, 0, pages.Length, monitor ).clean( _executor );

			  // THEN
			  AssertPagesVisited( monitor, pages.Length );
			  AssertCleanedCrashPointers( monitor, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReportErrorsOnCleanPages() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotReportErrorsOnCleanPages()
		 {
			  // GIVEN
			  Page[] pages = With( LeafWith(), InternalWith() );
			  InitializeFile( _pagedFile, pages );

			  // WHEN
			  SimpleCleanupMonitor monitor = new SimpleCleanupMonitor();
			  CrashGenerationCleaner( _pagedFile, 0, pages.Length, monitor ).clean( _executor );

			  // THEN
			  AssertPagesVisited( monitor, 2 );
			  AssertCleanedCrashPointers( monitor, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCleanOneCrashPerPage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCleanOneCrashPerPage()
		 {
			  // GIVEN
			  Page[] pages = With( LeafWith( GBPTreeCorruption.Crashed( GBPTreePointerType.leftSibling() ) ), InternalWith(GBPTreeCorruption.Crashed(GBPTreePointerType.leftSibling())), LeafWith(GBPTreeCorruption.Crashed(GBPTreePointerType.rightSibling())), InternalWith(GBPTreeCorruption.Crashed(GBPTreePointerType.rightSibling())), LeafWith(GBPTreeCorruption.Crashed(GBPTreePointerType.successor())), InternalWith(GBPTreeCorruption.Crashed(GBPTreePointerType.successor())), InternalWith(GBPTreeCorruption.Crashed(GBPTreePointerType.child(0))) );
			  InitializeFile( _pagedFile, pages );

			  // WHEN
			  SimpleCleanupMonitor monitor = new SimpleCleanupMonitor();
			  CrashGenerationCleaner( _pagedFile, 0, pages.Length, monitor ).clean( _executor );

			  // THEN
			  AssertPagesVisited( monitor, pages.Length );
			  AssertCleanedCrashPointers( monitor, 7 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCleanMultipleCrashPerPage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCleanMultipleCrashPerPage()
		 {
			  // GIVEN
			  Page[] pages = With( LeafWith( GBPTreeCorruption.Crashed( GBPTreePointerType.leftSibling() ), GBPTreeCorruption.Crashed(GBPTreePointerType.rightSibling()), GBPTreeCorruption.Crashed(GBPTreePointerType.successor()) ), InternalWith(GBPTreeCorruption.Crashed(GBPTreePointerType.leftSibling()), GBPTreeCorruption.Crashed(GBPTreePointerType.rightSibling()), GBPTreeCorruption.Crashed(GBPTreePointerType.successor()), GBPTreeCorruption.Crashed(GBPTreePointerType.child(0))) );
			  InitializeFile( _pagedFile, pages );

			  // WHEN
			  SimpleCleanupMonitor monitor = new SimpleCleanupMonitor();
			  CrashGenerationCleaner( _pagedFile, 0, pages.Length, monitor ).clean( _executor );

			  // THEN
			  AssertPagesVisited( monitor, pages.Length );
			  AssertCleanedCrashPointers( monitor, 7 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCleanLargeFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCleanLargeFile()
		 {
			  // GIVEN
			  int numberOfPages = _randomRule.intBetween( 1_000, 10_000 );
			  int corruptionPercent = _randomRule.Next( 90 );
			  MutableInt totalNumberOfCorruptions = new MutableInt( 0 );

			  Page[] pages = new Page[numberOfPages];
			  for ( int i = 0; i < numberOfPages; i++ )
			  {
					Page page = RandomPage( corruptionPercent, totalNumberOfCorruptions );
					pages[i] = page;
			  }
			  InitializeFile( _pagedFile, pages );

			  // WHEN
			  SimpleCleanupMonitor monitor = new SimpleCleanupMonitor();
			  CrashGenerationCleaner( _pagedFile, 0, numberOfPages, monitor ).clean( _executor );

			  // THEN
			  AssertPagesVisited( monitor, numberOfPages );
			  AssertCleanedCrashPointers( monitor, totalNumberOfCorruptions.Value );
		 }

		 private CrashGenerationCleaner CrashGenerationCleaner( PagedFile pagedFile, int lowTreeNodeId, int highTreeNodeId, SimpleCleanupMonitor monitor )
		 {
			  return new CrashGenerationCleaner( pagedFile, _treeNode, lowTreeNodeId, highTreeNodeId, _unstableTreeState.stableGeneration(), _unstableTreeState.unstableGeneration(), monitor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void initializeFile(org.neo4j.io.pagecache.PagedFile pagedFile, Page... pages) throws java.io.IOException
		 private void InitializeFile( PagedFile pagedFile, params Page[] pages )
		 {
			  using ( PageCursor cursor = pagedFile.Io( 0, Org.Neo4j.Io.pagecache.PagedFile_Fields.PfSharedWriteLock ) )
			  {
					foreach ( Page page in pages )
					{
						 cursor.Next();
						 page.Write( pagedFile, cursor, _treeNode, _layout, _checkpointedTreeState, _unstableTreeState );
					}
			  }
		 }

		 /* Assertions */
		 private void AssertCleanedCrashPointers( SimpleCleanupMonitor monitor, int expectedNumberOfCleanedCrashPointers )
		 {
			  assertEquals( "Expected number of cleaned crash pointers to be " + expectedNumberOfCleanedCrashPointers + " but was " + monitor.NumberOfCleanedCrashPointers, expectedNumberOfCleanedCrashPointers, monitor.NumberOfCleanedCrashPointers );
		 }

		 private void AssertPagesVisited( SimpleCleanupMonitor monitor, int expectedNumberOfPagesVisited )
		 {
			  assertEquals( "Expected number of visited pages to be " + expectedNumberOfPagesVisited + " but was " + monitor.NumberOfPagesVisited, expectedNumberOfPagesVisited, monitor.NumberOfPagesVisited );
		 }

		 /* Random page */
		 private Page RandomPage( int corruptionPercent, MutableInt totalNumberOfCorruptions )
		 {
			  int numberOfCorruptions = 0;
			  bool @internal = _randomRule.nextBoolean();
			  if ( _randomRule.Next( 100 ) < corruptionPercent )
			  {
					int maxCorruptions = @internal ? _possibleCorruptionsInInternal.Count : _possibleCorruptionsInLeaf.Count;
					numberOfCorruptions = _randomRule.intBetween( 1, maxCorruptions );
					totalNumberOfCorruptions.add( numberOfCorruptions );
			  }
			  return @internal ? RandomInternal( numberOfCorruptions ) : RandomLeaf( numberOfCorruptions );
		 }

		 private Page RandomLeaf( int numberOfCorruptions )
		 {
			  Collections.shuffle( _possibleCorruptionsInLeaf );
			  GBPTreeCorruption.PageCorruption[] corruptions = new GBPTreeCorruption.PageCorruption[numberOfCorruptions];
			  for ( int i = 0; i < numberOfCorruptions; i++ )
			  {
					corruptions[i] = _possibleCorruptionsInLeaf[i];
			  }
			  return LeafWith( corruptions );
		 }

		 private Page RandomInternal( int numberOfCorruptions )
		 {
			  Collections.shuffle( _possibleCorruptionsInInternal );
			  GBPTreeCorruption.PageCorruption[] corruptions = new GBPTreeCorruption.PageCorruption[numberOfCorruptions];
			  for ( int i = 0; i < numberOfCorruptions; i++ )
			  {
					corruptions[i] = _possibleCorruptionsInInternal[i];
			  }
			  return InternalWith( corruptions );
		 }

		 /* Page */
		 private Page[] With( params Page[] pages )
		 {
			  return pages;
		 }

		 private Page LeafWith( params GBPTreeCorruption.PageCorruption<MutableLong, MutableLong>[] pageCorruptions )
		 {
			  return new Page( this, PageType.Leaf, pageCorruptions );
		 }

		 private Page InternalWith( params GBPTreeCorruption.PageCorruption<MutableLong, MutableLong>[] pageCorruptions )
		 {
			  return new Page( this, PageType.Internal, pageCorruptions );
		 }

		 private class Page
		 {
			 private readonly CrashGenerationCleanerTest _outerInstance;

			  internal readonly PageType Type;
			  internal readonly GBPTreeCorruption.PageCorruption<MutableLong, MutableLong>[] PageCorruptions;

			  internal Page( CrashGenerationCleanerTest outerInstance, PageType type, params GBPTreeCorruption.PageCorruption<MutableLong, MutableLong>[] pageCorruptions )
			  {
				  this._outerInstance = outerInstance;
					this.Type = type;
					this.PageCorruptions = pageCorruptions;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void write(org.neo4j.io.pagecache.PagedFile pagedFile, org.neo4j.io.pagecache.PageCursor cursor, TreeNode<org.apache.commons.lang3.mutable.MutableLong,org.apache.commons.lang3.mutable.MutableLong> node, Layout<org.apache.commons.lang3.mutable.MutableLong,org.apache.commons.lang3.mutable.MutableLong> layout, TreeState checkpointedTreeState, TreeState unstableTreeState) throws java.io.IOException
			  internal virtual void Write( PagedFile pagedFile, PageCursor cursor, TreeNode<MutableLong, MutableLong> node, Layout<MutableLong, MutableLong> layout, TreeState checkpointedTreeState, TreeState unstableTreeState )
			  {
					Type.write( cursor, node, layout, checkpointedTreeState );
					foreach ( GBPTreeCorruption.PageCorruption<MutableLong, MutableLong> pc in PageCorruptions )
					{
						 pc.Corrupt( cursor, layout, node, unstableTreeState );
					}
			  }
		 }

		 internal abstract class PageType
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           LEAF { void write(org.neo4j.io.pagecache.PageCursor cursor, TreeNode<org.apache.commons.lang3.mutable.MutableLong, org.apache.commons.lang3.mutable.MutableLong> treeNode, Layout<org.apache.commons.lang3.mutable.MutableLong, org.apache.commons.lang3.mutable.MutableLong> layout, TreeState treeState) { treeNode.initializeLeaf(cursor, treeState.stableGeneration(), treeState.unstableGeneration()); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           INTERNAL { void write(org.neo4j.io.pagecache.PageCursor cursor, TreeNode<org.apache.commons.lang3.mutable.MutableLong, org.apache.commons.lang3.mutable.MutableLong> treeNode, Layout<org.apache.commons.lang3.mutable.MutableLong, org.apache.commons.lang3.mutable.MutableLong> layout, TreeState treeState) { treeNode.initializeInternal(cursor, treeState.stableGeneration(), treeState.unstableGeneration()); long super = IdSpace.MIN_TREE_NODE_ID; int keyCount; for(keyCount = 0; treeNode.internalOverflow(cursor, keyCount, layout.newKey()) == Overflow.NO; keyCount++) { long child = super + keyCount; treeNode.setChildAt(cursor, child, keyCount, treeState.stableGeneration(), treeState.unstableGeneration()); } setKeyCount(cursor, keyCount); } };

			  private static readonly IList<PageType> valueList = new List<PageType>();

			  static PageType()
			  {
				  valueList.Add( LEAF );
				  valueList.Add( INTERNAL );
			  }

			  public enum InnerEnum
			  {
				  LEAF,
				  INTERNAL
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private PageType( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal abstract void write( Org.Neo4j.Io.pagecache.PageCursor cursor, TreeNode<org.apache.commons.lang3.mutable.MutableLong, org.apache.commons.lang3.mutable.MutableLong> treeNode, Layout<org.apache.commons.lang3.mutable.MutableLong, org.apache.commons.lang3.mutable.MutableLong> layout, TreeState treeState );

			 public static IList<PageType> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static PageType valueOf( string name )
			 {
				 foreach ( PageType enumInstance in PageType.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }
	}

}