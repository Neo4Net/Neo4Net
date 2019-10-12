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
namespace Neo4Net.Consistency.checking.cache
{
	using AfterAll = org.junit.jupiter.api.AfterAll;
	using BeforeAll = org.junit.jupiter.api.BeforeAll;
	using Test = org.junit.jupiter.api.Test;


	using DefaultCounts = Neo4Net.Consistency.statistics.DefaultCounts;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class DefaultClientTest
	{

		 private static ExecutorService _executor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeAll static void setUp()
		 internal static void SetUp()
		 {
			  _executor = Executors.newSingleThreadExecutor();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterAll static void tearDown()
		 internal static void TearDown()
		 {
			  _executor.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkClientsIdBounds() throws java.util.concurrent.ExecutionException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CheckClientsIdBounds()
		 {
			  int threads = 2;
			  DefaultCounts counts = new DefaultCounts( threads );
			  DefaultCacheAccess cacheAccess = new DefaultCacheAccess( counts, threads );
			  cacheAccess.PrepareForProcessingOfSingleStore( 34 );

			  CacheAccess_Client client1 = cacheAccess.Client();
			  assertTrue( client1.WithinBounds( 0 ) );
			  assertTrue( client1.WithinBounds( 10 ) );
			  assertTrue( client1.WithinBounds( 33 ) );
			  assertFalse( client1.WithinBounds( 34 ) );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> secondClientIdChecks = executor.submit(() ->
			  Future<object> secondClientIdChecks = _executor.submit(() =>
			  {
				CacheAccess_Client client = cacheAccess.Client();
				assertFalse( client.WithinBounds( 5 ) );
				assertFalse( client.WithinBounds( 33 ) );
				assertTrue( client.WithinBounds( 34 ) );
				assertTrue( client.WithinBounds( 67 ) );
				assertFalse( client.WithinBounds( 68 ) );
			  });
			  secondClientIdChecks.get();
		 }
	}

}