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
namespace Org.Neo4j.Io.pagecache.tracing.linear
{
	using Disabled = org.junit.jupiter.api.Disabled;
	using Test = org.junit.jupiter.api.Test;

	using RandomPageCacheTestHarness = Org.Neo4j.Io.pagecache.randomharness.RandomPageCacheTestHarness;

	public class LinearHistoryPageCacheTracerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Disabled("This test is only here for checking that the output from the LinearHistoryPageCacheTracer looks good. " + "This is pretty subjective and requires manual inspection. Therefore there's no point in running it " + "automatically in all our builds. Instead, run it as needed when you make changes to the printout code.") @Test void makeSomeTestOutput() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MakeSomeTestOutput()
		 {
			  LinearTracers linearTracers = LinearHistoryTracerFactory.PageCacheTracer();
			  using ( RandomPageCacheTestHarness harness = new RandomPageCacheTestHarness() )
			  {
					harness.UseAdversarialIO = true;
					harness.Tracer = linearTracers.PageCacheTracer;
					harness.CursorTracerSupplier = linearTracers.CursorTracerSupplier;
					harness.CommandCount = 100;
					harness.ConcurrencyLevel = 2;
					harness.Preparation = ( pageCache, fs, files ) => linearTracers.processHistory(hEvent =>
					{
					});

					harness.Run( 1, TimeUnit.MINUTES );

					linearTracers.PrintHistory( System.out );
			  }

		 }
	}

}