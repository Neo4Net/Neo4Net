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
namespace Neo4Net.Server.rest
{
	using HttpResponse = org.apache.http.HttpResponse;
	using HttpClient = org.apache.http.client.HttpClient;
	using HttpGet = org.apache.http.client.methods.HttpGet;
	using DefaultHttpClient = org.apache.http.impl.client.DefaultHttpClient;
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class DisableWADLIT : AbstractRestFunctionalTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void should404OnAnyUriEndingInWADL() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Should404OnAnyUriEndingInWADL()
		 {
			  URI nodeUri = new URI( Server().baseUri() + "db/data/application.wadl" );

			  HttpClient httpclient = new DefaultHttpClient();
			  try
			  {
					HttpGet httpget = new HttpGet( nodeUri );

					httpget.setHeader( "Accept", "*/*" );
					HttpResponse response = httpclient.execute( httpget );

					assertEquals( 404, response.StatusLine.StatusCode );

			  }
			  finally
			  {
					httpclient.ConnectionManager.shutdown();
			  }
		 }
	}

}