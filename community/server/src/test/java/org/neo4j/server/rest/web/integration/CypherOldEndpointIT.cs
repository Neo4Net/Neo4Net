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
namespace Org.Neo4j.Server.rest.web.integration
{
	using Test = org.junit.Test;

	using HTTP = Org.Neo4j.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.RawPayload.quotedJson;

	public class CypherOldEndpointIT : AbstractRestFunctionalTestBase
	{
		 private readonly HTTP.Builder _http = HTTP.withBaseUri( Server().baseUri() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void periodicCommitTest() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PeriodicCommitTest()
		 {
			  ServerTestUtils.withCSVFile(2, url =>
			  {
				// begin
				HTTP.Response begin = _http.POST( "db/data/cypher", quotedJson( "{ 'query': 'USING PERIODIC COMMIT 100 LOAD CSV FROM \\\"" + url + "\\\" AS line CREATE ();' }" ) );
				assertThat( begin.status(), equalTo(200) );
			  });
		 }
	}


}