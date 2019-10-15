using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
namespace Neo4Net.Bolt.runtime
{
	using ExceptionUtils = org.apache.commons.lang3.exception.ExceptionUtils;


	using Job = Neo4Net.Bolt.v1.runtime.Job;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using Group = Neo4Net.Scheduler.Group;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.concurrent.Futures.failedFuture;

	public class ExecutorBoltScheduler : BoltScheduler, BoltConnectionLifetimeListener, BoltConnectionQueueMonitor
	{
		 private readonly string _connector;
		 private readonly ExecutorFactory _executorFactory;
		 private readonly IJobScheduler _scheduler;
		 private readonly Log _log;
		 private readonly ConcurrentDictionary<string, BoltConnection> _activeConnections = new ConcurrentDictionary<string, BoltConnection>();
		 private readonly ConcurrentDictionary<string, CompletableFuture<bool>> _activeWorkItems = new ConcurrentDictionary<string, CompletableFuture<bool>>();
		 private readonly int _corePoolSize;
		 private readonly int _maxPoolSize;
		 private readonly Duration _keepAlive;
		 private readonly int _queueSize;
		 private readonly ExecutorService _forkJoinPool;

		 private ExecutorService _threadPool;

		 public ExecutorBoltScheduler( string connector, ExecutorFactory executorFactory, IJobScheduler scheduler, LogService logService, int corePoolSize, int maxPoolSize, Duration keepAlive, int queueSize, ExecutorService forkJoinPool )
		 {
			  this._connector = connector;
			  this._executorFactory = executorFactory;
			  this._scheduler = scheduler;
			  this._log = logService.GetInternalLog( this.GetType() );
			  this._corePoolSize = corePoolSize;
			  this._maxPoolSize = maxPoolSize;
			  this._keepAlive = keepAlive;
			  this._queueSize = queueSize;
			  this._forkJoinPool = forkJoinPool;
		 }

		 internal virtual bool IsRegistered( BoltConnection connection )
		 {
			  return _activeConnections.ContainsKey( connection.Id() );
		 }

		 internal virtual bool IsActive( BoltConnection connection )
		 {
			  return _activeWorkItems.ContainsKey( connection.Id() );
		 }

		 public override string Connector()
		 {
			  return _connector;
		 }

		 public override void Start()
		 {
			  _threadPool = _executorFactory.create( _corePoolSize, _maxPoolSize, _keepAlive, _queueSize, true, new NameAppendingThreadFactory( _connector, _scheduler.threadFactory( Group.BOLT_WORKER ) ) );
		 }

		 public override void Stop()
		 {
			  if ( _threadPool != null )
			  {
					_activeConnections.Values.forEach( this.stopConnection );

					_threadPool.shutdown();
			  }
		 }

		 public override void Created( BoltConnection connection )
		 {
			  BoltConnection previous = _activeConnections[connection.Id()] = connection;
			  // We do not expect the same (keyed) connection twice
			  Debug.Assert( previous == null );
		 }

		 public override void Closed( BoltConnection connection )
		 {
			  string id = connection.Id();

			  try
			  {
					CompletableFuture<bool> currentFuture = _activeWorkItems.Remove( id );
					if ( currentFuture != null )
					{
						 currentFuture.cancel( false );
					}
			  }
			  finally
			  {
					_activeConnections.Remove( id );
			  }
		 }

		 public override void Enqueued( BoltConnection to, Job job )
		 {
			  HandleSubmission( to );
		 }

		 public override void Drained( BoltConnection from, ICollection<Job> batch )
		 {

		 }

		 private void HandleSubmission( BoltConnection connection )
		 {
			  _activeWorkItems.computeIfAbsent( connection.Id(), key => ScheduleBatchOrHandleError(connection).whenCompleteAsync((result, error) => handleCompletion(connection, result, error), _forkJoinPool) );
		 }

		 private CompletableFuture<bool> ScheduleBatchOrHandleError( BoltConnection connection )
		 {
			  try
			  {
					return CompletableFuture.supplyAsync( () => ExecuteBatch(connection), _threadPool );
			  }
			  catch ( RejectedExecutionException ex )
			  {
					return failedFuture( ex );
			  }
		 }

		 private bool ExecuteBatch( BoltConnection connection )
		 {
			  Thread currentThread = Thread.CurrentThread;
			  string originalName = currentThread.Name;
			  string newName = string.Format( "{0} [{1}] ", originalName, connection.RemoteAddress() );

			  currentThread.Name = newName;
			  try
			  {
					return connection.ProcessNextBatch();
			  }
			  finally
			  {
					currentThread.Name = originalName;
			  }
		 }

		 private void HandleCompletion( BoltConnection connection, bool? shouldContinueScheduling, Exception error )
		 {
			  try
			  {
					if ( error != null && ExceptionUtils.hasCause( error, typeof( RejectedExecutionException ) ) )
					{
						 connection.HandleSchedulingError( error );
						 return;
					}
			  }
			  finally
			  {
					// we need to ensure that the entry is removed only after any possible handleSchedulingError
					// call is completed. Otherwise, we can end up having different threads executing against
					// bolt state machine.
					_activeWorkItems.Remove( connection.Id() );
			  }

			  if ( error != null )
			  {
					_log.error( string.Format( "Unexpected error during job scheduling for session '{0}'.", connection.Id() ), error );
					StopConnection( connection );
			  }
			  else
			  {
					if ( shouldContinueScheduling && connection.HasPendingJobs() )
					{
						 HandleSubmission( connection );
					}
			  }
		 }

		 private void StopConnection( BoltConnection connection )
		 {
			  try
			  {
					connection.Stop();
			  }
			  catch ( Exception t )
			  {
					_log.warn( string.Format( "An unexpected error occurred while stopping BoltConnection [{0}]", connection.Id() ), t );
			  }
		 }

		 private class NameAppendingThreadFactory : ThreadFactory
		 {
			  internal readonly string NameToAppend;
			  internal readonly ThreadFactory Factory;

			  internal NameAppendingThreadFactory( string nameToAppend, ThreadFactory factory )
			  {
					this.NameToAppend = nameToAppend;
					this.Factory = factory;
			  }

			  public override Thread NewThread( ThreadStart r )
			  {
					Thread newThread = Factory.newThread( r );
					newThread.Name = string.Format( "{0} [{1}]", newThread.Name, NameToAppend );
					return newThread;
			  }
		 }
	}

}