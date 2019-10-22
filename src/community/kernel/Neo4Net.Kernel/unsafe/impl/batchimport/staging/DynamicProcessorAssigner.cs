using System;
using System.Collections.Generic;

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

	using Neo4Net.Helpers.Collections;
	using Keys = Neo4Net.@unsafe.Impl.Batchimport.stats.Keys;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;

	/// <summary>
	/// Monitors <seealso cref="StageExecution executions"/> and makes changes as the execution goes:
	/// <ul>
	/// <li>Figures out roughly how many CPUs (henceforth called processors) are busy processing batches.
	/// The most busy step will have its <seealso cref="Step.processors(int) processors"/> counted as 1 processor each, all other
	/// will take into consideration how idle the CPUs executing each step is, counted as less than one.</li>
	/// <li>Constantly figures out bottleneck steps and assigns more processors those.</li>
	/// <li>Constantly figures out if there are steps that are way faster than the second fastest step and
	/// removes processors from those steps.</li>
	/// <li>At all times keeps the total number of processors assigned to steps to a total of less than or equal to
	/// <seealso cref="Configuration.maxNumberOfProcessors()"/>.</li>
	/// </ul>
	/// </summary>
	public class DynamicProcessorAssigner : ExecutionMonitor_Adapter
	{
		 private readonly Configuration _config;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Map<Step<?>,long> lastChangedProcessors = new java.util.HashMap<>();
		 private readonly IDictionary<Step<object>, long> _lastChangedProcessors = new Dictionary<Step<object>, long>();
		 private readonly int _availableProcessors;

		 public DynamicProcessorAssigner( Configuration config ) : base( 1, SECONDS )
		 {
			  this._config = config;
			  this._availableProcessors = config.MaxNumberOfProcessors();
		 }

		 public override void Start( StageExecution execution )
		 { // A new stage begins, any data that we had is irrelevant
			  _lastChangedProcessors.Clear();
		 }

		 public override void Check( StageExecution execution )
		 {
			  if ( execution.StillExecuting() )
			  {
					int permits = _availableProcessors - CountActiveProcessors( execution );
					if ( permits > 0 )
					{
						 // Be swift at assigning processors to slow steps, i.e. potentially multiple per round
						 AssignProcessorsToPotentialBottleNeck( execution, permits );
					}
					// Be a little more conservative removing processors from too fast steps
					RemoveProcessorFromPotentialIdleStep( execution );
			  }
		 }

		 private void AssignProcessorsToPotentialBottleNeck( StageExecution execution, int permits )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.helpers.collection.Pair<Step<?>,float> bottleNeck = execution.stepsOrderedBy(org.Neo4Net.unsafe.impl.batchimport.stats.Keys.avg_processing_time, false).iterator().next();
			  Pair<Step<object>, float> bottleNeck = execution.StepsOrderedBy( Keys.avg_processing_time, false ).GetEnumerator().next();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Step<?> bottleNeckStep = bottleNeck.first();
			  Step<object> bottleNeckStep = bottleNeck.First();
			  long doneBatches = Batches( bottleNeckStep );
			  if ( bottleNeck.Other() > 1.0f && BatchesPassedSinceLastChange(bottleNeckStep, doneBatches) >= _config.movingAverageSize() )
			  {
					// Assign 1/10th of the remaining permits. This will have processors being assigned more
					// aggressively in the beginning of the run
					int optimalProcessorIncrement = min( max( 1, ( int ) bottleNeck.Other() - 1 ), permits );
					int before = bottleNeckStep.Processors( 0 );
					int after = bottleNeckStep.Processors( max( optimalProcessorIncrement, permits / 10 ) );
					if ( after > before )
					{
						 _lastChangedProcessors[bottleNeckStep] = doneBatches;
					}
			  }
		 }

		 private void RemoveProcessorFromPotentialIdleStep( StageExecution execution )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.Neo4Net.helpers.collection.Pair<Step<?>,float> fast : execution.stepsOrderedBy(org.Neo4Net.unsafe.impl.batchimport.stats.Keys.avg_processing_time, true))
			  foreach ( Pair<Step<object>, float> fast in execution.StepsOrderedBy( Keys.avg_processing_time, true ) )
			  {
					int numberOfProcessors = fast.First().processors(0);
					if ( numberOfProcessors == 1 )
					{
						 continue;
					}

					// Translate the factor compared to the next (slower) step and see if this step would still
					// be faster if we decremented the processor count, with a slight conservative margin as well
					// (0.8 instead of 1.0 so that we don't decrement and immediately become the bottleneck ourselves).
					float factorWithDecrementedProcessorCount = fast.Other() * numberOfProcessors / (numberOfProcessors - 1);
					if ( factorWithDecrementedProcessorCount < 0.8f )
					{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Step<?> fastestStep = fast.first();
						 Step<object> fastestStep = fast.First();
						 long doneBatches = Batches( fastestStep );
						 if ( BatchesPassedSinceLastChange( fastestStep, doneBatches ) >= _config.movingAverageSize() )
						 {
							  int before = fastestStep.Processors( 0 );
							  if ( fastestStep.Processors( -1 ) < before )
							  {
									_lastChangedProcessors[fastestStep] = doneBatches;
									return;
							  }
						 }
					}
			  }
		 }

		 private long Avg<T1>( Step<T1> step )
		 {
			  return step.Stats().stat(Keys.avg_processing_time).asLong();
		 }

		 private long Batches<T1>( Step<T1> step )
		 {
			  return step.Stats().stat(Keys.done_batches).asLong();
		 }

		 private int CountActiveProcessors( StageExecution execution )
		 {
			  float processors = 0;
			  if ( execution.StillExecuting() )
			  {
					long highestAverage = Avg( execution.StepsOrderedBy( Keys.avg_processing_time, false ).GetEnumerator().next().first() );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (Step<?> step : execution.steps())
					foreach ( Step<object> step in execution.Steps() )
					{
						 // Calculate how active each step is so that a step that is very cheap
						 // and idles a lot counts for less than 1 processor, so that bottlenecks can
						 // "steal" some of its processing power.
						 long avg = avg( step );
						 float factor = ( float )avg / ( float )highestAverage;
						 processors += factor * step.Processors( 0 );
					}
			  }
			  return ( int )Math.Round( processors, MidpointRounding.AwayFromZero );
		 }

		 private long BatchesPassedSinceLastChange<T1>( Step<T1> step, long doneBatches )
		 {
			  return _lastChangedProcessors.ContainsKey( step ) ? doneBatches - _lastChangedProcessors[step] : _config.movingAverageSize();
		 }
	}

}