using System;
using System.Collections.Generic;

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
namespace Neo4Net.Ext.Udc.impl
{
	using HttpEntity = org.apache.http.HttpEntity;
	using HttpHost = org.apache.http.HttpHost;
	using HttpStatus = org.apache.http.HttpStatus;
	using CloseableHttpResponse = org.apache.http.client.methods.CloseableHttpResponse;
	using HttpGet = org.apache.http.client.methods.HttpGet;
	using DefaultHttpClient = org.apache.http.impl.client.DefaultHttpClient;
	using LocalServerTestBase = org.apache.http.localserver.LocalServerTestBase;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using HostnamePort = Neo4Net.Helpers.HostnamePort;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.notNullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.ext.udc.UdcConstants.ID;

	/// <summary>
	/// Unit tests for the UDC statistics pinger.
	/// </summary>
	public class PingerTest : LocalServerTestBase
	{
		 private readonly string _expectedKernelVersion = "1.0";
		 private readonly string _expectedStoreId = "CAFE";
		 private string _hostname = "localhost";
		 private string _serverUrl;

		 internal PingerHandler Handler;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before @Override public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public override void SetUp()
		 {
			  base.SetUp();
			  Handler = new PingerHandler();
			  this.serverBootstrap.registerHandler( "/*", Handler );
			  HttpHost target = start();
			  _hostname = target.HostName;
			  _serverUrl = "http://" + _hostname + ":" + target.Port;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @After public void shutDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public override void ShutDown()
		 {
			  if ( httpclient != null )
			  {
					httpclient.close();
			  }
			  if ( server != null )
			  {
					server.shutdown( 0, TimeUnit.MILLISECONDS );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondToHttpClientGet() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondToHttpClientGet()
		 {
			  using ( DefaultHttpClient httpclient = new DefaultHttpClient() )
			  {
					HttpGet httpget = new HttpGet( _serverUrl + "/?id=storeId+v=kernelVersion" );
					using ( CloseableHttpResponse response = httpclient.execute( httpget ) )
					{
						 HttpEntity entity = response.Entity;
						 if ( entity != null )
						 {
							  using ( Stream instream = entity.Content )
							  {
									sbyte[] tmp = new sbyte[2048];
									while ( ( instream.Read( tmp, 0, tmp.Length ) ) != -1 )
									{
									}
							  }
						 }
						 assertThat( response, notNullValue() );
						 assertThat( response.StatusLine.StatusCode, @is( HttpStatus.SC_OK ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPingServer() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPingServer()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.helpers.HostnamePort hostURL = new org.neo4j.helpers.HostnamePort(hostname, server.getLocalPort());
			  HostnamePort hostURL = new HostnamePort( _hostname, server.LocalPort );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<String,String> udcFields = new java.util.HashMap<>();
			  IDictionary<string, string> udcFields = new Dictionary<string, string>();
			  udcFields[ID] = _expectedStoreId;
			  udcFields[UdcConstants.VERSION] = _expectedKernelVersion;

			  Pinger p = new Pinger( hostURL, new TestUdcCollector( udcFields ) );
			  p.Ping();

			  IDictionary<string, string> actualQueryMap = Handler.QueryMap;
			  assertThat( actualQueryMap, notNullValue() );
			  assertThat( actualQueryMap[ID], @is( _expectedStoreId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludePingCountInURI() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIncludePingCountInURI()
		 {
			  const int expectedPingCount = 16;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.helpers.HostnamePort hostURL = new org.neo4j.helpers.HostnamePort(hostname, server.getLocalPort());
			  HostnamePort hostURL = new HostnamePort( _hostname, server.LocalPort );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<String,String> udcFields = new java.util.HashMap<>();
			  IDictionary<string, string> udcFields = new Dictionary<string, string>();

			  Pinger p = new Pinger( hostURL, new TestUdcCollector( udcFields ) );
			  for ( int i = 0; i < expectedPingCount; i++ )
			  {
					p.Ping();
			  }

			  assertThat( p.PingCount, @is( equalTo( expectedPingCount ) ) );

			  IDictionary<string, string> actualQueryMap = Handler.QueryMap;
			  assertThat( actualQueryMap[UdcConstants.PING], @is( Convert.ToString( expectedPingCount ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void normalPingSequenceShouldBeOneThenTwoThenThreeEtc() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NormalPingSequenceShouldBeOneThenTwoThenThreeEtc()
		 {
			  int[] expectedSequence = new int[] { 1, 2, 3, 4 };
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.helpers.HostnamePort hostURL = new org.neo4j.helpers.HostnamePort(hostname, server.getLocalPort());
			  HostnamePort hostURL = new HostnamePort( _hostname, server.LocalPort );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<String,String> udcFields = new java.util.HashMap<>();
			  IDictionary<string, string> udcFields = new Dictionary<string, string>();

			  Pinger p = new Pinger( hostURL, new TestUdcCollector( udcFields ) );
			  foreach ( int s in expectedSequence )
			  {
					p.Ping();
					int count = int.Parse( Handler.QueryMap[UdcConstants.PING] );
					assertEquals( s, count );
			  }
		 }

		 internal class TestUdcCollector : UdcInformationCollector
		 {
			  internal readonly IDictionary<string, string> Params;

			  internal TestUdcCollector( IDictionary<string, string> @params )
			  {
					this.Params = @params;
			  }

			  public virtual IDictionary<string, string> UdcParams
			  {
				  get
				  {
						return Params;
				  }
			  }

			  public virtual string StoreId
			  {
				  get
				  {
						return UdcParams[ID];
				  }
			  }

		 }
	}

}