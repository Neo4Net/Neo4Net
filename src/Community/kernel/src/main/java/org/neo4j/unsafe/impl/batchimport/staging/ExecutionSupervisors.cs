/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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

	/// <summary>
	/// Convenience around executing and supervising <seealso cref="Stage stages"/>.
	/// </summary>
	public class ExecutionSupervisors
	{
		 private ExecutionSupervisors()
		 {
		 }

		 /// <summary>
		 /// Using an <seealso cref="ExecutionMonitors.invisible() invisible"/> monitor. </summary>
		 /// <param name="stage"> <seealso cref="Stage"/> to supervise. </param>
		 /// <seealso cref= #superviseDynamicExecution(ExecutionMonitor, Stage) </seealso>
		 public static void SuperviseDynamicExecution( Stage stage )
		 {
			  SuperviseDynamicExecution( ExecutionMonitors.Invisible(), stage );
		 }

		 /// <summary>
		 /// With <seealso cref="Configuration.DEFAULT"/>. </summary>
		 /// <param name="monitor"> <seealso cref="ExecutionMonitor"/> notifying user about progress. </param>
		 /// <param name="stage"> <seealso cref="Stage"/> to supervise. </param>
		 /// <seealso cref= #superviseDynamicExecution(ExecutionMonitor, Configuration, Stage) </seealso>
		 public static void SuperviseDynamicExecution( ExecutionMonitor monitor, Stage stage )
		 {
			  SuperviseDynamicExecution( monitor, Configuration.DEFAULT, stage );
		 }

		 /// <summary>
		 /// Supervises an execution with the given monitor AND a <seealso cref="DynamicProcessorAssigner"/> to give
		 /// the execution a dynamic and optimal nature. </summary>
		 /// <param name="monitor"> <seealso cref="ExecutionMonitor"/> notifying user about progress. </param>
		 /// <param name="config"> <seealso cref="Configuration"/> of the import. </param>
		 /// <param name="stage"> <seealso cref="Stage"/> to supervise.
		 /// </param>
		 /// <seealso cref= #superviseExecution(ExecutionMonitor, Stage) </seealso>
		 public static void SuperviseDynamicExecution( ExecutionMonitor monitor, Configuration config, Stage stage )
		 {
			  SuperviseExecution( WithDynamicProcessorAssignment( monitor, config ), stage );
		 }

		 /// <summary>
		 /// Executes a number of stages simultaneously, letting the given {@code monitor} get insight into the
		 /// execution.
		 /// </summary>
		 /// <param name="monitor"> <seealso cref="ExecutionMonitor"/> to get insight into the execution. </param>
		 /// <param name="stage"> <seealso cref="Stage stages"/> to execute. </param>
		 public static void SuperviseExecution( ExecutionMonitor monitor, Stage stage )
		 {
			  ExecutionSupervisor supervisor = new ExecutionSupervisor( Clocks.systemClock(), monitor );
			  StageExecution execution = null;
			  try
			  {
					execution = stage.Execute();
					supervisor.Supervise( execution );
			  }
			  finally
			  {
					stage.Close();
					if ( execution != null )
					{
						 execution.AssertHealthy();
					}
			  }
		 }

		 /// <summary>
		 /// Decorates an <seealso cref="ExecutionMonitor"/> with a <seealso cref="DynamicProcessorAssigner"/> responsible for
		 /// constantly assigning and reevaluating an optimal number of processors to all individual steps.
		 /// </summary>
		 /// <param name="monitor"> <seealso cref="ExecutionMonitor"/> to decorate. </param>
		 /// <param name="config"> <seealso cref="Configuration"/> that the <seealso cref="DynamicProcessorAssigner"/> will use. Max total processors
		 /// in a <seealso cref="Stage"/> will be the smallest of that value and <seealso cref="Runtime.availableProcessors()"/>. </param>
		 /// <returns> the decorated monitor with dynamic processor assignment capabilities. </returns>
		 public static ExecutionMonitor WithDynamicProcessorAssignment( ExecutionMonitor monitor, Configuration config )
		 {
			  DynamicProcessorAssigner dynamicProcessorAssigner = new DynamicProcessorAssigner( config );
			  return new MultiExecutionMonitor( monitor, dynamicProcessorAssigner );
		 }
	}

}