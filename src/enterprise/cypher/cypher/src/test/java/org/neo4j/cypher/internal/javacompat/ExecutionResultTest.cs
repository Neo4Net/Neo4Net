using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Cypher.Internal.javacompat
{
	using CoreMatchers = org.hamcrest.CoreMatchers;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ExecutionPlanDescription = Neo4Net.Graphdb.ExecutionPlanDescription;
	using Result = Neo4Net.Graphdb.Result;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using TestEnterpriseGraphDatabaseFactory = Neo4Net.Test.TestEnterpriseGraphDatabaseFactory;
	using EnterpriseDatabaseRule = Neo4Net.Test.rule.EnterpriseDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsNull.notNullValue;

	public class ExecutionResultTest
	{
		 private const string CURRENT_VERSION = "CYPHER 3.5";
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.EnterpriseDatabaseRule db = new org.neo4j.test.rule.EnterpriseDatabaseRule();
		 public readonly EnterpriseDatabaseRule Db = new EnterpriseDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBePossibleToConsumeCompiledExecutionResultsWithIterator()
		 public virtual void ShouldBePossibleToConsumeCompiledExecutionResultsWithIterator()
		 {
			  // Given
			  CreateNode();
			  CreateNode();

			  // When
			  IList<IDictionary<string, object>> listResult;
			  using ( Result result = Db.execute( "CYPHER runtime=compiled MATCH (n) RETURN n" ) )
			  {
					listResult = Iterators.asList( result );
			  }

			  // Then
			  assertThat( listResult, hasSize( 2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBePossibleToCloseNotFullyConsumedCompiledExecutionResults()
		 public virtual void ShouldBePossibleToCloseNotFullyConsumedCompiledExecutionResults()
		 {
			  // Given
			  CreateNode();
			  CreateNode();

			  // When
			  IDictionary<string, object> firstRow = null;
			  using ( Result result = Db.execute( "CYPHER runtime=compiled MATCH (n) RETURN n" ) )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( result.HasNext() )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 firstRow = result.Next();
					}
			  }

			  // Then
			  assertThat( firstRow, notNullValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBePossibleToConsumeCompiledExecutionResultsWithVisitor()
		 public virtual void ShouldBePossibleToConsumeCompiledExecutionResultsWithVisitor()
		 {
			  // Given
			  CreateNode();
			  CreateNode();

			  // When
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<org.neo4j.graphdb.Result_ResultRow> listResult = new java.util.ArrayList<>();
			  IList<Neo4Net.Graphdb.Result_ResultRow> listResult = new List<Neo4Net.Graphdb.Result_ResultRow>();
			  using ( Result result = Db.execute( "CYPHER runtime=compiled MATCH (n) RETURN n" ) )
			  {
					result.Accept(row =>
					{
					 listResult.Add( row );
					 return true;
					});
			  }

			  // Then
			  assertThat( listResult, hasSize( 2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBePossibleToCloseNotFullyVisitedCompiledExecutionResult()
		 public virtual void ShouldBePossibleToCloseNotFullyVisitedCompiledExecutionResult()
		 {
			  // Given
			  CreateNode();
			  CreateNode();

			  // When
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<org.neo4j.graphdb.Result_ResultRow> listResult = new java.util.ArrayList<>();
			  IList<Neo4Net.Graphdb.Result_ResultRow> listResult = new List<Neo4Net.Graphdb.Result_ResultRow>();
			  using ( Result result = Db.execute( "CYPHER runtime=compiled MATCH (n) RETURN n" ) )
			  {
					result.Accept(row =>
					{
					 listResult.Add( row );
					 // return false so that no more result rows would be visited
					 return false;
					});
			  }

			  // Then
			  assertThat( listResult, hasSize( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBePossibleToCloseNotConsumedCompiledExecutionResult()
		 public virtual void ShouldBePossibleToCloseNotConsumedCompiledExecutionResult()
		 {
			  // Given
			  CreateNode();

			  // Then
			  // just close result without consuming it
			  Db.execute( "CYPHER runtime=compiled MATCH (n) RETURN n" ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateAndDropUniqueConstraints()
		 public virtual void ShouldCreateAndDropUniqueConstraints()
		 {
			  Result create = Db.execute( "CREATE CONSTRAINT ON (n:L) ASSERT n.prop IS UNIQUE" );
			  Result drop = Db.execute( "DROP CONSTRAINT ON (n:L) ASSERT n.prop IS UNIQUE" );

			  assertThat( create.QueryStatistics.ConstraintsAdded, equalTo( 1 ) );
			  assertThat( create.QueryStatistics.ConstraintsRemoved, equalTo( 0 ) );
			  assertThat( drop.QueryStatistics.ConstraintsAdded, equalTo( 0 ) );
			  assertThat( drop.QueryStatistics.ConstraintsRemoved, equalTo( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateAndDropExistenceConstraints()
		 public virtual void ShouldCreateAndDropExistenceConstraints()
		 {
			  Result create = Db.execute( "CREATE CONSTRAINT ON (n:L) ASSERT exists(n.prop)" );
			  Result drop = Db.execute( "DROP CONSTRAINT ON (n:L) ASSERT exists(n.prop)" );

			  assertThat( create.QueryStatistics.ConstraintsAdded, equalTo( 1 ) );
			  assertThat( create.QueryStatistics.ConstraintsRemoved, equalTo( 0 ) );
			  assertThat( drop.QueryStatistics.ConstraintsAdded, equalTo( 0 ) );
			  assertThat( drop.QueryStatistics.ConstraintsRemoved, equalTo( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShowRuntimeInExecutionPlanDescription()
		 public virtual void ShouldShowRuntimeInExecutionPlanDescription()
		 {
			  // Given
			  Result result = Db.execute( "EXPLAIN MATCH (n) RETURN n.prop" );

			  // When
			  IDictionary<string, object> arguments = result.ExecutionPlanDescription.Arguments;

			  // Then
			  assertThat( arguments["version"], equalTo( CURRENT_VERSION ) );
			  assertThat( arguments["planner"], equalTo( "COST" ) );
			  assertThat( arguments["planner-impl"], equalTo( "IDP" ) );
			  assertThat( arguments["runtime"], notNullValue() );
			  assertThat( arguments["runtime-impl"], notNullValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShowCompiledRuntimeInExecutionPlan()
		 public virtual void ShouldShowCompiledRuntimeInExecutionPlan()
		 {
			  // Given
			  Result result = Db.execute( "EXPLAIN CYPHER runtime=compiled MATCH (n) RETURN n.prop" );

			  // When
			  IDictionary<string, object> arguments = result.ExecutionPlanDescription.Arguments;

			  // Then
			  assertThat( arguments["version"], equalTo( CURRENT_VERSION ) );
			  assertThat( arguments["planner"], equalTo( "COST" ) );
			  assertThat( arguments["planner-impl"], equalTo( "IDP" ) );
			  assertThat( arguments["runtime"], equalTo( "COMPILED" ) );
			  assertThat( arguments["runtime-impl"], equalTo( "COMPILED" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShowInterpretedRuntimeInExecutionPlan()
		 public virtual void ShouldShowInterpretedRuntimeInExecutionPlan()
		 {
			  // Given
			  Result result = Db.execute( "EXPLAIN CYPHER runtime=interpreted MATCH (n) RETURN n.prop" );

			  // When
			  IDictionary<string, object> arguments = result.ExecutionPlanDescription.Arguments;

			  // Then
			  assertThat( arguments["version"], equalTo( CURRENT_VERSION ) );
			  assertThat( arguments["planner"], equalTo( "COST" ) );
			  assertThat( arguments["planner-impl"], equalTo( "IDP" ) );
			  assertThat( arguments["runtime"], equalTo( "INTERPRETED" ) );
			  assertThat( arguments["runtime-impl"], equalTo( "INTERPRETED" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShowArgumentsExecutionPlan()
		 public virtual void ShouldShowArgumentsExecutionPlan()
		 {
			  // Given
			  Result result = Db.execute( "EXPLAIN CALL db.labels()" );

			  // When
			  IDictionary<string, object> arguments = result.ExecutionPlanDescription.Arguments;

			  // Then
			  assertThat( arguments["version"], equalTo( CURRENT_VERSION ) );
			  assertThat( arguments["planner"], equalTo( "PROCEDURE" ) );
			  assertThat( arguments["planner-impl"], equalTo( "PROCEDURE" ) );
			  assertThat( arguments["runtime"], equalTo( "PROCEDURE" ) );
			  assertThat( arguments["runtime-impl"], equalTo( "PROCEDURE" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShowArgumentsInProfileExecutionPlan()
		 public virtual void ShouldShowArgumentsInProfileExecutionPlan()
		 {
			  // Given
			  Result result = Db.execute( "PROFILE CALL db.labels()" );
			  result.ResultAsString();

			  // When
			  IDictionary<string, object> arguments = result.ExecutionPlanDescription.Arguments;

			  // Then
			  assertThat( arguments["version"], equalTo( CURRENT_VERSION ) );
			  assertThat( arguments["planner"], equalTo( "PROCEDURE" ) );
			  assertThat( arguments["planner-impl"], equalTo( "PROCEDURE" ) );
			  assertThat( arguments["runtime"], equalTo( "PROCEDURE" ) );
			  assertThat( arguments["runtime-impl"], equalTo( "PROCEDURE" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShowArgumentsInSchemaExecutionPlan()
		 public virtual void ShouldShowArgumentsInSchemaExecutionPlan()
		 {
			  // Given
			  Result result = Db.execute( "EXPLAIN CREATE INDEX ON :L(prop)" );

			  // When
			  IDictionary<string, object> arguments = result.ExecutionPlanDescription.Arguments;

			  // Then
			  assertThat( arguments["version"], equalTo( CURRENT_VERSION ) );
			  assertThat( arguments["planner"], equalTo( "PROCEDURE" ) );
			  assertThat( arguments["planner-impl"], equalTo( "PROCEDURE" ) );
			  assertThat( arguments["runtime"], equalTo( "PROCEDURE" ) );
			  assertThat( arguments["runtime-impl"], equalTo( "PROCEDURE" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnListFromSplit()
		 public virtual void ShouldReturnListFromSplit()
		 {
			  assertThat( Db.execute( "RETURN split('hello, world', ',') AS s" ).next().get("s"), instanceOf(typeof(System.Collections.IList)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnCorrectArrayType()
		 public virtual void ShouldReturnCorrectArrayType()
		 {
			  // Given
			  Db.execute( "CREATE (p:Person {names:['adsf', 'adf' ]})" );

			  // When
			  object result = Db.execute( "MATCH (n) RETURN n.names" ).next().get("n.names");

			  // Then
			  assertThat( result, CoreMatchers.instanceOf( typeof( string[] ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldContainCompletePlanFromFromLegacyVersions()
		 public virtual void ShouldContainCompletePlanFromFromLegacyVersions()
		 {
			  foreach ( string version in new string[]{ "2.3", "3.1", "3.4", "3.5" } )
			  {
					// Given
					Result result = Db.execute( string.Format( "EXPLAIN CYPHER {0} MATCH (n) RETURN n", version ) );

					// When
					ExecutionPlanDescription description = result.ExecutionPlanDescription;

					// Then
					assertThat( description.Name, equalTo( "ProduceResults" ) );
					assertThat( description.Children[0].Name, equalTo( "AllNodesScan" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldContainCompleteProfileFromFromLegacyVersions()
		 public virtual void ShouldContainCompleteProfileFromFromLegacyVersions()
		 {
			  // Given
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode();

					tx.Success();
			  }

			  foreach ( string version in new string[]{ "2.3", "3.1", "3.4", "3.5" } )
			  {
					// When
					Result result = Db.execute( string.Format( "PROFILE CYPHER {0} MATCH (n) RETURN n", version ) );
					result.ResultAsString();
					Neo4Net.Graphdb.ExecutionPlanDescription_ProfilerStatistics stats = result.ExecutionPlanDescription.Children[0].ProfilerStatistics;

					// Then
					assertThat( "Mismatching db-hits for version " + version, stats.DbHits, equalTo( 2L ) );
					assertThat( "Mismatching rows for version " + version, stats.Rows, equalTo( 1L ) );

					//These stats are not available in older versions, but should at least return 0, and >0 for newer
					assertThat( "Mismatching page cache hits for version " + version, stats.PageCacheHits, greaterThanOrEqualTo( 0L ) );
					assertThat( "Mismatching page cache misses for version " + version, stats.PageCacheMisses, greaterThanOrEqualTo( 0L ) );
					assertThat( "Mismatching page cache hit ratio for version " + version, stats.PageCacheHitRatio, greaterThanOrEqualTo( 0.0 ) );
			  }
		 }

		 private void CreateNode()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode();
					tx.Success();
			  }
		 }
	}

}