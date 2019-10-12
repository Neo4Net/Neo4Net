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
namespace Org.Neo4j.Kernel.@internal
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;

	public class VersionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExposeCleanAndDetailedVersions()
		 public virtual void ShouldExposeCleanAndDetailedVersions()
		 {
			  assertThat( Version( "1.2.3-M01,abcdef012345" ).ReleaseVersion, equalTo( "1.2.3-M01" ) );
			  assertThat( Version( "1.2.3-M01,abcdef012345" ).getVersion(), equalTo("1.2.3-M01,abcdef012345") );
			  assertThat( Version( "1.2.3-M01,abcdef012345-dirty" ).getVersion(), equalTo("1.2.3-M01,abcdef012345-dirty") );

			  assertThat( Version( "1.2.3,abcdef012345" ).ReleaseVersion, equalTo( "1.2.3" ) );
			  assertThat( Version( "1.2.3,abcdef012345" ).getVersion(), equalTo("1.2.3,abcdef012345") );
			  assertThat( Version( "1.2.3,abcdef012345-dirty" ).getVersion(), equalTo("1.2.3,abcdef012345-dirty") );

			  assertThat( Version( "1.2.3-GA,abcdef012345" ).ReleaseVersion, equalTo( "1.2.3-GA" ) );
			  assertThat( Version( "1.2.3-GA,abcdef012345" ).getVersion(), equalTo("1.2.3-GA,abcdef012345") );
			  assertThat( Version( "1.2.3-GA,abcdef012345-dirty" ).getVersion(), equalTo("1.2.3-GA,abcdef012345-dirty") );

			  assertThat( Version( "1.2.3M01,abcdef012345" ).ReleaseVersion, equalTo( "1.2.3M01" ) );
			  assertThat( Version( "1.2.3M01,abcdef012345" ).getVersion(), equalTo("1.2.3M01,abcdef012345") );
			  assertThat( Version( "1.2.3M01,abcdef012345-dirty" ).getVersion(), equalTo("1.2.3M01,abcdef012345-dirty") );

			  assertThat( Version( "1.2" ).ReleaseVersion, equalTo( "1.2" ) );
			  assertThat( Version( "1.2" ).getVersion(), equalTo("1.2") );

			  assertThat( Version( "0" ).ReleaseVersion, equalTo( "0" ) );
			  assertThat( Version( "0" ).getVersion(), equalTo("0") );
		 }

		 private Version Version( string version )
		 {
			  return new Version( "test-component", version );
		 }
	}

}