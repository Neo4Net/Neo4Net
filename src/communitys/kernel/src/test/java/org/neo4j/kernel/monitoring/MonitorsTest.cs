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
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;

	public class MonitorsTest
	{
		 internal interface MyMonitor
		 {
			  void AVoid();
			  void TakesArgs( string arg1, long arg2, params object[] moreArgs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideNoOpDelegate()
		 public virtual void ShouldProvideNoOpDelegate()
		 {
			  // Given
			  Monitors monitors = new Monitors();

			  // When
			  MyMonitor monitor = monitors.NewMonitor( typeof( MyMonitor ) );

			  // Then those should be no-ops
			  monitor.AVoid();
			  monitor.TakesArgs( "ha", 12, new object() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRegister()
		 public virtual void ShouldRegister()
		 {
			  // Given
			  Monitors monitors = new Monitors();

			  MyMonitor listener = mock( typeof( MyMonitor ) );
			  MyMonitor monitor = monitors.NewMonitor( typeof( MyMonitor ) );
			  object obj = new object();

			  // When
			  monitors.AddMonitorListener( listener );
			  monitor.AVoid();
			  monitor.TakesArgs( "ha", 12, obj );

			  // Then
			  verify( listener ).aVoid();
			  verify( listener ).takesArgs( "ha", 12, obj );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUnregister()
		 public virtual void ShouldUnregister()
		 {
			  // Given
			  Monitors monitors = new Monitors();

			  MyMonitor listener = mock( typeof( MyMonitor ) );
			  MyMonitor monitor = monitors.NewMonitor( typeof( MyMonitor ) );
			  object obj = new object();

			  monitors.AddMonitorListener( listener );

			  // When
			  monitors.RemoveMonitorListener( listener );
			  monitor.AVoid();
			  monitor.TakesArgs( "ha", 12, obj );

			  // Then
			  verifyNoMoreInteractions( listener );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespectTags()
		 public virtual void ShouldRespectTags()
		 {
			  // Given
			  Monitors monitors = new Monitors();

			  MyMonitor listener = mock( typeof( MyMonitor ) );
			  MyMonitor monitorTag1 = monitors.NewMonitor( typeof( MyMonitor ), "tag1" );
			  MyMonitor monitorTag2 = monitors.NewMonitor( typeof( MyMonitor ), "tag2" );

			  // When
			  monitors.AddMonitorListener( listener, "tag2" );

			  // Then
			  monitorTag1.AVoid();
			  verifyZeroInteractions( listener );
			  monitorTag2.AVoid();
			  verify( listener, times( 1 ) ).aVoid();
			  verifyNoMoreInteractions( listener );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTellIfMonitorHasListeners()
		 public virtual void ShouldTellIfMonitorHasListeners()
		 {
			  // Given
			  Monitors monitors = new Monitors();
			  MyMonitor listener = mock( typeof( MyMonitor ) );

			  // When I have a monitor with no listeners
			  monitors.NewMonitor( typeof( MyMonitor ) );

			  // Then
			  assertFalse( monitors.HasListeners( typeof( MyMonitor ) ) );

			  // When I add a listener
			  monitors.AddMonitorListener( listener );

			  // Then
			  assertTrue( monitors.HasListeners( typeof( MyMonitor ) ) );

			  // When that listener is removed again
			  monitors.RemoveMonitorListener( listener );

			  // Then
			  assertFalse( monitors.HasListeners( typeof( MyMonitor ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void multipleListenersRegistration()
		 public virtual void MultipleListenersRegistration()
		 {
			  Monitors monitors = new Monitors();
			  MyMonitor listener1 = mock( typeof( MyMonitor ) );
			  MyMonitor listener2 = mock( typeof( MyMonitor ) );

			  assertFalse( monitors.HasListeners( typeof( MyMonitor ) ) );

			  monitors.AddMonitorListener( listener1 );
			  monitors.AddMonitorListener( listener2 );
			  assertTrue( monitors.HasListeners( typeof( MyMonitor ) ) );

			  monitors.RemoveMonitorListener( listener1 );
			  assertTrue( monitors.HasListeners( typeof( MyMonitor ) ) );

			  monitors.RemoveMonitorListener( listener2 );
			  assertFalse( monitors.HasListeners( typeof( MyMonitor ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void eventShouldBubbleUp()
		 public virtual void EventShouldBubbleUp()
		 {
			  Monitors parent = new Monitors();
			  MyMonitor parentListener = mock( typeof( MyMonitor ) );
			  parent.AddMonitorListener( parentListener );

			  Monitors child = new Monitors( parent );
			  MyMonitor childListener = mock( typeof( MyMonitor ) );
			  child.AddMonitorListener( childListener );

			  // Calls on monitors from parent should not reach child listeners
			  MyMonitor parentMonitor = parent.NewMonitor( typeof( MyMonitor ) );
			  parentMonitor.AVoid();
			  verify( parentListener, times( 1 ) ).aVoid();
			  verifyZeroInteractions( childListener );

			  // Calls on monitors from child should reach both listeners
			  MyMonitor childMonitor = child.NewMonitor( typeof( MyMonitor ) );
			  childMonitor.AVoid();
			  verify( parentListener, times( 2 ) ).aVoid();
			  verify( childListener, times( 1 ) ).aVoid();
		 }
	}

}