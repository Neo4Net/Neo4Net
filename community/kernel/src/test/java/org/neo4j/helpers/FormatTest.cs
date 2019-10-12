using System;

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
namespace Org.Neo4j.Helpers
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Format.duration;

	internal class FormatTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDisplayBytes()
		 internal virtual void ShouldDisplayBytes()
		 {
			  // when
			  string format = Format.Bytes( 123 );

			  // then
			  assertTrue( format.Contains( 123.ToString() ) );
			  assertTrue( format.EndsWith( " B", StringComparison.Ordinal ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDisplayKiloBytes()
		 internal virtual void ShouldDisplayKiloBytes()
		 {
			  // when
			  string format = Format.Bytes( 1_234 );

			  // then
			  assertTrue( format.StartsWith( "1", StringComparison.Ordinal ) );
			  assertTrue( format.EndsWith( " kB", StringComparison.Ordinal ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDisplayMegaBytes()
		 internal virtual void ShouldDisplayMegaBytes()
		 {
			  // when
			  string format = Format.Bytes( 1_234_567 );

			  // then
			  assertTrue( format.StartsWith( "1", StringComparison.Ordinal ) );
			  assertTrue( format.EndsWith( " MB", StringComparison.Ordinal ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDisplayGigaBytes()
		 internal virtual void ShouldDisplayGigaBytes()
		 {
			  // when
			  string format = Format.Bytes( 1_234_567_890 );

			  // then
			  assertTrue( format.StartsWith( "1", StringComparison.Ordinal ) );
			  assertTrue( format.EndsWith( " GB", StringComparison.Ordinal ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDisplayPlainCount()
		 internal virtual void ShouldDisplayPlainCount()
		 {
			  // when
			  string format = Format.Count( 10 );

			  // then
			  assertTrue( format.StartsWith( "10", StringComparison.Ordinal ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDisplayThousandCount()
		 internal virtual void ShouldDisplayThousandCount()
		 {
			  // when
			  string format = Format.Count( 2_000 );

			  // then
			  assertTrue( format.StartsWith( "2", StringComparison.Ordinal ) );
			  assertTrue( format.EndsWith( "k", StringComparison.Ordinal ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDisplayMillionCount()
		 internal virtual void ShouldDisplayMillionCount()
		 {
			  // when
			  string format = Format.Count( 2_000_000 );

			  // then
			  assertTrue( format.StartsWith( "2", StringComparison.Ordinal ) );
			  assertTrue( format.EndsWith( "M", StringComparison.Ordinal ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDisplayBillionCount()
		 internal virtual void ShouldDisplayBillionCount()
		 {
			  // when
			  string format = Format.Count( 2_000_000_000 );

			  // then
			  assertTrue( format.StartsWith( "2", StringComparison.Ordinal ) );
			  assertTrue( format.EndsWith( "G", StringComparison.Ordinal ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDisplayTrillionCount()
		 internal virtual void ShouldDisplayTrillionCount()
		 {
			  // when
			  string format = Format.Count( 4_000_000_000_000L );

			  // then
			  assertTrue( format.StartsWith( "4", StringComparison.Ordinal ) );
			  assertTrue( format.EndsWith( "T", StringComparison.Ordinal ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void displayDuration()
		 internal virtual void DisplayDuration()
		 {
			  assertThat( duration( MINUTES.toMillis( 1 ) + SECONDS.toMillis( 2 ) ), @is( "1m 2s" ) );
			  assertThat( duration( 42 ), @is( "42ms" ) );
			  assertThat( duration( 0 ), @is( "0ms" ) );
		 }
	}

}