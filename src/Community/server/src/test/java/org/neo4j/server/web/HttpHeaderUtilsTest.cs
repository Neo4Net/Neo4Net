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
namespace Neo4Net.Server.web
{
	using HttpHeaders = org.apache.http.HttpHeaders;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;

	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using Log = Neo4Net.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.web.HttpHeaderUtils.MAX_EXECUTION_TIME_HEADER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.web.HttpHeaderUtils.getTransactionTimeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.web.HttpHeaderUtils.isValidHttpHeaderName;

	public class HttpHeaderUtilsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.logging.AssertableLogProvider logProvider = new org.neo4j.logging.AssertableLogProvider(true);
		 public AssertableLogProvider LogProvider = new AssertableLogProvider( true );
		 private HttpServletRequest _request;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _request = Mockito.mock( typeof( HttpServletRequest ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void retrieveCustomTransactionTimeout()
		 public virtual void RetrieveCustomTransactionTimeout()
		 {
			  when( _request.getHeader( MAX_EXECUTION_TIME_HEADER ) ).thenReturn( "100" );
			  Log log = LogProvider.getLog( typeof( HttpServletRequest ) );
			  long transactionTimeout = getTransactionTimeout( _request, log );
			  assertEquals( "Transaction timeout should be retrieved.", 100, transactionTimeout );
			  LogProvider.assertNoLoggingOccurred();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void defaultValueWhenCustomTransactionTimeoutNotSpecified()
		 public virtual void DefaultValueWhenCustomTransactionTimeoutNotSpecified()
		 {
			  Log log = LogProvider.getLog( typeof( HttpServletRequest ) );
			  long transactionTimeout = getTransactionTimeout( _request, log );
			  assertEquals( "Transaction timeout not specified.", 0, transactionTimeout );
			  LogProvider.assertNoLoggingOccurred();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void defaultValueWhenCustomTransactionTimeoutNotANumber()
		 public virtual void DefaultValueWhenCustomTransactionTimeoutNotANumber()
		 {
			  when( _request.getHeader( MAX_EXECUTION_TIME_HEADER ) ).thenReturn( "aa" );
			  Log log = LogProvider.getLog( typeof( HttpServletRequest ) );
			  long transactionTimeout = getTransactionTimeout( _request, log );
			  assertEquals( "Transaction timeout not specified.", 0, transactionTimeout );
			  LogProvider.rawMessageMatcher().assertContains("Fail to parse `max-execution-time` " + "header with value: 'aa'. Should be a positive number.");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCheckHttpHeaders()
		 public virtual void ShouldCheckHttpHeaders()
		 {
			  assertFalse( isValidHttpHeaderName( null ) );
			  assertFalse( isValidHttpHeaderName( "" ) );
			  assertFalse( isValidHttpHeaderName( " " ) );
			  assertFalse( isValidHttpHeaderName( "      " ) );
			  assertFalse( isValidHttpHeaderName( " \r " ) );
			  assertFalse( isValidHttpHeaderName( " \r\n\t " ) );

			  assertTrue( isValidHttpHeaderName( HttpHeaders.ACCEPT ) );
			  assertTrue( isValidHttpHeaderName( HttpHeaders.ACCEPT_ENCODING ) );
			  assertTrue( isValidHttpHeaderName( HttpHeaders.AGE ) );
			  assertTrue( isValidHttpHeaderName( HttpHeaders.CONTENT_ENCODING ) );
			  assertTrue( isValidHttpHeaderName( HttpHeaders.EXPIRES ) );
			  assertTrue( isValidHttpHeaderName( HttpHeaders.IF_MATCH ) );
			  assertTrue( isValidHttpHeaderName( HttpHeaders.TRANSFER_ENCODING ) );
			  assertTrue( isValidHttpHeaderName( "Weird Header With Spaces" ) );

			  assertFalse( isValidHttpHeaderName( "My\nHeader" ) );
			  assertFalse( isValidHttpHeaderName( "Other\rStrange-Header" ) );
			  assertFalse( isValidHttpHeaderName( "Header-With-Tab\t" ) );
		 }
	}

}