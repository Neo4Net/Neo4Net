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
namespace Neo4Net.Server.rest.transactional.integration
{
	using JsonNode = org.codehaus.jackson.JsonNode;
	using Test = org.junit.Test;

	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class TemporalTypeIT : AbstractRestFunctionalTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkWithDateTime() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWorkWithDateTime()
		 {
			  HTTP.Response response = RunQuery( "RETURN datetime({year: 1, month:10, day:2, timezone:\\\"+01:00\\\"})" );

			  assertEquals( 200, response.Status() );
			  AssertNoErrors( response );
			  JsonNode data = GetSingleData( response );
			  AssertTemporalEquals( data, "0001-10-02T00:00+01:00", "datetime" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkWithTime() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWorkWithTime()
		 {
			  HTTP.Response response = RunQuery( "RETURN time({hour: 23, minute: 19, second: 55, timezone:\\\"-07:00\\\"})" );

			  assertEquals( 200, response.Status() );
			  AssertNoErrors( response );
			  JsonNode data = GetSingleData( response );
			  AssertTemporalEquals( data, "23:19:55-07:00", "time" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkWithLocalDateTime() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWorkWithLocalDateTime()
		 {
			  HTTP.Response response = RunQuery( "RETURN localdatetime({year: 1984, month: 10, day: 21, hour: 12, minute: 34})" );

			  assertEquals( 200, response.Status() );
			  AssertNoErrors( response );
			  JsonNode data = GetSingleData( response );
			  AssertTemporalEquals( data, "1984-10-21T12:34", "localdatetime" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkWithDate() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWorkWithDate()
		 {
			  HTTP.Response response = RunQuery( "RETURN date({year: 1984, month: 10, day: 11})" );

			  assertEquals( 200, response.Status() );
			  AssertNoErrors( response );
			  JsonNode data = GetSingleData( response );
			  AssertTemporalEquals( data, "1984-10-11", "date" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkWithLocalTime() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWorkWithLocalTime()
		 {
			  HTTP.Response response = RunQuery( "RETURN localtime({hour:12, minute:31, second:14, nanosecond: 645876123})" );

			  assertEquals( 200, response.Status() );
			  AssertNoErrors( response );
			  JsonNode data = GetSingleData( response );
			  AssertTemporalEquals( data, "12:31:14.645876123", "localtime" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkWithDuration() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWorkWithDuration()
		 {
			  HTTP.Response response = RunQuery( "RETURN duration({weeks:2, days:3})" );

			  assertEquals( 200, response.Status() );
			  AssertNoErrors( response );
			  JsonNode data = GetSingleData( response );
			  AssertTemporalEquals( data, "P17D", "duration" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyGetNodeTypeInMetaAsNodeProperties() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOnlyGetNodeTypeInMetaAsNodeProperties()
		 {
			  HTTP.Response response = RunQuery( "CREATE (account {name: \\\"zhen\\\", creationTime: localdatetime({year: 1984, month: 10, day: 21, hour: 12, minute: 34})}) RETURN account" );

			  assertEquals( 200, response.Status() );
			  AssertNoErrors( response );
			  JsonNode data = GetSingleData( response );

			  JsonNode row = GetSingle( data, "row" );
			  assertThat( row.get( "creationTime" ).asText(), equalTo("1984-10-21T12:34") );
			  assertThat( row.get( "name" ).asText(), equalTo("zhen") );

			  JsonNode meta = GetSingle( data, "meta" );
			  assertThat( meta.get( "type" ).asText(), equalTo("node") );
		 }

		 private void AssertTemporalEquals( JsonNode data, string value, string type )
		 {
			  JsonNode row = GetSingle( data, "row" );
			  assertThat( row.asText(), equalTo(value) );

			  JsonNode meta = GetSingle( data, "meta" );
			  assertEquals( type, meta.get( "type" ).asText() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.codehaus.jackson.JsonNode getSingleData(org.neo4j.test.server.HTTP.Response response) throws org.neo4j.server.rest.domain.JsonParseException
		 private static JsonNode GetSingleData( HTTP.Response response )
		 {
			  JsonNode data = response.Get( "results" ).get( 0 ).get( "data" );
			  assertEquals( 1, data.size() );
			  return data.get( 0 );
		 }

		 private static JsonNode GetSingle( JsonNode node, string key )
		 {
			  JsonNode data = node.get( key );
			  assertEquals( 1, data.size() );
			  return data.get( 0 );
		 }
	}

}