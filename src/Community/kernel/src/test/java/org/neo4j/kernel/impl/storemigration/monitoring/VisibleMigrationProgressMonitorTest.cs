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
namespace Neo4Net.Kernel.impl.storemigration.monitoring
{
	using Test = org.junit.Test;

	using ProgressReporter = Neo4Net.Kernel.impl.util.monitoring.ProgressReporter;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using Log = Neo4Net.Logging.Log;
	using FakeClock = Neo4Net.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;

	public class VisibleMigrationProgressMonitorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportAllPercentageSteps()
		 public virtual void ShouldReportAllPercentageSteps()
		 {
			  // GIVEN
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  Log log = logProvider.getLog( this.GetType() );
			  VisibleMigrationProgressMonitor monitor = new VisibleMigrationProgressMonitor( log );
			  monitor.Started( 1 );

			  // WHEN
			  MonitorSection( monitor, "First", 100, 40, 25, 23, 10, 50 );
			  monitor.Completed();

			  // THEN
			  VerifySectionReportedCorrectly( logProvider );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void progressNeverReportMoreThenHundredPercent()
		 public virtual void ProgressNeverReportMoreThenHundredPercent()
		 {
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  Log log = logProvider.getLog( this.GetType() );
			  VisibleMigrationProgressMonitor monitor = new VisibleMigrationProgressMonitor( log );

			  monitor.Started( 1 );
			  MonitorSection( monitor, "First", 100, 1, 10, 99, 170 );
			  monitor.Completed();

			  VerifySectionReportedCorrectly( logProvider );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeDurationInCompletionMessage()
		 public virtual void ShouldIncludeDurationInCompletionMessage()
		 {
			  // given
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  Log log = logProvider.getLog( this.GetType() );
			  FakeClock clock = new FakeClock();
			  VisibleMigrationProgressMonitor monitor = new VisibleMigrationProgressMonitor( log, clock );

			  // when
			  monitor.Started( 1 );
			  clock.Forward( 1500, TimeUnit.MILLISECONDS );
			  monitor.Completed();

			  // then
			  logProvider.FormattedMessageMatcher().assertContains("took 1s 500ms");
		 }

		 private void VerifySectionReportedCorrectly( AssertableLogProvider logProvider )
		 {
			  logProvider.FormattedMessageMatcher().assertContains(VisibleMigrationProgressMonitor.MESSAGE_STARTED);
			  for ( int i = 10; i <= 100; i += 10 )
			  {
					logProvider.FormattedMessageMatcher().assertContains(i + "%");
			  }
			  logProvider.AssertNone( AssertableLogProvider.inLog( typeof( VisibleMigrationProgressMonitor ) ).info( containsString( "110%" ) ) );
			  logProvider.FormattedMessageMatcher().assertContains(VisibleMigrationProgressMonitor.MESSAGE_COMPLETED);
		 }

		 private void MonitorSection( VisibleMigrationProgressMonitor monitor, string name, int max, params int[] steps )
		 {
			  ProgressReporter progressReporter = monitor.StartSection( name );
			  progressReporter.Start( max );
			  foreach ( int step in steps )
			  {
					progressReporter.Progress( step );
			  }
			  progressReporter.Completed();
		 }
	}

}