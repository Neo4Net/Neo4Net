using System;
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
namespace Neo4Net.causalclustering.core.state.snapshot
{

	using CatchupAddressProvider = Neo4Net.causalclustering.catchup.CatchupAddressProvider;
	using DatabaseShutdownException = Neo4Net.causalclustering.catchup.storecopy.DatabaseShutdownException;
	using TimeoutStrategy = Neo4Net.causalclustering.helper.TimeoutStrategy;
	using DatabaseHealth = Neo4Net.Kernel.Internal.DatabaseHealth;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Log = Neo4Net.Logging.Log;

	public class PersistentSnapshotDownloader : ThreadStart
	{
		 public interface Monitor
		 {
			  void StartedDownloadingSnapshot();

			  void DownloadSnapshotComplete();
		 }

		 internal const string OPERATION_NAME = "download of snapshot";

		 private readonly CommandApplicationProcess _applicationProcess;
		 private readonly CatchupAddressProvider _addressProvider;
		 private readonly CoreStateDownloader _downloader;
		 private readonly Log _log;
		 private readonly Neo4Net.causalclustering.helper.TimeoutStrategy_Timeout _timeout;
		 private readonly System.Func<DatabaseHealth> _dbHealth;
		 private readonly Monitor _monitor;
		 private volatile State _state;
		 private volatile bool _keepRunning;

		 internal PersistentSnapshotDownloader( CatchupAddressProvider addressProvider, CommandApplicationProcess applicationProcess, CoreStateDownloader downloader, Log log, Neo4Net.causalclustering.helper.TimeoutStrategy_Timeout pauseStrategy, System.Func<DatabaseHealth> dbHealth, Monitors monitors )
		 {
			  this._applicationProcess = applicationProcess;
			  this._addressProvider = addressProvider;
			  this._downloader = downloader;
			  this._log = log;
			  this._timeout = pauseStrategy;
			  this._dbHealth = dbHealth;
			  this._monitor = monitors.NewMonitor( typeof( Monitor ) );
			  this._state = State.Initiated;
			  this._keepRunning = true;
		 }

		 private enum State
		 {
			  Initiated,
			  Running,
			  Completed
		 }

		 public override void Run()
		 {
			  if ( !MoveToRunningState() )
			  {
					return;
			  }

			  try
			  {
					_monitor.startedDownloadingSnapshot();
					_applicationProcess.pauseApplier( OPERATION_NAME );
					while ( _keepRunning && !_downloader.downloadSnapshot( _addressProvider ) )
					{
						 Thread.Sleep( _timeout.Millis );
						 _timeout.increment();
					}
			  }
			  catch ( InterruptedException )
			  {
					Thread.CurrentThread.Interrupt();
					_log.warn( "Persistent snapshot downloader was interrupted" );
			  }
			  catch ( DatabaseShutdownException e )
			  {
					_log.warn( "Store copy aborted due to shut down", e );
			  }
			  catch ( Exception e )
			  {
					_log.error( "Unrecoverable error during store copy", e );
					_dbHealth.get().panic(e);
			  }
			  finally
			  {
					_applicationProcess.resumeApplier( OPERATION_NAME );
					_monitor.downloadSnapshotComplete();
					_state = State.Completed;
			  }
		 }

		 private bool MoveToRunningState()
		 {
			 lock ( this )
			 {
				  if ( _state != State.Initiated )
				  {
						return false;
				  }
				  else
				  {
						_state = State.Running;
						return true;
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void stop() throws InterruptedException
		 internal virtual void Stop()
		 {
			  this._keepRunning = false;

			  while ( !HasCompleted() )
			  {
					Thread.Sleep( 100 );
			  }
		 }

		 internal virtual bool HasCompleted()
		 {
			  return _state == State.Completed;
		 }
	}

}