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
namespace Org.Neo4j.Logging
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.sameInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class DuplicatingLogProviderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnSameLoggerForSameClass()
		 internal virtual void ShouldReturnSameLoggerForSameClass()
		 {
			  // Given
			  DuplicatingLogProvider logProvider = new DuplicatingLogProvider();

			  // Then
			  DuplicatingLog log = logProvider.getLog( this.GetType() );
			  assertThat( logProvider.GetLog( typeof( DuplicatingLogProviderTest ) ), sameInstance( log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnSameLoggerForSameContext()
		 internal virtual void ShouldReturnSameLoggerForSameContext()
		 {
			  // Given
			  DuplicatingLogProvider logProvider = new DuplicatingLogProvider();

			  // Then
			  DuplicatingLog log = logProvider.GetLog( "test context" );
			  assertThat( logProvider.GetLog( "test context" ), sameInstance( log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRemoveLogProviderFromDuplication()
		 internal virtual void ShouldRemoveLogProviderFromDuplication()
		 {
			  // Given
			  AssertableLogProvider logProvider1 = new AssertableLogProvider();
			  AssertableLogProvider logProvider2 = new AssertableLogProvider();

			  DuplicatingLogProvider logProvider = new DuplicatingLogProvider( logProvider1, logProvider2 );

			  // When
			  Log log = logProvider.getLog( this.GetType() );
			  log.Info( "When the going gets weird" );
			  assertTrue( logProvider.Remove( logProvider1 ) );
			  log.Info( "The weird turn pro" );

			  // Then
			  logProvider1.AssertExactly( AssertableLogProvider.inLog( this.GetType() ).Info("When the going gets weird") );
			  logProvider2.AssertExactly( AssertableLogProvider.inLog( this.GetType() ).Info("When the going gets weird"), AssertableLogProvider.inLog(this.GetType()).Info("The weird turn pro") );
		 }
	}

}