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
	using DigestUtils = org.apache.commons.codec.digest.DigestUtils;
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.fail;

	internal class TimeZonesTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void weSupportAllJavaZoneIds()
		 internal virtual void WeSupportAllJavaZoneIds()
		 {
			  ZoneId.AvailableZoneIds.forEach(s =>
			  {
				short num = TimeZones.map( s );
				assertThat( "Our time zone table does not have a mapping for " + s, num, greaterThanOrEqualTo( ( short ) 0 ) );

				string nameFromTable = TimeZones.Map( num );
				if ( !s.Equals( nameFromTable ) )
				{
					 // The test is running on an older Java version and `s` has been removed since, thus it points to a different zone now.
					 // That zone should point to itself, however.
					 assertThat( "Our time zone table has inconsistent mapping for " + nameFromTable, TimeZones.Map( TimeZones.Map( nameFromTable ) ), equalTo( nameFromTable ) );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void weSupportDeletedZoneIdEastSaskatchewan()
		 internal virtual void WeSupportDeletedZoneIdEastSaskatchewan()
		 {
			  try
			  {
					short eastSaskatchewan = TimeZones.Map( "Canada/East-Saskatchewan" );
					assertThat( "Our time zone table does not remap Canada/East-Saskatchewan to Canada/Saskatchewan", TimeZones.Map( eastSaskatchewan ), equalTo( "Canada/Saskatchewan" ) );
			  }
			  catch ( System.ArgumentException )
			  {
					fail( "Our time zone table does not support Canada/East-Saskatchewan" );
			  }
		 }

		 /// <summary>
		 /// If this test fails, you have changed something in TZIDS. This is fine, as long as you only append lines to the end,
		 /// or add a mapping to a deleted timezone. You are not allowed to change the order of lines or remove a line.
		 /// p>
		 /// If your changes were legit, please change the expected byte[] below.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void tzidsOrderMustNotChange() throws java.net.URISyntaxException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TzidsOrderMustNotChange()
		 {
			  Path path = Paths.get( typeof( TimeZones ).getResource( "/TZIDS" ).toURI() );
			  sbyte[] timeZonesInfo = Files.readAllBytes( path );
			  sbyte[] timeZonesHash = DigestUtils.sha256( timeZonesInfo );
			  assertThat( timeZonesHash, equalTo( new sbyte[]{ 27, ( sbyte ) - 102, 116, 117, ( sbyte ) - 108, ( sbyte ) - 114, 65, 81, 88, 52, 25, 112, ( sbyte ) - 67, 3, ( sbyte ) - 99, 69, ( sbyte ) - 26, 100, ( sbyte ) - 38, ( sbyte ) - 2, 29, ( sbyte ) - 41, 60, ( sbyte ) - 85, ( sbyte ) - 58, 102, ( sbyte ) - 101, ( sbyte ) - 122, ( sbyte ) - 40, ( sbyte ) - 66, 49, ( sbyte ) - 65 } ) );
		 }
	}

}