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
namespace Neo4Net.Server.rest.repr
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	internal class RepresentationTestBase
	{
		 internal static readonly URI BaseUri = URI.create( "http://neo4j.org/" );
		 internal const string NODE_URI_PATTERN = "http://.*/node/[0-9]+";
		 internal const string RELATIONSHIP_URI_PATTERN = "http://.*/relationship/[0-9]+";

		 internal static void AssertUriMatches( string expectedRegex, ValueRepresentation uriRepr )
		 {
			  assertUriMatches( expectedRegex, RepresentationTestAccess.Serialize( uriRepr ) );
		 }

		 internal static void AssertUriMatches( string expectedRegex, string actualUri )
		 {
			  assertTrue( "expected <" + expectedRegex + "> got <" + actualUri + ">", actualUri.matches( expectedRegex ) );
		 }

		 internal static string UriPattern( string subPath )
		 {
			  return "http://.*/[0-9]+" + subPath;
		 }

		 private RepresentationTestBase()
		 {
			  // only static resource
		 }
	}

}