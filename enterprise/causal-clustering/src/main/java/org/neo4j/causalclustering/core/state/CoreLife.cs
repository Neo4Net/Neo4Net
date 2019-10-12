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
namespace Org.Neo4j.causalclustering.core.state
{

	using LocalDatabase = Org.Neo4j.causalclustering.catchup.storecopy.LocalDatabase;
	using RaftMachine = Org.Neo4j.causalclustering.core.consensus.RaftMachine;
	using CoreStateMachines = Org.Neo4j.causalclustering.core.state.machines.CoreStateMachines;
	using CoreSnapshot = Org.Neo4j.causalclustering.core.state.snapshot.CoreSnapshot;
	using CoreStateDownloaderService = Org.Neo4j.causalclustering.core.state.snapshot.CoreStateDownloaderService;
	using BoundState = Org.Neo4j.causalclustering.identity.BoundState;
	using ClusterBinder = Org.Neo4j.causalclustering.identity.ClusterBinder;
	using Org.Neo4j.causalclustering.messaging;
	using SafeLifecycle = Org.Neo4j.Kernel.Lifecycle.SafeLifecycle;
	using JobHandle = Org.Neo4j.Scheduler.JobHandle;

	public class CoreLife : SafeLifecycle
	{
		 private readonly RaftMachine _raftMachine;
		 private readonly LocalDatabase _localDatabase;
		 private readonly ClusterBinder _clusterBinder;

		 private readonly CommandApplicationProcess _applicationProcess;
		 private readonly CoreStateMachines _coreStateMachines;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.neo4j.causalclustering.messaging.LifecycleMessageHandler<?> raftMessageHandler;
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