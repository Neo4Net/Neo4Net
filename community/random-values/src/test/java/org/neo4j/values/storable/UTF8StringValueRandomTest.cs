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
namespace Org.Neo4j.Values.Storable
{
	using Disabled = org.junit.jupiter.api.Disabled;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using Inject = Org.Neo4j.Test.extension.Inject;
	using RandomExtension = Org.Neo4j.Test.extension.RandomExtension;
	using RandomRule = Org.Neo4j.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(RandomExtension.class) class UTF8StringValueRandomTest
	internal class UTF8StringValueRandomTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject RandomRule random;
		 internal RandomRule Random;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCompareToRandomAlphanumericString()
		 internal virtual void ShouldCompareToRandomAlphanumericString()
		 {
			  for ( int i = 0; i < 100; i++ )
			  {
					string string1 = Random.nextAlphaNumericString();
					string string2 = Random.nextAlphaNumericString();
					UTF8StringValueTest.AssertCompareTo( string1, string2 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCompareToAsciiString()
		 internal virtual void ShouldCompareToAsciiString()
		 {
			  for ( int i = 0; i < 100; i++ )
			  {
					string string1 = Random.nextAsciiString();
					string string2 = Random.nextAsciiString();
					UTF8StringValueTest.AssertCompareTo( string1, string2 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCompareBasicMultilingualPlaneString()
		 internal virtual void ShouldCompareBasicMultilingualPlaneString()
		 {
			  for ( int i = 0; i < 100; i++ )
			  {
					string string1 = Random.nextBasicMultilingualPlaneString();
					string string2 = Random.nextBasicMultilingualPlaneString();
					UTF8StringValueTest.AssertCompareTo( string1, string2 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Disabled("Comparing strings with higher than 16 bits code points is known to be inconsistent between StringValue and UTF8StringValue") @Test void shouldCompareToRandomString()
		 internal virtual void ShouldCompareToRandomString()
		 {
			  for ( int i = 0; i < 100; i++ )
			  {
					string string1 = Random.nextString();
					string string2 = Random.nextString();
					UTF8StringValueTest.AssertCompareTo( string1, string2 );
			  }
		 }
	}

}