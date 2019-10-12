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
namespace Org.Neo4j.Kernel.impl.pagecache
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using PagedFile = Org.Neo4j.Io.pagecache.PagedFile;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Org.Neo4j.Scheduler.ThreadPoolJobScheduler;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using VerboseTimeout = Org.Neo4j.Test.rule.VerboseTimeout;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class ConfigurableStandalonePageCacheFactoryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.VerboseTimeout timeout = org.neo4j.test.rule.VerboseTimeout.builder().withTimeout(30, java.util.concurrent.TimeUnit.SECONDS).build();
		 public VerboseTimeout Timeout = VerboseTimeout.builder().withTimeout(30, TimeUnit.SECONDS).build();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustAutomaticallyStartEvictionThread() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustAutomaticallyStartEvictionThread()
		 {
			  using ( FileSystemAbstraction fs = new DefaultFileSystemAbstraction(), JobScheduler jobScheduler = new ThreadPoolJobScheduler() )
			  {
					File file = ( new File( TestDirectory.directory(), "a" ) ).CanonicalFile;
					fs.Create( file ).close();

					using ( PageCache cache = ConfigurableStandalonePageCacheFactory.CreatePageCache( fs, jobScheduler ), PagedFile pf = cache.Map( file, 4096 ), PageCursor cursor = pf.Io( 0, Org.Neo4j.Io.pagecache.PagedFile_Fields.PfSharedWriteLock ) )
					{
						 // The default size is currently 8MBs.
						 // It should be possible to write more than that.
						 // If the eviction thread has not been started, then this test will block forever.
						 for ( int i = 0; i < 10_000; i++ )
						 {
							  assertTrue( cursor.Next() );
							  cursor.PutInt( 42 );
						 }
					}
			  }
		 }
	}

}