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

	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Thread.sleep;

	/// <summary>
	/// Supervises a <seealso cref="StageExecution"/> until it is no longer <seealso cref="StageExecution.stillExecuting() executing"/>.
	/// Meanwhile it feeds information about the execution to an <seealso cref="ExecutionMonitor"/>.
	/// </summary>
	public class ExecutionSupervisor
	{
		 private readonly Clock _clock;
		 private readonly ExecutionMonitor _monitor;

		 public ExecutionSupervisor( Clock clock, ExecutionMonitor monitor )
		 {
			  this._clock = clock;
			  this._monitor = monitor;
		 }

		 public ExecutionSupervisor( ExecutionMonitor monitor ) : this( Clocks.systemClock(), monitor )
		 {
		 }

		 /// <summary>
		 /// Supervises <seealso cref="StageExecution"/>, provides continuous information to the <seealso cref="ExecutionMonitor"/>
		 /// and returns when the execution is done or an error occurs, in which case an exception is thrown.
		 /// 
		 /// Made synchronized to ensure that only one set of executions take place at any given time
		 /// and also to make sure the calling thread goes through a memory barrier (useful both before and after execution).
		 /// </summary>
		 /// <param name="execution"> <seealso cref="StageExecution"/> instances to supervise simultaneously. </param>
		 public virtual void Supervise( StageExecution execution )
		 {
			 lock ( this )
			 {
				  long startTime = CurrentTimeMillis();
				  Start( execution );
      
				  while ( execution.StillExecuting() )
				  {
						FinishAwareSleep( execution );
						_monitor.check( execution );
				  }
				  End( execution, CurrentTimeMillis() - startTime );
			 }
		 }

		 private long CurrentTimeMillis()
		 {
			  return _clock.millis();
		 }

		 protected internal virtual void End( StageExecution execution, long totalTimeMillis )
		 {
			  _monitor.end( execution, totalTimeMillis );
		 }

		 protected internal virtual void Start( StageExecution execution )
		 {
			  _monitor.start( execution );
		 }

		 private void FinishAwareSleep( StageExecution execution )
		 {
			  long endTime = _monitor.nextCheckTime();
			  while ( CurrentTimeMillis() < endTime )
			  {
					if ( !execution.StillExecuting() )
					{
						 break;
					}

					try
					{
						 sleep( min( 10, max( 0, endTime - CurrentTimeMillis() ) ) );
					}
					catch ( InterruptedException e )
					{
						 execution.Panic( e );
						 break;
					}
			  }
		 }
	}

}