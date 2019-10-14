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
namespace Neo4Net.Server.rest.transactional
{
	using JsonNode = org.codehaus.jackson.JsonNode;
	using Test = org.junit.Test;
	using ThrowsException = org.mockito.Internal.stubbing.answers.ThrowsException;


	using MapRow = Neo4Net.Cypher.Internal.javacompat.MapRow;
	using ExecutionPlanDescription = Neo4Net.Graphdb.ExecutionPlanDescription;
	using InputPosition = Neo4Net.Graphdb.InputPosition;
	using Node = Neo4Net.Graphdb.Node;
	using Notification = Neo4Net.Graphdb.Notification;
	using Path = Neo4Net.Graphdb.Path;
	using QueryExecutionType = Neo4Net.Graphdb.QueryExecutionType;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Result = Neo4Net.Graphdb.Result;
	using NotificationCode = Neo4Net.Graphdb.impl.notification.NotificationCode;
	using Coordinate = Neo4Net.Graphdb.spatial.Coordinate;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using Neo4jError = Neo4Net.Server.rest.transactional.error.Neo4jError;
	using GraphMock = Neo4Net.Test.mockito.mock.GraphMock;
	using Link = Neo4Net.Test.mockito.mock.Link;
	using SpatialMocks = Neo4Net.Test.mockito.mock.SpatialMocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.sameInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.domain.JsonHelper.jsonNode;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.domain.JsonHelper.readJson;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.Property.property;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.mock.GraphMock.link;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.mock.GraphMock.node;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.mock.GraphMock.path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.mock.GraphMock.relationship;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.mock.Properties.properties;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.mock.SpatialMocks.mockCartesian;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.mock.SpatialMocks.mockCartesian_3D;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.mock.SpatialMocks.mockWGS84;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.mock.SpatialMocks.mockWGS84_3D;

	public class ExecutionResultSerializerTest : TxStateCheckerTestSupport
	{

		 private ExecutionResultSerializer GetSerializerWith( Stream output )
		 {
			  return GetSerializerWith( output, null );
		 }

		 private ExecutionResultSerializer GetSerializerWith( Stream output, string uri )
		 {
			  return GetSerializerWith( output, uri, NullLogProvider.Instance );
		 }

		 private ExecutionResultSerializer GetSerializerWith( Stream output, string uri, LogProvider logProvider )
		 {
			  return new ExecutionResultSerializer( output, string.ReferenceEquals( uri, null ) ? null : URI.create( uri ), logProvider, Tptpmc );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeResponseWithCommitUriOnly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeResponseWithCommitUriOnly()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output );

			  // when
			  serializer.TransactionCommitUri( URI.create( "commit/uri/1" ) );
			  serializer.Finish();

			  // then
			  string result = output.ToString( UTF_8.name() );
			  assertEquals( "{\"commit\":\"commit/uri/1\",\"results\":[],\"errors\":[]}", result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeResponseWithCommitUriAndResults() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeResponseWithCommitUriAndResults()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output );

			  Result executionResult = MockExecutionResult( map( "column1", "value1", "column2", "value2" ) );

			  // when
			  serializer.TransactionCommitUri( URI.create( "commit/uri/1" ) );
			  serializer.StatementResult( executionResult, false );
			  serializer.Finish();

			  // then
			  string result = output.ToString( UTF_8.name() );
			  assertEquals( "{\"commit\":\"commit/uri/1\",\"results\":[{\"columns\":[\"column1\",\"column2\"]," + "\"data\":[{\"row\":[\"value1\",\"value2\"],\"meta\":[null,null]}]}],\"errors\":[]}", result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeResponseWithResultsOnly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeResponseWithResultsOnly()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output );

			  Result executionResult = MockExecutionResult( map( "column1", "value1", "column2", "value2" ) );

			  // when
			  serializer.StatementResult( executionResult, false );
			  serializer.Finish();

			  // then
			  string result = output.ToString( UTF_8.name() );
			  assertEquals( "{\"results\":[{\"columns\":[\"column1\",\"column2\"]," + "\"data\":[{\"row\":[\"value1\",\"value2\"],\"meta\":[null,null]}]}],\"errors\":[]}", result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeResponseWithCommitUriAndResultsAndErrors() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeResponseWithCommitUriAndResultsAndErrors()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output );

			  Result executionResult = MockExecutionResult( map( "column1", "value1", "column2", "value2" ) );

			  // when
			  serializer.TransactionCommitUri( URI.create( "commit/uri/1" ) );
			  serializer.StatementResult( executionResult, false );
			  serializer.Errors( asList( new Neo4jError( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, new Exception( "cause1" ) ) ) );
			  serializer.Finish();

			  // then
			  string result = output.ToString( UTF_8.name() );
			  assertEquals( "{\"commit\":\"commit/uri/1\",\"results\":[{\"columns\":[\"column1\",\"column2\"]," + "\"data\":[{\"row\":[\"value1\",\"value2\"],\"meta\":[null,null]}]}]," + "\"errors\":[{\"code\":\"Neo.ClientError.Request.InvalidFormat\",\"message\":\"cause1\"}]}", result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeResponseWithResultsAndErrors() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeResponseWithResultsAndErrors()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output );

			  Result executionResult = MockExecutionResult( map( "column1", "value1", "column2", "value2" ) );

			  // when
			  serializer.StatementResult( executionResult, false );
			  serializer.Errors( asList( new Neo4jError( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, new Exception( "cause1" ) ) ) );
			  serializer.Finish();

