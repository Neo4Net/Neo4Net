using System;
using System.Threading;

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


	using Keys = Neo4Net.@unsafe.Impl.Batchimport.stats.Keys;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.Configuration.DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.staging.Step_Fields.ORDER_SEND_DOWNSTREAM;

	public class StageTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReceiveBatchesInOrder()
		 public virtual void ShouldReceiveBatchesInOrder()
		 {
			  // GIVEN
			  Configuration config = new Configuration_OverriddenAnonymousInnerClass( this, DEFAULT );
			  Stage stage = new Stage( "Test stage", null, config, ORDER_SEND_DOWNSTREAM );
			  long batches = 1000;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long items = batches * config.batchSize();
			  long items = batches * config.BatchSize();
			  stage.Add( new PullingProducerStepAnonymousInnerClass( this, stage.Control(), config, items ) );

			  for ( int i = 0; i < 3; i++ )
			  {
					stage.Add( new ReceiveOrderAssertingStep( stage.Control(), "Step" + i, config, i, false ) );
			  }
			  stage.Add( new ReceiveOrderAssertingStep( stage.Control(), "Final step", config, 0, true ) );

			  // WHEN
			  StageExecution execution = stage.Execute();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (Step<?> step : execution.steps())
			  foreach ( Step<object> step in execution.Steps() )
			  {
					// we start off with two in each step
					step.Processors( 1 );
			  }
			  ( new ExecutionSupervisor( ExecutionMonitors.Invisible() ) ).supervise(execution);

			  // THEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (Step<?> step : execution.steps())
			  foreach ( Step<object> step in execution.Steps() )
			  {
					assertEquals( "For " + step, batches, step.Stats().stat(Keys.done_batches).asLong() );
			  }
			  stage.Close();
		 }

		 private class Configuration_OverriddenAnonymousInnerClass : Neo4Net.@unsafe.Impl.Batchimport.Configuration_Overridden
		 {
			 private readonly StageTest _outerInstance;

			 public Configuration_OverriddenAnonymousInnerClass( StageTest outerInstance, UnknownType @default ) : base( @default )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override int batchSize()
			 {
				  return 10;
			 }
		 }

		 private class PullingProducerStepAnonymousInnerClass : PullingProducerStep
		 {
			 private readonly StageTest _outerInstance;

			 private long _items;

			 public PullingProducerStepAnonymousInnerClass( StageTest outerInstance, Neo4Net.@unsafe.Impl.Batchimport.staging.StageControl control, Configuration config, long items ) : base( control, config )
			 {
				 this.outerInstance = outerInstance;
				 this._items = items;
				 theObject = new object();
			 }

			 private readonly object theObject;
			 private long i;

			 protected internal override object nextBatchOrNull( long ticket, int batchSize )
			 {
				  if ( i >= _items )
				  {
						return null;
				  }

				  object[] batch = new object[batchSize];
				  Arrays.fill( batch, theObject );
				  i += batchSize;
				  return batch;
			 }

			 protected internal override long position()
			 {
				  return 0;
			 }
		 }

		 private class ReceiveOrderAssertingStep : ProcessorStep<object>
		 {
			  internal readonly AtomicLong LastTicket = new AtomicLong();
			  internal readonly long ProcessingTime;
			  internal readonly bool EndOfLine;

			  internal ReceiveOrderAssertingStep( StageControl control, string name, Configuration config, long processingTime, bool endOfLine ) : base( control, name, config, 1 )
			  {
					this.ProcessingTime = processingTime;
					this.EndOfLine = endOfLine;
			  }

			  public override long Receive( long ticket, object batch )
			  {
					assertEquals( "For " + batch + " in " + Name(), LastTicket.AndIncrement, ticket );
					return base.Receive( ticket, batch );
			  }

			  protected internal override void Process( object batch, BatchSender sender )
			  {
					try
					{
						 Thread.Sleep( ProcessingTime );
					}
					catch ( InterruptedException e )
					{
						 throw new Exception( e );
					}

					if ( !EndOfLine )
					{
						 sender.Send( batch );
					}
			  }
		 }
	}

}