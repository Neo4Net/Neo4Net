using System;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Kernel.stresstests.transaction.checkpoint.workload
{

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Resource = Neo4Net.Graphdb.Resource;
	using RandomMutation = Neo4Net.Kernel.stresstests.transaction.checkpoint.mutation.RandomMutation;

	public class Workload : Resource
	{
		 private readonly int _threads;
		 private readonly SyncMonitor _sync;
		 private readonly Worker _worker;
		 private readonly ExecutorService _executor;

		 public Workload( GraphDatabaseService db, RandomMutation randomMutation, int threads )
		 {
			  this._threads = threads;
			  this._sync = new SyncMonitor( threads );
			  this._worker = new Worker( db, randomMutation, _sync, 100 );
			  this._executor = Executors.newCachedThreadPool();
		 }

		 public interface TransactionThroughput
		 {

			  void Report( long transactions, long timeSlotMillis );
		 }

		 public static class TransactionThroughput_Fields
		 {
			 private readonly Workload _outerInstance;

			 public TransactionThroughput_Fields( Workload outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public static readonly TransactionThroughput None = ( transactions, timeSlotMillis ) =>
			  {
				// ignore
			  };

		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void run(long runningTimeMillis, TransactionThroughput throughput) throws InterruptedException
		 public virtual void Run( long runningTimeMillis, TransactionThroughput throughput )
		 {
			  for ( int i = 0; i < _threads; i++ )
			  {
					_executor.submit( _worker );
			  }

			  TimeUnit.SECONDS.sleep( 20 ); // sleep to make sure workers are started

			  long now = DateTimeHelper.CurrentUnixTimeMillis();
			  long previousReportTime = DateTimeHelper.CurrentUnixTimeMillis();
			  long finishLine = runningTimeMillis + now;
			  long sampleRate = TimeUnit.SECONDS.toMillis( 10 );
			  long lastReport = sampleRate + now;
			  long previousTransactionCount = _sync.transactions();
			  Thread.Sleep( sampleRate );
			  do
			  {
					now = DateTimeHelper.CurrentUnixTimeMillis();
					if ( lastReport <= now )
					{
						 long currentTransactionCount = _sync.transactions();
						 long diff = currentTransactionCount - previousTransactionCount;
						 throughput.Report( diff, now - previousReportTime );

						 previousReportTime = now;
						 previousTransactionCount = currentTransactionCount;

						 lastReport = sampleRate + now;
						 Thread.Sleep( sampleRate );
					}
					else
					{
						 Thread.Sleep( 10 );
					}
			  } while ( now < finishLine );

			  if ( lastReport < now )
			  {
					long diff = _sync.transactions() - previousTransactionCount;
					throughput.Report( diff, now - previousReportTime );
			  }
			  _sync.stopAndWaitWorkers();

		 }

		 public override void Close()
		 {
			  try
			  {
					_executor.shutdown();
					_executor.awaitTermination( 10, TimeUnit.SECONDS );
			  }
			  catch ( Exception e )
			  {
					throw new Exception( e );
			  }
		 }
	}

}