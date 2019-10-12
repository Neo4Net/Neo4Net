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
namespace Neo4Net.Util.concurrent
{
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;

	internal class FuturesTest
	{
		 private static readonly ThreadStart _noop = () =>
		 {
		 };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void combinedFutureShouldGetResultsAfterAllComplete() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CombinedFutureShouldGetResultsAfterAllComplete()
		 {
			  FutureTask<string> task1 = new FutureTask<string>( _noop, "1" );
			  FutureTask<string> task2 = new FutureTask<string>( _noop, "2" );
			  FutureTask<string> task3 = new FutureTask<string>( _noop, "3" );

			  Future<IList<string>> combined = Futures.Combine( task1, task2, task3 );

			  assertThrows( typeof( TimeoutException ), () => combined.get(10, TimeUnit.MILLISECONDS) );

			  task3.run();
			  task2.run();

			  assertThrows( typeof( TimeoutException ), () => combined.get(10, TimeUnit.MILLISECONDS) );

			  task1.run();

			  IList<string> result = combined.get();
			  assertThat( result, contains( "1", "2", "3" ) );
		 }
	}

}