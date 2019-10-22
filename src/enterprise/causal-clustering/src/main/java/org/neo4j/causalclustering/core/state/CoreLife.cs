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
namespace Neo4Net.causalclustering.core.state
{

	using LocalDatabase = Neo4Net.causalclustering.catchup.storecopy.LocalDatabase;
	using RaftMachine = Neo4Net.causalclustering.core.consensus.RaftMachine;
	using CoreStateMachines = Neo4Net.causalclustering.core.state.machines.CoreStateMachines;
	using CoreSnapshot = Neo4Net.causalclustering.core.state.snapshot.CoreSnapshot;
	using CoreStateDownloaderService = Neo4Net.causalclustering.core.state.snapshot.CoreStateDownloaderService;
	using BoundState = Neo4Net.causalclustering.identity.BoundState;
	using ClusterBinder = Neo4Net.causalclustering.identity.ClusterBinder;
	using Neo4Net.causalclustering.messaging;
	using SafeLifecycle = Neo4Net.Kernel.Lifecycle.SafeLifecycle;
	using JobHandle = Neo4Net.Scheduler.JobHandle;

	public class CoreLife : SafeLifecycle
	{
		 private readonly RaftMachine _raftMachine;
		 private readonly LocalDatabase _localDatabase;
		 private readonly ClusterBinder _clusterBinder;

		 private readonly CommandApplicationProcess _applicationProcess;
		 private readonly CoreStateMachines _coreStateMachines;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.Neo4Net.causalclustering.messaging.LifecycleMessageHandler<?> raftMessageHandler;
		 private readonly LifecycleMessageHandler<object> _raftMessageHandler;
		 private readonly CoreSnapshotService _snapshotService;
		 private readonly CoreStateDownloaderService _downloadService;

		 public CoreLife<T1>( RaftMachine raftMachine, LocalDatabase localDatabase, ClusterBinder clusterBinder, CommandApplicationProcess commandApplicationProcess, CoreStateMachines coreStateMachines, LifecycleMessageHandler<T1> raftMessageHandler, CoreSnapshotService snapshotService, CoreStateDownloaderService downloadService )
		 {
			  this._raftMachine = raftMachine;
			  this._localDatabase = localDatabase;
			  this._clusterBinder = clusterBinder;
			  this._applicationProcess = commandApplicationProcess;
			  this._coreStateMachines = coreStateMachines;
			  this._raftMessageHandler = raftMessageHandler;
			  this._snapshotService = snapshotService;
			  this._downloadService = downloadService;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void init0() throws Throwable
		 public override void Init0()
		 {
			  _localDatabase.init();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start0() throws Throwable
		 public override void Start0()
		 {
			  BoundState boundState = _clusterBinder.bindToCluster();
			  _raftMessageHandler.start( boundState.ClusterId() );

			  bool startedByDownloader = false;
			  if ( boundState.Snapshot().Present )
			  {
					// this means that we bootstrapped the cluster
					CoreSnapshot snapshot = boundState.Snapshot().get();
					_snapshotService.installSnapshot( snapshot );
			  }
			  else
			  {
					_snapshotService.awaitState();
					Optional<JobHandle> downloadJob = _downloadService.downloadJob();
					if ( downloadJob.Present )
					{
						 downloadJob.get().waitTermination();
						 startedByDownloader = true;
					}
			  }

			  if ( !startedByDownloader )
			  {
					_localDatabase.start();
					_coreStateMachines.installCommitProcess( _localDatabase.CommitProcess );
			  }
			  _applicationProcess.start();
			  _raftMachine.postRecoveryActions();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop0() throws Throwable
		 public override void Stop0()
		 {
			  _raftMachine.stopTimers();
			  _raftMessageHandler.stop();
			  _applicationProcess.stop();
			  _localDatabase.stop();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown0() throws Throwable
		 public override void Shutdown0()
		 {
			  _localDatabase.shutdown();
		 }
	}

}