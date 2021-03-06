﻿using System;

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

	using CatchupAddressProvider = Org.Neo4j.causalclustering.catchup.CatchupAddressProvider;
	using LocalDatabase = Org.Neo4j.causalclustering.catchup.storecopy.LocalDatabase;
	using RaftMachine = Org.Neo4j.causalclustering.core.consensus.RaftMachine;
	using RaftMessages = Org.Neo4j.causalclustering.core.consensus.RaftMessages;
	using ConsensusOutcome = Org.Neo4j.causalclustering.core.consensus.outcome.ConsensusOutcome;
	using CoreStateDownloaderService = Org.Neo4j.causalclustering.core.state.snapshot.CoreStateDownloaderService;
	using ClusterId = Org.Neo4j.causalclustering.identity.ClusterId;
	using Org.Neo4j.causalclustering.messaging;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using JobHandle = Org.Neo4j.Scheduler.JobHandle;

	public class RaftMessageApplier : LifecycleMessageHandler<Org.Neo4j.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<JavaToDotNetGenericWildcard>>
	{
		 private readonly LocalDatabase _localDatabase;
		 private readonly Log _log;
		 private readonly RaftMachine _raftMachine;
		 private readonly CoreStateDownloaderService _downloadService;
		 private readonly CommandApplicationProcess _applicationProcess;
		 private Org.Neo4j.causalclustering.catchup.CatchupAddressProvider_PrioritisingUpstreamStrategyBasedAddressProvider _catchupAddressProvider;

		 public RaftMessageApplier( LocalDatabase localDatabase, LogProvider logProvider, RaftMachine raftMachine, CoreStateDownloaderService downloadService, CommandApplicationProcess applicationProcess, Org.Neo4j.causalclustering.catchup.CatchupAddressProvider_PrioritisingUpstreamStrategyBasedAddressProvider catchupAddressProvider )
		 {
			  this._localDatabase = localDatabase;
			  this._log = logProvider.getLog( this.GetType() );
			  this._raftMachine = raftMachine;
			  this._downloadService = downloadService;
			  this._applicationProcess = applicationProcess;
			  this._catchupAddressProvider = catchupAddressProvider;
		 }

		 public override void Handle<T1>( Org.Neo4j.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<T1> wrappedMessage )
		 {
			 lock ( this )
			 {
				  try
				  {
						ConsensusOutcome outcome = _raftMachine.handle( wrappedMessage.message() );
						if ( outcome.NeedsFreshSnapshot() )
						{
							 Optional<JobHandle> downloadJob = _downloadService.scheduleDownload( _catchupAddressProvider );
							 if ( downloadJob.Present )
							 {
								  downloadJob.get().waitTermination();
							 }
						}
						else
						{
							 NotifyCommitted( outcome.CommitIndex );
						}
				  }
				  catch ( Exception e )
				  {
						_log.error( "Error handling message", e );
						_raftMachine.panic();
						_localDatabase.panic( e );
				  }
			 }
		 }

		 public override void Start( ClusterId clusterId )
		 {
			 lock ( this )
			 {
				  // no-op
			 }
		 }

		 public override void Stop()
		 {
			 lock ( this )
			 {
				  // no-op
			 }
		 }

		 private void NotifyCommitted( long commitIndex )
		 {
			  _applicationProcess.notifyCommitted( commitIndex );
		 }
	}

}