			  // then
			  string result = output.ToString( UTF_8.name() );
			  assertEquals( "{\"results\":[{\"columns\":[\"column1\",\"column2\"]," + "\"data\":[{\"row\":[\"value1\",\"value2\"],\"meta\":[null,null]}]}]," + "\"errors\":[{\"code\":\"Neo.ClientError.Request.InvalidFormat\",\"message\":\"cause1\"}]}", result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeResponseWithCommitUriAndErrors() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeResponseWithCommitUriAndErrors()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output );

			  // when
			  serializer.TransactionCommitUri( URI.create( "commit/uri/1" ) );
			  serializer.Errors( asList( new Neo4jError( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, new Exception( "cause1" ) ) ) );
			  serializer.Finish();

			  // then
			  string result = output.ToString( UTF_8.name() );
			  assertEquals( "{\"commit\":\"commit/uri/1\",\"results\":[],\"errors\":[{\"code\":\"Neo.ClientError.Request.InvalidFormat\"," + "\"message\":\"cause1\"}]}", result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeResponseWithErrorsOnly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeResponseWithErrorsOnly()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output );

			  // when
			  serializer.Errors( asList( new Neo4jError( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, new Exception( "cause1" ) ) ) );
			  serializer.Finish();

			  // then
			  string result = output.ToString( UTF_8.name() );
			  assertEquals( "{\"results\":[],\"errors\":[{\"code\":\"Neo.ClientError.Request.InvalidFormat\",\"message\":\"cause1\"}]}", result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeResponseWithNoCommitUriResultsOrErrors() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeResponseWithNoCommitUriResultsOrErrors()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output );

			  // when
			  serializer.Finish();

			  // then
			  string result = output.ToString( UTF_8.name() );
			  assertEquals( "{\"results\":[],\"errors\":[]}", result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeResponseWithMultipleResultRows() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeResponseWithMultipleResultRows()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output );

			  Result executionResult = MockExecutionResult( map( "column1", "value1", "column2", "value2" ), map( "column1", "value3", "column2", "value4" ) );

			  // when
			  serializer.StatementResult( executionResult, false );
			  serializer.Finish();

			  // then
			  string result = output.ToString( UTF_8.name() );
			  assertEquals( "{\"results\":[{\"columns\":[\"column1\",\"column2\"]," + "\"data\":[{\"row\":[\"value1\",\"value2\"],\"meta\":[null,null]}," + "{\"row\":[\"value3\",\"value4\"],\"meta\":[null,null]}]}]," + "\"errors\":[]}", result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeResponseWithMultipleResults() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeResponseWithMultipleResults()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output );

			  Result executionResult1 = MockExecutionResult( map( "column1", "value1", "column2", "value2" ) );
			  Result executionResult2 = MockExecutionResult( map( "column3", "value3", "column4", "value4" ) );

			  // when
			  serializer.StatementResult( executionResult1, false );
			  serializer.StatementResult( executionResult2, false );
			  serializer.Finish();

			  // then
			  string result = output.ToString( UTF_8.name() );
			  assertEquals( "{\"results\":[" + "{\"columns\":[\"column1\",\"column2\"],\"data\":[{\"row\":[\"value1\",\"value2\"],\"meta\":[null,null]}]}," + "{\"columns\":[\"column3\",\"column4\"],\"data\":[{\"row\":[\"value3\",\"value4\"],\"meta\":[null,null]}]}]," + "\"errors\":[]}", result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeNodeAsMapOfProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeNodeAsMapOfProperties()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output );

			  Result executionResult = MockExecutionResult( map( "node", node( 1, properties( property( "a", 12 ), property( "b", true ), property( "c", new int[]{ 1, 0, 1, 2 } ), property( "d", new sbyte[]{ 1, 0, 1, 2 } ), property( "e", new string[]{ "a", "b", "ääö" } ) ) ) ) );

			  // when
			  serializer.StatementResult( executionResult, false );
			  serializer.Finish();

			  // then
			  string result = output.ToString( UTF_8.name() );
			  assertEquals( "{\"results\":[{\"columns\":[\"node\"]," + "\"data\":[{\"row\":[{\"a\":12,\"b\":true,\"c\":[1,0,1,2],\"d\":[1,0,1,2],\"e\":[\"a\",\"b\",\"ääö\"]}]," + "\"meta\":[{\"id\":1,\"type\":\"node\",\"deleted\":false}]}]}]," + "\"errors\":[]}", result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeNestedEntities() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeNestedEntities()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output );

			  Node a = node( 1, properties( property( "foo", 12 ) ) );
			  Node b = node( 2, properties( property( "bar", false ) ) );
			  Relationship r = relationship( 1, properties( property( "baz", "quux" ) ), a, "FRAZZLE", b );
			  Result executionResult = MockExecutionResult( map( "nested", new SortedDictionary<>( map( "node", a, "edge", r, "path", path( a, link( r, b ) ) ) ) ) );

			  // when
			  serializer.StatementResult( executionResult, false );
			  serializer.Finish();

			  // then
			  string result = output.ToString( UTF_8.name() );
			  assertEquals( "{\"results\":[{\"columns\":[\"nested\"]," + "\"data\":[{\"row\":[{\"edge\":{\"baz\":\"quux\"},\"node\":{\"foo\":12}," + "\"path\":[{\"foo\":12},{\"baz\":\"quux\"},{\"bar\":false}]}]," + "\"meta\":[{\"id\":1,\"type\":\"relationship\",\"deleted\":false}," + "{\"id\":1,\"type\":\"node\",\"deleted\":false},[{\"id\":1,\"type\":\"node\",\"deleted\":false}," + "{\"id\":1,\"type\":\"relationship\",\"deleted\":false},{\"id\":2,\"type\":\"node\",\"deleted\":false}]]}]}]," + "\"errors\":[]}", result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializePathAsListOfMapsOfProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializePathAsListOfMapsOfProperties()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output );

