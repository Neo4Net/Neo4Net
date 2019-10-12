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
namespace Org.Neo4j.Io.pagecache.stress
{

	using Configuration = Org.Neo4j.Graphdb.config.Configuration;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using SingleFilePageSwapperFactory = Org.Neo4j.Io.pagecache.impl.SingleFilePageSwapperFactory;
	using MuninnPageCache = Org.Neo4j.Io.pagecache.impl.muninn.MuninnPageCache;
	using PageCacheTracer = Org.Neo4j.Io.pagecache.tracing.PageCacheTracer;
	using PageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Org.Neo4j.Scheduler.ThreadPoolJobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.tracing.PageCacheTracer.NULL;

	/// <summary>
	/// A stress test for page cache(s).
	/// 
	/// The test will stress a page cache by mutating records and keeping an invariant for each record. Thus, before writing
	/// to a record, the record is be tested to see if the invariant still holds. Also, at the end of the test all records
	/// are verified in that same manner.
	/// 
	/// The test runs using multiple threads. It relies on page cache's exclusive locks to maintain the invariant.
	/// 
	/// The page cache covers a fraction of a file, and the access pattern is uniformly random, so that pages are loaded
	/// and evicted frequently.
	/// 
	/// Records: a record is 1x counter for each thread, indexed by the threads' number, with 1x checksum = sum of counters.
	/// 
	/// Invariant: the sum of counters is always equal to the checksum. For a blank file, this is trivially true:
	/// sum(0, 0, 0, ...) = 0. Any record mutation is a counter increment and checksum increment.
	/// </summary>
	public class PageCacheStressTest
	{
		 private readonly int _numberOfPages;
		 private readonly int _numberOfThreads;

		 private readonly int _numberOfCachePages;

		 private readonly PageCacheTracer _tracer;
		 private readonly PageCursorTracerSupplier _pageCursorTracerSupplier;
		 private readonly Condition _condition;

		 private readonly File _workingDirectory;

		 private PageCacheStressTest( Builder builder )
		 {
			  this._numberOfPages = builder.NumberOfPages;
			  this._numberOfThreads = builder.NumberOfThreads;

			  this._numberOfCachePages = builder.NumberOfCachePages;

			  this._tracer = builder.Tracer;
			  this._pageCursorTracerSupplier = builder.PageCursorTracerSupplier;
			  this._condition = builder.Condition;

			  this._workingDirectory = builder.WorkingDirectory;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void run() throws Exception
		 public virtual void Run()
		 {
			  using ( FileSystemAbstraction fs = new DefaultFileSystemAbstraction(), JobScheduler jobScheduler = new ThreadPoolJobScheduler() )
			  {
					PageSwapperFactory swapperFactory = new SingleFilePageSwapperFactory();
					swapperFactory.Open( fs, Configuration.EMPTY );
					using ( PageCache pageCacheUnderTest = new MuninnPageCache( swapperFactory, _numberOfCachePages, _tracer, _pageCursorTracerSupplier, EmptyVersionContextSupplier.EMPTY, jobScheduler ) )
					{
						 PageCacheStresser pageCacheStresser = new PageCacheStresser( _numberOfPages, _numberOfThreads, _workingDirectory );
						 pageCacheStresser.Stress( pageCacheUnderTest, _condition );
					}
			  }
		 }

		 public class Builder
		 {
			  internal int NumberOfPages = 10000;
			  internal int NumberOfThreads = 7;

			  internal int NumberOfCachePages = 1000;

			  internal PageCacheTracer Tracer = NULL;
			  internal PageCursorTracerSupplier PageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null;
			  internal Condition Condition;

			  internal File WorkingDirectory;

			  public virtual PageCacheStressTest Build()
			  {
					assertThat( "the cache should cover only a fraction of the mapped file", NumberOfPages, @is( greaterThanOrEqualTo( 10 * NumberOfCachePages ) ) );
					return new PageCacheStressTest( this );
			  }

			  public virtual Builder With( PageCacheTracer tracer )
			  {
					this.Tracer = tracer;
					return this;
			  }

			  public virtual Builder With( Condition condition )
			  {
					this.Condition = condition;
					return this;
			  }

			  public virtual Builder WithNumberOfPages( int value )
			  {
					this.NumberOfPages = value;
					return this;
			  }

			  public virtual Builder WithNumberOfThreads( int numberOfThreads )
			  {
					this.NumberOfThreads = numberOfThreads;
					return this;
			  }

			  public virtual Builder WithNumberOfCachePages( int numberOfCachePages )
			  {
					this.NumberOfCachePages = numberOfCachePages;
					return this;
			  }

			  public virtual Builder WithWorkingDirectory( File workingDirectory )
			  {
					this.WorkingDirectory = workingDirectory;
					return this;
			  }

			  public virtual Builder WithPageCursorTracerSupplier( PageCursorTracerSupplier cursorTracerSupplier )
			  {
					this.PageCursorTracerSupplier = cursorTracerSupplier;
					return this;
			  }
		 }
	}

}