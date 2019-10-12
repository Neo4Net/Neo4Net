using System;
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
namespace Org.Apache.Lucene.Index
{
	using RandomUtils = org.apache.commons.lang3.RandomUtils;
	using Codec = org.apache.lucene.codecs.Codec;
	using Directory = org.apache.lucene.store.Directory;
	using Version = org.apache.lucene.util.Version;
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using Mockito = org.mockito.Mockito;


	using MapUtil = Org.Neo4j.Helpers.Collection.MapUtil;
	using ThreadTestUtils = Org.Neo4j.Test.ThreadTestUtils;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTimeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	internal class PooledConcurrentMergeSchedulerTest
	{

		 private TestPooledConcurrentMergeScheduler _mergeScheduler;
		 private IndexWriter _indexWriter = mock( typeof( IndexWriter ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  _mergeScheduler = new TestPooledConcurrentMergeScheduler();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void tearDown()
		 internal virtual void TearDown()
		 {
			  _mergeScheduler.ExecutionLatch.Signal();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void doNotAddMergeTaskWhenWriterDoesNotHaveMergesToDo() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void DoNotAddMergeTaskWhenWriterDoesNotHaveMergesToDo()
		 {
			  IndexWriter indexWriter = mock( typeof( IndexWriter ) );

			  _mergeScheduler.merge( indexWriter, MergeTrigger.EXPLICIT, false );

			  assertEquals( 0, _mergeScheduler.WriterTaskCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addMergeTaskWhenWriterHasOneMergeToPerform() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void AddMergeTaskWhenWriterHasOneMergeToPerform()
		 {
			  SegmentCommitInfo segmentCommitInfo = SegmentCommitInfo;

			  Mockito.when( _indexWriter.NextMerge ).thenReturn( new TestOneMerge( segmentCommitInfo ) ).thenReturn( null );

			  _mergeScheduler.merge( _indexWriter, MergeTrigger.EXPLICIT, false );

			  assertEquals( 1, _mergeScheduler.WriterTaskCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addTwoMergeTasksWhenWriterHastwoMergeToPerform() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void AddTwoMergeTasksWhenWriterHastwoMergeToPerform()
		 {
			  SegmentCommitInfo segmentCommitInfo = SegmentCommitInfo;

			  Mockito.when( _indexWriter.NextMerge ).thenReturn( new TestOneMerge( segmentCommitInfo ) ).thenReturn( new TestOneMerge( segmentCommitInfo ) ).thenReturn( null );

			  _mergeScheduler.merge( _indexWriter, MergeTrigger.EXPLICIT, false );

			  assertEquals( 2, _mergeScheduler.WriterTaskCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writerCloseWaitForMergesInMergeQueue()
		 internal virtual void WriterCloseWaitForMergesInMergeQueue()
		 {
			  assertTimeout(Duration.ofSeconds(10), () =>
			  {
				_indexWriter = mock( typeof( IndexWriter ) );
				SegmentCommitInfo segmentCommitInfo = SegmentCommitInfo;

				Mockito.when( _indexWriter.NextMerge ).thenReturn( new TestOneMerge( segmentCommitInfo ) ).thenReturn( null );

				_mergeScheduler.merge( _indexWriter, MergeTrigger.EXPLICIT, false );

				assertEquals( 1, _mergeScheduler.WriterTaskCount );

				Thread closeSchedulerThread = ThreadTestUtils.fork( () => _mergeScheduler.close() );
				ThreadTestUtils.awaitThreadState( closeSchedulerThread, TimeUnit.SECONDS.toMillis( 5 ), Thread.State.TIMED_WAITING );
				_mergeScheduler.ExecutionLatch.Signal();
				closeSchedulerThread.Join();

				assertEquals( 0, _mergeScheduler.WriterTaskCount );
			  });
		 }

		 private static SegmentCommitInfo SegmentCommitInfo
		 {
			 get
			 {
				  SegmentInfo segmentInfo = new SegmentInfo( mock( typeof( Directory ) ), Version.LATEST, "test", int.MaxValue, true, mock( typeof( Codec ) ), MapUtil.stringMap(), RandomUtils.NextBytes(16), MapUtil.stringMap() );
				  return new SegmentCommitInfo( segmentInfo, 1, 1L, 1L, 1L );
			 }
		 }

		 private class TestPooledConcurrentMergeScheduler : PooledConcurrentMergeScheduler
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal System.Threading.CountdownEvent ExecutionLatchConflict = new System.Threading.CountdownEvent( 1 );

			  protected internal override MergeThread GetMergeThread( IndexWriter writer, MergePolicy.OneMerge merge )
			  {
				  lock ( this )
				  {
						return new BlockingMerge( this, writer, merge, ExecutionLatchConflict );
				  }
			  }

			  internal virtual System.Threading.CountdownEvent ExecutionLatch
			  {
				  get
				  {
						return ExecutionLatchConflict;
				  }
			  }

			  internal class BlockingMerge : ConcurrentMergeScheduler.MergeThread
			  {
				  private readonly PooledConcurrentMergeSchedulerTest.TestPooledConcurrentMergeScheduler _outerInstance;


					internal System.Threading.CountdownEvent ExecutionLatch;

					internal BlockingMerge( PooledConcurrentMergeSchedulerTest.TestPooledConcurrentMergeScheduler outerInstance, IndexWriter writer, MergePolicy.OneMerge merge, System.Threading.CountdownEvent executionLatch ) : base( writer, merge )
					{
						this._outerInstance = outerInstance;
						 this.ExecutionLatch = executionLatch;
					}

					public override void Run()
					{
						 try
						 {
							  ExecutionLatch.await();
						 }
						 catch ( InterruptedException e )
						 {
							  throw new Exception( "Interrupted while waiting for a latch", e );
						 }
					}
			  }
		 }

		 private class TestOneMerge : MergePolicy.OneMerge
		 {
			  internal TestOneMerge( SegmentCommitInfo segmentCommitInfo ) : base( Collections.singletonList( segmentCommitInfo ) )
			  {
			  }
		 }
	}

}