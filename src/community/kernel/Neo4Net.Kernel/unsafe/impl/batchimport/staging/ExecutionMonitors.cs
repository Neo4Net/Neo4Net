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
namespace Neo4Net.@unsafe.Impl.Batchimport.staging
{

	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.staging.HumanUnderstandableExecutionMonitor.NO_MONITOR;

	/// <summary>
	/// Common <seealso cref="ExecutionMonitor"/> implementations.
	/// </summary>
	public class ExecutionMonitors
	{
		 private ExecutionMonitors()
		 {
			  throw new AssertionError( "No instances allowed" );
		 }

		 public static ExecutionMonitor DefaultVisible( IJobScheduler jobScheduler )
		 {
			  return DefaultVisible( System.in, jobScheduler );
		 }

		 public static ExecutionMonitor DefaultVisible( Stream @in, IJobScheduler jobScheduler )
		 {
			  ProgressRestoringMonitor monitor = new ProgressRestoringMonitor();
			  return new MultiExecutionMonitor( new HumanUnderstandableExecutionMonitor( NO_MONITOR, monitor ), new OnDemandDetailsExecutionMonitor( System.out, @in, monitor, jobScheduler ) );
		 }

		 private static readonly ExecutionMonitor INVISIBLE = new ExecutionMonitorAnonymousInnerClass();

		 private class ExecutionMonitorAnonymousInnerClass : ExecutionMonitor
		 {
			 public void start( StageExecution execution )
			 { // Do nothing
			 }

			 public void end( StageExecution execution, long totalTimeMillis )
			 { // Do nothing
			 }

			 public long nextCheckTime()
			 {
				  return long.MaxValue;
			 }

			 public void check( StageExecution execution )
			 { // Do nothing
			 }

			 public void done( bool successful, long totalTimeMillis, string additionalInformation )
			 { // Do nothing
			 }
		 }

		 public static ExecutionMonitor Invisible()
		 {
			  return INVISIBLE;
		 }
	}

}