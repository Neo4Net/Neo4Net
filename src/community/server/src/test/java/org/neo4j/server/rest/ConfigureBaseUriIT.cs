using System;

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
namespace Neo4Net.Server.rest
{
	using HttpResponse = org.apache.http.HttpResponse;
	using HttpClient = org.apache.http.client.HttpClient;
	using HttpGet = org.apache.http.client.methods.HttpGet;
	using DefaultHttpClient = org.apache.http.impl.client.DefaultHttpClient;
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;


	using FunctionalTestHelper = Neo4Net.Server.helpers.FunctionalTestHelper;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class ConfigureBaseUriIT : AbstractRestFunctionalTestBase
	{
		 private static FunctionalTestHelper _functionalTestHelper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setupServer()
		 public static void SetupServer()
		 {
			  _functionalTestHelper = new FunctionalTestHelper( Server() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldForwardHttpAndHost() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldForwardHttpAndHost()
		 {
			  URI rootUri = _functionalTestHelper.baseUri();

			  HttpClient httpclient = new DefaultHttpClient();
			  try
			  {
					HttpGet httpget = new HttpGet( rootUri );

					httpget.setHeader( "Accept", "application/json" );
					httpget.setHeader( "X-Forwarded-Host", "foobar.com" );
					httpget.setHeader( "X-Forwarded-Proto", "http" );

					HttpResponse response = httpclient.execute( httpget );

					string length = response.getHeaders( "CONTENT-LENGTH" )[0].Value;
					sbyte[] data = new sbyte[Convert.ToInt32( length )];
					response.Entity.Content.read( data );

					string responseEntityBody = StringHelper.NewString( data );

					assertTrue( responseEntityBody.Contains( "http://foobar.com" ) );
					assertFalse( responseEntityBody.Contains( "http://localhost" ) );
			  }
			  finally
			  {
					httpclient.ConnectionManager.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldForwardHttpsAndHost() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldForwardHttpsAndHost()
		 {
			  URI rootUri = _functionalTestHelper.baseUri();

			  HttpClient httpclient = new DefaultHttpClient();
			  try
			  {
					HttpGet httpget = new HttpGet( rootUri );

					httpget.setHeader( "Accept", "application/json" );
					httpget.setHeader( "X-Forwarded-Host", "foobar.com" );
					httpget.setHeader( "X-Forwarded-Proto", "https" );

					HttpResponse response = httpclient.execute( httpget );

					string length = response.getHeaders( "CONTENT-LENGTH" )[0].Value;
					sbyte[] data = new sbyte[Convert.ToInt32( length )];
					response.Entity.Content.read( data );

					string responseEntityBody = StringHelper.NewString( data );

					assertTrue( responseEntityBody.Contains( "https://foobar.com" ) );
					assertFalse( responseEntityBody.Contains( "https://localhost" ) );
			  }
			  finally
			  {
					httpclient.ConnectionManager.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldForwardHttpAndHostOnDifferentPort() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldForwardHttpAndHostOnDifferentPort()
		 {

			  URI rootUri = _functionalTestHelper.baseUri();

			  HttpClient httpclient = new DefaultHttpClient();
			  try
			  {
					HttpGet httpget = new HttpGet( rootUri );

					httpget.setHeader( "Accept", "application/json" );
					httpget.setHeader( "X-Forwarded-Host", "foobar.com:9999" );
					httpget.setHeader( "X-Forwarded-Proto", "http" );

					HttpResponse response = httpclient.execute( httpget );

					string length = response.getHeaders( "CONTENT-LENGTH" )[0].Value;
					sbyte[] data = new sbyte[Convert.ToInt32( length )];
					response.Entity.Content.read( data );

					string responseEntityBody = StringHelper.NewString( data );

					assertTrue( responseEntityBody.Contains( "http://foobar.com:9999" ) );
					assertFalse( responseEntityBody.Contains( "http://localhost" ) );
			  }
			  finally
			  {
					httpclient.ConnectionManager.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldForwardHttpAndFirstHost() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldForwardHttpAndFirstHost()
		 {
			  URI rootUri = _functionalTestHelper.baseUri();

			  HttpClient httpclient = new DefaultHttpClient();
			  try
			  {
					HttpGet httpget = new HttpGet( rootUri );

					httpget.setHeader( "Accept", "application/json" );
					httpget.setHeader( "X-Forwarded-Host", "foobar.com, bazbar.com" );
					httpget.setHeader( "X-Forwarded-Proto", "http" );

					HttpResponse response = httpclient.execute( httpget );

					string length = response.getHeaders( "CONTENT-LENGTH" )[0].Value;
					sbyte[] data = new sbyte[Convert.ToInt32( length )];
					response.Entity.Content.read( data );

					string responseEntityBody = StringHelper.NewString( data );

					assertTrue( responseEntityBody.Contains( "http://foobar.com" ) );
					assertFalse( responseEntityBody.Contains( "http://localhost" ) );
			  }
			  finally
			  {
					httpclient.ConnectionManager.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldForwardHttpsAndHostOnDifferentPort() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldForwardHttpsAndHostOnDifferentPort()
		 {
			  URI rootUri = _functionalTestHelper.baseUri();

			  HttpClient httpclient = new DefaultHttpClient();
			  try
			  {
					HttpGet httpget = new HttpGet( rootUri );

					httpget.setHeader( "Accept", "application/json" );
					httpget.setHeader( "X-Forwarded-Host", "foobar.com:9999" );
					httpget.setHeader( "X-Forwarded-Proto", "https" );

					HttpResponse response = httpclient.execute( httpget );

					string length = response.getHeaders( "CONTENT-LENGTH" )[0].Value;
					sbyte[] data = new sbyte[Convert.ToInt32( length )];
					response.Entity.Content.read( data );

					string responseEntityBody = StringHelper.NewString( data );

					assertTrue( responseEntityBody.Contains( "https://foobar.com:9999" ) );
					assertFalse( responseEntityBody.Contains( "https://localhost" ) );
			  }
			  finally
			  {
					httpclient.ConnectionManager.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseRequestUriWhenNoXForwardHeadersPresent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUseRequestUriWhenNoXForwardHeadersPresent()
		 {
			  URI rootUri = _functionalTestHelper.baseUri();

			  HttpClient httpclient = new DefaultHttpClient();
			  try
			  {
					HttpGet httpget = new HttpGet( rootUri );

					httpget.setHeader( "Accept", "application/json" );

					HttpResponse response = httpclient.execute( httpget );

					string length = response.getHeaders( "CONTENT-LENGTH" )[0].Value;
					sbyte[] data = new sbyte[Convert.ToInt32( length )];
					response.Entity.Content.read( data );

					string responseEntityBody = StringHelper.NewString( data );

					assertFalse( responseEntityBody.Contains( "https://foobar.com" ) );
					assertFalse( responseEntityBody.Contains( ":0" ) );
					assertTrue( responseEntityBody.Contains( "http://localhost" ) );
			  }
			  finally
			  {
					httpclient.ConnectionManager.shutdown();
			  }
		 }
	}

}