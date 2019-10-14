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
namespace Neo4Net.Logging
{
	using Test = org.junit.jupiter.api.Test;

	internal class DuplicatingLogTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldOutputToMultipleLogs()
		 internal virtual void ShouldOutputToMultipleLogs()
		 {
			  // Given
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  Log log1 = logProvider.GetLog( "log 1" );
			  Log log2 = logProvider.GetLog( "log 2" );

			  DuplicatingLog log = new DuplicatingLog( log1, log2 );

			  // When
			  log.Info( "When the going gets weird" );

			  // Then
			  logProvider.AssertExactly( AssertableLogProvider.InLog( "log 1" ).info( "When the going gets weird" ), AssertableLogProvider.InLog( "log 2" ).info( "When the going gets weird" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBulkOutputToMultipleLogs()
		 internal virtual void ShouldBulkOutputToMultipleLogs()
		 {
			  // Given
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  Log log1 = logProvider.GetLog( "log 1" );
			  Log log2 = logProvider.GetLog( "log 2" );

			  DuplicatingLog log = new DuplicatingLog( log1, log2 );

			  // When
			  log.Bulk( bulkLog => bulkLog.info( "When the going gets weird" ) );

			  // Then
			  logProvider.AssertExactly( AssertableLogProvider.InLog( "log 1" ).info( "When the going gets weird" ), AssertableLogProvider.InLog( "log 2" ).info( "When the going gets weird" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRemoveLogFromDuplication()
		 internal virtual void ShouldRemoveLogFromDuplication()
		 {
			  // Given
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  Log log1 = logProvider.GetLog( "log 1" );
			  Log log2 = logProvider.GetLog( "log 2" );

			  DuplicatingLog log = new DuplicatingLog( log1, log2 );

			  // When
			  log.Info( "When the going gets weird" );
			  log.Remove( log1 );
			  log.Info( "The weird turn pro" );

			  // Then
			  logProvider.AssertExactly( AssertableLogProvider.InLog( "log 1" ).info( "When the going gets weird" ), AssertableLogProvider.InLog( "log 2" ).info( "When the going gets weird" ), AssertableLogProvider.InLog( "log 2" ).info( "The weird turn pro" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRemoveLoggersFromDuplication()
		 internal virtual void ShouldRemoveLoggersFromDuplication()
		 {
			  // Given
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  Log log1 = logProvider.GetLog( "log 1" );
			  Log log2 = logProvider.GetLog( "log 2" );

			  DuplicatingLog log = new DuplicatingLog( log1, log2 );
			  Logger logger = log.InfoLogger();

			  // When
			  logger.Log( "When the going gets weird" );
			  log.Remove( log1 );
			  logger.Log( "The weird turn pro" );

			  // Then
			  logProvider.AssertExactly( AssertableLogProvider.InLog( "log 1" ).info( "When the going gets weird" ), AssertableLogProvider.InLog( "log 2" ).info( "When the going gets weird" ), AssertableLogProvider.InLog( "log 2" ).info( "The weird turn pro" ) );
		 }
	}

}