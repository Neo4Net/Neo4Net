using System;
using System.Collections.Generic;
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
namespace Neo4Net.@unsafe.Impl.Batchimport.staging
{
	using Test = org.junit.Test;


	using Keys = Neo4Net.@unsafe.Impl.Batchimport.stats.Keys;
	using StepStats = Neo4Net.@unsafe.Impl.Batchimport.stats.StepStats;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterables.asList;

	public class ForkedProcessorStepTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProcessAllSingleThreaded() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProcessAllSingleThreaded()
		 {
			  // GIVEN
			  StageControl control = mock( typeof( StageControl ) );
			  int processors = 10;

			  int batches = 10;
			  BatchProcessor step = new BatchProcessor( control, processors );
			  TrackingStep downstream = new TrackingStep();
			  step.Downstream = downstream;
			  step.Processors( processors - step.Processors( 0 ) );

			  // WHEN
			  step.Start( 0 );
			  for ( int i = 1; i <= batches; i++ )
			  {
					step.Receive( i, new Batch( processors ) );
			  }
			  step.EndOfUpstream();
			  step.AwaitCompleted();
			  step.Close();

			  // THEN
			  assertEquals( batches, downstream.Received.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 10_000) public void shouldProcessAllBatchesOnSingleCoreSystems() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProcessAllBatchesOnSingleCoreSystems()
		 {
			  // GIVEN
			  StageControl control = mock( typeof( StageControl ) );
			  int processors = 1;

			  int batches = 10;
			  BatchProcessor step = new BatchProcessor( control, processors );
			  TrackingStep downstream = new TrackingStep();
			  step.Downstream = downstream;

			  // WHEN
			  step.Start( 0 );
			  for ( int i = 1; i <= batches; i++ )
			  {
					step.Receive( i, new Batch( processors ) );
			  }
			  step.EndOfUpstream();
			  step.AwaitCompleted();
			  step.Close();

			  // THEN
			  assertEquals( batches, downstream.Received.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotDetachProcessorsFromBatchChains() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustNotDetachProcessorsFromBatchChains()
		 {
			  // GIVEN
			  StageControl control = mock( typeof( StageControl ) );
			  int processors = 1;

			  int batches = 10;
			  BatchProcessor step = new BatchProcessor( control, processors );
			  TrackingStep downstream = new TrackingStep();
			  step.Downstream = downstream;
			  int delta = processors - step.Processors( 0 );
			  step.Processors( delta );

			  // WHEN
			  step.Start( 0 );
			  for ( int i = 1; i <= batches; i++ )
			  {
					step.Receive( i, new Batch( processors ) );
			  }
			  step.EndOfUpstream();
			  step.AwaitCompleted();
			  step.Close();

			  // THEN
			  assertEquals( batches, downstream.Received.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProcessAllMultiThreadedAndWithChangingProcessorCount() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProcessAllMultiThreadedAndWithChangingProcessorCount()
		 {
			  // GIVEN
			  StageControl control = mock( typeof( StageControl ) );
			  int availableProcessors = Runtime.Runtime.availableProcessors();
			  BatchProcessor step = new BatchProcessor( control, availableProcessors );
			  TrackingStep downstream = new TrackingStep();
			  step.Downstream = downstream;

			  // WHEN
			  step.Start( 0 );
			  AtomicLong nextTicket = new AtomicLong();
			  Thread[] submitters = new Thread[3];
			  AtomicBoolean end = new AtomicBoolean();
			  for ( int i = 0; i < submitters.Length; i++ )
			  {
					submitters[i] = new Thread(() =>
					{
					ThreadLocalRandom random = ThreadLocalRandom.current();
					while ( !end.get() )
					{
						lock ( nextTicket )
						{
							if ( random.nextFloat() < 0.1 )
							{
								step.Processors( random.Next( -2, 4 ) );
							}
							long ticket = nextTicket.incrementAndGet();
							Batch batch = new Batch( step.Processors( 0 ) );
							step.Receive( ticket, batch );
						}
					}
					});
					submitters[i].Start();
			  }

			  while ( downstream.Received.get() < 200 )
			  {
					Thread.Sleep( 10 );
			  }
			  end.set( true );
			  foreach ( Thread submitter in submitters )
			  {
					submitter.Join();
			  }
			  step.EndOfUpstream();
			  step.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepForkedOrderIntactWhenChangingProcessorCount() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKeepForkedOrderIntactWhenChangingProcessorCount()
		 {
			  int length = 100;
			  AtomicIntegerArray reference = new AtomicIntegerArray( length );

			  // GIVEN
			  StageControl control = mock( typeof( StageControl ) );
			  int availableProcessors = Runtime.Runtime.availableProcessors();
			  ForkedProcessorStep<int[]> step = new ForkedProcessorStepAnonymousInnerClass( this, control, Config( availableProcessors ), reference );
			  DeadEndStep downstream = new DeadEndStep( control );
			  step.Downstream = downstream;

			  // WHEN
			  step.Start( 0 );
			  downstream.Start( 0 );
			  ThreadLocalRandom random = ThreadLocalRandom.current();
			  for ( int ticket = 0; ticket < 200; ticket++ )
			  {
					// The processor count is changed here in this block simply because otherwise
					// it's very hard to know how many processors we expect to see have processed
					// a particular batch.
					if ( random.nextFloat() < 0.1 )
					{
						 int p = step.Processors( random.Next( -2, 4 ) );
					}

					int[] batch = new int[length];
					batch[0] = ticket;
					for ( int j = 1; j < batch.Length; j++ )
					{
						 batch[j] = j - 1;
					}
					step.Receive( ticket, batch );
			  }
			  step.EndOfUpstream();
			  step.AwaitCompleted();
			  step.Close();
		 }

		 private class ForkedProcessorStepAnonymousInnerClass : ForkedProcessorStep<int[]>
		 {
			 private readonly ForkedProcessorStepTest _outerInstance;

			 private AtomicIntegerArray _reference;

			 public ForkedProcessorStepAnonymousInnerClass( ForkedProcessorStepTest outerInstance, Neo4Net.@unsafe.Impl.Batchimport.staging.StageControl control, Configuration config, AtomicIntegerArray reference ) : base( control, "Processor", config )
			 {
				 this.outerInstance = outerInstance;
				 this._reference = reference;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void forkedProcess(int id, int processors, int[] batch) throws InterruptedException
			 protected internal override void forkedProcess( int id, int processors, int[] batch )
			 {
				  int ticket = batch[0];
				  Thread.Sleep( ThreadLocalRandom.current().Next(10) );
				  for ( int i = 1; i < batch.Length; i++ )
				  {
						if ( batch[i] % processors == id )
						{
							 bool compareAndSet = _reference.compareAndSet( batch[i], ticket, ticket + 1 );
							 assertTrue( "I am " + id + ". Was expecting " + ticket + " for " + batch[i] + " but was " + _reference.get( batch[i] ), compareAndSet );
						}
				  }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPanicOnFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPanicOnFailure()
		 {
			  // GIVEN
			  SimpleStageControl control = new SimpleStageControl();
			  int availableProcessors = Runtime.Runtime.availableProcessors();
			  Exception testPanic = new Exception();
			  ForkedProcessorStep<Void> step = new ForkedProcessorStepAnonymousInnerClass2( this, control, Config( availableProcessors ), testPanic );

			  // WHEN
			  step.Start( 0 );
			  step.Receive( 1, null );
			  control.Steps( step );

			  // THEN
			  step.AwaitCompleted();
			  try
			  {
					control.AssertHealthy();
			  }
			  catch ( Exception e )
			  {
					assertSame( testPanic, e );
			  }
		 }

		 private class ForkedProcessorStepAnonymousInnerClass2 : ForkedProcessorStep<Void>
		 {
			 private readonly ForkedProcessorStepTest _outerInstance;

			 private Exception _testPanic;

			 public ForkedProcessorStepAnonymousInnerClass2( ForkedProcessorStepTest outerInstance, Neo4Net.@unsafe.Impl.Batchimport.staging.SimpleStageControl control, Configuration config, Exception testPanic ) : base( control, "Processor", config )
			 {
				 this.outerInstance = outerInstance;
				 this._testPanic = testPanic;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void forkedProcess(int id, int processors, Void batch) throws Throwable
			 protected internal override void forkedProcess( int id, int processors, Void batch )
			 {
				  throw _testPanic;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 60_000) public void shouldBeAbleToProgressUnderStressfulProcessorChangesWhenOrdered() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToProgressUnderStressfulProcessorChangesWhenOrdered()
		 {
			  ShouldBeAbleToProgressUnderStressfulProcessorChanges( Step_Fields.ORDER_SEND_DOWNSTREAM );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 60_000) public void shouldBeAbleToProgressUnderStressfulProcessorChangesWhenUnordered() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToProgressUnderStressfulProcessorChangesWhenUnordered()
		 {
			  ShouldBeAbleToProgressUnderStressfulProcessorChanges( 0 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldBeAbleToProgressUnderStressfulProcessorChanges(int orderingGuarantees) throws Exception
		 private void ShouldBeAbleToProgressUnderStressfulProcessorChanges( int orderingGuarantees )
		 {
			  // given
			  int batches = 100;
			  int processors = Runtime.Runtime.availableProcessors() * 10;
			  Configuration config = new Configuration_OverriddenAnonymousInnerClass( this, Configuration.DEFAULT, processors );
			  Stage stage = new StressStage( config, orderingGuarantees, batches );
			  StageExecution execution = stage.Execute();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<Step<?>> steps = asList(execution.steps());
			  IList<Step<object>> steps = new IList<Step<object>> { execution.Steps() };
			  steps[1].Processors( processors / 3 );

			  // when
			  ThreadLocalRandom random = ThreadLocalRandom.current();
			  while ( execution.StillExecuting() )
			  {
					steps[2].Processors( random.Next( -2, 5 ) );
					Thread.Sleep( 1 );
			  }
			  execution.AssertHealthy();

			  // then
			  assertEquals( batches, steps[steps.Count - 1].Stats().stat(Keys.done_batches).asLong() );
		 }

		 private class Configuration_OverriddenAnonymousInnerClass : Neo4Net.@unsafe.Impl.Batchimport.Configuration_Overridden
		 {
			 private readonly ForkedProcessorStepTest _outerInstance;

			 private int _processors;

			 public Configuration_OverriddenAnonymousInnerClass( ForkedProcessorStepTest outerInstance, UnknownType @default, int processors ) : base( @default )
			 {
				 this.outerInstance = outerInstance;
				 this._processors = processors;
			 }

			 public override int maxNumberOfProcessors()
			 {
				  return _processors;
			 }
		 }

		 private class StressStage : Stage
		 {
			  internal StressStage( Configuration config, int orderingGuarantees, int batches ) : base( "Stress", null, config, orderingGuarantees )
			  {

					Add( new PullingProducerStepAnonymousInnerClass( this, Control(), config, batches ) );
					Add( new ProcessorStepAnonymousInnerClass( this, Control(), config ) );
					Add( new ForkedProcessorStepAnonymousInnerClass3( this, Control(), config ) );
					Add( new DeadEndStep( Control() ) );
			  }

			  private class PullingProducerStepAnonymousInnerClass : PullingProducerStep
			  {
				  private readonly StressStage _outerInstance;

				  private int _batches;

				  public PullingProducerStepAnonymousInnerClass( StressStage outerInstance, Neo4Net.@unsafe.Impl.Batchimport.staging.StageControl control, Configuration config, int batches ) : base( control, config )
				  {
					  this.outerInstance = outerInstance;
					  this._batches = batches;
				  }

				  protected internal override long position()
				  {
						return 0;
				  }

				  protected internal override object nextBatchOrNull( long ticket, int batchSize )
				  {
						return ticket < _batches ? ticket : null;
				  }
			  }

			  private class ProcessorStepAnonymousInnerClass : ProcessorStep<long>
			  {
				  private readonly StressStage _outerInstance;

				  public ProcessorStepAnonymousInnerClass( StressStage outerInstance, Neo4Net.@unsafe.Impl.Batchimport.staging.StageControl control, Configuration config ) : base( control, "Yeah", config, 3 )
				  {
					  this.outerInstance = outerInstance;
				  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void process(System.Nullable<long> batch, BatchSender sender) throws Throwable
				  protected internal override void process( long? batch, BatchSender sender )
				  {
						Thread.Sleep( 0, ThreadLocalRandom.current().Next(100_000) );
						sender.Send( batch );
				  }
			  }

			  private class ForkedProcessorStepAnonymousInnerClass3 : ForkedProcessorStep<long>
			  {
				  private readonly StressStage _outerInstance;

				  public ForkedProcessorStepAnonymousInnerClass3( StressStage outerInstance, Neo4Net.@unsafe.Impl.Batchimport.staging.StageControl control, Configuration config ) : base( control, "Subject", config )
				  {
					  this.outerInstance = outerInstance;
				  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void forkedProcess(int id, int processors, System.Nullable<long> batch) throws Throwable
				  protected internal override void forkedProcess( int id, int processors, long? batch )
				  {
						Thread.Sleep( 0, ThreadLocalRandom.current().Next(100_000) );
				  }
			  }
		 }

		 private class BatchProcessor : ForkedProcessorStep<Batch>
		 {
			  protected internal BatchProcessor( StageControl control, int processors ) : base( control, "PROCESSOR", Config( processors ) )
			  {
			  }

			  protected internal override void ForkedProcess( int id, int processors, Batch batch )
			  {
					batch.ProcessedBy( id );
			  }
		 }

		 private static Configuration Config( int processors )
		 {
			  return new ConfigurationAnonymousInnerClass( processors );
		 }

		 private class ConfigurationAnonymousInnerClass : Configuration
		 {
			 private int _processors;

			 public ConfigurationAnonymousInnerClass( int processors )
			 {
				 this._processors = processors;
			 }

			 public int maxNumberOfProcessors()
			 {
				  return _processors;
			 }
		 }

		 private class Batch
		 {
			  internal readonly bool[] Processed;

			  internal Batch( int processors )
			  {
					this.Processed = new bool[processors];
			  }

			  internal virtual void ProcessedBy( int id )
			  {
					assertFalse( Processed[id] );
					Processed[id] = true;
			  }
		 }

		 private class TrackingStep : Step<Batch>
		 {
			  internal readonly AtomicLong Received = new AtomicLong();

			  public override void ReceivePanic( Exception cause )
			  {
			  }

			  public override void Start( int orderingGuarantees )
			  {
			  }

			  public override string Name()
			  {
					return "END";
			  }

			  public override long Receive( long ticket, Batch batch )
			  {
					int count = 0;
					for ( int i = 0; i < batch.Processed.Length; i++ )
					{
						 if ( batch.Processed[i] )
						 {
							  count++;
						 }
					}
					assertEquals( batch.Processed.Length, count );
					if ( !Received.compareAndSet( ticket - 1, ticket ) )
					{
						 fail( "Hmm " + ticket + " " + Received.get() );
					}
					return 0;
			  }

			  public override StepStats Stats()
			  {
					return null;
			  }

			  public override void EndOfUpstream()
			  {
			  }

			  public virtual bool Completed
			  {
				  get
				  {
						return false;
				  }
			  }

			  public override void AwaitCompleted()
			  {
					throw new System.NotSupportedException();
			  }

			  public virtual Step<T1> Downstream<T1>
			  {
				  set
				  {
				  }
			  }

			  public override void Close()
			  {
			  }
		 }
	}

}