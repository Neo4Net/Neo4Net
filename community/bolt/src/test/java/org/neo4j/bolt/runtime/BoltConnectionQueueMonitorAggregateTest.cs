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
namespace Org.Neo4j.Bolt.runtime
{
	using Test = org.junit.Test;


	using Job = Org.Neo4j.Bolt.v1.runtime.Job;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

	public class BoltConnectionQueueMonitorAggregateTest
	{
		 private BoltConnection _connection = mock( typeof( BoltConnection ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallEnqueuedOnSingleMonitor()
		 public virtual void ShouldCallEnqueuedOnSingleMonitor()
		 {
			  Job job = mock( typeof( Job ) );
			  BoltConnectionQueueMonitor monitor = mock( typeof( BoltConnectionQueueMonitor ) );
			  BoltConnectionQueueMonitorAggregate monitorAggregate = new BoltConnectionQueueMonitorAggregate( monitor );

			  monitorAggregate.Enqueued( _connection, job );

			  verify( monitor ).enqueued( _connection, job );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallDrainedOnSingleMonitor()
		 public virtual void ShouldCallDrainedOnSingleMonitor()
		 {
			  ICollection<Job> batch = new List<Job>();
			  BoltConnectionQueueMonitor monitor = mock( typeof( BoltConnectionQueueMonitor ) );
			  BoltConnectionQueueMonitorAggregate monitorAggregate = new BoltConnectionQueueMonitorAggregate( monitor );

			  monitorAggregate.Drained( _connection, batch );

			  verify( monitor ).drained( _connection, batch );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallEnqueuedOnEachMonitor()
		 public virtual void ShouldCallEnqueuedOnEachMonitor()
		 {
			  Job job = mock( typeof( Job ) );
			  BoltConnectionQueueMonitor monitor1 = mock( typeof( BoltConnectionQueueMonitor ) );
			  BoltConnectionQueueMonitor monitor2 = mock( typeof( BoltConnectionQueueMonitor ) );
			  BoltConnectionQueueMonitorAggregate monitorAggregate = new BoltConnectionQueueMonitorAggregate( monitor1, monitor2 );

			  monitorAggregate.Enqueued( _connection, job );

			  verify( monitor1 ).enqueued( _connection, job );
			  verify( monitor2 ).enqueued( _connection, job );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallDrainedOnEachMonitor()
		 public virtual void ShouldCallDrainedOnEachMonitor()
		 {
			  ICollection<Job> batch = new List<Job>();
			  BoltConnectionQueueMonitor monitor1 = mock( typeof( BoltConnectionQueueMonitor ) );
			  BoltConnectionQueueMonitor monitor2 = mock( typeof( BoltConnectionQueueMonitor ) );
			  BoltConnectionQueueMonitorAggregate monitorAggregate = new BoltConnectionQueueMonitorAggregate( monitor1, monitor2 );

			  monitorAggregate.Drained( _connection, batch );

			  verify( monitor1 ).drained( _connection, batch );
			  verify( monitor2 ).drained( _connection, batch );
		 }
	}

}