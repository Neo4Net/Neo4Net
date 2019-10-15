﻿using System;
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
namespace Neo4Net.Kernel.impl.transaction.log.checkpoint
{

	using Predicates = Neo4Net.Functions.Predicates;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using UnderlyingStorageException = Neo4Net.Kernel.impl.store.UnderlyingStorageException;
	using DatabaseHealth = Neo4Net.Kernel.Internal.DatabaseHealth;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using FeatureToggles = Neo4Net.Utils.FeatureToggles;

	public class CheckPointScheduler : LifecycleAdapter
	{
		 /// <summary>
		 /// The max number of consecutive check point failures that can be tolerated before treating
		 /// check point failures more seriously, with a panic.
		 /// </summary>
		 internal static readonly int MaxConsecutiveFailuresTolerance = FeatureToggles.getInteger( typeof( CheckPointScheduler ), "failure_tolerance", 10 );

		 private readonly CheckPointer _checkPointer;
		 private readonly IOLimiter _ioLimiter;
		 private readonly IJobScheduler _scheduler;
		 private readonly long _recurringPeriodMillis;
		 private readonly DatabaseHealth _health;
		 private readonly Exception[] _failures = new Exception[MaxConsecutiveFailuresTolerance];
		 private volatile int _consecutiveFailures;
		 private readonly ThreadStart job = new RunnableAnonymousInnerClass();

		 private class RunnableAnonymousInnerClass : ThreadStart
		 {
			 public void run()
			 {
				  try
				  {
						outerInstance.checkPointing = true;
						if ( outerInstance.stopped )
						{
							 return;
						}
						outerInstance.checkPointer.checkPointIfNeeded( new SimpleTriggerInfo( "Scheduled checkpoint" ) );

						// There were previous unsuccessful attempts, but this attempt was a success
						// so let's clear those previous errors.
						if ( outerInstance.consecutiveFailures > 0 )
						{
							 Arrays.fill( outerInstance.failures, null );
							 outerInstance.consecutiveFailures = 0;
						}
				  }
				  catch ( Exception t )
				  {
						outerInstance.failures[outerInstance.consecutiveFailures++] = t;

						// We're counting check pointer to log about the failure itself
						if ( outerInstance.consecutiveFailures >= MaxConsecutiveFailuresTolerance )
						{
							 UnderlyingStorageException combinedFailure = constructCombinedFailure();
							 outerInstance.health.panic( combinedFailure );
							 throw combinedFailure;
						}
				  }
				  finally
				  {
						outerInstance.checkPointing = false;
				  }

				  // reschedule only if it is not stopped
				  if ( !outerInstance.stopped )
				  {
						outerInstance.handle = outerInstance.scheduler.schedule( Group.CHECKPOINT, job, outerInstance.recurringPeriodMillis, MILLISECONDS );
				  }
			 }

			 private UnderlyingStorageException constructCombinedFailure()
			 {
				  UnderlyingStorageException combined = new UnderlyingStorageException( "Error performing check point" );
				  for ( int i = 0; i < outerInstance.consecutiveFailures; i++ )
				  {
						combined.addSuppressed( outerInstance.failures[i] );
				  }
				  return combined;
			 }
		 }

		 private volatile JobHandle _handle;
		 private volatile bool _stopped;
		 private volatile bool _checkPointing;
		 private readonly System.Func<bool> checkPointingCondition = () =>
		 {
			return !_checkPointing;
		 };

		 public CheckPointScheduler( CheckPointer checkPointer, IOLimiter ioLimiter, IJobScheduler scheduler, long recurringPeriodMillis, DatabaseHealth health )
		 {
			  this._checkPointer = checkPointer;
			  this._ioLimiter = ioLimiter;
			  this._scheduler = scheduler;
			  this._recurringPeriodMillis = recurringPeriodMillis;
			  this._health = health;
		 }

		 public override void Start()
		 {
			  _handle = _scheduler.schedule( Group.CHECKPOINT, job, _recurringPeriodMillis, MILLISECONDS );
		 }

		 public override void Stop()
		 {
			  _stopped = true;
			  if ( _handle != null )
			  {
					_handle.cancel( false );
			  }
			  WaitOngoingCheckpointCompletion();
		 }

		 private void WaitOngoingCheckpointCompletion()
		 {
			  _ioLimiter.disableLimit();
			  try
			  {
					Predicates.awaitForever( checkPointingCondition, 100, MILLISECONDS );
			  }
			  finally
			  {
					_ioLimiter.enableLimit();
			  }
		 }
	}

}