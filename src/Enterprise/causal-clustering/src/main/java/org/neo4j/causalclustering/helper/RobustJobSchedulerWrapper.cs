using System;

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
namespace Neo4Net.causalclustering.helper
{
	using Neo4Net.Function;
	using Log = Neo4Net.Logging.Log;
	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;

	/// <summary>
	/// A robust job catches and logs any exceptions, but keeps running if the job
	/// is a recurring one. Any exceptions also end up in the supplied log instead
	/// of falling through to syserr. Remaining Throwables (generally errors) are
	/// logged but fall through, so that recurring jobs are stopped and the error
	/// gets double visibility.
	/// </summary>
	public class RobustJobSchedulerWrapper
	{
		 private readonly JobScheduler @delegate;
		 private readonly Log _log;

		 public RobustJobSchedulerWrapper( JobScheduler @delegate, Log log )
		 {
			  this.@delegate = @delegate;
			  this._log = log;
		 }

		 public virtual JobHandle Schedule( Group group, long delayMillis, ThrowingAction<Exception> action )
		 {
			  return @delegate.Schedule( group, () => withErrorHandling(action), delayMillis, MILLISECONDS );
		 }

		 public virtual JobHandle ScheduleRecurring( Group group, long periodMillis, ThrowingAction<Exception> action )
		 {
			  return @delegate.ScheduleRecurring( group, () => withErrorHandling(action), periodMillis, MILLISECONDS );
		 }

		 /// <summary>
		 /// Last line of defense error handling.
		 /// </summary>
		 private void WithErrorHandling( ThrowingAction<Exception> action )
		 {
			  try
			  {
					action.Apply();
			  }
			  catch ( Exception e )
			  {
					_log.warn( "Uncaught exception", e );
			  }
			  catch ( Exception t )
			  {
					_log.error( "Uncaught error rethrown", t );
					throw t;
			  }
		 }
	}

}