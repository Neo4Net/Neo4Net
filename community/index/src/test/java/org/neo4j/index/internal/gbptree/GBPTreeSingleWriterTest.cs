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
	using MutableLong = org.apache.commons.lang3.mutable.MutableLong;
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using Configuration = Org.Neo4j.Graphdb.config.Configuration;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using MemoryAllocator = Org.Neo4j.Io.mem.MemoryAllocator;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using SingleFilePageSwapperFactory = Org.Neo4j.Io.pagecache.impl.SingleFilePageSwapperFactory;
	using MuninnPageCache = Org.Neo4j.Io.pagecache.impl.muninn.MuninnPageCache;
	using PageCacheTracer = Org.Neo4j.Io.pagecache.tracing.PageCacheTracer;
	using PageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using LocalMemoryTracker = Org.Neo4j.Memory.LocalMemoryTracker;
	using ThreadPoolJobScheduler = Org.Neo4j.Scheduler.ThreadPoolJobScheduler;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier.EMPTY;

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
			  _pageCache = new MuninnPageCache( factory, mman, 256, PageCacheTracer.NULL, Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, EMPTY, _jobScheduler );
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