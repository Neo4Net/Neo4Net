using System;
using System.Collections.Generic;
using System.Text;

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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.staging
{

	using Clocks = Org.Neo4j.Time.Clocks;


	/// <summary>
	/// Processor assigner strategies that are useful for testing <seealso cref="ParallelBatchImporter"/> as to exercise
	/// certain aspects of parallelism which would otherwise only be exercised on particular machines and datasets.
	/// </summary>
	public class ProcessorAssignmentStrategies
	{

		 private ProcessorAssignmentStrategies()
		 {
		 }

		 /// <summary>
		 /// Right of the bat assigns all permitted processors to random steps that allow multiple threads.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static ExecutionMonitor eagerRandomSaturation(final int availableProcessor)
		 public static ExecutionMonitor EagerRandomSaturation( int availableProcessor )
		 {
			  return new AbstractAssignerAnonymousInnerClass( Clocks.systemClock(), SECONDS, availableProcessor );
		 }

		 private class AbstractAssignerAnonymousInnerClass : AbstractAssigner
		 {
			 private int _availableProcessor;

			 public AbstractAssignerAnonymousInnerClass( Clock systemClock, UnknownType seconds, int availableProcessor ) : base( systemClock, 10, seconds )
			 {
				 this._availableProcessor = availableProcessor;
			 }

			 public override void start( StageExecution execution )
			 {
				  saturate( _availableProcessor, execution );
				  registerProcessorCount( execution );
			 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void saturate(final int availableProcessor, StageExecution execution)
			 private void saturate( int availableProcessor, StageExecution execution )
			 {
				  Random random = ThreadLocalRandom.current();
				  int processors = availableProcessor;
				  for ( int rounds = 0; rounds < availableProcessor && processors > 0; rounds++ )
				  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (Step<?> step : execution.steps())
						foreach ( Step<object> step in execution.Steps() )
						{
							 int before = step.Processors( 0 );
							 if ( random.nextBoolean() && step.Processors(1) > before && --processors == 0 )
							 {
								  return;
							 }
						}
				  }
			 }

			 public override void check( StageExecution execution )
			 { // We do everything in start
			 }
		 }

		 /// <summary>
		 /// For every check assigns a random number of more processors to random steps that allow multiple threads.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static ExecutionMonitor randomSaturationOverTime(final int availableProcessor)
		 public static ExecutionMonitor RandomSaturationOverTime( int availableProcessor )
		 {
			  return new AbstractAssignerAnonymousInnerClass2( Clocks.systemClock(), MILLISECONDS, availableProcessor );
		 }

		 private class AbstractAssignerAnonymousInnerClass2 : AbstractAssigner
		 {
			 private int _availableProcessor;

			 public AbstractAssignerAnonymousInnerClass2( Clock systemClock, UnknownType milliseconds, int availableProcessor ) : base( systemClock, 100, milliseconds )
			 {
				 this._availableProcessor = availableProcessor;
			 }

			 private int processors = _availableProcessor;

			 public override void check( StageExecution execution )
			 {
				  saturate( execution );
				  registerProcessorCount( execution );
			 }

			 private void saturate( StageExecution execution )
			 {
				  if ( processors == 0 )
				  {
						return;
				  }

				  Random random = ThreadLocalRandom.current();
				  int maxThisCheck = random.Next( processors - 1 ) + 1;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (Step<?> step : execution.steps())
				  foreach ( Step<object> step in execution.Steps() )
				  {
						int before = step.Processors( 0 );
						if ( random.nextBoolean() && step.Processors(-1) < before )
						{
							 processors--;
							 if ( --maxThisCheck == 0 )
							 {
								  return;
							 }
						}
				  }
			 }
		 }

		 private abstract class AbstractAssigner : ExecutionMonitor_Adapter
		 {
			  internal readonly IDictionary<string, IDictionary<string, int>> Processors = new Dictionary<string, IDictionary<string, int>>();

			  protected internal AbstractAssigner( Clock clock, long time, TimeUnit unit ) : base( clock, time, unit )
			  {
			  }

			  protected internal virtual void RegisterProcessorCount( StageExecution execution )
			  {
					IDictionary<string, int> byStage = new Dictionary<string, int>();
					Processors[execution.Name()] = byStage;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (Step<?> step : execution.steps())
					foreach ( Step<object> step in execution.Steps() )
					{
						 byStage[step.Name()] = step.Processors(0);
					}
			  }

			  public override string ToString()
			  {
					// For debugging purposes. Includes information about how many processors each step got.
					StringBuilder builder = new StringBuilder();
					foreach ( string stage in Processors.Keys )
					{
						 builder.Append( stage ).Append( ':' );
						 IDictionary<string, int> byStage = Processors[stage];
						 foreach ( string step in byStage.Keys )
						 {
							  builder.Append( format( "%n  %s:%d", step, byStage[step] ) );
						 }
						 builder.Append( format( "%n" ) );
					}
					return builder.ToString();
			  }
		 }
	}

}