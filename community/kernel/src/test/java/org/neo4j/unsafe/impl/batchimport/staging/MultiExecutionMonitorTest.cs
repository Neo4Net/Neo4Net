﻿/*
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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.staging
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using CleanupRule = Org.Neo4j.Test.rule.CleanupRule;
	using Clocks = Org.Neo4j.Time.Clocks;
	using FakeClock = Org.Neo4j.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class MultiExecutionMonitorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.CleanupRule cleanup = new org.neo4j.test.rule.CleanupRule();
		 public readonly CleanupRule Cleanup = new CleanupRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCheckMultipleMonitors()
		 public virtual void ShouldCheckMultipleMonitors()
		 {
			  // GIVEN
			  FakeClock clock = Clocks.fakeClock();
			  TestableMonitor first = new TestableMonitor( clock, 100, MILLISECONDS, "first" );
			  TestableMonitor other = new TestableMonitor( clock, 250, MILLISECONDS, "other" );
			  MultiExecutionMonitor multiMonitor = new MultiExecutionMonitor( clock, first, other );

			  // WHEN/THEN
			  clock.Forward( 110, MILLISECONDS );
			  ExpectCallsToCheck( multiMonitor, first, 1, other, 0 );
			  clock.Forward( 100, MILLISECONDS );
			  ExpectCallsToCheck( multiMonitor, first, 2, other, 0 );
			  clock.Forward( 45, MILLISECONDS );
			  ExpectCallsToCheck( multiMonitor, first, 2, other, 1 );
		 }

		 private void ExpectCallsToCheck( ExecutionMonitor multiMonitor, params object[] alternatingMonitorAndCount )
		 {
			  multiMonitor.Check( null ); // null, knowing that our monitors in this test doesn't use 'em
			  for ( int i = 0; i < alternatingMonitorAndCount.Length; i++ )
			  {
					TestableMonitor monitor = ( TestableMonitor ) alternatingMonitorAndCount[i++];
					int count = ( int? ) alternatingMonitorAndCount[i].Value;
					assertThat( monitor.TimesPolled, lessThanOrEqualTo( count ) );
					if ( monitor.TimesPolled < count )
					{
						 fail( "Polls didn't occur, expected " + Arrays.ToString( alternatingMonitorAndCount ) );
					}
			  }
		 }

		 private class TestableMonitor : ExecutionMonitor_Adapter
		 {
			  internal int TimesPolled;
			  internal readonly string Name;

			  internal TestableMonitor( Clock clock, long interval, TimeUnit unit, string name ) : base( clock, interval, unit )
			  {
					this.Name = name;
			  }

			  public override void Check( StageExecution execution )
			  {
					TimesPolled++;
			  }

			  public override string ToString()
			  {
					return "[" + Name + ":" + TimesPolled + "]";
			  }
		 }
	}

}