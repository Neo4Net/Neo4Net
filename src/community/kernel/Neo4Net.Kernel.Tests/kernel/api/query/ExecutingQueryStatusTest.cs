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
namespace Neo4Net.Kernel.Api.query
{
	using Test = org.junit.Test;


	using PageCursorTracer = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracer;
	using ActiveLock = Neo4Net.Kernel.impl.locking.ActiveLock;
	using HeapAllocation = Neo4Net.Resources.HeapAllocation;
	using ResourceType = Neo4Net.Kernel.Api.StorageEngine.@lock.ResourceType;
	using Neo4Net.Kernel.Api.StorageEngine.@lock;
	using FakeCpuClock = Neo4Net.Test.FakeCpuClock;
	using Clocks = Neo4Net.Time.Clocks;
	using FakeClock = Neo4Net.Time.FakeClock;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class ExecutingQueryStatusTest
	{
		 private readonly FakeClock _clock = Clocks.fakeClock( ZonedDateTime.parse( "2016-12-16T16:14:12+01:00" ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProduceSensibleMapRepresentationInRunningState()
		 public virtual void ShouldProduceSensibleMapRepresentationInRunningState()
		 {
			  // when
			  string status = SimpleState.Running().name();

			  // then
			  assertEquals( "running", status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProduceSensibleMapRepresentationInPlanningState()
		 public virtual void ShouldProduceSensibleMapRepresentationInPlanningState()
		 {
			  // when
			  string status = SimpleState.Planning().name();

			  // then
			  assertEquals( "planning", status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProduceSensibleMapRepresentationInWaitingOnLockState()
		 public virtual void ShouldProduceSensibleMapRepresentationInWaitingOnLockState()
		 {
			  // given
			  long[] resourceIds = new long[] { 17 };
			  WaitingOnLock status = new WaitingOnLock( Neo4Net.Kernel.impl.locking.ActiveLock_Fields.EXCLUSIVE_MODE, ResourceType( "NODE" ), resourceIds, _clock.nanos() );
			  _clock.forward( 17, TimeUnit.MILLISECONDS );

			  // when
			  IDictionary<string, object> statusMap = status.ToMap( _clock.nanos() );

			  // then
			  assertEquals( "waiting", status.Name() );
			  IDictionary<string, object> expected = new Dictionary<string, object>();
			  expected["waitTimeMillis"] = 17L;
			  expected["lockMode"] = "EXCLUSIVE";
			  expected["resourceType"] = "NODE";
			  expected["resourceIds"] = resourceIds;
			  assertEquals( expected, statusMap );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProduceSensibleMapRepresentationInWaitingOnQueryState()
		 public virtual void ShouldProduceSensibleMapRepresentationInWaitingOnQueryState()
		 {
			  // given
			  WaitingOnQuery status = new WaitingOnQuery( new ExecutingQuery( 12, null, null, "", VirtualValues.emptyMap(), null, () => 0, PageCursorTracer.NULL, Thread.CurrentThread.Id, Thread.CurrentThread.Name, _clock, FakeCpuClock.NOT_AVAILABLE, HeapAllocation.NOT_AVAILABLE ), _clock.nanos() );
			  _clock.forward( 1025, TimeUnit.MILLISECONDS );

			  // when
			  IDictionary<string, object> statusMap = status.ToMap( _clock.nanos() );

			  // then
			  assertEquals( "waiting", status.Name() );
			  IDictionary<string, object> expected = new Dictionary<string, object>();
			  expected["waitTimeMillis"] = 1025L;
			  expected["queryId"] = "query-12";
			  assertEquals( expected, statusMap );
		 }

		 internal static ResourceType ResourceType( string name )
		 {
			  return new ResourceTypeAnonymousInnerClass( name );
		 }

		 private class ResourceTypeAnonymousInnerClass : ResourceType
		 {
			 private string _name;

			 public ResourceTypeAnonymousInnerClass( string name )
			 {
				 this._name = name;
			 }

			 public override string ToString()
			 {
				  return name();
			 }

			 public int typeId()
			 {
				  throw new System.NotSupportedException( "not used" );
			 }

			 public WaitStrategy waitStrategy()
			 {
				  throw new System.NotSupportedException( "not used" );
			 }

			 public string name()
			 {
				  return _name;
			 }
		 }
	}

}