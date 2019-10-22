using System;

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
namespace Neo4Net.causalclustering.core.consensus.membership
{

	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;


	public class MembershipWaiterLifecycle : LifecycleAdapter
	{
		 private readonly MembershipWaiter _membershipWaiter;
		 private readonly long _joinCatchupTimeout;
		 private readonly RaftMachine _raft;
		 private readonly Log _log;

		 public MembershipWaiterLifecycle( MembershipWaiter membershipWaiter, long joinCatchupTimeout, RaftMachine raft, LogProvider logProvider )
		 {
			  this._membershipWaiter = membershipWaiter;
			  this._joinCatchupTimeout = joinCatchupTimeout;
			  this._raft = raft;
			  this._log = logProvider.getLog( this.GetType() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {
			  CompletableFuture<bool> caughtUp = _membershipWaiter.waitUntilCaughtUpMember( _raft );

			  try
			  {
					caughtUp.get( _joinCatchupTimeout, MILLISECONDS );
			  }
			  catch ( ExecutionException e )
			  {
					_log.error( "Server failed to join cluster", e.InnerException );
					throw e.InnerException;
			  }
			  catch ( Exception e ) when ( e is InterruptedException || e is TimeoutException )
			  {
					string message = format( "Server failed to join cluster within catchup time limit [%d ms]", _joinCatchupTimeout );
					_log.error( message, e );
					throw new Exception( message, e );
			  }
			  finally
			  {
					caughtUp.cancel( true );
			  }
		 }
	}

}