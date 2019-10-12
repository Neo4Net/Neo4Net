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
namespace Neo4Net.causalclustering.catchup
{

	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using Group = Neo4Net.Scheduler.Group;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using LockingExecutor = Neo4Net.Scheduler.LockingExecutor;

	public class CheckPointerService
	{
		 private readonly System.Func<CheckPointer> _checkPointerSupplier;
		 private readonly Executor _lockingCheckpointExecutor;

		 public CheckPointerService( System.Func<CheckPointer> checkPointerSupplier, JobScheduler jobScheduler, Group group )
		 {
			  this._checkPointerSupplier = checkPointerSupplier;
			  this._lockingCheckpointExecutor = new LockingExecutor( jobScheduler, group );
		 }

		 public virtual CheckPointer CheckPointer
		 {
			 get
			 {
				  return _checkPointerSupplier.get();
			 }
		 }

		 public virtual long LastCheckPointedTransactionId()
		 {
			  return _checkPointerSupplier.get().lastCheckPointedTransactionId();
		 }

		 public virtual void TryAsyncCheckpoint( System.Action<IOException> exceptionHandler )
		 {
			  _lockingCheckpointExecutor.execute(() =>
			  {
				try
				{
					 CheckPointer.tryCheckPoint( new SimpleTriggerInfo( "Store file copy" ) );
				}
				catch ( IOException e )
				{
					 exceptionHandler( e );
				}
			  });
		 }
	}

}