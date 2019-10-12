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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;

	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class PageCacheLongArrayTest
	{
		private bool InstanceFieldsInitialized = false;

		public PageCacheLongArrayTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _fs ).around( _dir ).around( _random ).around( _pageCacheRule );
		}

		 private const int COUNT = 1_000_000;

		 private readonly DefaultFileSystemRule _fs = new DefaultFileSystemRule();
		 private readonly TestDirectory _dir = TestDirectory.testDirectory();
		 private readonly RandomRule _random = new RandomRule();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fs).around(dir).around(random).around(pageCacheRule);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void verifyPageCacheLongArray() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void VerifyPageCacheLongArray()
		 {
			  PageCache pageCache = _pageCacheRule.getPageCache( _fs );
			  PagedFile file = pageCache.Map( _dir.file( "file" ), pageCache.PageSize(), CREATE, DELETE_ON_CLOSE );

			  using ( LongArray array = new PageCacheLongArray( file, COUNT, 0, 0 ) )
			  {
					VerifyBehaviour( array );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void verifyChunkingArrayWithPageCacheLongArray()
		 public virtual void VerifyChunkingArrayWithPageCacheLongArray()
		 {
			  PageCache pageCache = _pageCacheRule.getPageCache( _fs );
			  File directory = _dir.directory();
			  NumberArrayFactory numberArrayFactory = NumberArrayFactory.auto( pageCache, directory, false, NumberArrayFactory_Fields.NoMonitor );
			  using ( LongArray array = numberArrayFactory.NewDynamicLongArray( COUNT / 1_000, 0 ) )
			  {
					VerifyBehaviour( array );
			  }
		 }

		 private void VerifyBehaviour( LongArray array )
		 {
			  // insert
			  for ( int i = 0; i < COUNT; i++ )
			  {
					array.Set( i, i );
			  }

			  // verify inserted data
			  for ( int i = 0; i < COUNT; i++ )
			  {
					assertEquals( i, array.Get( i ) );
			  }

			  // verify inserted data with random access patterns
			  int stride = 12_345_678;
			  int next = _random.Next( COUNT );
			  for ( int i = 0; i < COUNT; i++ )
			  {
					assertEquals( next, array.Get( next ) );
					next = ( next + stride ) % COUNT;
			  }
		 }
	}

}