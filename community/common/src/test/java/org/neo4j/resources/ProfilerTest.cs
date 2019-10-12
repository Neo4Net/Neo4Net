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
namespace Org.Neo4j.Resources
{
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;

	internal class ProfilerTest
	{
		 private const int COMPUTE_WORK_MILLIS = 1000;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void profilerMustNoticeWhereTimeGoes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ProfilerMustNoticeWhereTimeGoes()
		 {
			  Profiler profiler = Profiler.profiler();
			  using ( Profiler_ProfiledInterval ignored = profiler.Profile() )
			  {
					ExpensiveComputation();
			  }
			  string output = GetProfilerOutput( profiler );
			  assertThat( output, containsString( "expensiveComputation" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void profilerMustLimitItselfToProfiledRegion() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ProfilerMustLimitItselfToProfiledRegion()
		 {
			  Profiler profiler = Profiler.profiler();
			  using ( Profiler_ProfiledInterval ignored = profiler.Profile() )
			  {
					ExpensiveComputation();
			  }
			  OtherIntenseWork();
			  string output = GetProfilerOutput( profiler );
			  assertThat( output, not( containsString( "otherIntensiveWork" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void profilerMustWaitUntilAfterAnInitialDelay() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ProfilerMustWaitUntilAfterAnInitialDelay()
		 {
			  Profiler profiler = Profiler.profiler();
			  long initialDelayNanos = TimeUnit.MILLISECONDS.toNanos( COMPUTE_WORK_MILLIS * 3 );
			  using ( Profiler_ProfiledInterval ignored = profiler.Profile( Thread.CurrentThread, initialDelayNanos ) )
			  {
					ExpensiveComputation();
			  }
			  string output = GetProfilerOutput( profiler );
			  // The initial delay is far longer than the profiled interval, so we should not get any samples.
			  assertThat( output, not( containsString( "expensiveComputation" ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String getProfilerOutput(Profiler profiler) throws InterruptedException
		 private string GetProfilerOutput( Profiler profiler )
		 {
			  profiler.Finish();
			  MemoryStream buffer = new MemoryStream();
			  using ( PrintStream @out = new PrintStream( buffer ) )
			  {
					profiler.PrintProfile( @out, "Profile" );
					@out.flush();
			  }
			  return buffer.ToString();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void expensiveComputation() throws InterruptedException
		 private void ExpensiveComputation()
		 {
			  Thread.Sleep( COMPUTE_WORK_MILLIS );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void otherIntenseWork() throws InterruptedException
		 private void OtherIntenseWork()
		 {
			  Thread.Sleep( COMPUTE_WORK_MILLIS );
		 }
	}

}