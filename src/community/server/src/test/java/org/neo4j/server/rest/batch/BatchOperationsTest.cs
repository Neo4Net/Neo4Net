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
namespace Neo4Net.Server.rest.batch
{
	using Test = org.junit.Test;


	using InternalJettyServletRequest = Neo4Net.Server.rest.web.InternalJettyServletRequest;
	using RequestData = Neo4Net.Server.rest.web.InternalJettyServletRequest.RequestData;
	using InternalJettyServletResponse = Neo4Net.Server.rest.web.InternalJettyServletResponse;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.assertion.Assert.assertException;

	public class BatchOperationsTest
	{

		 private readonly BatchOperations ops = new BatchOperationsAnonymousInnerClass();

		 private class BatchOperationsAnonymousInnerClass : BatchOperations
		 {
			 public BatchOperationsAnonymousInnerClass() : base(null)
			 {
			 }

			 protected internal override void invoke( string method, string path, string body, int? id, URI targetUri, InternalJettyServletRequest req, InternalJettyServletResponse res )
			 {
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testReplaceLocations()
		 public virtual void TestReplaceLocations()
		 {
			  IDictionary<int, string> map = new Dictionary<int, string>();
			  map[100] = "bar";
			  assertEquals( "foo", ops.replaceLocationPlaceholders( "foo", map ) );
			  assertEquals( "foo bar", ops.replaceLocationPlaceholders( "foo {100}", map ) );
			  assertEquals( "bar foo bar", ops.replaceLocationPlaceholders( "{100} foo {100}", map ) );
			  assertEquals( "bar bar foo bar bar", ops.replaceLocationPlaceholders( "bar {100} foo {100} bar", map ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemeInInternalJettyServletRequestForHttp() throws java.io.UnsupportedEncodingException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestSchemeInInternalJettyServletRequestForHttp()
		 {
			  // when
			  InternalJettyServletRequest req = new InternalJettyServletRequest( "POST", "http://localhost:7473/db/data/node", "{'name':'node1'}", new InternalJettyServletResponse(), mock(typeof(InternalJettyServletRequest.RequestData)) );

			  // then
			  assertEquals( "http",req.Scheme );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemeInInternalJettyServletRequestForHttps() throws java.io.UnsupportedEncodingException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestSchemeInInternalJettyServletRequestForHttps()
		 {
			  // when
			  InternalJettyServletRequest req = new InternalJettyServletRequest( "POST", "https://localhost:7473/db/data/node", "{'name':'node1'}", new InternalJettyServletResponse(), mock(typeof(InternalJettyServletRequest.RequestData)) );

			  // then
			  assertEquals( "https",req.Scheme );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldForwardMetadataFromRequestData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldForwardMetadataFromRequestData()
		 {
			  // Given
			  InternalJettyServletRequest.RequestData mock = new InternalJettyServletRequest.RequestData( "127.0.0.1", true, 1, "TheLocalName", "129.0.0.1", 2, "authorization/auth" );

			  InternalJettyServletRequest req = new InternalJettyServletRequest( "POST", "https://localhost:7473/db/data/node", "", new InternalJettyServletResponse(), mock );

			  // When & then
			  assertEquals( "127.0.0.1", req.RemoteAddr );
			  assertException( req.getRemoteHost, typeof( System.NotSupportedException ), "Remote host-name lookup might prove expensive, this should be explicitly considered." );
			  assertTrue( req.Secure );
			  assertEquals( 1, req.RemotePort );
			  assertEquals( "TheLocalName", req.LocalName );
			  assertEquals( "129.0.0.1", req.LocalAddr );
			  assertEquals( 2, req.LocalPort );
			  assertEquals( "authorization/auth", req.AuthType );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreUnknownAndUnparseablePlaceholders()
		 public virtual void ShouldIgnoreUnknownAndUnparseablePlaceholders()
		 {
			  // When/then
			  assertEquals( "foo {00000000010001010001001100111000100101010111001101110111}", ops.replaceLocationPlaceholders( "foo {00000000010001010001001100111000100101010111001101110111}", Collections.emptyMap() ) );
			  assertEquals( "foo {2147483648}", ops.replaceLocationPlaceholders( "foo {2147483648}", Collections.emptyMap() ) );
		 }
	}

}