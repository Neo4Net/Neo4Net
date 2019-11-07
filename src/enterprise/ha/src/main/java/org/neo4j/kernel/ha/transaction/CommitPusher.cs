using System;
using System.Collections.Generic;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.ha.transaction
{

	using Neo4Net.com;
	using Slave = Neo4Net.Kernel.ha.com.master.Slave;
	using Group = Neo4Net.Scheduler.Group;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

	public class CommitPusher
	{
		 private class PullUpdateFuture : FutureTask<object>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Slave SlaveConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long TxIdConflict;

			  internal PullUpdateFuture( Slave slave, long txId ) : base( () -> null )
			  {
					this.SlaveConflict = slave;
					this.TxIdConflict = txId;
			  }

			  public override void Done()
			  {
					base.set( null );
					base.Done();
			  }

			  public override Exception Exception
			  {
				  set
				  {
						base.Exception = value;
				  }
			  }

			  public virtual Slave Slave
			  {
				  get
				  {
						return SlaveConflict;
				  }
			  }

			  internal virtual long TxId
			  {
				  get
				  {
						return TxIdConflict;
				  }
			  }
		 }

		 private const int PULL_UPDATES_QUEUE_SIZE = 100;

		 private readonly IDictionary<int, BlockingQueue<PullUpdateFuture>> _pullUpdateQueues = new Dictionary<int, BlockingQueue<PullUpdateFuture>>();
		 private readonly IJobScheduler _scheduler;

		 public CommitPusher( IJobScheduler scheduler )
		 {
			  this._scheduler = scheduler;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void queuePush(Neo4Net.kernel.ha.com.master.Slave slave, final long txId)
		 public virtual void QueuePush( Slave slave, long txId )
		 {
			  PullUpdateFuture pullRequest = new PullUpdateFuture( slave, txId );

			  BlockingQueue<PullUpdateFuture> queue = GetOrCreateQueue( slave );

			  // Add our request to the queue
			  while ( !queue.offer( pullRequest ) )
			  {
					Thread.yield();
			  }

			  try
			  {
					// Wait for request to finish
					pullRequest.get();
			  }
			  catch ( InterruptedException e )
			  {
					Thread.interrupted(); // Clear interrupt flag
					throw new Exception( e );
			  }
			  catch ( ExecutionException e )
			  {
					if ( e.InnerException is Exception )
					{
						 throw ( Exception ) e.InnerException;
					}
					else
					{
						 throw new Exception( e.InnerException );
					}
			  }
		 }

		 private BlockingQueue<PullUpdateFuture> GetOrCreateQueue( Slave slave )
		 {
			 lock ( this )
			 {
				  BlockingQueue<PullUpdateFuture> queue = _pullUpdateQueues[slave.ServerId];
				  if ( queue == null )
				  {
						// Create queue and worker
						queue = new ArrayBlockingQueue<PullUpdateFuture>( PULL_UPDATES_QUEUE_SIZE );
						_pullUpdateQueues[slave.ServerId] = queue;
      
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.BlockingQueue<PullUpdateFuture> finalQueue = queue;
						BlockingQueue<PullUpdateFuture> finalQueue = queue;
						_scheduler.schedule( Group.MASTER_TRANSACTION_PUSHING, new RunnableAnonymousInnerClass( this, finalQueue ) );
				  }
				  return queue;
			 }
		 }

		 private class RunnableAnonymousInnerClass : ThreadStart
		 {
			 private readonly CommitPusher _outerInstance;

			 private BlockingQueue<PullUpdateFuture> _finalQueue;

			 public RunnableAnonymousInnerClass( CommitPusher outerInstance, BlockingQueue<PullUpdateFuture> finalQueue )
			 {
				 this.outerInstance = outerInstance;
				 this._finalQueue = finalQueue;
				 currentPulls = new List<>();
			 }

			 internal IList<PullUpdateFuture> currentPulls;

			 public void run()
			 {
				  try
				  {
						while ( true )
						{
							 // Poll queue and call pullUpdate
							 currentPulls.clear();
							 currentPulls.add( _finalQueue.take() );

							 _finalQueue.drainTo( currentPulls );

							 try
							 {
								  PullUpdateFuture pullUpdateFuture = currentPulls.get( 0 );
								  outerInstance.askSlaveToPullUpdates( pullUpdateFuture );

								  // Notify the futures
								  foreach ( PullUpdateFuture currentPull in currentPulls )
								  {
										currentPull.Done();
								  }
							 }
							 catch ( Exception e )
							 {
								  // Notify the futures
								  foreach ( PullUpdateFuture currentPull in currentPulls )
								  {
										currentPull.Exception = e;
								  }
							 }
						}
				  }
				  catch ( InterruptedException )
				  {
						// Quit
				  }
			 }
		 }

		 private void AskSlaveToPullUpdates( PullUpdateFuture pullUpdateFuture )
		 {
			  Slave slave = pullUpdateFuture.Slave;
			  long lastTxId = pullUpdateFuture.TxId;
			  using ( Response<Void> ignored = slave.PullUpdates( lastTxId ) )
			  {
					// Slave will come back to me(master) and pull updates
			  }
		 }
	}

}