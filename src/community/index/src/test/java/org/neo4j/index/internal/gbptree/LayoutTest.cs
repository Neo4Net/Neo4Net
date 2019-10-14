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
namespace Neo4Net.Index.Internal.gbptree
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;

	internal class LayoutTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateDifferentIdentifierWithDifferentName()
		 internal virtual void ShouldCreateDifferentIdentifierWithDifferentName()
		 {
			  // GIVEN
			  string firstName = "one";
			  string secondName = "two";
			  int checksum = 123;

			  // WHEN
			  long firstIdentifier = Layout.namedIdentifier( firstName, checksum );
			  long secondIdentifier = Layout.namedIdentifier( secondName, checksum );

			  // THEN
			  assertNotEquals( firstIdentifier, secondIdentifier );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateDifferentIdentifierWithDifferentChecksums()
		 internal virtual void ShouldCreateDifferentIdentifierWithDifferentChecksums()
		 {
			  // GIVEN
			  string name = "name";
			  int firstChecksum = 123;
			  int secondChecksum = 456;

			  // WHEN
			  long firstIdentifier = Layout.namedIdentifier( name, firstChecksum );
			  long secondIdentifier = Layout.namedIdentifier( name, secondChecksum );

			  // THEN
			  assertNotEquals( firstIdentifier, secondIdentifier );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailOnTooLongName()
		 internal virtual void ShouldFailOnTooLongName()
		 {
			  assertThrows( typeof( System.ArgumentException ), () => Layout.namedIdentifier("too-long", 12) );
		 }
	}

}