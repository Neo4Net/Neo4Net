using System.Diagnostics;
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
namespace Neo4Net.Consistency.checking.full
{

	/// <summary>
	/// Base class for workers that processes records during consistency check.
	/// </summary>
	public class RecordCheckWorker<RECORD> : ThreadStart
	{
		 private volatile bool _done;
		 protected internal readonly BlockingQueue<RECORD> RecordsQ;
		 private readonly int _id;
		 private readonly AtomicInteger _idQueue;
		 private readonly RecordProcessor<RECORD> _processor;

		 public RecordCheckWorker( int id, AtomicInteger idQueue, BlockingQueue<RECORD> recordsQ, RecordProcessor<RECORD> processor )
		 {
			  this._id = id;
			  this._idQueue = idQueue;
			  this.RecordsQ = recordsQ;
			  this._processor = processor;
		 }

		 public virtual void Done()
		 {
			  _done = true;
		 }

		 public override void Run()
		 {
			  // We assign threads to ids, first come first serve and the the thread assignment happens
			  // inside the record processing which accesses CacheAccess#client() and that happens
			  // lazily. So... we need to coordinate so that the processing threads initializes the processing
			  // in order of thread id. This may change later so that the thread ids are assigned
			  // explicitly on creating the threads... which should be much better, although hard with
			  // the current design due to the state living inside ThreadLocal which makes it depend
			  // on the actual and correct thread making the call... which is what we do here.
			  AwaitMyTurnToInitialize();

			  // This was the first record, the first record processing has now happened and so we
			  // can notify the others that we have initialized this thread id and the next one
			  // can go ahead and do so.
			  _processor.init( _id );
			  TellNextThreadToInitialize();

			  while ( !_done || !RecordsQ.Empty )
			  {
					RECORD record;
					try
					{
						 record = RecordsQ.poll( 10, TimeUnit.MILLISECONDS );
						 if ( record != default( RECORD ) )
						 {
							  _processor.process( record );
						 }
					}
					catch ( InterruptedException )
					{
						 Thread.interrupted();
						 break;
					}
			  }
		 }

		 private void AwaitMyTurnToInitialize()
		 {
			  while ( _idQueue.get() < _id - 1 )
			  {
					try
					{
						 Thread.Sleep( 10 );
					}
					catch ( InterruptedException )
					{
						 Thread.interrupted();
						 break;
					}
			  }
		 }

		 private void TellNextThreadToInitialize()
		 {
			  bool set = _idQueue.compareAndSet( _id - 1, _id );
			  Debug.Assert( set, "Something wrong with the design here" );
		 }
	}

}