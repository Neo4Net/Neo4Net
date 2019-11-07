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
	using MutableLong = org.apache.commons.lang3.mutable.MutableLong;
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using Configuration = Neo4Net.GraphDb.config.Configuration;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using MemoryAllocator = Neo4Net.Io.mem.MemoryAllocator;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using SingleFilePageSwapperFactory = Neo4Net.Io.pagecache.impl.SingleFilePageSwapperFactory;
	using MuninnPageCache = Neo4Net.Io.pagecache.impl.muninn.MuninnPageCache;
	using PageCacheTracer = Neo4Net.Io.pagecache.tracing.PageCacheTracer;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using LocalMemoryTracker = Neo4Net.Memory.LocalMemoryTracker;
	using ThreadPoolJobScheduler = Neo4Net.Scheduler.ThreadPoolJobScheduler;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier.EMPTY;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class GBPTreeSingleWriterTest
	internal class GBPTreeSingleWriterTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject TestDirectory directory;
		 internal TestDirectory Directory;
		 private PageCache _pageCache;
		 private SimpleLongLayout _layout;
		 private ThreadPoolJobScheduler _jobScheduler;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void createPageCache()
		 internal virtual void CreatePageCache()
		 {
			  SingleFilePageSwapperFactory factory = new SingleFilePageSwapperFactory();
			  factory.Open( new DefaultFileSystemAbstraction(), Configuration.EMPTY );
			  MemoryAllocator mman = MemoryAllocator.createAllocator( "8 MiB", new LocalMemoryTracker() );
			  _jobScheduler = new ThreadPoolJobScheduler();
			  _pageCache = new MuninnPageCache( factory, mman, 256, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, EMPTY, _jobScheduler );
			  _layout = SimpleLongLayout.LongLayout().withFixedSize(true).build();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void tearDownPageCache()
		 internal virtual void TearDownPageCache()
		 {
			  _pageCache.close();
			  _jobScheduler.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReInitializeTreeLogicWithSameSplitRatioAsInitiallySet0() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReInitializeTreeLogicWithSameSplitRatioAsInitiallySet0()
		 {
			  TreeHeightTracker treeHeightTracker = new TreeHeightTracker( this );
			  try (GBPTree<MutableLong, MutableLong> gbpTree = new GBPTreeBuilder<>(_pageCache, Directory.file("index"), _layout)
						 .with( treeHeightTracker ).build();
					  Writer<MutableLong, MutableLong> writer = gbpTree.writer( 0 ))
					  {
					MutableLong dontCare = _layout.value( 0 );

					long keySeed = 10_000;
					while ( treeHeightTracker.TreeHeight < 5 )
					{
						 MutableLong key = _layout.key( keySeed-- );
						 writer.Put( key, dontCare );
					}
					// We now have a tree with height 6.
					// The leftmost node on all levels should have only a single key.
					KeyCountingVisitor keyCountingVisitor = new KeyCountingVisitor( this );
					gbpTree.visit( keyCountingVisitor );
					foreach ( int? leftmostKeyCount in keyCountingVisitor.KeyCountOnLeftmostPerLevel )
					{
						 assertEquals( 1, leftmostKeyCount.Value );
					}
					  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReInitializeTreeLogicWithSameSplitRatioAsInitiallySet1() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReInitializeTreeLogicWithSameSplitRatioAsInitiallySet1()
		 {
			  TreeHeightTracker treeHeightTracker = new TreeHeightTracker( this );
			  try (GBPTree<MutableLong, MutableLong> gbpTree = new GBPTreeBuilder<>(_pageCache, Directory.file("index"), _layout)
						 .with( treeHeightTracker ).build();
					  Writer<MutableLong, MutableLong> writer = gbpTree.writer( 1 ))
					  {
					MutableLong dontCare = _layout.value( 0 );

					long keySeed = 0;
					while ( treeHeightTracker.TreeHeight < 5 )
					{
						 MutableLong key = _layout.key( keySeed++ );
						 writer.Put( key, dontCare );
					}
					// We now have a tree with height 6.
					// The rightmost node on all levels should have either one or zero key (zero for internal nodes).
					KeyCountingVisitor keyCountingVisitor = new KeyCountingVisitor( this );
					gbpTree.visit( keyCountingVisitor );
					foreach ( int? rightmostKeyCount in keyCountingVisitor.KeyCountOnRightmostPerLevel )
					{
						 assertTrue( rightmostKeyCount == 0 || rightmostKeyCount == 1 );
					}
					  }
		 }

		 private class KeyCountingVisitor : GBPTreeVisitor_Adaptor<MutableLong, MutableLong>
		 {
			 private readonly GBPTreeSingleWriterTest _outerInstance;

			 public KeyCountingVisitor( GBPTreeSingleWriterTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal bool NewLevel;
			  internal IList<int> KeyCountOnLeftmostPerLevel = new List<int>();
			  internal IList<int> KeyCountOnRightmostPerLevel = new List<int>();
			  internal int RightmostKeyCountOnLevelSoFar;

			  public override void BeginLevel( int level )
			  {
					NewLevel = true;
					RightmostKeyCountOnLevelSoFar = -1;
			  }

			  public override void EndLevel( int level )
			  {
					KeyCountOnRightmostPerLevel.Add( RightmostKeyCountOnLevelSoFar );
			  }

			  public override void BeginNode( long pageId, bool isLeaf, long generation, int keyCount )
			  {
					if ( NewLevel )
					{
						 NewLevel = false;
						 KeyCountOnLeftmostPerLevel.Add( keyCount );
					}
					RightmostKeyCountOnLevelSoFar = keyCount;
			  }
		 }

		 private class TreeHeightTracker : GBPTree.Monitor_Adaptor
		 {
			 private readonly GBPTreeSingleWriterTest _outerInstance;

			 public TreeHeightTracker( GBPTreeSingleWriterTest outerInstance ) : base( outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal int TreeHeight;

			  public override void TreeGrowth()
			  {
					TreeHeight++;
			  }

			  public override void TreeShrink()
			  {
					TreeHeight--;
			  }
		 }
	}

}