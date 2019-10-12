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
namespace Org.Neo4j.Server.rest
{

	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;

	using Documented = Org.Neo4j.Kernel.Impl.Annotations.Documented;
	using FunctionalTestHelper = Org.Neo4j.Server.helpers.FunctionalTestHelper;
	using GraphDbHelper = Org.Neo4j.Server.rest.domain.GraphDbHelper;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.allOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class DatabaseMetadataServiceIT : AbstractRestFunctionalTestBase
	{
		 private static FunctionalTestHelper _functionalTestHelper;
		 private static GraphDbHelper _helper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setupServer()
		 public static void SetupServer()
		 {
			  _functionalTestHelper = new FunctionalTestHelper( Server() );
			  _helper = _functionalTestHelper.GraphDbHelper;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Get relationship types.") @Test public void shouldReturn200OnGet()
		 [Documented("Get relationship types.")]
		 public virtual void ShouldReturn200OnGet()
		 {
			  _helper.createRelationship( "KNOWS" );
			  _helper.createRelationship( "LOVES" );

			  string result = GenConflict.get().expectedStatus(200).get(_functionalTestHelper.dataUri() + "relationship/types").entity();
			  assertThat( result, allOf( containsString( "KNOWS" ), containsString( "LOVES" ) ) );
		 }
	}

}