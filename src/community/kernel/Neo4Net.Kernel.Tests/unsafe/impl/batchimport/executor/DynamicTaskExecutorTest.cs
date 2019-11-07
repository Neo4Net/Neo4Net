using System;
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
namespace Neo4Net.@unsafe.Impl.Batchimport.executor
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Exceptions = Neo4Net.Helpers.Exceptions;
	using Barrier = Neo4Net.Test.Barrier;
	using DoubleLatch = Neo4Net.Test.DoubleLatch;
	using Neo4Net.Test;
	using Race = Neo4Net.Test.Race;
	using RepeatRule = Neo4Net.Test.rule.RepeatRule;
	using Repeat = Neo4Net.Test.rule.RepeatRule.Repeat;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class DynamicTaskExecutorTest
	{
		 private static readonly ParkStrategy_Park _park = new ParkStrategy_Park( 1, MILLISECONDS );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.RepeatRule repeater = new Neo4Net.test.rule.RepeatRule();
		 public readonly RepeatRule Repeater = new RepeatRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExecuteTasksInParallel()
		 public virtual void ShouldExecuteTasksInParallel()
		 {
			  // GIVEN
			  TaskExecutor<Void> executor = new DynamicTaskExecutor<Void>( 2, 0, 5, _park, this.GetType().Name );
			  ControlledTask task1 = new ControlledTask();
			  TestTask task2 = new TestTask();

			  // WHEN
			  executor.Submit( task1 );
			  task1.Latch.waitForAllToStart();
			  executor.Submit( task2 );
			  //noinspection StatementWithEmptyBody
			  while ( task2.Executed == 0 )
			  { // Busy loop
			  }
			  task1.Latch.finish();
			  //noinspection StatementWithEmptyBody
			  while ( task1.Executed == 0 )
			  { // Busy loop
			  }
			  executor.Close();

			  // THEN
			  assertEquals( 1, task1.Executed );
			  assertEquals( 1, task2.Executed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncrementNumberOfProcessorsWhenRunning()
		 public virtual void ShouldIncrementNumberOfProcessorsWhenRunning()
		 {
			  // GIVEN
			  TaskExecutor<Void> executor = new DynamicTaskExecutor<Void>( 1, 0, 5, _park, this.GetType().Name );
			  ControlledTask task1 = new ControlledTask();
			  TestTask task2 = new TestTask();

			  // WHEN
			  executor.Submit( task1 );
			  task1.Latch.waitForAllToStart();
			  executor.Submit( task2 );
			  executor.Processors( 1 ); // now at 2
			  //noinspection StatementWithEmptyBody
			  while ( task2.Executed == 0 )
			  { // With one additional worker, the second task can execute even if task one is still executing
			  }
			  task1.Latch.finish();
			  //noinspection StatementWithEmptyBody
			  while ( task1.Executed == 0 )
			  { // Busy loop
			  }
			  executor.Close();

			  // THEN
			  assertEquals( 1, task1.Executed );
			  assertEquals( 1, task2.Executed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDecrementNumberOfProcessorsWhenRunning() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDecrementNumberOfProcessorsWhenRunning()
		 {
			  // GIVEN
			  TaskExecutor<Void> executor = new DynamicTaskExecutor<Void>( 2, 0, 5, _park, this.GetType().Name );
			  ControlledTask task1 = new ControlledTask();
			  ControlledTask task2 = new ControlledTask();
			  ControlledTask task3 = new ControlledTask();
			  TestTask task4 = new TestTask();

			  // WHEN
			  executor.Submit( task1 );
			  executor.Submit( task2 );
			  task1.Latch.waitForAllToStart();
			  task2.Latch.waitForAllToStart();
			  executor.Submit( task3 );
			  executor.Submit( task4 );
			  executor.Processors( -1 ); // it started at 2 ^^^
			  task1.Latch.finish();
			  task2.Latch.finish();
			  task3.Latch.waitForAllToStart();
			  Thread.Sleep( 200 ); // gosh, a Thread.sleep...
			  assertEquals( 0, task4.Executed );
			  task3.Latch.finish();
			  executor.Close();

			  // THEN
			  assertEquals( 1, task1.Executed );
			  assertEquals( 1, task2.Executed );
			  assertEquals( 1, task3.Executed );
			  assertEquals( 1, task4.Executed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExecuteMultipleTasks()
		 public virtual void ShouldExecuteMultipleTasks()
		 {
			  // GIVEN
			  TaskExecutor<Void> executor = new DynamicTaskExecutor<Void>( 30, 0, 5, _park, this.GetType().Name );
			  ExpensiveTask[] tasks = new ExpensiveTask[1000];

			  // WHEN
			  for ( int i = 0; i < tasks.Length; i++ )
			  {
					executor.Submit( tasks[i] = new ExpensiveTask( 10 ) );
			  }
			  executor.Close();

			  // THEN
			  foreach ( ExpensiveTask task in tasks )
			  {
					assertEquals( 1, task.Executed );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShutDownOnTaskFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldShutDownOnTaskFailure()
		 {
			  // GIVEN
			  TaskExecutor<Void> executor = new DynamicTaskExecutor<Void>( 30, 0, 5, _park, this.GetType().Name );

			  // WHEN
			  IOException exception = new IOException( "Test message" );
			  FailingTask task = new FailingTask( exception );
			  executor.Submit( task );
			  task.Latch.await();
			  task.Latch.release();

			  // THEN
			  AssertExceptionOnSubmit( executor, exception );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShutDownOnTaskFailureEvenIfOtherTasksArePending() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldShutDownOnTaskFailureEvenIfOtherTasksArePending()
		 {
			  // GIVEN
			  TaskExecutor<Void> executor = new DynamicTaskExecutor<Void>( 2, 0, 10, _park, this.GetType().Name );
			  IOException exception = new IOException( "Test message" );
			  ControlledTask firstBlockingTask = new ControlledTask();
			  ControlledTask secondBlockingTask = new ControlledTask();
			  executor.Submit( firstBlockingTask );
			  executor.Submit( secondBlockingTask );
			  firstBlockingTask.Latch.waitForAllToStart();
			  secondBlockingTask.Latch.waitForAllToStart();

			  FailingTask failingTask = new FailingTask( exception );
			  executor.Submit( failingTask );

			  ControlledTask thirdBlockingTask = new ControlledTask();
			  executor.Submit( thirdBlockingTask );

			  // WHEN
			  firstBlockingTask.Latch.finish();
			  failingTask.Latch.await();
			  failingTask.Latch.release();

			  // THEN
			  AssertExceptionOnSubmit( executor, exception );
			  executor.Close(); // call would block if the shutdown as part of failure doesn't complete properly

			  secondBlockingTask.Latch.finish();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSurfaceTaskErrorInAssertHealthy() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSurfaceTaskErrorInAssertHealthy()
		 {
			  // GIVEN
			  TaskExecutor<Void> executor = new DynamicTaskExecutor<Void>( 2, 0, 10, _park, this.GetType().Name );
			  IOException exception = new IOException( "Failure" );

			  // WHEN
			  FailingTask failingTask = new FailingTask( exception );
			  executor.Submit( failingTask );
			  failingTask.Latch.await();
			  failingTask.Latch.release();

			  // WHEN
			  for ( int i = 0; i < 5; i++ )
			  {
					try
					{
						 executor.AssertHealthy();
						 // OK, so the executor hasn't executed the finally block after task was done yet
						 Thread.Sleep( 100 );
					}
					catch ( Exception e )
					{
						 assertTrue( Exceptions.contains( e, exception.Message, exception.GetType() ) );
						 return;
					}
			  }
			  fail( "Should not be considered healthy after failing task" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLetShutdownCompleteInEventOfPanic() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLetShutdownCompleteInEventOfPanic()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final TaskExecutor<Void> executor = new DynamicTaskExecutor<>(2, 0, 10, PARK, getClass().getSimpleName());
			  TaskExecutor<Void> executor = new DynamicTaskExecutor<Void>( 2, 0, 10, _park, this.GetType().Name );
			  IOException exception = new IOException( "Failure" );

			  // WHEN
			  FailingTask failingTask = new FailingTask( exception );
			  executor.Submit( failingTask );
			  failingTask.Latch.await();

			  // WHEN
			  using ( OtherThreadExecutor<Void> closer = new OtherThreadExecutor<Void>( "closer", null ) )
			  {
					Future<Void> shutdown = closer.ExecuteDontWait(state =>
					{
					 executor.Close();
					 return null;
					});
					while ( !closer.WaitUntilWaiting().isAt(typeof(DynamicTaskExecutor), "close") )
					{
						 Thread.Sleep( 10 );
					}

					// Here we've got a shutdown call stuck awaiting queue to be empty (since true was passed in)
					// at the same time we've got a FailingTask ready to throw its exception and another task
					// sitting in the queue after it. Now make the task throw that exception.
					failingTask.Latch.release();

					// Some time after throwing this, the shutdown request should have been completed.
					shutdown.get();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespectMaxProcessors()
		 public virtual void ShouldRespectMaxProcessors()
		 {
			  // GIVEN
			  int maxProcessors = 4;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final TaskExecutor<Void> executor = new DynamicTaskExecutor<>(1, maxProcessors, 10, PARK, getClass().getSimpleName());
			  TaskExecutor<Void> executor = new DynamicTaskExecutor<Void>( 1, maxProcessors, 10, _park, this.GetType().Name );

			  // WHEN/THEN
			  assertEquals( 1, executor.Processors( 0 ) );
			  assertEquals( 2, executor.Processors( 1 ) );
			  assertEquals( 4, executor.Processors( 3 ) );
			  assertEquals( 4, executor.Processors( 0 ) );
			  assertEquals( 4, executor.Processors( 1 ) );
			  assertEquals( 3, executor.Processors( -1 ) );
			  assertEquals( 1, executor.Processors( -2 ) );
			  assertEquals( 1, executor.Processors( -2 ) );
			  assertEquals( 1, executor.Processors( 0 ) );
			  executor.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Repeat(times = 10) @Test public void shouldCopeWithConcurrentIncrementOfProcessorsAndShutdown() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Repeat(times : 10)]
		 public virtual void ShouldCopeWithConcurrentIncrementOfProcessorsAndShutdown()
		 {
			  // GIVEN
			  TaskExecutor<Void> executor = new DynamicTaskExecutor<Void>( 1, 2, 2, _park, "test" );
			  Race race = ( new Race() ).withRandomStartDelays();
			  race.AddContestant( executor.close );
			  race.AddContestant( () => executor.Processors(1) );

			  // WHEN
			  race.Go( 10, SECONDS );

			  // THEN we should be able to do so, there was a recent fix here and before that fix
			  // shutdown() would hang, that's why we wait for 10 seconds here to cap it if there's an issue.
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNoticeBadHealthBeforeBeingClosed()
		 public virtual void ShouldNoticeBadHealthBeforeBeingClosed()
		 {
			  // GIVEN
			  TaskExecutor<Void> executor = new DynamicTaskExecutor<Void>( 1, 2, 2, _park, "test" );
			  Exception panic = new Exception( "My failure" );

			  // WHEN
			  executor.ReceivePanic( panic );

			  try
			  {
					// THEN
					executor.AssertHealthy();
					fail( "Should have failed" );
			  }
			  catch ( TaskExecutionPanicException e )
			  {
					assertSame( panic, e.InnerException );
			  }

			  // and WHEN
			  executor.Close();

			  try
			  {
					// THEN
					executor.AssertHealthy();
					fail( "Should have failed" );
			  }
			  catch ( TaskExecutionPanicException e )
			  {
					assertSame( panic, e.InnerException );
			  }
		 }

		 private void AssertExceptionOnSubmit( TaskExecutor<Void> executor, IOException exception )
		 {
			  Exception submitException = null;
			  for ( int i = 0; i < 5 && submitException == null; i++ )
			  {
					try
					{
						 executor.Submit( new EmptyTask() );
						 Thread.Sleep( 100 );
					}
					catch ( Exception e )
					{
						 submitException = e;
					}
			  }
			  assertNotNull( submitException );
			  assertEquals( exception, submitException.InnerException );
		 }

		 private class TestTask : Task<Void>
		 {
			  protected internal volatile int Executed;

			  public override void Run( Void nothing )
			  {
					Executed++;
			  }
		 }

		 private class EmptyTask : Task<Void>
		 {
			  public override void Run( Void nothing )
			  { // Do nothing
			  }
		 }

		 private class FailingTask : Task<Void>
		 {
			  internal readonly Exception Exception;
			  internal readonly Neo4Net.Test.Barrier_Control Latch = new Neo4Net.Test.Barrier_Control();

			  internal FailingTask( Exception exception )
			  {
					this.Exception = exception;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void run(Void nothing) throws Exception
			  public override void Run( Void nothing )
			  {
					try
					{
						 throw Exception;
					}
					finally
					{
						 Latch.reached();
					}
			  }
		 }

		 private class ExpensiveTask : TestTask
		 {
			  internal readonly int Millis;

			  internal ExpensiveTask( int millis )
			  {
					this.Millis = millis;
			  }

			  public override void Run( Void nothing )
			  {
					try
					{
						 Thread.Sleep( Millis );
					}
					catch ( InterruptedException e )
					{
						 throw new Exception( e );
					}
					base.Run( nothing );
			  }
		 }

		 private class ControlledTask : TestTask
		 {
			  internal readonly DoubleLatch Latch = new DoubleLatch();

			  public override void Run( Void nothing )
			  {
					Latch.startAndWaitForAllToStartAndFinish();
					base.Run( nothing );
			  }
		 }
	}

}