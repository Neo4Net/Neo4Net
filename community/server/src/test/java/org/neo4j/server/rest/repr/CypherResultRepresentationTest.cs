﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Server.rest.repr
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ExecutionPlanDescription = Org.Neo4j.Graphdb.ExecutionPlanDescription;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Result = Org.Neo4j.Graphdb.Result;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using MapUtil = Org.Neo4j.Helpers.Collection.MapUtil;
	using JsonFormat = Org.Neo4j.Server.rest.repr.formats.JsonFormat;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.domain.JsonHelper.jsonToMap;

	public class CypherResultRepresentationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.DatabaseRule database = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public DatabaseRule Database = new ImpermanentDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @SuppressWarnings("unchecked") public void shouldSerializeProfilingResult() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeProfilingResult()
		 {
			  // Given
			  string name = "Kalle";

			  ExecutionPlanDescription plan = GetMockDescription( name );
			  ExecutionPlanDescription childPlan = GetMockDescription( "child" );
			  when( plan.Children ).thenReturn( asList( childPlan ) );
			  when( plan.HasProfilerStatistics() ).thenReturn(true);

			  Org.Neo4j.Graphdb.ExecutionPlanDescription_ProfilerStatistics stats = mock( typeof( Org.Neo4j.Graphdb.ExecutionPlanDescription_ProfilerStatistics ) );
			  when( stats.DbHits ).thenReturn( 13L );
			  when( stats.Rows ).thenReturn( 25L );

			  when( plan.ProfilerStatistics ).thenReturn( stats );

			  Result result = mock( typeof( Result ) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  when( result.HasNext() ).thenReturn(false);
			  when( result.Columns() ).thenReturn(new List<>());
			  when( result.ExecutionPlanDescription ).thenReturn( plan );

			  // When
			  IDictionary<string, object> serialized = SerializeToStringThenParseAsToMap( new CypherResultRepresentation( result, false, true ) );

			  // Then
			  IDictionary<string, object> serializedPlan = ( IDictionary<string, object> ) serialized["plan"];
			  assertThat( serializedPlan["name"], equalTo( name ) );
			  assertThat( serializedPlan["rows"], @is( 25 ) );
			  assertThat( serializedPlan["dbHits"], @is( 13 ) );

			  IList<IDictionary<string, object>> children = ( IList<IDictionary<string, object>> ) serializedPlan["children"];
			  assertThat( children.Count, @is( 1 ) );

			  IDictionary<string, object> args = ( IDictionary<string, object> ) serializedPlan["args"];
			  assertThat( args["argumentKey"], @is( "argumentValue" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @SuppressWarnings("unchecked") public void shouldNotIncludePlanUnlessAskedFor() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotIncludePlanUnlessAskedFor()
		 {
			  // Given
			  Result result = mock( typeof( Result ) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  when( result.HasNext() ).thenReturn(false);
			  when( result.Columns() ).thenReturn(new List<>());

			  // When
			  IDictionary<string, object> serialized = SerializeToStringThenParseAsToMap( new CypherResultRepresentation( result, false, false ) );

			  // Then
			  assertFalse( "Didn't expect to see a plan here", serialized.ContainsKey( "plan" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFormatMapsProperly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFormatMapsProperly()
		 {
			  GraphDatabaseService graphdb = Database.GraphDatabaseAPI;
			  Result result = graphdb.Execute( "RETURN {one:{two:['wait for it...', {three: 'GO!'}]}}" );
			  CypherResultRepresentation representation = new CypherResultRepresentation( result, false, false );

			  // When
			  IDictionary<string, object> serialized = SerializeToStringThenParseAsToMap( representation );

			  // Then
			  System.Collections.IDictionary one = ( System.Collections.IDictionary )( ( System.Collections.IDictionary )( ( System.Collections.IList )( ( System.Collections.IList ) serialized["data"] )[0] )[0] )["one"];
			  System.Collections.IList two = ( System.Collections.IList ) one["two"];
			  assertThat( two[0], @is( "wait for it..." ) );
			  System.Collections.IDictionary foo = ( System.Collections.IDictionary ) two[1];
			  assertThat( foo["three"], @is( "GO!" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRenderNestedEntities() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRenderNestedEntities()
		 {
			  using ( Transaction ignored = Database.GraphDatabaseAPI.beginTx() )
			  {
					GraphDatabaseService graphdb = Database.GraphDatabaseAPI;
					graphdb.Execute( "CREATE (n {name: 'Sally'}), (m {age: 42}), (n)-[r:FOO {drunk: false}]->(m)" );
					Result result = graphdb.Execute( "MATCH p=(n)-[r]->(m) RETURN n, r, p, {node: n, edge: r, path: p}" );
					CypherResultRepresentation representation = new CypherResultRepresentation( result, false, false );

					// When
					IDictionary<string, object> serialized = SerializeToStringThenParseAsToMap( representation );

					// Then
					object firstRow = ( ( System.Collections.IList ) serialized["data"] )[0];
					System.Collections.IDictionary nested = ( System.Collections.IDictionary )( ( System.Collections.IList ) firstRow )[3];
					assertThat( nested["node"], @is( equalTo( ( ( System.Collections.IList ) firstRow )[0] ) ) );
					assertThat( nested["edge"], @is( equalTo( ( ( System.Collections.IList ) firstRow )[1] ) ) );
					assertThat( nested["path"], @is( equalTo( ( ( System.Collections.IList ) firstRow )[2] ) ) );
			  }
		 }

		 private ExecutionPlanDescription GetMockDescription( string name )
		 {
			  ExecutionPlanDescription plan = mock( typeof( ExecutionPlanDescription ) );
			  when( plan.Name ).thenReturn( name );
			  when( plan.Arguments ).thenReturn( MapUtil.map( "argumentKey", "argumentValue" ) );
			  return plan;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.Map<String, Object> serializeToStringThenParseAsToMap(CypherResultRepresentation repr) throws Exception
		 private IDictionary<string, object> SerializeToStringThenParseAsToMap( CypherResultRepresentation repr )
		 {
			  OutputFormat format = new OutputFormat( new JsonFormat(), new URI("http://localhost/"), null );
			  return jsonToMap( format.Assemble( repr ) );
		 }
	}

}