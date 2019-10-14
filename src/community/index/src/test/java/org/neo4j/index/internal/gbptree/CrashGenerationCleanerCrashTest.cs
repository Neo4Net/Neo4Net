using System;
using System.Collections.Generic;
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


	using Exceptions = Neo4Net.Helpers.Exceptions;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;
	using StubPagedFile = Neo4Net.Io.pagecache.StubPagedFile;
	using PageCacheAndDependenciesRule = Neo4Net.Test.rule.PageCacheAndDependenciesRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.CrashGenerationCleaner.MAX_BATCH_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.GBPTree.NO_MONITOR;

	public class CrashGenerationCleanerCrashTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.PageCacheAndDependenciesRule store = new org.neo4j.test.rule.PageCacheAndDependenciesRule();
		 public PageCacheAndDependenciesRule Store = new PageCacheAndDependenciesRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotLeakTasksOnCrash()
		 public virtual void MustNotLeakTasksOnCrash()
		 {
			  // Given
			  string exceptionMessage = "When there's no more room in hell, the dead will walk the earth";
			  CrashGenerationCleaner cleaner = NewCrashingCrashGenerationCleaner( exceptionMessage );
			  ExecutorService executorService = Executors.newFixedThreadPool( Runtime.Runtime.availableProcessors() );

			  try
			  {
					// When
					cleaner.Clean( executorService );
					fail( "Expected to throw" );
			  }
			  catch ( Exception e )
			  {
					Exception rootCause = Exceptions.rootCause( e );
					assertTrue( rootCause is IOException );
					assertEquals( exceptionMessage, rootCause.Message );
			  }
			  finally
			  {
					// Then
					IList<ThreadStart> tasks = executorService.shutdownNow();
					assertEquals( 0, tasks.Count );
			  }
		 }

		 private CrashGenerationCleaner NewCrashingCrashGenerationCleaner( string message )
		 {
			  int pageSize = 8192;
			  PagedFile pagedFile = new StubPagedFileAnonymousInnerClass( this, pageSize, message );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return new CrashGenerationCleaner(pagedFile, new TreeNodeFixedSize<>(pageSize, SimpleLongLayout.longLayout().build()), 0, MAX_BATCH_SIZE * 1_000_000_000, 5, 7, NO_MONITOR);
			  return new CrashGenerationCleaner( pagedFile, new TreeNodeFixedSize<object, ?>( pageSize, SimpleLongLayout.LongLayout().build() ), 0, MAX_BATCH_SIZE * 1_000_000_000, 5, 7, NO_MONITOR );
		 }

		 private class StubPagedFileAnonymousInnerClass : StubPagedFile
		 {
			 private readonly CrashGenerationCleanerCrashTest _outerInstance;

			 private string _message;

			 public StubPagedFileAnonymousInnerClass( CrashGenerationCleanerCrashTest outerInstance, int pageSize, string message ) : base( pageSize )
			 {
				 this.outerInstance = outerInstance;
				 this._message = message;
				 first = new AtomicBoolean( true );
			 }

			 internal AtomicBoolean first;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.pagecache.PageCursor io(long pageId, int pf_flags) throws java.io.IOException
			 public override PageCursor io( long pageId, int pfFlags )
			 {
				  try
				  {
						Thread.Sleep( 1 );
				  }
				  catch ( InterruptedException e )
				  {
						throw new Exception( e );
				  }
				  if ( first.getAndSet( false ) )
				  {
						throw new IOException( _message );
				  }
				  return base.io( pageId, pfFlags );
			 }
		 }
	}

}