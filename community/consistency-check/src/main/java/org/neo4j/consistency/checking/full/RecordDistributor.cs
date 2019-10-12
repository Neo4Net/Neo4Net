using System;
using System.Collections.Generic;
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
namespace Org.Neo4j.Consistency.checking.full
{

	using Org.Neo4j.Consistency.checking.full;
	using ProgressListener = Org.Neo4j.Helpers.progress.ProgressListener;
	using Org.Neo4j.@unsafe.Impl.Batchimport.cache.idmapping.@string;

	/// <summary>
	/// Takes a stream of RECORDs and distributes them, via <seealso cref="BlockingQueue"/> onto multiple workers.
	/// </summary>
	public class RecordDistributor
	{
		 private RecordDistributor()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <RECORD> void distributeRecords(int numberOfThreads, String workerNames, int queueSize, java.util.Iterator<RECORD> records, final org.neo4j.helpers.progress.ProgressListener progress, RecordProcessor<RECORD> processor, org.neo4j.consistency.checking.full.QueueDistribution_QueueDistributor<RECORD> idDistributor)
		 public static void DistributeRecords<RECORD>( int numberOfThreads, string workerNames, int queueSize, IEnumerator<RECORD> records, ProgressListener progress, RecordProcessor<RECORD> processor, QueueDistribution_QueueDistributor<RECORD> idDistributor )
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( !records.hasNext() )
			  {
					return;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") final java.util.concurrent.ArrayBlockingQueue<RECORD>[] recordQ = new java.util.concurrent.ArrayBlockingQueue[numberOfThreads];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
			  ArrayBlockingQueue<RECORD>[] recordQ = new ArrayBlockingQueue[numberOfThreads];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.unsafe.impl.batchimport.cache.idmapping.string.Workers<RecordCheckWorker<RECORD>> workers = new org.neo4j.unsafe.impl.batchimport.cache.idmapping.string.Workers<>(workerNames);
			  Workers<RecordCheckWorker<RECORD>> workers = new Workers<RecordCheckWorker<RECORD>>( workerNames );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger idGroup = new java.util.concurrent.atomic.AtomicInteger(-1);
			  AtomicInteger idGroup = new AtomicInteger( -1 );
			  for ( int threadId = 0; threadId < numberOfThreads; threadId++ )
			  {
					recordQ[threadId] = new ArrayBlockingQueue<RECORD>( queueSize );
					workers.Start( new RecordCheckWorker<RECORD>( threadId, idGroup, recordQ[threadId], processor ) );
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int[] recsProcessed = new int[numberOfThreads];
			  int[] recsProcessed = new int[numberOfThreads];
			  RecordConsumer<RECORD> recordConsumer = ( record, qIndex ) =>
			  {
				recordQ[qIndex].put( record );
				recsProcessed[qIndex]++;
			  };

			  try
			  {
					while ( records.MoveNext() )
					{
						 try
						 {
							  // Put records into the queues using the queue distributor. Each Worker will pull and process.
							  RECORD record = records.Current;
							  idDistributor.Distribute( record, recordConsumer );
							  progress.Add( 1 );
						 }
						 catch ( InterruptedException )
						 {
							  Thread.CurrentThread.Interrupt();
							  break;
						 }
					}

					// No more records to distribute, mark as done so that the workers will exit when no more records in queue.
					foreach ( RecordCheckWorker<RECORD> worker in workers )
					{
						 worker.Done();
					}

					workers.AwaitAndThrowOnError();
			  }
			  catch ( InterruptedException )
			  {
					Thread.CurrentThread.Interrupt();
					throw new Exception( "Was interrupted while awaiting completion" );
			  }
		 }

		 /// <summary>
		 /// Consumers records from a <seealso cref="QueueDistribution"/>, feeding into correct queue.
		 /// </summary>
		 internal interface RecordConsumer<RECORD>
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void accept(RECORD record, int qIndex) throws InterruptedException;
			  void Accept( RECORD record, int qIndex );
		 }

		 public static long CalculateRecordsPerCpu( long highId, int numberOfThreads )
		 {
			  bool hasRest = highId % numberOfThreads > 0;
			  long result = highId / numberOfThreads;
			  if ( hasRest )
			  {
					result++;
			  }
			  return result;
		 }
	}

}