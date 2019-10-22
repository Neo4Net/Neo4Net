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
namespace Neo4Net.causalclustering.readreplica
{

	using CatchupPollingProcess = Neo4Net.causalclustering.catchup.tx.CatchupPollingProcess;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	internal class WaitForUpToDateStore : LifecycleAdapter
	{
		 private readonly CatchupPollingProcess _catchupProcess;
		 private readonly Log _log;

		 internal WaitForUpToDateStore( CatchupPollingProcess catchupProcess, LogProvider logProvider )
		 {
			  this._catchupProcess = catchupProcess;
			  this._log = logProvider.getLog( this.GetType() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {
			  WaitForUpToDateStoreConflict();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void waitForUpToDateStore() throws InterruptedException, java.util.concurrent.ExecutionException
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 private void WaitForUpToDateStoreConflict()
		 {
			  bool upToDate = false;
			  do
			  {
					try
					{
						 upToDate = _catchupProcess.upToDateFuture().get(1, MINUTES);
					}
					catch ( TimeoutException )
					{
						 _log.warn( "Waiting for up-to-date store. State: " + _catchupProcess.describeState() );
					}
			  } while ( !upToDate );
		 }
	}

}