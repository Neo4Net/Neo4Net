using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Server
{
	using After = org.junit.After;
	using Test = org.junit.Test;


	using ServerSettings = Neo4Net.Server.configuration.ServerSettings;
	using ExclusiveServerTestBase = Neo4Net.Test.server.ExclusiveServerTestBase;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status_Transaction.TransactionNotFound;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.helpers.CommunityServerBuilder.serverOnRandomPorts;

	public class TransactionTimeoutIT : ExclusiveServerTestBase
	{
		 private CommunityNeoServer _server;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void stopTheServer()
		 public virtual void StopTheServer()
		 {
			  _server.stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHonorReallyLowSessionTimeout() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHonorReallyLowSessionTimeout()
		 {
			  // Given
			  _server = serverOnRandomPorts().withProperty(ServerSettings.transaction_idle_timeout.name(), "1").build();
			  _server.start();

			  string tx = HTTP.POST( TxURI(), asList(map("statement", "CREATE (n)")) ).location();

			  // When
			  Thread.Sleep( 1000 * 5 );
			  IDictionary<string, object> response = HTTP.POST( tx + "/commit" ).content();

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<java.util.Map<String, Object>> errors = (java.util.List<java.util.Map<String, Object>>) response.get("errors");
			  IList<IDictionary<string, object>> errors = ( IList<IDictionary<string, object>> ) response["errors"];
			  assertThat( errors[0]["code"], equalTo( TransactionNotFound.code().serialize() ) );
		 }

		 private string TxURI()
		 {
			  return _server.baseUri().ToString() + "db/data/transaction";
		 }

	}

}