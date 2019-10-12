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
namespace Org.Neo4j.Server.rest.transactional
{
	using JsonFactory = org.codehaus.jackson.JsonFactory;
	using JsonGenerator = org.codehaus.jackson.JsonGenerator;
	using JsonNode = org.codehaus.jackson.JsonNode;
	using MatcherAssert = org.hamcrest.MatcherAssert;
	using Test = org.junit.Test;


	using MapRow = Org.Neo4j.Cypher.@internal.javacompat.MapRow;
	using JsonParseException = Org.Neo4j.Server.rest.domain.JsonParseException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.domain.JsonHelper.jsonNode;

	public class RestRepresentationWriterTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteNestedMaps() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteNestedMaps()
		 {
			  MemoryStream @out = new MemoryStream();
			  JsonGenerator json = ( new JsonFactory( new Neo4jJsonCodec() ) ).createJsonGenerator(@out);

			  JsonNode rest = Serialize( @out, json, new RestRepresentationWriter( URI.create( "localhost" ) ) );

			  MatcherAssert.assertThat( rest.size(), equalTo(1) );

			  JsonNode firstCell = rest.get( 0 );
			  MatcherAssert.assertThat( firstCell.get( "one" ).get( "two" ).size(), @is(2) );
			  MatcherAssert.assertThat( firstCell.get( "one" ).get( "two" ).get( 0 ).asBoolean(), @is(true) );
			  MatcherAssert.assertThat( firstCell.get( "one" ).get( "two" ).get( 1 ).get( "three" ).asInt(), @is(42) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.codehaus.jackson.JsonNode serialize(java.io.ByteArrayOutputStream out, org.codehaus.jackson.JsonGenerator json, ResultDataContentWriter resultDataContentWriter) throws java.io.IOException, org.neo4j.server.rest.domain.JsonParseException
		 private JsonNode Serialize( MemoryStream @out, JsonGenerator json, ResultDataContentWriter resultDataContentWriter )
		 {
			  json.writeStartObject();
			  // RETURN {one:{two:[true, {three: 42}]}}
			  resultDataContentWriter.Write( json, asList( "the column" ), new MapRow( map( "the column", map( "one", map( "two", asList( true, map( "three", 42 ) ) ) ) ) ), null );
			  json.writeEndObject();
			  json.flush();
			  json.close();

			  string jsonAsString = @out.ToString();
			  return jsonNode( jsonAsString ).get( "rest" );
		 }
	}

}