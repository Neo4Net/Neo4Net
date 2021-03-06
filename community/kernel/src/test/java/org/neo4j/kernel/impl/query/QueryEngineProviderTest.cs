﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.query
{
	using Test = org.junit.Test;

	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class QueryEngineProviderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUsePickTheEngineWithLowestPriority()
		 public virtual void ShouldUsePickTheEngineWithLowestPriority()
		 {
			  // Given
			  QueryEngineProvider provider1 = mock( typeof( QueryEngineProvider ) );
			  QueryEngineProvider provider2 = mock( typeof( QueryEngineProvider ) );
			  when( provider1.EnginePriority() ).thenReturn(1);
			  when( provider2.EnginePriority() ).thenReturn(2);
			  Dependencies deps = new Dependencies();
			  GraphDatabaseAPI graphAPI = mock( typeof( GraphDatabaseAPI ) );
			  QueryExecutionEngine executionEngine = mock( typeof( QueryExecutionEngine ) );
			  QueryExecutionEngine executionEngine2 = mock( typeof( QueryExecutionEngine ) );
			  when( provider1.CreateEngine( any(), any() ) ).thenReturn(executionEngine);
			  when( provider2.CreateEngine( any(), any() ) ).thenReturn(executionEngine2);

			  // When
			  IEnumerable<QueryEngineProvider> providers = Iterables.asIterable( provider1, provider2 );
			  QueryExecutionEngine engine = QueryEngineProvider.Initialize( deps, graphAPI, providers );

			  // Then
			  assertSame( executionEngine, engine );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPickTheOneAndOnlyQueryEngineAvailable()
		 public virtual void ShouldPickTheOneAndOnlyQueryEngineAvailable()
		 {
			  // Given
			  QueryEngineProvider provider = mock( typeof( QueryEngineProvider ) );
			  when( provider.EnginePriority() ).thenReturn(1);
			  Dependencies deps = new Dependencies();
			  GraphDatabaseAPI graphAPI = mock( typeof( GraphDatabaseAPI ) );
			  QueryExecutionEngine executionEngine = mock( typeof( QueryExecutionEngine ) );
			  when( provider.CreateEngine( any(), any() ) ).thenReturn(executionEngine);

			  // When
			  IEnumerable<QueryEngineProvider> providers = Iterables.asIterable( provider );
			  QueryExecutionEngine engine = QueryEngineProvider.Initialize( deps, graphAPI, providers );

			  // Then
			  assertSame( executionEngine, engine );
		 }
	}

}