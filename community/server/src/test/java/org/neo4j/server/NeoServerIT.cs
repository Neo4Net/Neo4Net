﻿/*
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
namespace Org.Neo4j.Server
{

	using Test = org.junit.Test;

	using AbstractRestFunctionalTestBase = Org.Neo4j.Server.rest.AbstractRestFunctionalTestBase;
	using HTTP = Org.Neo4j.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class NeoServerIT : AbstractRestFunctionalTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void whenServerIsStartedItshouldStartASingleDatabase()
		 public virtual void WhenServerIsStartedItshouldStartASingleDatabase()
		 {
			  assertNotNull( Server().Database );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRedirectRootToBrowser()
		 public virtual void ShouldRedirectRootToBrowser()
		 {
			  assertFalse( Server().baseUri().ToString().Contains("browser") );

			  HTTP.Response res = HTTP.withHeaders( HttpHeaders.ACCEPT, MediaType.TEXT_HTML ).GET( Server().baseUri().ToString() );
			  assertThat( res.Header( "Location" ), containsString( "browser" ) );
		 }
	}

}