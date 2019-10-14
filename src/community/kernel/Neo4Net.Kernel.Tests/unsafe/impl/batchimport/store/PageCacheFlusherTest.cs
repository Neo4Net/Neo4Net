using System;

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
namespace Neo4Net.@unsafe.Impl.Batchimport.store
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Barrier = Neo4Net.Test.Barrier;
	using Neo4Net.Test.rule.concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class PageCacheFlusherTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.concurrent.OtherThreadRule<Void> t2 = new org.neo4j.test.rule.concurrent.OtherThreadRule<>();
		 public readonly OtherThreadRule<Void> T2 = new OtherThreadRule<Void>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 10_000) public void shouldWaitForCompletionInHalt() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWaitForCompletionInHalt()
		 {
			  // GIVEN
			  PageCache pageCache = mock( typeof( PageCache ) );
			  Neo4Net.Test.Barrier_Control barrier = new Neo4Net.Test.Barrier_Control();
			  doAnswer(invocation =>
			  {
				barrier.Reached();
				return null;
			  }).when( pageCache ).flushAndForce();
			  PageCacheFlusher flusher = new PageCacheFlusher( pageCache );
			  flusher.Start();

			  // WHEN
			  barrier.Await();
			  Future<object> halt = T2.execute(state =>
			  {
				flusher.Halt();
				return null;
			  });
			  T2.get().waitUntilWaiting(details => details.isAt(typeof(PageCacheFlusher), "halt"));
			  barrier.Release();

			  // THEN halt call exits normally after (confirmed) ongoing flushAndForce call completed.
			  halt.get();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExitOnErrorInHalt() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExitOnErrorInHalt()
		 {
			  // GIVEN
			  PageCache pageCache = mock( typeof( PageCache ) );
			  Exception failure = new Exception();
			  doAnswer(invocation =>
			  {
				throw failure;
			  }).when( pageCache ).flushAndForce();
			  PageCacheFlusher flusher = new PageCacheFlusher( pageCache );
			  flusher.Run();

			  // WHEN
			  try
			  {
					flusher.Halt();
					fail();
			  }
			  catch ( Exception e )
			  {
					// THEN
					assertSame( failure, e );
			  }
		 }
	}

}