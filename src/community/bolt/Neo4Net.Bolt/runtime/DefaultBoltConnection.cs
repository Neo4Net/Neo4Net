using System;
using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Bolt.runtime
{
	using Channel = io.netty.channel.Channel;
	using ExceptionUtils = org.apache.commons.lang3.exception.ExceptionUtils;


	using PackOutput = Neo4Net.Bolt.v1.packstream.PackOutput;
	using Job = Neo4Net.Bolt.v1.runtime.Job;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using FeatureToggles = Neo4Net.Utils.FeatureToggles;

	public class DefaultBoltConnection : BoltConnection
	{
		 protected internal static readonly int DefaultMaxBatchSize = FeatureToggles.getInteger( typeof( BoltServer ), "max_batch_size", 100 );

		 private readonly string _id;

		 private readonly BoltChannel _channel;
		 private readonly BoltStateMachine _machine;
		 private readonly BoltConnectionLifetimeListener _listener;
		 private readonly BoltConnectionQueueMonitor _queueMonitor;
		 private readonly PackOutput _output;

		 private readonly Log _log;
		 private readonly Log _userLog;

		 private readonly int _maxBatchSize;
		 private readonly IList<Job> _batch;
		 private readonly LinkedBlockingQueue<Job> _queue = new LinkedBlockingQueue<Job>();

		 private readonly AtomicBoolean _shouldClose = new AtomicBoolean();
		 private readonly AtomicBoolean _closed = new AtomicBoolean();

		 public DefaultBoltConnection( BoltChannel channel, PackOutput output, BoltStateMachine machine, LogService logService, BoltConnectionLifetimeListener listener, BoltConnectionQueueMonitor queueMonitor ) : this( channel, output, machine, logService, listener, queueMonitor, DefaultMaxBatchSize )
		 {
		 }

		 public DefaultBoltConnection( BoltChannel channel, PackOutput output, BoltStateMachine machine, LogService logService, BoltConnectionLifetimeListener listener, BoltConnectionQueueMonitor queueMonitor, int maxBatchSize )
		 {
			  this._id = channel.Id();
			  this._channel = channel;
			  this._output = output;
			  this._machine = machine;
			  this._listener = listener;
			  this._queueMonitor = queueMonitor;
			  this._log = logService.GetInternalLog( this.GetType() );
			  this._userLog = logService.GetUserLog( this.GetType() );
			  this._maxBatchSize = maxBatchSize;
			  this._batch = new List<Job>( maxBatchSize );
		 }

		 public override string Id()
		 {
			  return _id;
		 }

		 public override SocketAddress LocalAddress()
		 {
			  return _channel.serverAddress();
		 }

		 public override SocketAddress RemoteAddress()
		 {
			  return _channel.clientAddress();
		 }

		 public override Channel Channel()
		 {
			  return _channel.rawChannel();
		 }

		 public override PackOutput Output()
		 {
			  return _output;
		 }

		 public override bool HasPendingJobs()
		 {
			  return !_queue.Empty;
		 }

		 public override void Start()
		 {
			  NotifyCreated();
		 }

		 public override void Enqueue( Job job )
		 {
			  EnqueueInternal( job );
		 }

		 public override bool ProcessNextBatch()
		 {
			  return ProcessNextBatch( _maxBatchSize, false );
		 }

		 protected internal virtual bool ProcessNextBatch( int batchCount, bool exitIfNoJobsAvailable )
		 {
			  try
			  {
					bool waitForMessage = false;
					bool loop = false;
					do
					{
						 // exit loop if we'll close the connection
						 if ( WillClose() )
						 {
							  break;
						 }

						 // do we have pending jobs or shall we wait for new jobs to
						 // arrive, which is required only for releasing stickiness
						 // condition to this thread
						 if ( waitForMessage || !_queue.Empty )
						 {
							  _queue.drainTo( _batch, batchCount );
							  // if we expect one message but did not get any (because it was already
							  // processed), silently exit
							  if ( _batch.Count == 0 && !exitIfNoJobsAvailable )
							  {
									// loop until we get a new job, if we cannot then validate
									// transaction to check for termination condition. We'll
									// break loop if we'll close the connection
									while ( !WillClose() )
									{
										 Job nextJob = _queue.poll( 10, SECONDS );
										 if ( nextJob != null )
										 {
											  _batch.Add( nextJob );

											  break;
										 }
										 else
										 {
											  _machine.validateTransaction();
										 }
									}
							  }
							  NotifyDrained( _batch );

							  // execute each job that's in the batch
							  while ( _batch.Count > 0 )
							  {
									Job current = _batch.RemoveAt( 0 );

									current.Perform( _machine );
							  }

							  // do we have any condition that require this connection to
							  // stick to the current thread (i.e. is there an open statement
							  // or an open transaction)?
							  loop = _machine.shouldStickOnThread();
							  waitForMessage = loop;
						 }

						 // we processed all pending messages, let's flush underlying channel
						 if ( _queue.size() == 0 )
						 {
							  _output.flush();
						 }
					} while ( loop );

					// assert only if we'll stay alive
					if ( !WillClose() )
					{
						 Debug.Assert( !_machine.hasOpenStatement() );
					}
			  }
			  catch ( BoltConnectionAuthFatality ex )
			  {
					_shouldClose.set( true );
					if ( ex.Loggable )
					{
						 _userLog.warn( ex.Message );
					}
			  }
			  catch ( BoltProtocolBreachFatality ex )
			  {
					_shouldClose.set( true );
					_log.error( string.Format( "Protocol breach detected in bolt session '{0}'.", Id() ), ex );
			  }
			  catch ( InterruptedException )
			  {
					_shouldClose.set( true );
					_log.info( "Bolt session '%s' is interrupted probably due to server shutdown.", Id() );
			  }
			  catch ( Exception t )
			  {
					_shouldClose.set( true );
					_userLog.error( string.Format( "Unexpected error detected in bolt session '{0}'.", Id() ), t );
			  }
			  finally
			  {
					if ( WillClose() )
					{
						 Close();
					}
			  }

			  return !_closed.get();
		 }

		 public override void HandleSchedulingError( Exception t )
		 {
			  // if the connection is closing, don't output any logs
			  if ( !WillClose() )
			  {
					string message;
					Neo4NetError error;
					if ( ExceptionUtils.hasCause( t, typeof( RejectedExecutionException ) ) )
					{
						 error = Neo4NetError.From( Neo4Net.Kernel.Api.Exceptions.Status_Request.NoThreadsAvailable, Neo4Net.Kernel.Api.Exceptions.Status_Request.NoThreadsAvailable.code().description() );
						 message = string.Format( "Unable to schedule bolt session '{0}' for execution since there are no available threads to " + "serve it at the moment. You can retry at a later time or consider increasing max thread pool size for bolt connector(s).", Id() );
					}
					else
					{
						 error = Neo4NetError.FatalFrom( t );
						 message = string.Format( "Unexpected error during scheduling of bolt session '{0}'.", Id() );
					}

					_log.error( message, t );
					_userLog.error( message );
					_machine.markFailed( error );
			  }

			  // this will ensure that the scheduled job will be executed on this thread (fork-join pool)
			  // and it will either send a failure response to the client or close the connection and its
			  // related resources (if closing)
			  ProcessNextBatch( 1, true );
			  // we close the connection directly to enforce the client to stop waiting for
			  // any more messages responses besides the failure message.
			  Close();
		 }

		 public override void Interrupt()
		 {
			  _machine.interrupt();
		 }

		 public override void Stop()
		 {
			  if ( _shouldClose.compareAndSet( false, true ) )
			  {
					_machine.markForTermination();

					// Enqueue an empty job for close to be handled linearly
					// This is for already executing connections
					EnqueueInternal(ignore =>
					{

					});
			  }
		 }

		 private bool WillClose()
		 {
			  return _shouldClose.get();
		 }

		 private void Close()
		 {
			  if ( _closed.compareAndSet( false, true ) )
			  {
					try
					{
						 _output.Dispose();
					}
					catch ( Exception t )
					{
						 _log.error( string.Format( "Unable to close pack output of bolt session '{0}'.", Id() ), t );
					}

					try
					{
						 _machine.close();
					}
					catch ( Exception t )
					{
						 _log.error( string.Format( "Unable to close state machine of bolt session '{0}'.", Id() ), t );
					}
					finally
					{
						 NotifyDestroyed();
					}
			  }
		 }

		 private void EnqueueInternal( Job job )
		 {
			  _queue.offer( job );
			  NotifyEnqueued( job );
		 }

		 private void NotifyCreated()
		 {
			  if ( _listener != null )
			  {
					_listener.created( this );
			  }
		 }

		 private void NotifyDestroyed()
		 {
			  if ( _listener != null )
			  {
					_listener.closed( this );
			  }
		 }

		 private void NotifyEnqueued( Job job )
		 {
			  if ( _queueMonitor != null )
			  {
					_queueMonitor.enqueued( this, job );
			  }
		 }

		 private void NotifyDrained( IList<Job> jobs )
		 {
			  if ( _queueMonitor != null && jobs.Count > 0 )
			  {
					_queueMonitor.drained( this, jobs );
			  }
		 }
	}

}