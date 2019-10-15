﻿using System;

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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{

	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using StandalonePageCacheFactory = Neo4Net.Io.pagecache.impl.muninn.StandalonePageCacheFactory;
	using ThreadPoolJobScheduler = Neo4Net.Scheduler.ThreadPoolJobScheduler;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

	public class NumberArrayPageCacheTestSupport
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static Fixture prepareDirectoryAndPageCache(Class testClass) throws java.io.IOException
		 internal static Fixture PrepareDirectoryAndPageCache( Type testClass )
		 {
			  DefaultFileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction();
			  TestDirectory testDirectory = TestDirectory.testDirectory( testClass, fileSystem );
			  File dir = testDirectory.PrepareDirectoryForTest( "test" );
			  ThreadPoolJobScheduler scheduler = new ThreadPoolJobScheduler();
			  PageCache pageCache = StandalonePageCacheFactory.createPageCache( fileSystem, scheduler );
			  return new Fixture( pageCache, fileSystem, dir, scheduler );
		 }

		 public class Fixture : IDisposable
		 {
			  public readonly PageCache PageCache;
			  public readonly FileSystemAbstraction FileSystem;
			  public readonly File Directory;
			  internal readonly ThreadPoolJobScheduler Scheduler;

			  internal Fixture( PageCache pageCache, FileSystemAbstraction fileSystem, File directory, ThreadPoolJobScheduler scheduler )
			  {
					this.PageCache = pageCache;
					this.FileSystem = fileSystem;
					this.Directory = directory;
					this.Scheduler = scheduler;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
			  public override void Close()
			  {
					PageCache.close();
					Scheduler.close();
					FileSystem.Dispose();
			  }
		 }
	}

}