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
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4Net.Test.OtherThreadExecutor;
	using Neo4Net.Test.rule.concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Matchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.staging.Step_Fields.ORDER_SEND_DOWNSTREAM;

	public class ProcessorStepTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.concurrent.OtherThreadRule<Void> t2 = new Neo4Net.test.rule.concurrent.OtherThreadRule<>();
		 public readonly OtherThreadRule<Void> T2 = new OtherThreadRule<Void>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpholdProcessOrderingGuarantee() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpholdProcessOrderingGuarantee()
		 {
			  // GIVEN
			  StageControl control = mock( typeof( StageControl ) );
			  MyProcessorStep step = new MyProcessorStep( control, 0 );
			  step.Start( ORDER_SEND_DOWNSTREAM );
			  step.Processors( 4 ); // now at 5

			  // WHEN
			  int batches = 10;
			  for ( int i = 0; i < batches; i++ )
			  {
					step.Receive( i, i );
			  }
			  step.EndOfUpstream();
			  step.AwaitCompleted();

			  // THEN
			  assertEquals( batches, step.NextExpected.get() );
			  step.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveTaskQueueSizeEqualToMaxNumberOfProcessors() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHaveTaskQueueSizeEqualToMaxNumberOfProcessors()
		 {
			  // GIVEN
			  StageControl control = mock( typeof( StageControl ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch latch = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );
			  const int processors = 2;
			  int maxProcessors = 5;
			  Configuration configuration = new ConfigurationAnonymousInnerClass( this, maxProcessors );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ProcessorStep<Void> step = new BlockingProcessorStep(control, configuration, processors, latch);
			  ProcessorStep<Void> step = new BlockingProcessorStep( control, configuration, processors, latch );
			  step.Start( ORDER_SEND_DOWNSTREAM );
			  step.Processors( 1 ); // now at 2
			  // adding up to max processors should be fine
			  for ( int i = 0; i < processors + maxProcessors ; i++ )
			  {
					step.Receive( i, null );
			  }

			  // WHEN
			  Future<Void> receiveFuture = T2.execute( Receive( processors, step ) );
			  T2.get().waitUntilThreadState(Thread.State.TIMED_WAITING);
			  latch.Signal();

			  // THEN
			  receiveFuture.get();
		 }

		 private class ConfigurationAnonymousInnerClass : Configuration
		 {
			 private readonly ProcessorStepTest _outerInstance;

			 private int _maxProcessors;

			 public ConfigurationAnonymousInnerClass( ProcessorStepTest outerInstance, int maxProcessors )
			 {
				 this.outerInstance = outerInstance;
				 this._maxProcessors = maxProcessors;
			 }

			 public int maxNumberOfProcessors()
			 {
				  return _maxProcessors;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecycleDoneBatches() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecycleDoneBatches()
		 {
			  // GIVEN
			  StageControl control = mock( typeof( StageControl ) );
			  MyProcessorStep step = new MyProcessorStep( control, 0 );
			  step.Start( ORDER_SEND_DOWNSTREAM );

			  // WHEN
			  int batches = 10;
			  for ( int i = 0; i < batches; i++ )
			  {
					step.Receive( i, i );
			  }
			  step.EndOfUpstream();
			  step.AwaitCompleted();

			  // THEN
			  verify( control, times( batches ) ).recycle( any() );
			  step.Close();
		 }

		 private class BlockingProcessorStep : ProcessorStep<Void>
		 {
			  internal readonly System.Threading.CountdownEvent Latch;

			  internal BlockingProcessorStep( StageControl control, Configuration configuration, int maxProcessors, System.Threading.CountdownEvent latch ) : base( control, "test", configuration, maxProcessors )
			  {
					this.Latch = latch;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void process(Void batch, BatchSender sender) throws Throwable
			  protected internal override void Process( Void batch, BatchSender sender )
			  {
					Latch.await();
			  }
		 }

		 private class MyProcessorStep : ProcessorStep<int>
		 {
			  internal readonly AtomicInteger NextExpected = new AtomicInteger();

			  internal MyProcessorStep( StageControl control, int maxProcessors ) : base( control, "test", Configuration.DEFAULT, maxProcessors )
			  {
			  }

			  protected internal override void Process( int? batch, BatchSender sender )
			  { // No processing in this test
					NextExpected.incrementAndGet();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Neo4Net.test.OtherThreadExecutor.WorkerCommand<Void,Void> receive(final int processors, final ProcessorStep<Void> step)
		 private WorkerCommand<Void, Void> Receive( int processors, ProcessorStep<Void> step )
		 {
			  return state =>
			  {
				step.Receive( processors, null );
				return null;
			  };
		 }
	}

}