			  Result executionResult = MockExecutionResult( map( "path", MockPath( map( "key1", "value1" ), map( "key2", "value2" ), map( "key3", "value3" ) ) ) );

			  // when
			  serializer.StatementResult( executionResult, false );
			  serializer.Finish();

			  // then
			  string result = output.ToString( UTF_8.name() );
			  assertEquals( "{\"results\":[{\"columns\":[\"path\"]," + "\"data\":[{\"row\":[[{\"key1\":\"value1\"},{\"key2\":\"value2\"},{\"key3\":\"value3\"}]]," + "\"meta\":[[{\"id\":1,\"type\":\"node\",\"deleted\":false}," + "{\"id\":1,\"type\":\"relationship\",\"deleted\":false},{\"id\":2,\"type\":\"node\",\"deleted\":false}]]}]}]," + "\"errors\":[]}", result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializePointsAsListOfMapsOfProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializePointsAsListOfMapsOfProperties()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output );

			  Result executionResult = MockExecutionResult( map( "geom", SpatialMocks.mockPoint( 12.3, 45.6, mockWGS84() ) ), map("geom", SpatialMocks.mockPoint(123, 456, mockCartesian())), map("geom", SpatialMocks.mockPoint(12.3, 45.6, 78.9, mockWGS84_3D())), map("geom", SpatialMocks.mockPoint(123, 456, 789, mockCartesian_3D())) );

			  // when
			  serializer.StatementResult( executionResult, false );
			  serializer.Finish();

			  // then
			  string result = output.ToString( UTF_8.name() );
			  assertEquals( "{\"results\":[{\"columns\":[\"geom\"],\"data\":[" + "{\"row\":[{\"type\":\"Point\",\"coordinates\":[12.3,45.6],\"crs\":" + "{\"srid\":4326,\"name\":\"WGS-84\",\"type\":\"link\",\"properties\":" + "{\"href\":\"http://spatialreference.org/ref/epsg/4326/ogcwkt/\",\"type\":\"ogcwkt\"}" + "}}],\"meta\":[{\"type\":\"point\"}]}," + "{\"row\":[{\"type\":\"Point\",\"coordinates\":[123.0,456.0],\"crs\":" + "{\"srid\":7203,\"name\":\"cartesian\",\"type\":\"link\",\"properties\":" + "{\"href\":\"http://spatialreference.org/ref/sr-org/7203/ogcwkt/\",\"type\":\"ogcwkt\"}" + "}}],\"meta\":[{\"type\":\"point\"}]}," + "{\"row\":[{\"type\":\"Point\",\"coordinates\":[12.3,45.6,78.9],\"crs\":" + "{\"srid\":4979,\"name\":\"WGS-84-3D\",\"type\":\"link\",\"properties\":" + "{\"href\":\"http://spatialreference.org/ref/epsg/4979/ogcwkt/\",\"type\":\"ogcwkt\"}" + "}}],\"meta\":[{\"type\":\"point\"}]}," + "{\"row\":[{\"type\":\"Point\",\"coordinates\":[123.0,456.0,789.0],\"crs\":" + "{\"srid\":9157,\"name\":\"cartesian-3D\",\"type\":\"link\",\"properties\":" + "{\"href\":\"http://spatialreference.org/ref/sr-org/9157/ogcwkt/\",\"type\":\"ogcwkt\"}" + "}}],\"meta\":[{\"type\":\"point\"}]}" + "]}],\"errors\":[]}", result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeTemporalAsListOfMapsOfProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeTemporalAsListOfMapsOfProperties()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output );

			  Result executionResult = MockExecutionResult( map( "temporal", LocalDate.of( 2018, 3, 12 ) ), map( "temporal", ZonedDateTime.of( 2018, 3, 12, 13, 2, 10, 10, ZoneId.of( "UTC+1" ) ) ), map( "temporal", OffsetTime.of( 12, 2, 4, 71, ZoneOffset.UTC ) ), map( "temporal", new DateTime( 2018, 3, 12, 13, 2, 10, 10 ) ), map( "temporal", LocalTime.of( 13, 2, 10, 10 ) ), map( "temporal", Duration.of( 12, ChronoUnit.HOURS ) ) );

			  // when
			  serializer.StatementResult( executionResult, false );
			  serializer.Finish();

			  // then
			  string result = output.ToString( UTF_8.name() );
			  assertEquals( "{\"results\":[{\"columns\":[\"temporal\"],\"data\":[" + "{\"row\":[\"2018-03-12\"],\"meta\":[{\"type\":\"date\"}]}," + "{\"row\":[\"2018-03-12T13:02:10.000000010+01:00[UTC+01:00]\"],\"meta\":[{\"type\":\"datetime\"}]}," + "{\"row\":[\"12:02:04.000000071Z\"],\"meta\":[{\"type\":\"time\"}]}," + "{\"row\":[\"2018-03-12T13:02:10.000000010\"],\"meta\":[{\"type\":\"localdatetime\"}]}," + "{\"row\":[\"13:02:10.000000010\"],\"meta\":[{\"type\":\"localtime\"}]}," + "{\"row\":[\"PT12H\"],\"meta\":[{\"type\":\"duration\"}]}" + "]}],\"errors\":[]}", result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldErrorWhenSerializingUnknownGeometryType() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldErrorWhenSerializingUnknownGeometryType()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output );

