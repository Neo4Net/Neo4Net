using System.Collections.Generic;

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
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameter = org.junit.runners.Parameterized.Parameter;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.stats.Keys.done_batches;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class CoarseBoundedProgressExecutionMonitorTest
	public class CoarseBoundedProgressExecutionMonitorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static Iterable<int> parameters()
		 public static IEnumerable<int> Parameters()
		 {
			  return Arrays.asList( 1, 10, 123 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameter public int batchSize;
		 public int BatchSize;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportProgressOnSingleExecution()
		 public virtual void ShouldReportProgressOnSingleExecution()
		 {
			  // GIVEN
			  Configuration config = config();
			  ProgressExecutionMonitor progressExecutionMonitor = new ProgressExecutionMonitor( this, BatchSize, config() );

			  // WHEN
			  long total = MonitorSingleStageExecution( progressExecutionMonitor, config );

			  // THEN
			  assertEquals( total, progressExecutionMonitor.Progress );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void progressOnMultipleExecutions()
		 public virtual void ProgressOnMultipleExecutions()
		 {
			  Configuration config = config();
			  ProgressExecutionMonitor progressExecutionMonitor = new ProgressExecutionMonitor( this, BatchSize, config );

			  long total = progressExecutionMonitor.Total();

			  for ( int i = 0; i < 4; i++ )
			  {
					progressExecutionMonitor.Start( Execution( 0, config ) );
					progressExecutionMonitor.Check( Execution( total / 4, config ) );
			  }
			  progressExecutionMonitor.Done( true, 0, "Completed" );

			  assertEquals( "Each item should be completed", total, progressExecutionMonitor.Progress );
		 }

		 private long MonitorSingleStageExecution( ProgressExecutionMonitor progressExecutionMonitor, Configuration config )
		 {
			  progressExecutionMonitor.Start( Execution( 0, config ) );
			  long total = progressExecutionMonitor.Total();
			  long part = total / 10;
			  for ( int i = 0; i < 9; i++ )
			  {
					progressExecutionMonitor.Check( Execution( part * ( i + 1 ), config ) );
					assertTrue( progressExecutionMonitor.Progress < total );
			  }
			  progressExecutionMonitor.Done( true, 0, "Test" );
			  return total;
		 }

		 private StageExecution Execution( long doneBatches, Configuration config )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Step<?> step = ControlledStep.stepWithStats("Test", 0, done_batches, doneBatches);
			  Step<object> step = ControlledStep.StepWithStats( "Test", 0, done_batches, doneBatches );
			  StageExecution execution = new StageExecution( "Test", null, config, Collections.singletonList( step ), 0 );
			  return execution;
		 }

		 private Configuration Config()
		 {
			  return new Configuration_OverriddenAnonymousInnerClass( this, Configuration.DEFAULT );
		 }

		 private class Configuration_OverriddenAnonymousInnerClass : Neo4Net.@unsafe.Impl.Batchimport.Configuration_Overridden
		 {
			 private readonly CoarseBoundedProgressExecutionMonitorTest _outerInstance;

			 public Configuration_OverriddenAnonymousInnerClass( CoarseBoundedProgressExecutionMonitorTest outerInstance, UnknownType @default ) : base( @default )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override int batchSize()
			 {
				  return _outerInstance.batchSize;
			 }
		 }

		 private class ProgressExecutionMonitor : CoarseBoundedProgressExecutionMonitor
		 {
			 private readonly CoarseBoundedProgressExecutionMonitorTest _outerInstance;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long ProgressConflict;

			  internal ProgressExecutionMonitor( CoarseBoundedProgressExecutionMonitorTest outerInstance, int batchSize, Configuration configuration ) : base( 100 * batchSize, 100 * batchSize, configuration )
			  {
				  this._outerInstance = outerInstance;
			  }

			  protected internal override void Progress( long progress )
			  {
					this.ProgressConflict += progress;
			  }

			  public virtual long Progress
			  {
				  get
				  {
						return ProgressConflict;
				  }
			  }
		 }
	}

}