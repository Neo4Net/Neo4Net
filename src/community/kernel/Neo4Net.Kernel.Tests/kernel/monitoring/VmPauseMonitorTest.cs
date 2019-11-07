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
namespace Neo4Net.Kernel.monitoring
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using VmPauseInfo = Neo4Net.Kernel.monitoring.VmPauseMonitor.VmPauseInfo;
	using NullLog = Neo4Net.Logging.NullLog;
	using Group = Neo4Net.Scheduler.Group;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using JobHandle = Neo4Net.Scheduler.JobHandle;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doReturn;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

	public class VmPauseMonitorTest
	{
		private bool InstanceFieldsInitialized = false;

		public VmPauseMonitorTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_monitor = spy( new VmPauseMonitor( ofMillis( 1 ), ofMillis( 0 ), NullLog.Instance, _jobScheduler, _listener ) );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private final System.Action<Neo4Net.kernel.monitoring.VmPauseMonitor.VmPauseInfo> listener = mock(System.Action.class);
		 private readonly System.Action<VmPauseInfo> _listener = mock( typeof( System.Action ) );
		 private readonly JobHandle _jobHandle = mock( typeof( JobHandle ) );
		 private readonly IJobScheduler _jobScheduler = mock( typeof( IJobScheduler ) );
		 private VmPauseMonitor _monitor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  doReturn( _jobHandle ).when( _jobScheduler ).schedule( any( typeof( Group ) ), any( typeof( ThreadStart ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCtorParametersValidation()
		 public virtual void TestCtorParametersValidation()
		 {
			  AssertThatFails( typeof( System.NullReferenceException ), () => new VmPauseMonitor(ofSeconds(1), ofSeconds(1), null, _jobScheduler, _listener) );
			  AssertThatFails( typeof( System.NullReferenceException ), () => new VmPauseMonitor(ofSeconds(1), ofSeconds(1), NullLog.Instance, null, _listener) );
			  AssertThatFails( typeof( System.NullReferenceException ), () => new VmPauseMonitor(ofSeconds(1), ofSeconds(1), NullLog.Instance, _jobScheduler, null) );
			  AssertThatFails( typeof( System.ArgumentException ), () => new VmPauseMonitor(ofSeconds(0), ofSeconds(1), NullLog.Instance, _jobScheduler, _listener) );
			  AssertThatFails( typeof( System.ArgumentException ), () => new VmPauseMonitor(ofSeconds(1), ofSeconds(-1), NullLog.Instance, _jobScheduler, _listener) );
			  AssertThatFails( typeof( System.ArgumentException ), () => new VmPauseMonitor(ofSeconds(-1), ofSeconds(1), NullLog.Instance, _jobScheduler, _listener) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStartAndStop()
		 public virtual void TestStartAndStop()
		 {
			  _monitor.start();
			  _monitor.stop();

			  verify( _jobScheduler ).schedule( any( typeof( Group ) ), any( typeof( ThreadStart ) ) );
			  verify( _jobHandle ).cancel( eq( true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRestart()
		 public virtual void TestRestart()
		 {
			  _monitor.start();
			  _monitor.stop();
			  _monitor.start();

			  verify( _jobScheduler, times( 2 ) ).schedule( any( typeof( Group ) ), any( typeof( ThreadStart ) ) );
			  verify( _jobHandle ).cancel( eq( true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void testFailStopWithoutStart()
		 public virtual void TestFailStopWithoutStart()
		 {
			  _monitor.stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void testFailOnDoubleStart()
		 public virtual void TestFailOnDoubleStart()
		 {
			  _monitor.start();
			  _monitor.start();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void testFailOnDoubleStop()
		 public virtual void TestFailOnDoubleStop()
		 {
			  _monitor.start();
			  _monitor.stop();
			  _monitor.stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNotifyListener() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestNotifyListener()
		 {
			  doReturn( false, true ).when( _monitor ).Stopped;
			  _monitor.monitor();
			  verify( _listener ).accept( any( typeof( VmPauseInfo ) ) );
		 }

		 private static void AssertThatFails( Type exceptionClass, ThreadStart action )
		 {
			  try
			  {
					action.run();
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					fail( "Expected exception was not thrown: " + exceptionClass.FullName );
			  }
			  catch ( Exception e )
			  {
					assertSame( exceptionClass, e.GetType() );
			  }
		 }
	}

}