			  IList<Coordinate> points = new List<Coordinate>();
			  points.Add( new Coordinate( 1, 2 ) );
			  points.Add( new Coordinate( 2, 3 ) );
			  Result executionResult = MockExecutionResult( map( "geom", SpatialMocks.mockGeometry( "LineString", points, mockCartesian() ) ) );

			  // when
			  try
			  {
					serializer.StatementResult( executionResult, false );
					fail( "should have thrown exception" );
			  }
			  catch ( Exception e )
			  {
					serializer.Errors( asList( new Neo4jError( Neo4Net.Kernel.Api.Exceptions.Status_Statement.ExecutionFailed, e ) ) );
			  }

			  // then
			  string result = output.ToString( UTF_8.name() );
			  assertThat( result, startsWith( "{\"results\":[{\"columns\":[\"geom\"],\"data\":[" + "{\"row\":[{\"type\":\"LineString\",\"coordinates\":[[1.0,2.0],[2.0,3.0]],\"crs\":" + "{\"srid\":7203,\"name\":\"cartesian\",\"type\":\"link\",\"properties\":" + "{\"href\":\"http://spatialreference.org/ref/sr-org/7203/ogcwkt/\",\"type\":\"ogcwkt\"}}}],\"meta\":[]}]}]," + "\"errors\":[{\"code\":\"Neo.DatabaseError.Statement.ExecutionFailed\"," + "\"message\":\"Unsupported Geometry type: type=MockGeometry, value=LineString\"," + "\"stackTrace\":\"java.lang.IllegalArgumentException: Unsupported Geometry type: type=MockGeometry, value=LineString" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProduceWellFormedJsonEvenIfResultIteratorThrowsExceptionOnNext() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProduceWellFormedJsonEvenIfResultIteratorThrowsExceptionOnNext()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output );

			  IDictionary<string, object> data = map( "column1", "value1", "column2", "value2" );
			  Result executionResult = mock( typeof( Result ) );
			  MockAccept( executionResult );
			  when( executionResult.Columns() ).thenReturn(new List<>(data.Keys));
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  when( executionResult.HasNext() ).thenReturn(true, true, false);
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  when( executionResult.Next() ).thenReturn(data).thenThrow(new Exception("Stuff went wrong!"));

			  // when
			  try
			  {
					serializer.StatementResult( executionResult, false );
					fail( "should have thrown exception" );
			  }
			  catch ( Exception e )
			  {
					serializer.Errors( asList( new Neo4jError( Neo4Net.Kernel.Api.Exceptions.Status_Statement.ExecutionFailed, e ) ) );
			  }
			  serializer.Finish();

			  // then
			  string result = output.ToString( UTF_8.name() );
			  assertEquals( "{\"results\":[{\"columns\":[\"column1\",\"column2\"]," + "\"data\":[{\"row\":[\"value1\",\"value2\"],\"meta\":[null,null]}]}]," + "\"errors\":[{\"code\":\"Neo.DatabaseError.Statement.ExecutionFailed\"," + "\"message\":\"Stuff went wrong!\",\"stackTrace\":***}]}", ReplaceStackTrace( result, "***" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProduceWellFormedJsonEvenIfResultIteratorThrowsExceptionOnHasNext() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProduceWellFormedJsonEvenIfResultIteratorThrowsExceptionOnHasNext()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output );

			  IDictionary<string, object> data = map( "column1", "value1", "column2", "value2" );
			  Result executionResult = mock( typeof( Result ) );
			  MockAccept( executionResult );
			  when( executionResult.Columns() ).thenReturn(new List<>(data.Keys));
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  when( executionResult.HasNext() ).thenReturn(true).thenThrow(new Exception("Stuff went wrong!"));
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  when( executionResult.Next() ).thenReturn(data);

			  // when
			  try
			  {
					serializer.StatementResult( executionResult, false );
					fail( "should have thrown exception" );
			  }
			  catch ( Exception e )
			  {
					serializer.Errors( asList( new Neo4jError( Neo4Net.Kernel.Api.Exceptions.Status_Statement.ExecutionFailed, e ) ) );
			  }
			  serializer.Finish();

			  // then
			  string result = output.ToString( UTF_8.name() );
			  assertEquals( "{\"results\":[{\"columns\":[\"column1\",\"column2\"]," + "\"data\":[{\"row\":[\"value1\",\"value2\"],\"meta\":[null,null]}]}]," + "\"errors\":[{\"code\":\"Neo.DatabaseError.Statement.ExecutionFailed\",\"message\":\"Stuff went wrong!\"," + "\"stackTrace\":***}]}", ReplaceStackTrace( result, "***" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProduceResultStreamWithGraphEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProduceResultStreamWithGraphEntries()
		 {
			  // given
			  Node[] node = new Node[] { node( 0, properties( property( "name", "node0" ) ), "Node" ), node( 1, properties( property( "name", "node1" ) ) ), node( 2, properties( property( "name", "node2" ) ), "This", "That" ), node( 3, properties( property( "name", "node3" ) ), "Other" ) };
			  Relationship[] rel = new Relationship[] { relationship( 0, node[0], "KNOWS", node[1], property( "name", "rel0" ) ), relationship( 1, node[2], "LOVES", node[3], property( "name", "rel1" ) ) };

			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output );

			  // when
			  serializer.StatementResult( MockExecutionResult( map( "node", node[0], "rel", rel[0] ), map( "node", node[2], "rel", rel[1] ) ), false, ResultDataContent.Row, ResultDataContent.Graph );
			  serializer.Finish();

			  // then
			  string result = output.ToString( UTF_8.name() );

			  // Nodes and relationships form sets, so we cannot test for a fixed string, since we don't know the order.
			  string node0 = "{\"id\":\"0\",\"labels\":[\"Node\"],\"properties\":{\"name\":\"node0\"}}";
			  string node1 = "{\"id\":\"1\",\"labels\":[],\"properties\":{\"name\":\"node1\"}}";
			  string node2 = "{\"id\":\"2\",\"labels\":[\"This\",\"That\"],\"properties\":{\"name\":\"node2\"}}";
			  string node3 = "{\"id\":\"3\",\"labels\":[\"Other\"],\"properties\":{\"name\":\"node3\"}}";
			  string rel0 = "\"relationships\":[{\"id\":\"0\",\"type\":\"KNOWS\"," +
						 "\"startNode\":\"0\",\"endNode\":\"1\",\"properties\":{\"name\":\"rel0\"}}]}";
			  string rel1 = "\"relationships\":[{\"id\":\"1\",\"type\":\"LOVES\"," +
						 "\"startNode\":\"2\",\"endNode\":\"3\",\"properties\":{\"name\":\"rel1\"}}]}";
			  string row0 = "{\"row\":[{\"name\":\"node0\"},{\"name\":\"rel0\"}]," +
						 "\"meta\":[{\"id\":0,\"type\":\"node\",\"deleted\":false}," +
						 "{\"id\":0,\"type\":\"relationship\",\"deleted\":false}],\"graph\":{\"nodes\":[";
			  string row1 = "{\"row\":[{\"name\":\"node2\"},{\"name\":\"rel1\"}]," +
						 "\"meta\":[{\"id\":2,\"type\":\"node\",\"deleted\":false}," +
						 "{\"id\":1,\"type\":\"relationship\",\"deleted\":false}],\"graph\":{\"nodes\":[";
			  int n0 = result.IndexOf( node0, StringComparison.Ordinal );
			  int n1 = result.IndexOf( node1, StringComparison.Ordinal );
			  int n2 = result.IndexOf( node2, StringComparison.Ordinal );
			  int n3 = result.IndexOf( node3, StringComparison.Ordinal );
			  int r0 = result.IndexOf( rel0, StringComparison.Ordinal );
			  int r1 = result.IndexOf( rel1, StringComparison.Ordinal );
			  int row0Index = result.IndexOf( row0, StringComparison.Ordinal );
			  int row1Index = result.IndexOf( row1, StringComparison.Ordinal );
			  assertTrue( "result should contain row0", row0Index > 0 );
			  assertTrue( "result should contain row1 after row0", row1Index > row0Index );
			  assertTrue( "result should contain node0 after row0", n0 > row0Index );
			  assertTrue( "result should contain node1 after row0", n1 > row0Index );
			  assertTrue( "result should contain node2 after row1", n2 > row1Index );
			  assertTrue( "result should contain node3 after row1", n3 > row1Index );
			  assertTrue( "result should contain rel0 after node0 and node1", r0 > n0 && r0 > n1 );
			  assertTrue( "result should contain rel1 after node2 and node3", r1 > n2 && r1 > n3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProduceResultStreamWithLegacyRestFormat() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProduceResultStreamWithLegacyRestFormat()
		 {
			  // given
			  Node[] node = new Node[] { node( 0, properties( property( "name", "node0" ) ) ), node( 1, properties( property( "name", "node1" ) ) ), node( 2, properties( property( "name", "node2" ) ) ) };
			  Relationship[] rel = new Relationship[] { relationship( 0, node[0], "KNOWS", node[1], property( "name", "rel0" ) ), relationship( 1, node[2], "LOVES", node[1], property( "name", "rel1" ) ) };
			  Path path = GraphMock.path( node[0], link( rel[0], node[1] ), link( rel[1], node[2] ) );

			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output, "http://base.uri/" );

			  // when
			  serializer.StatementResult( MockExecutionResult( map( "node", node[0], "rel", rel[0], "path", path, "map", map( "n1", node[1], "r1", rel[1] ) ) ), false, ResultDataContent.Rest );
			  serializer.Finish();

			  // then
			  string result = output.ToString( UTF_8.name() );
			  JsonNode json = jsonNode( result );
			  IDictionary<string, int> columns = new Dictionary<string, int>();
			  int col = 0;
			  JsonNode results = json.get( "results" ).get( 0 );
			  foreach ( JsonNode column in results.get( "columns" ) )
			  {
					columns[column.TextValue] = col++;
			  }
			  JsonNode row = results.get( "data" ).get( 0 ).get( "rest" );
			  JsonNode jsonNode = row.get( columns["node"] );
			  JsonNode jsonRel = row.get( columns["rel"] );
			  JsonNode jsonPath = row.get( columns["path"] );
			  JsonNode jsonMap = row.get( columns["map"] );
			  assertEquals( "http://base.uri/node/0", jsonNode.get( "self" ).TextValue );
			  assertEquals( "http://base.uri/relationship/0", jsonRel.get( "self" ).TextValue );
			  assertEquals( 2, jsonPath.get( "length" ).NumberValue );
			  assertEquals( "http://base.uri/node/0", jsonPath.get( "start" ).TextValue );
			  assertEquals( "http://base.uri/node/2", jsonPath.get( "end" ).TextValue );
			  assertEquals( "http://base.uri/node/1", jsonMap.get( "n1" ).get( "self" ).TextValue );
			  assertEquals( "http://base.uri/relationship/1", jsonMap.get( "r1" ).get( "self" ).TextValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProduceResultStreamWithLegacyRestFormatAndNestedMaps() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProduceResultStreamWithLegacyRestFormatAndNestedMaps()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output, "http://base.uri/" );

			  // when
			  serializer.StatementResult( MockExecutionResult( map( "map", map( "one", map( "two", asList( "wait for it...", map( "three", "GO!" ) ) ) ) ) ), false, ResultDataContent.Rest );
			  serializer.Finish();

			  // then
			  string result = output.ToString( UTF_8.name() );
			  JsonNode json = jsonNode( result );
			  IDictionary<string, int> columns = new Dictionary<string, int>();
			  int col = 0;
			  JsonNode results = json.get( "results" ).get( 0 );
			  foreach ( JsonNode column in results.get( "columns" ) )
			  {
					columns[column.TextValue] = col++;
			  }
			  JsonNode row = results.get( "data" ).get( 0 ).get( "rest" );
			  JsonNode jsonMap = row.get( columns["map"] );
			  assertEquals( "wait for it...", jsonMap.get( "one" ).get( "two" ).get( 0 ).asText() );
			  assertEquals( "GO!", jsonMap.get( "one" ).get( "two" ).get( 1 ).get( "three" ).asText() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializePlanWithoutChildButAllKindsOfSupportedArguments() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializePlanWithoutChildButAllKindsOfSupportedArguments()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output, "http://base.uri/" );

			  string operatorType = "Ich habe einen Plan";

			  // This is the full set of types that we allow in plan arguments

			  IDictionary<string, object> args = new Dictionary<string, object>();
			  args["string"] = "A String";
			  args["bool"] = true;
			  args["number"] = 1;
			  args["double"] = 2.3;
			  args["listOfInts"] = asList( 1, 2, 3 );
			  args["listOfListOfInts"] = asList( asList( 1, 2, 3 ) );

			  // when
			  ExecutionPlanDescription planDescription = MockedPlanDescription( operatorType, _noIds, args, _noPlans );
			  serializer.StatementResult( MockExecutionResult( planDescription ), false, ResultDataContent.Rest );
			  serializer.Finish();
			  string resultString = output.ToString( UTF_8.name() );

			  // then
			  AssertIsPlanRoot( resultString );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<String, ?> rootMap = planRootMap(resultString);
			  IDictionary<string, ?> rootMap = PlanRootMap( resultString );

			  assertEquals( asSet( "operatorType", "identifiers", "children", "string", "bool", "number", "double", "listOfInts", "listOfListOfInts" ), rootMap.Keys );

			  assertEquals( operatorType, rootMap["operatorType"] );
			  assertEquals( args["string"], rootMap["string"] );
			  assertEquals( args["bool"], rootMap["bool"] );
			  assertEquals( args["number"], rootMap["number"] );
			  assertEquals( args["double"], rootMap["double"] );
			  assertEquals( args["listOfInts"], rootMap["listOfInts"] );
			  assertEquals( args["listOfListOfInts"], rootMap["listOfListOfInts"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializePlanWithoutChildButWithIdentifiers() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializePlanWithoutChildButWithIdentifiers()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output, "http://base.uri/" );

			  string operatorType = "Ich habe einen Plan";
			  string id1 = "id1";
			  string id2 = "id2";
			  string id3 = "id3";

			  // This is the full set of types that we allow in plan arguments

			  // when
			  ExecutionPlanDescription planDescription = MockedPlanDescription( operatorType, asSet( id1, id2, id3 ), _noArgs, _noPlans );
			  serializer.StatementResult( MockExecutionResult( planDescription ), false, ResultDataContent.Rest );
			  serializer.Finish();
			  string resultString = output.ToString( UTF_8.name() );

			  // then
			  AssertIsPlanRoot( resultString );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<String,?> rootMap = planRootMap(resultString);
			  IDictionary<string, ?> rootMap = PlanRootMap( resultString );

			  assertEquals( asSet( "operatorType", "identifiers", "children" ), rootMap.Keys );

			  assertEquals( operatorType, rootMap["operatorType"] );
			  assertEquals( asList( id2, id1, id3 ), rootMap["identifiers"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializePlanWithChildren() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializePlanWithChildren()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output, "http://base.uri/" );

			  string leftId = "leftId";
			  string rightId = "rightId";
			  string parentId = "parentId";

			  // when
			  ExecutionPlanDescription left = MockedPlanDescription( "child", asSet( leftId ), MapUtil.map( "id", 1 ), _noPlans );
			  ExecutionPlanDescription right = MockedPlanDescription( "child", asSet( rightId ), MapUtil.map( "id", 2 ), _noPlans );
			  ExecutionPlanDescription parent = MockedPlanDescription( "parent", asSet( parentId ), MapUtil.map( "id", 0 ), new IList<ExecutionPlanDescription> { left, right } );

			  serializer.StatementResult( MockExecutionResult( parent ), false, ResultDataContent.Rest );
			  serializer.Finish();

			  // then
			  string result = output.ToString( UTF_8.name() );
			  JsonNode root = AssertIsPlanRoot( result );

			  assertEquals( "parent", root.get( "operatorType" ).TextValue );
			  assertEquals( 0, root.get( "id" ).asLong() );
			  assertEquals( asSet( parentId ), IdentifiersOf( root ) );

			  ISet<int> childIds = new HashSet<int>();
			  ISet<ISet<string>> identifiers = new HashSet<ISet<string>>();
			  foreach ( JsonNode child in root.get( "children" ) )
			  {
					assertTrue( "Expected object", child.Object );
					assertEquals( "child", child.get( "operatorType" ).TextValue );
					identifiers.Add( IdentifiersOf( child ) );
					childIds.Add( child.get( "id" ).asInt() );
			  }
			  assertEquals( asSet( 1, 2 ), childIds );
			  assertEquals( asSet( asSet( leftId ), asSet( rightId ) ), identifiers );
		 }

		 private ISet<string> IdentifiersOf( JsonNode root )
		 {
			  ISet<string> parentIds = new HashSet<string>();
			  foreach ( JsonNode id in root.get( "identifiers" ) )
			  {
					parentIds.Add( id.asText() );
			  }
			  return parentIds;
		 }

		 private static readonly IDictionary<string, object> _noArgs = Collections.emptyMap();
		 private static readonly ISet<string> _noIds = Collections.emptySet();
		 private static readonly IList<ExecutionPlanDescription> _noPlans = Collections.emptyList();

		 private ExecutionPlanDescription MockedPlanDescription( string operatorType, ISet<string> identifiers, IDictionary<string, object> args, IList<ExecutionPlanDescription> children )
		 {
			  ExecutionPlanDescription planDescription = mock( typeof( ExecutionPlanDescription ) );
			  when( planDescription.Children ).thenReturn( children );
			  when( planDescription.Name ).thenReturn( operatorType );
			  when( planDescription.Arguments ).thenReturn( args );
			  when( planDescription.Identifiers ).thenReturn( identifiers );
			  return planDescription;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.codehaus.jackson.JsonNode assertIsPlanRoot(String result) throws org.neo4j.server.rest.domain.JsonParseException
		 private JsonNode AssertIsPlanRoot( string result )
		 {
			  JsonNode json = jsonNode( result );
			  JsonNode results = json.get( "results" ).get( 0 );

			  JsonNode plan = results.get( "plan" );
			  assertTrue( "Expected plan to be an object", plan != null && plan.Object );

			  JsonNode root = plan.get( "root" );
			  assertTrue( "Expected plan to be an object", root != null && root.Object );

			  return root;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private java.util.Map<String, ?> planRootMap(String resultString) throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private IDictionary<string, ?> PlanRootMap( string resultString )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<String, ?> resultMap = (java.util.Map<String, ?>)((java.util.List<?>)((java.util.Map<String, ?>)(readJson(resultString))).get("results")).get(0);
			  IDictionary<string, ?> resultMap = ( IDictionary<string, ?> )( ( IList<object> )( ( IDictionary<string, ?> )( readJson( resultString ) ) )["results"] )[0];
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<String, ?> planMap = (java.util.Map<String, ?>)(resultMap.get("plan"));
			  IDictionary<string, ?> planMap = ( IDictionary<string, ?> )( resultMap["plan"] );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return (java.util.Map<String, ?>)(planMap.get("root"));
			  return ( IDictionary<string, ?> )( planMap["root"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogIOErrors()
		 public virtual void ShouldLogIOErrors()
		 {
			  // given
			  IOException failure = new IOException();
			  Stream output = mock( typeof( Stream ), new ThrowsException( failure ) );
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  ExecutionResultSerializer serializer = GetSerializerWith( output, null, logProvider );

			  // when
			  serializer.Finish();

			  // then
			  logProvider.AssertExactly( AssertableLogProvider.inLog( typeof( ExecutionResultSerializer ) ).error( @is( "Failed to generate JSON output." ), sameInstance( failure ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAbbreviateWellKnownIOErrors()
		 public virtual void ShouldAbbreviateWellKnownIOErrors()
		 {
			  // given
			  Stream output = mock( typeof( Stream ), new ThrowsException( new IOException( "Broken pipe" ) ) );
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  ExecutionResultSerializer serializer = GetSerializerWith( output, null, logProvider );

			  // when
			  serializer.Finish();

			  // then
			  logProvider.AssertExactly( AssertableLogProvider.inLog( typeof( ExecutionResultSerializer ) ).error( "Unable to reply to request, because the client has closed the connection (Broken pipe)." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNotifications() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnNotifications()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output );

			  Notification notification = NotificationCode.CARTESIAN_PRODUCT.notification( new InputPosition( 1, 2, 3 ) );
			  IList<Notification> notifications = Arrays.asList( notification );
			  Result executionResult = MockExecutionResult( null, notifications, map( "column1", "value1", "column2", "value2" ) );

			  // when
			  serializer.TransactionCommitUri( URI.create( "commit/uri/1" ) );
			  serializer.StatementResult( executionResult, false );
			  serializer.Notifications( notifications );
			  serializer.Finish();

			  // then
			  string result = output.ToString( UTF_8.name() );

			  assertEquals( "{\"commit\":\"commit/uri/1\",\"results\":[{\"columns\":[\"column1\",\"column2\"]," + "\"data\":[{\"row\":[\"value1\",\"value2\"],\"meta\":[null,null]}]}],\"notifications\":[{\"code\":\"Neo" + ".ClientNotification.Statement.CartesianProductWarning\",\"severity\":\"WARNING\",\"title\":\"This " + "query builds a cartesian product between disconnected patterns.\",\"description\":\"If a " + "part of a query contains multiple disconnected patterns, this will build a cartesian product" + " between all those parts. This may produce a large amount of data and slow down query " + "processing. While occasionally intended, it may often be possible to reformulate the query " + "that avoids the use of this cross product, perhaps by adding a relationship between the " + "different parts or by using OPTIONAL MATCH\",\"position\":{\"offset\":1,\"line\":2," + "\"column\":3}}],\"errors\":[]}", result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReturnNotificationsWhenEmptyNotifications() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotReturnNotificationsWhenEmptyNotifications()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output );

			  IList<Notification> notifications = Collections.emptyList();
			  Result executionResult = MockExecutionResult( null, notifications, map( "column1", "value1", "column2", "value2" ) );

			  // when
			  serializer.TransactionCommitUri( URI.create( "commit/uri/1" ) );
			  serializer.StatementResult( executionResult, false );
			  serializer.Notifications( notifications );
			  serializer.Finish();

			  // then
			  string result = output.ToString( UTF_8.name() );

			  assertEquals( "{\"commit\":\"commit/uri/1\",\"results\":[{\"columns\":[\"column1\",\"column2\"]," + "\"data\":[{\"row\":[\"value1\",\"value2\"],\"meta\":[null,null]}]}],\"errors\":[]}", result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReturnPositionWhenEmptyPosition() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotReturnPositionWhenEmptyPosition()
		 {
			  // given
			  MemoryStream output = new MemoryStream();
			  ExecutionResultSerializer serializer = GetSerializerWith( output );

			  Notification notification = NotificationCode.CARTESIAN_PRODUCT.notification( InputPosition.empty );

			  IList<Notification> notifications = Arrays.asList( notification );
			  Result executionResult = MockExecutionResult( null, notifications, map( "column1", "value1", "column2", "value2" ) );

			  // when
			  serializer.TransactionCommitUri( URI.create( "commit/uri/1" ) );
			  serializer.StatementResult( executionResult, false );
			  serializer.Notifications( notifications );
			  serializer.Finish();

			  // then
			  string result = output.ToString( UTF_8.name() );

			  assertEquals( "{\"commit\":\"commit/uri/1\",\"results\":[{\"columns\":[\"column1\",\"column2\"]," + "\"data\":[{\"row\":[\"value1\",\"value2\"],\"meta\":[null,null]}]}],\"notifications\":[{\"code\":\"Neo" + ".ClientNotification.Statement.CartesianProductWarning\",\"severity\":\"WARNING\",\"title\":\"This " + "query builds a cartesian product between disconnected patterns.\",\"description\":\"If a " + "part of a query contains multiple disconnected patterns, this will build a cartesian product" + " between all those parts. This may produce a large amount of data and slow down query " + "processing. While occasionally intended, it may often be possible to reformulate the query " + "that avoids the use of this cross product, perhaps by adding a relationship between the " + "different parts or by using OPTIONAL MATCH\"}],\"errors\":[]}", result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private static org.neo4j.graphdb.Result mockExecutionResult(java.util.Map<String, Object>... rows)
		 private static Result MockExecutionResult( params IDictionary<string, object>[] rows )
		 {
			  return MockExecutionResult( null, rows );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private static org.neo4j.graphdb.Result mockExecutionResult(org.neo4j.graphdb.ExecutionPlanDescription planDescription, java.util.Map<String,Object>... rows)
		 private static Result MockExecutionResult( ExecutionPlanDescription planDescription, params IDictionary<string, object>[] rows )
		 {
			  return MockExecutionResult( planDescription, Collections.emptyList(), rows );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private static org.neo4j.graphdb.Result mockExecutionResult(org.neo4j.graphdb.ExecutionPlanDescription planDescription, Iterable<org.neo4j.graphdb.Notification> notifications, java.util.Map<String, Object>... rows)
		 private static Result MockExecutionResult( ExecutionPlanDescription planDescription, IEnumerable<Notification> notifications, params IDictionary<string, object>[] rows )
		 {
			  ISet<string> keys = new SortedSet<string>();
			  foreach ( IDictionary<string, object> row in rows )
			  {
					keys.addAll( row.Keys );
			  }
			  Result executionResult = mock( typeof( Result ) );

			  when( executionResult.Columns() ).thenReturn(new List<>(keys));

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Iterator<java.util.Map<String, Object>> inner = asList(rows).iterator();
			  IEnumerator<IDictionary<string, object>> inner = asList( rows ).GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  when( executionResult.HasNext() ).thenAnswer(invocation => inner.hasNext());
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  when( executionResult.Next() ).thenAnswer(invocation => inner.next());

			  when( executionResult.QueryExecutionType ).thenReturn( null != planDescription ? QueryExecutionType.profiled( QueryExecutionType.QueryType.READ_WRITE ) : QueryExecutionType.query( QueryExecutionType.QueryType.READ_WRITE ) );
			  if ( executionResult.QueryExecutionType.requestedExecutionPlanDescription() )
			  {
					when( executionResult.ExecutionPlanDescription ).thenReturn( planDescription );
			  }
			  MockAccept( executionResult );

			  when( executionResult.Notifications ).thenReturn( notifications );

			  return executionResult;
		 }

		 private static void MockAccept( Result mock )
		 {
			  doAnswer(invocation =>
			  {
				Result result = ( Result ) invocation.Mock;
				Result.ResultVisitor visitor = invocation.getArgument( 0 );
				while ( result.hasNext() )
				{
					 visitor.visit( new MapRow( result.next() ) );
				}
				return null;
			  }).when( mock ).accept( ( Result.ResultVisitor<Exception> ) any( typeof( Result.ResultVisitor ) ) );
		 }

		 private static Path MockPath( IDictionary<string, object> startNodeProperties, IDictionary<string, object> relationshipProperties, IDictionary<string, object> endNodeProperties )
		 {
			  Node startNode = node( 1, properties( startNodeProperties ) );
			  Node endNode = node( 2, properties( endNodeProperties ) );
			  Relationship relationship = relationship( 1, properties( relationshipProperties ), startNode, "RELATED", endNode );
			  return path( startNode, Link.link( relationship, endNode ) );
		 }

		 private string ReplaceStackTrace( string json, string matchableStackTrace )
		 {
			  return json.replaceAll( "\"stackTrace\":\"[^\"]*\"", "\"stackTrace\":" + matchableStackTrace );
		 }
	}

}