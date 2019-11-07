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
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.staging.ControlledStep.stepWithStats;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.staging.Step_Fields.ORDER_SEND_DOWNSTREAM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.stats.Keys.avg_processing_time;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.stats.Keys.done_batches;

	public class DynamicProcessorAssignerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAssignAdditionalCPUToBottleNeckStep()
		 public virtual void ShouldAssignAdditionalCPUToBottleNeckStep()
		 {
			  // GIVEN
			  Configuration config = config( 10, 5 );
			  DynamicProcessorAssigner assigner = new DynamicProcessorAssigner( config );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ControlledStep<?> slowStep = stepWithStats("slow", 0, avg_processing_time, 10L, done_batches, 10L);
			  ControlledStep<object> slowStep = stepWithStats( "slow", 0, avg_processing_time, 10L, done_batches, 10L );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ControlledStep<?> fastStep = stepWithStats("fast", 0, avg_processing_time, 2L, done_batches, 10L);
			  ControlledStep<object> fastStep = stepWithStats( "fast", 0, avg_processing_time, 2L, done_batches, 10L );

			  StageExecution execution = ExecutionOf( config, slowStep, fastStep );
			  assigner.Start( execution );

			  // WHEN
			  assigner.Check( execution );

			  // THEN
			  assertEquals( 5, slowStep.Processors( 0 ) );
			  assertEquals( 1, fastStep.Processors( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveCPUsFromWayTooFastStep()
		 public virtual void ShouldRemoveCPUsFromWayTooFastStep()
		 {
			  // GIVEN
			  Configuration config = config( 10, 3 );
			  // available processors = 2 is enough because it will see the fast step as only using 20% of a processor
			  // and it rounds down. So there's room for assigning one more.
			  DynamicProcessorAssigner assigner = new DynamicProcessorAssigner( config );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ControlledStep<?> slowStep = spy(stepWithStats("slow", 1, avg_processing_time, 6L, done_batches, 10L).setProcessors(2));
			  ControlledStep<object> slowStep = spy( stepWithStats( "slow", 1, avg_processing_time, 6L, done_batches, 10L ).setProcessors( 2 ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ControlledStep<?> fastStep = spy(stepWithStats("fast", 0, avg_processing_time, 2L, done_batches, 10L).setProcessors(2));
			  ControlledStep<object> fastStep = spy( stepWithStats( "fast", 0, avg_processing_time, 2L, done_batches, 10L ).setProcessors( 2 ) );

			  StageExecution execution = ExecutionOf( config, slowStep, fastStep );
			  assigner.Start( execution );

			  // WHEN checking
			  assigner.Check( execution );

			  // THEN one processor should be removed from the fast step
			  verify( fastStep, times( 1 ) ).processors( -1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveCPUsButNotSoThatTheFastStepBecomesBottleneck()
		 public virtual void ShouldRemoveCPUsButNotSoThatTheFastStepBecomesBottleneck()
		 {
			  // GIVEN
			  Configuration config = config( 10, 3 );
			  DynamicProcessorAssigner assigner = new DynamicProcessorAssigner( config );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ControlledStep<?> slowStep = spy(stepWithStats("slow", 1, avg_processing_time, 10L, done_batches, 10L));
			  ControlledStep<object> slowStep = spy( stepWithStats( "slow", 1, avg_processing_time, 10L, done_batches, 10L ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ControlledStep<?> fastStep = spy(stepWithStats("fast", 0, avg_processing_time, 7L, done_batches, 10L).setProcessors(3));
			  ControlledStep<object> fastStep = spy( stepWithStats( "fast", 0, avg_processing_time, 7L, done_batches, 10L ).setProcessors( 3 ) );

			  StageExecution execution = ExecutionOf( config, slowStep, fastStep );
			  assigner.Start( execution );

			  // WHEN checking the first time
			  assigner.Check( execution );

			  // THEN one processor should be removed from the fast step
			  verify( fastStep, never() ).processors(1);
			  verify( fastStep, never() ).processors(-1);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleZeroAverage()
		 public virtual void ShouldHandleZeroAverage()
		 {
			  // GIVEN
			  Configuration config = config( 10, 5 );
			  DynamicProcessorAssigner assigner = new DynamicProcessorAssigner( config );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ControlledStep<?> aStep = stepWithStats("slow", 0, avg_processing_time, 0L, done_batches, 0L);
			  ControlledStep<object> aStep = stepWithStats( "slow", 0, avg_processing_time, 0L, done_batches, 0L );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ControlledStep<?> anotherStep = stepWithStats("fast", 0, avg_processing_time, 0L, done_batches, 0L);
			  ControlledStep<object> anotherStep = stepWithStats( "fast", 0, avg_processing_time, 0L, done_batches, 0L );

			  StageExecution execution = ExecutionOf( config, aStep, anotherStep );
			  assigner.Start( execution );

			  // WHEN
			  assigner.Check( execution );

			  // THEN
			  assertEquals( 1, aStep.Processors( 0 ) );
			  assertEquals( 1, anotherStep.Processors( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveCPUsFromTooFastStepEvenIfThereIsAWayFaster()
		 public virtual void ShouldRemoveCPUsFromTooFastStepEvenIfThereIsAWayFaster()
		 {
			  // The point is that not only the fastest step is subject to have processors removed,
			  // it's the relationship between all pairs of steps. This is important since the DPA has got
			  // a max permit count of processors to assign, so reclaiming unnecessary assignments can
			  // have those be assigned to a more appropriate step instead, where it will benefit the Stage more.

			  // GIVEN
			  Configuration config = config( 10, 3 );
			  DynamicProcessorAssigner assigner = new DynamicProcessorAssigner( config );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Step<?> wayFastest = stepWithStats("wayFastest", 0, avg_processing_time, 50L, done_batches, 20L);
			  Step<object> wayFastest = stepWithStats( "wayFastest", 0, avg_processing_time, 50L, done_batches, 20L );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Step<?> fast = spy(stepWithStats("fast", 0, avg_processing_time, 100L, done_batches, 20L).setProcessors(3));
			  Step<object> fast = spy( stepWithStats( "fast", 0, avg_processing_time, 100L, done_batches, 20L ).setProcessors( 3 ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Step<?> slow = stepWithStats("slow", 1, avg_processing_time, 220L, done_batches, 20L);
			  Step<object> slow = stepWithStats( "slow", 1, avg_processing_time, 220L, done_batches, 20L );
			  StageExecution execution = ExecutionOf( config, slow, wayFastest, fast );
			  assigner.Start( execution );

			  // WHEN
			  assigner.Check( execution );

			  // THEN
			  verify( fast ).processors( -1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveCPUsFromTooFastStepEvenIfNotAllPermitsAreUsed()
		 public virtual void ShouldRemoveCPUsFromTooFastStepEvenIfNotAllPermitsAreUsed()
		 {
			  // GIVEN
			  Configuration config = config( 10, 20 );
			  DynamicProcessorAssigner assigner = new DynamicProcessorAssigner( config );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Step<?> wayFastest = spy(stepWithStats("wayFastest", 0, avg_processing_time, 50L, done_batches, 20L).setProcessors(5));
			  Step<object> wayFastest = spy( stepWithStats( "wayFastest", 0, avg_processing_time, 50L, done_batches, 20L ).setProcessors( 5 ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Step<?> fast = spy(stepWithStats("fast", 0, avg_processing_time, 100L, done_batches, 20L).setProcessors(3));
			  Step<object> fast = spy( stepWithStats( "fast", 0, avg_processing_time, 100L, done_batches, 20L ).setProcessors( 3 ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Step<?> slow = stepWithStats("slow", 1, avg_processing_time, 220L, done_batches, 20L);
			  Step<object> slow = stepWithStats( "slow", 1, avg_processing_time, 220L, done_batches, 20L );
			  StageExecution execution = ExecutionOf( config, slow, wayFastest, fast );
			  assigner.Start( execution );

			  // WHEN
			  assigner.Check( execution );

			  // THEN
			  verify( wayFastest ).processors( -1 );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Neo4Net.unsafe.impl.batchimport.Configuration config(final int movingAverage, int processors)
		 private Configuration Config( int movingAverage, int processors )
		 {
			  return new ConfigurationAnonymousInnerClass( this, movingAverage, processors );
		 }

		 private class ConfigurationAnonymousInnerClass : Configuration
		 {
			 private readonly DynamicProcessorAssignerTest _outerInstance;

			 private int _movingAverage;
			 private int _processors;

			 public ConfigurationAnonymousInnerClass( DynamicProcessorAssignerTest outerInstance, int movingAverage, int processors )
			 {
				 this.outerInstance = outerInstance;
				 this._movingAverage = movingAverage;
				 this._processors = processors;
			 }

			 public int movingAverageSize()
			 {
				  return _movingAverage;
			 }

			 public int maxNumberOfProcessors()
			 {
				  return _processors;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private StageExecution executionOf(Neo4Net.unsafe.impl.batchimport.Configuration config, Step<?>... steps)
		 private StageExecution ExecutionOf( Configuration config, params Step<object>[] steps )
		 {
			  return new StageExecution( "Test", null, config, Arrays.asList( steps ), ORDER_SEND_DOWNSTREAM );
		 }
	}

}