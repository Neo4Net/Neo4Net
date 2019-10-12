using System;
using System.Collections.Generic;
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
namespace Org.Neo4j.Index.@internal.gbptree
{
	using Test = org.junit.jupiter.api.Test;


	using Group = Org.Neo4j.Scheduler.Group;
	using JobHandle = Org.Neo4j.Scheduler.JobHandle;
	using JobSchedulerAdapter = Org.Neo4j.Scheduler.JobSchedulerAdapter;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class GroupingRecoveryCleanupWorkCollectorTest
	{
		private bool InstanceFieldsInitialized = false;

		public GroupingRecoveryCleanupWorkCollectorTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_jobScheduler = new SingleBackgroundThreadJobScheduler( this );
			_collector = new GroupingRecoveryCleanupWorkCollector( _jobScheduler );
		}

		 private SingleBackgroundThreadJobScheduler _jobScheduler;
		 private GroupingRecoveryCleanupWorkCollector _collector;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotAcceptJobsBeforeInit()
		 internal virtual void ShouldNotAcceptJobsBeforeInit()
		 {
			  // given
			  _collector.add( new DummyJob( this, "A", new List<DummyJob>() ) );

			  // when/then
			  assertThrows( typeof( System.InvalidOperationException ), _collector.init );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcceptJobsAfterStart()
		 public virtual void ShouldNotAcceptJobsAfterStart()
		 {
			  // given
			  _collector.init();
			  _collector.start();

			  // when/then
			  assertThrows( typeof( System.InvalidOperationException ), () => _collector.add(new DummyJob(this, "A", new List<DummyJob>())) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRunAllJobsBeforeOrDuringShutdown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRunAllJobsBeforeOrDuringShutdown()
		 {
			  // given
			  IList<DummyJob> allRuns = new List<DummyJob>();
			  IList<DummyJob> expectedJobs = SomeJobs( allRuns );
			  _collector.init();

			  // when
			  AddAll( expectedJobs );
			  _collector.start();
			  _collector.shutdown();

			  // then
			  assertEquals( allRuns, expectedJobs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustThrowIfOldJobsDuringInit()
		 internal virtual void MustThrowIfOldJobsDuringInit()
		 {
			  // given
			  IList<DummyJob> allRuns = new List<DummyJob>();
			  IList<DummyJob> someJobs = someJobs( allRuns );

			  // when
			  AddAll( someJobs );
			  assertThrows( typeof( System.InvalidOperationException ), _collector.init );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustCloseOldJobsOnShutdown() throws java.util.concurrent.ExecutionException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustCloseOldJobsOnShutdown()
		 {
			  // given
			  IList<DummyJob> allRuns = new List<DummyJob>();
			  IList<DummyJob> someJobs = someJobs( allRuns );

			  // when
			  _collector.init();
			  AddAll( someJobs );
			  _collector.shutdown();

			  // then
			  foreach ( DummyJob job in someJobs )
			  {
					assertTrue( job.Closed, "Expected all jobs to be closed" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustNotScheduleOldJobsOnInitShutdownInit() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustNotScheduleOldJobsOnInitShutdownInit()
		 {
			  // given
			  IList<DummyJob> allRuns = new List<DummyJob>();
			  IList<DummyJob> expectedJobs = SomeJobs( allRuns );

			  // when
			  _collector.init();
			  AddAll( expectedJobs );
			  _collector.start();
			  _collector.shutdown();
			  _collector.init();
			  _collector.start();

			  // then
			  AssertSame( expectedJobs, allRuns );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldExecuteAllTheJobsWhenSeparateJobFails() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldExecuteAllTheJobsWhenSeparateJobFails()
		 {
			  IList<DummyJob> allRuns = new List<DummyJob>();

			  DummyJob firstJob = new DummyJob( this, "first", allRuns );
			  DummyJob thirdJob = new DummyJob( this, "third", allRuns );
			  DummyJob fourthJob = new DummyJob( this, "fourth", allRuns );
			  IList<DummyJob> expectedJobs = Arrays.asList( firstJob, thirdJob, fourthJob );
			  _collector.init();

			  _collector.add( firstJob );
			  _collector.add( new EvilJob( this ) );
			  _collector.add( thirdJob );
			  _collector.add( fourthJob );

			  _collector.start();
			  _collector.shutdown();

			  AssertSame( expectedJobs, allRuns );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void throwOnAddingJobsAfterStart()
		 internal virtual void ThrowOnAddingJobsAfterStart()
		 {
			  _collector.init();
			  _collector.start();

			  assertThrows( typeof( System.InvalidOperationException ), () => _collector.add(new DummyJob(this, "first", new List<DummyJob>())) );
		 }

		 private void AddAll( ICollection<DummyJob> jobs )
		 {
			  jobs.forEach( _collector.add );
		 }

		 private void AssertSame( IList<DummyJob> someJobs, IList<DummyJob> actual )
		 {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'containsAll' method:
			  assertTrue( actual.containsAll( someJobs ) );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'containsAll' method:
			  assertTrue( someJobs.containsAll( actual ) );
		 }

		 private IList<DummyJob> SomeJobs( IList<DummyJob> allRuns )
		 {
			  return new List<DummyJob>(Arrays.asList(new DummyJob(this, "A", allRuns), new DummyJob(this, "B", allRuns), new DummyJob(this, "C", allRuns);
			 ));
		 }

		 private class SingleBackgroundThreadJobScheduler : JobSchedulerAdapter
		 {
			 private readonly GroupingRecoveryCleanupWorkCollectorTest _outerInstance;

			 public SingleBackgroundThreadJobScheduler( GroupingRecoveryCleanupWorkCollectorTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal readonly ExecutorService ExecutorService = Executors.newSingleThreadExecutor();

			  public override JobHandle Schedule( Group group, ThreadStart job )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> future = executorService.submit(job);
					Future<object> future = ExecutorService.submit( job );
					return new JobHandleAnonymousInnerClass( this, future );
			  }

			  private class JobHandleAnonymousInnerClass : JobHandle
			  {
				  private readonly SingleBackgroundThreadJobScheduler _outerInstance;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.concurrent.Future<JavaToDotNetGenericWildcard> future;
				  private Future<object> _future;

				  public JobHandleAnonymousInnerClass<T1>( SingleBackgroundThreadJobScheduler outerInstance, Future<T1> future )
				  {
					  this.outerInstance = outerInstance;
					  this._future = future;
				  }

				  public void cancel( bool mayInterruptIfRunning )
				  {
						_future.cancel( mayInterruptIfRunning );
				  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void waitTermination() throws InterruptedException, java.util.concurrent.ExecutionException, java.util.concurrent.CancellationException
				  public void waitTermination()
				  {
						_future.get();
				  }
			  }

			  public override void Shutdown()
			  {
					ExecutorService.shutdown();
			  }
		 }

		 private class EvilJob : CleanupJob
		 {
			 private readonly GroupingRecoveryCleanupWorkCollectorTest _outerInstance;

			 public EvilJob( GroupingRecoveryCleanupWorkCollectorTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override bool Needed()
			  {
					return false;
			  }

			  public override bool HasFailed()
			  {
					return false;
			  }

			  public virtual Exception Cause
			  {
				  get
				  {
						return null;
				  }
			  }

			  public override void Close()
			  {
					// nothing to close
			  }

			  public override void Run( ExecutorService executor )
			  {
					throw new Exception( "Resilient to run attempts" );
			  }
		 }

		 private class DummyJob : CleanupJob
		 {
			 private readonly GroupingRecoveryCleanupWorkCollectorTest _outerInstance;

			  internal readonly string Name;
			  internal readonly IList<DummyJob> AllRuns;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool ClosedConflict;

			  internal DummyJob( GroupingRecoveryCleanupWorkCollectorTest outerInstance, string name, IList<DummyJob> allRuns )
			  {
				  this._outerInstance = outerInstance;
					this.Name = name;
					this.AllRuns = allRuns;
			  }

			  public override string ToString()
			  {
					return Name;
			  }

			  public override bool Needed()
			  {
					return false;
			  }

			  public override bool HasFailed()
			  {
					return false;
			  }

			  public virtual Exception Cause
			  {
				  get
				  {
						return null;
				  }
			  }

			  public override void Close()
			  {
					ClosedConflict = true;
			  }

			  public override void Run( ExecutorService executor )
			  {
					AllRuns.Add( this );
			  }

			  public virtual bool Closed
			  {
				  get
				  {
						return ClosedConflict;
				  }
			  }
		 }
	}

}