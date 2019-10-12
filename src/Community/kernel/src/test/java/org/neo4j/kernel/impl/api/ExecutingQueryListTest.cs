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
namespace Neo4Net.Kernel.Impl.Api
{
	using Test = org.junit.Test;


	using PageCursorTracer = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracer;
	using ExecutingQuery = Neo4Net.Kernel.api.query.ExecutingQuery;
	using ClientConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.ClientConnectionInfo;
	using CpuClock = Neo4Net.Resources.CpuClock;
	using HeapAllocation = Neo4Net.Resources.HeapAllocation;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_MAP;

	public class ExecutingQueryListTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removingTheLastQueryReturnsAnEmptyList()
		 public virtual void RemovingTheLastQueryReturnsAnEmptyList()
		 {
			  // Given
			  ExecutingQuery aQuery = CreateExecutingQuery( 1, "query" );
			  ExecutingQueryList list = ExecutingQueryList.EMPTY.push( aQuery );

			  // When
			  ExecutingQueryList result = list.Remove( aQuery );

			  // Then
			  assertThat( result, equalTo( ExecutingQueryList.EMPTY ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotChangeAListWhenRemovingAQueryThatIsNotInTheList()
		 public virtual void ShouldNotChangeAListWhenRemovingAQueryThatIsNotInTheList()
		 {
			  // given
			  ExecutingQuery query1 = CreateExecutingQuery( 1, "query1" );
			  ExecutingQuery query2 = CreateExecutingQuery( 2, "query2" );
			  ExecutingQueryList list = ExecutingQueryList.EMPTY.push( query1 );

			  // when
			  ExecutingQueryList result = list.Remove( query2 );

			  // then
			  assertThat( result, equalTo( list ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addingQueriesKeepsInsertOrder()
		 public virtual void AddingQueriesKeepsInsertOrder()
		 {
			  // Given
			  ExecutingQuery query1 = CreateExecutingQuery( 1, "query1" );
			  ExecutingQuery query2 = CreateExecutingQuery( 2, "query2" );
			  ExecutingQuery query3 = CreateExecutingQuery( 3, "query3" );
			  ExecutingQuery query4 = CreateExecutingQuery( 4, "query4" );
			  ExecutingQuery query5 = CreateExecutingQuery( 5, "query5" );

			  ExecutingQueryList list = ExecutingQueryList.EMPTY.push( query1 ).push( query2 ).push( query3 ).push( query4 ).push( query5 );

			  // When
			  IList<ExecutingQuery> result = list.Queries().collect(Collectors.toList());

			  // Then
			  assertThat( result, equalTo( asList( query5, query4, query3, query2, query1 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removingQueryInTheMiddleKeepsOrder()
		 public virtual void RemovingQueryInTheMiddleKeepsOrder()
		 {
			  // Given
			  ExecutingQuery query1 = CreateExecutingQuery( 1, "query1" );
			  ExecutingQuery query2 = CreateExecutingQuery( 2, "query2" );
			  ExecutingQuery query3 = CreateExecutingQuery( 3, "query3" );
			  ExecutingQuery query4 = CreateExecutingQuery( 4, "query4" );
			  ExecutingQuery query5 = CreateExecutingQuery( 5, "query5" );

			  ExecutingQueryList list = ExecutingQueryList.EMPTY.push( query1 ).push( query2 ).push( query3 ).push( query4 ).push( query5 );

			  // When
			  IList<ExecutingQuery> result = list.Remove( query3 ).queries().collect(Collectors.toList());

			  // Then
			  assertThat( result, equalTo( asList( query5, query4, query2, query1 ) ) );
		 }

		 private ExecutingQuery CreateExecutingQuery( int queryId, string query )
		 {
			  return new ExecutingQuery( queryId, ClientConnectionInfo.EMBEDDED_CONNECTION, "me", query, EMPTY_MAP, Collections.emptyMap(), () => 0, PageCursorTracer.NULL, Thread.CurrentThread.Id, Thread.CurrentThread.Name, Clocks.nanoClock(), CpuClock.CPU_CLOCK, HeapAllocation.HEAP_ALLOCATION );
		 }
	}

}