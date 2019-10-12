using System.Threading;

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
	using Race = Neo4Net.Test.Race;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;


	public abstract class PageCacheNumberArrayConcurrencyTest
	{
		private bool InstanceFieldsInitialized = false;

		public PageCacheNumberArrayConcurrencyTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _fs ).around( _dir ).around( Random ).around( _pageCacheRule );
		}

		 protected internal const int COUNT = 100;
		 protected internal const int LAPS = 2_000;
		 protected internal const int CONTESTANTS = 10;

		 private readonly DefaultFileSystemRule _fs = new DefaultFileSystemRule();
		 private readonly TestDirectory _dir = TestDirectory.testDirectory();
		 protected internal readonly RandomRule Random = new RandomRule();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fs).around(dir).around(random).around(pageCacheRule);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleConcurrentAccessToSameData() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleConcurrentAccessToSameData()
		 {
			  DoRace( this.wholeFileRacer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleConcurrentAccessToDifferentData() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleConcurrentAccessToDifferentData()
		 {
			  DoRace( this.fileRangeRacer );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void doRace(System.Func<NumberArray,int,Runnable> contestantCreator) throws Throwable
		 private void DoRace( System.Func<NumberArray, int, ThreadStart> contestantCreator )
		 {
			  PageCache pageCache = _pageCacheRule.getPageCache( _fs );
			  PagedFile file = pageCache.Map( _dir.file( "file" ), pageCache.PageSize(), CREATE, DELETE_ON_CLOSE );
			  Race race = new Race();
			  using ( NumberArray array = GetNumberArray( file ) )
			  {
					for ( int i = 0; i < CONTESTANTS; i++ )
					{
						 race.AddContestant( contestantCreator( array, i ) );
					}
					race.Go();

			  }
		 }

		 protected internal abstract ThreadStart FileRangeRacer( NumberArray array, int contestant );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract NumberArray getNumberArray(org.neo4j.io.pagecache.PagedFile file) throws java.io.IOException;
		 protected internal abstract NumberArray GetNumberArray( PagedFile file );

		 protected internal abstract ThreadStart WholeFileRacer( NumberArray array, int contestant );

	}

}