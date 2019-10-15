using System;
using System.Threading;

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
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using SingleFilePageSwapperFactory = Neo4Net.Io.pagecache.impl.SingleFilePageSwapperFactory;
	using MuninnPageCache = Neo4Net.Io.pagecache.impl.muninn.MuninnPageCache;
	using PageCacheTracer = Neo4Net.Io.pagecache.tracing.PageCacheTracer;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Neo4Net.Scheduler.ThreadPoolJobScheduler;
	using PageCacheAndDependenciesRule = Neo4Net.Test.rule.PageCacheAndDependenciesRule;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.config.Configuration.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.GBPTree.NO_HEADER_READER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.SimpleLongLayout.longLayout;

	/// <summary>
	/// Tests functionality around process crashing, or similar, when having started, but not completed creation of an index file,
	/// i.e. opening an index file for the first time.
	/// 
	/// This test is an asset in finding potentially new issues regarding partially created index files over time.
	/// It will not guarantee, in one run, that every case has been covered. There are other specific test cases for that.
	/// When this test finds a new issue that should be encoded into a proper unit test in <seealso cref="GBPTreeTest"/> or similar.
	/// </summary>
	public class GBPTreePartialCreateFuzzIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.PageCacheAndDependenciesRule storage = new org.neo4j.test.rule.PageCacheAndDependenciesRule().with(new org.neo4j.test.rule.fs.DefaultFileSystemRule());
		 public readonly PageCacheAndDependenciesRule Storage = new PageCacheAndDependenciesRule().with(new DefaultFileSystemRule());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectAndThrowIOExceptionOnPartiallyCreatedFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectAndThrowIOExceptionOnPartiallyCreatedFile()
		 {
			  // given a crashed-on-open index
			  File file = Storage.directory().file("index");
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  Process process = ( new ProcessBuilder( asList( "java", "-cp", System.getProperty( "java.class.path" ), this.GetType().FullName, file.AbsolutePath ) ) ).redirectError(INHERIT).redirectOutput(INHERIT).start();
			  Thread.Sleep( ThreadLocalRandom.current().Next(1_000) );
			  int exitCode = process.destroyForcibly().waitFor();

			  // then reading it should either work or throw IOException
			  using ( PageCache pageCache = Storage.pageCache() )
			  {
					SimpleLongLayout layout = longLayout().build();

					// check readHeader
					try
					{
						 GBPTree.ReadHeader( pageCache, file, NO_HEADER_READER );
					}
					catch ( Exception e ) when ( e is MetadataMismatchException || e is IOException )
					{
						 // It's OK if the process was destroyed
						 assertNotEquals( 0, exitCode );
					}

					// check constructor
					try
					{
						 ( new GBPTreeBuilder<>( pageCache, file, layout ) ).Build().Dispose();
					}
					catch ( Exception e ) when ( e is MetadataMismatchException || e is IOException )
					{
						 // It's OK if the process was destroyed
						 assertNotEquals( 0, exitCode );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws Exception
		 public static void Main( string[] args )
		 {
			  // Just start and immediately close. The process spawning this subprocess will kill it in the middle of all this
			  File file = new File( args[0] );
			  using ( FileSystemAbstraction fs = new DefaultFileSystemAbstraction(), IJobScheduler jobScheduler = new ThreadPoolJobScheduler() )
			  {
					SingleFilePageSwapperFactory swapper = new SingleFilePageSwapperFactory();
					swapper.Open( fs, EMPTY );
					using ( PageCache pageCache = new MuninnPageCache( swapper, 10, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, EmptyVersionContextSupplier.EMPTY, jobScheduler ) )
					{
						 fs.DeleteFile( file );
						 ( new GBPTreeBuilder<>( pageCache, file, longLayout().build() ) ).Build().Dispose();
					}
			  }
		 }
	}

}