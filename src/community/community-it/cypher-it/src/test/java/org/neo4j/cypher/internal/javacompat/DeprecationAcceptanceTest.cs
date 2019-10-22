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
namespace Neo4Net.Cypher.Internal.javacompat
{
	using Matcher = org.hamcrest.Matcher;
	using Test = org.junit.Test;


	using InputPosition = Neo4Net.GraphDb.InputPosition;
	using Notification = Neo4Net.GraphDb.Notification;
	using Result = Neo4Net.GraphDb.Result;
	using SeverityLevel = Neo4Net.GraphDb.SeverityLevel;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using Procedure = Neo4Net.Procedure.Procedure;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class DeprecationAcceptanceTest : NotificationTestSupport
	{
		private bool InstanceFieldsInitialized = false;

		public DeprecationAcceptanceTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_deprecatedFeatureWarning = Deprecation( "The query used a deprecated function." );
			_deprecatedRulePlanner = Deprecation( "The rule planner, which was used to plan this query, is deprecated and will " + "be discontinued soon. If you did not explicitly choose the rule planner, " + "you should try to change your query so that the rule planner is not used" );
			_deprecatedCompiledRuntime = Deprecation( "The compiled runtime, which was requested to execute this query, is deprecated " + "and will be removed in a future release." );
			_deprecatedStartWarning = Deprecation( "START has been deprecated and will be removed in a future version. " );
			_deprecatedCreateUnique = Deprecation( "CREATE UNIQUE is deprecated and will be removed in a future version." );
			_deprecatedProcedureWarning = Deprecation( "The query used a deprecated procedure." );
			_deprecatedProcedureReturnFieldWarning = Deprecation( "The query used a deprecated field from a procedure." );
			_deprecatedBindingWarning = Deprecation( "Binding relationships to a list in a variable length pattern is deprecated." );
			_deprecatedSeparatorWarning = Deprecation( "The semantics of using colon in the separation of alternative relationship " + "types in conjunction with the use of variable binding, inlined property " + "predicates, or variable length will change in a future version." );
		}

		 // DEPRECATED PRE-PARSER OPTIONS

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deprecatedRulePlanner()
		 public virtual void DeprecatedRulePlanner()
		 {
			  // when
			  Result result = Db().execute("EXPLAIN CYPHER planner=rule RETURN 1");

			  // then
			  assertThat( result.Notifications, ContainsItem( _deprecatedRulePlanner ) );
			  result.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deprecatedCompiledRuntime()
		 public virtual void DeprecatedCompiledRuntime()
		 {
			  // when
			  Result result = Db().execute("EXPLAIN CYPHER runtime=compiled RETURN 1");

			  // then
			  assertThat( result.Notifications, ContainsItem( _deprecatedCompiledRuntime ) );
			  result.Close();
		 }

		 // DEPRECATED FUNCTIONS

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deprecatedToInt()
		 public virtual void DeprecatedToInt()
		 {
			  Stream.of( "CYPHER 3.1", "CYPHER 3.5" ).forEach( version => assertNotifications( version + " EXPLAIN RETURN ToInt('1') AS one", ContainsItem( _deprecatedFeatureWarning ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deprecatedUpper()
		 public virtual void DeprecatedUpper()
		 {
			  Stream.of( "CYPHER 3.1", "CYPHER 3.5" ).forEach( version => assertNotifications( version + " EXPLAIN RETURN upper('foo') AS one", ContainsItem( _deprecatedFeatureWarning ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deprecatedLower()
		 public virtual void DeprecatedLower()
		 {
			  Stream.of( "CYPHER 3.1", "CYPHER 3.5" ).forEach( version => assertNotifications( version + " EXPLAIN RETURN lower('BAR') AS one", ContainsItem( _deprecatedFeatureWarning ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deprecatedRels()
		 public virtual void DeprecatedRels()
		 {
			  Stream.of( "CYPHER 3.1", "CYPHER 3.5" ).forEach( version => assertNotifications( version + " EXPLAIN MATCH p = ()-->() RETURN rels(p) AS r", ContainsItem( _deprecatedFeatureWarning ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deprecatedFilter()
		 public virtual void DeprecatedFilter()
		 {
			  AssertNotifications( "EXPLAIN WITH [1,2,3] AS list RETURN filter(x IN list WHERE x % 2 = 1) AS odds", ContainsItem( _deprecatedFeatureWarning ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deprecatedExtract()
		 public virtual void DeprecatedExtract()
		 {
			  AssertNotifications( "EXPLAIN WITH [1,2,3] AS list RETURN extract(x IN list | x * 10) AS tens", ContainsItem( _deprecatedFeatureWarning ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deprecatedFilterShouldNotHitCacheForNewVersion()
		 public virtual void DeprecatedFilterShouldNotHitCacheForNewVersion()
		 {
			  AssertNotifications( "EXPLAIN WITH [1,2,3] AS list RETURN filter(x IN list WHERE x % 2 = 1) AS odds", ContainsItem( _deprecatedFeatureWarning ) );

			  using ( Result result = Db().execute("EXPLAIN WITH [1,2,3] AS list RETURN [x IN list WHERE x % 2 = 1] AS odds") )
			  {
					assertFalse( result.Notifications.GetEnumerator().hasNext() );
			  }

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deprecatedExtractShouldNotHitCacheForNewVersion()
		 public virtual void DeprecatedExtractShouldNotHitCacheForNewVersion()
		 {
			  AssertNotifications( "EXPLAIN WITH [1,2,3] AS list RETURN extract(x IN list | x * 10) AS tens", ContainsItem( _deprecatedFeatureWarning ) );

			  using ( Result result = Db().execute("EXPLAIN WITH [1,2,3] AS list RETURN [x IN list | x * 10] AS tens") )
			  {
					assertFalse( result.Notifications.GetEnumerator().hasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deprecatedProcedureCalls() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DeprecatedProcedureCalls()
		 {
			  Db().DependencyResolver.provideDependency(typeof(Procedures)).get().registerProcedure(typeof(TestProcedures));
			  Stream.of( "CYPHER 3.1", "CYPHER 3.5" ).forEach(version =>
			  {
																				 AssertNotifications( version + "explain CALL oldProc()", ContainsItem( _deprecatedProcedureWarning ) );
																				 AssertNotifications( version + "explain CALL oldProc() RETURN 1", ContainsItem( _deprecatedProcedureWarning ) );
			  });
		 }

		 // DEPRECATED PROCEDURE THINGS

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deprecatedProcedureResultField() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DeprecatedProcedureResultField()
		 {
			  Db().DependencyResolver.provideDependency(typeof(Procedures)).get().registerProcedure(typeof(TestProcedures));
			  Stream.of( "CYPHER 3.5" ).forEach( version => assertNotifications( version + "explain CALL changedProc() YIELD oldField RETURN oldField", ContainsItem( _deprecatedProcedureReturnFieldWarning ) ) );
		 }

		 // DEPRECATED START

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deprecatedStartAllNodeScan()
		 public virtual void DeprecatedStartAllNodeScan()
		 {
			  AssertNotifications( "EXPLAIN START n=node(*) RETURN n", ContainsItem( _deprecatedStartWarning ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deprecatedStartNodeById()
		 public virtual void DeprecatedStartNodeById()
		 {
			  AssertNotifications( "EXPLAIN START n=node(1337) RETURN n", ContainsItem( _deprecatedStartWarning ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deprecatedStartNodeByIds()
		 public virtual void DeprecatedStartNodeByIds()
		 {
			  AssertNotifications( "EXPLAIN START n=node(42,1337) RETURN n", ContainsItem( _deprecatedStartWarning ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deprecatedStartNodeIndexSeek()
		 public virtual void DeprecatedStartNodeIndexSeek()
		 {
			  using ( Transaction ignore = Db().beginTx() )
			  {
					Db().index().forNodes("index");
			  }
			  AssertNotifications( "EXPLAIN START n=node:index(key = 'value') RETURN n", ContainsItem( _deprecatedStartWarning ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deprecatedStartNodeIndexSearch()
		 public virtual void DeprecatedStartNodeIndexSearch()
		 {
			  using ( Transaction ignore = Db().beginTx() )
			  {
					Db().index().forNodes("index");
			  }
			  AssertNotifications( "EXPLAIN START n=node:index('key:value*') RETURN n", ContainsItem( _deprecatedStartWarning ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deprecatedStartAllRelScan()
		 public virtual void DeprecatedStartAllRelScan()
		 {
			  AssertNotifications( "EXPLAIN START r=relationship(*) RETURN r", ContainsItem( _deprecatedStartWarning ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deprecatedStartRelById()
		 public virtual void DeprecatedStartRelById()
		 {
			  AssertNotifications( "EXPLAIN START r=relationship(1337) RETURN r", ContainsItem( _deprecatedStartWarning ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deprecatedStartRelByIds()
		 public virtual void DeprecatedStartRelByIds()
		 {
			  AssertNotifications( "EXPLAIN START r=relationship(42,1337) RETURN r", ContainsItem( _deprecatedStartWarning ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deprecatedStartRelIndexSeek()
		 public virtual void DeprecatedStartRelIndexSeek()
		 {
			  using ( Transaction ignore = Db().beginTx() )
			  {
					Db().index().forRelationships("index");
			  }
			  AssertNotifications( "EXPLAIN START r=relationship:index(key = 'value') RETURN r", ContainsItem( _deprecatedStartWarning ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deprecatedStartRelIndexSearch()
		 public virtual void DeprecatedStartRelIndexSearch()
		 {
			  using ( Transaction ignore = Db().beginTx() )
			  {
					Db().index().forRelationships("index");
			  }
			  AssertNotifications( "EXPLAIN START r=relationship:index('key:value*') RETURN r", ContainsItem( _deprecatedStartWarning ) );
		 }

		 // DEPRECATED CREATE UNIQUE

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotifyWhenUsingCreateUniqueWhenCypherVersionIsDefault()
		 public virtual void ShouldNotifyWhenUsingCreateUniqueWhenCypherVersionIsDefault()
		 {
			  // when
			  Result result = Db().execute("MATCH (b) WITH b LIMIT 1 CREATE UNIQUE (b)-[:REL]->()");

			  // then
			  assertThat( result.Notifications, ContainsItem( _deprecatedCreateUnique ) );
			  result.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotifyWhenUsingCreateUniqueWhenCypherVersionIs3_5()
		 public virtual void ShouldNotifyWhenUsingCreateUniqueWhenCypherVersionIs3_5()
		 {
			  // when
			  Result result = Db().execute("CYPHER 3.5 MATCH (b) WITH b LIMIT 1 CREATE UNIQUE (b)-[:REL]->()");
			  InputPosition position = new InputPosition( 36, 1, 37 );

			  // then
			  assertThat( result.Notifications, ContainsItem( _deprecatedCreateUnique ) );
			  result.Close();
		 }

		 // DEPRECATED SYNTAX

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deprecatedFutureAmbiguousRelTypeSeparator()
		 public virtual void DeprecatedFutureAmbiguousRelTypeSeparator()
		 {
			  IList<string> deprecatedQueries = Arrays.asList( "explain MATCH (a)-[:A|:B|:C {foo:'bar'}]-(b) RETURN a,b", "explain MATCH (a)-[x:A|:B|:C]-() RETURN a", "explain MATCH (a)-[:A|:B|:C*]-() RETURN a" );

			  IList<string> nonDeprecatedQueries = Arrays.asList( "explain MATCH (a)-[:A|B|C {foo:'bar'}]-(b) RETURN a,b", "explain MATCH (a)-[:A|:B|:C]-(b) RETURN a,b", "explain MATCH (a)-[:A|B|C]-(b) RETURN a,b" );

			  foreach ( string query in deprecatedQueries )
			  {
					AssertNotifications( "CYPHER 3.5 " + query, ContainsItem( _deprecatedSeparatorWarning ) );
			  }

			  foreach ( string query in nonDeprecatedQueries )
			  {
					AssertNotifications( "CYPHER 3.5 " + query, ContainsNoItem( _deprecatedSeparatorWarning ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deprecatedBindingVariableLengthRelationship()
		 public virtual void DeprecatedBindingVariableLengthRelationship()
		 {
			  AssertNotifications( "CYPHER 3.5 explain MATCH ()-[rs*]-() RETURN rs", ContainsItem( _deprecatedBindingWarning ) );

			  AssertNotifications( "CYPHER 3.5 explain MATCH p = ()-[*]-() RETURN relationships(p) AS rs", ContainsNoItem( _deprecatedBindingWarning ) );
		 }

		 // MATCHERS & HELPERS

		 public class ChangedResults
		 {
			  [Obsolete]
			  public readonly string OldField = "deprecated";
			  public readonly string NewField = "use this";
		 }

		 public class TestProcedures
		 {

			  [Procedure("newProc")]
			  public virtual void NewProc()
			  {
			  }

			  [Obsolete, Procedure(name : "oldProc", deprecatedBy : "newProc")]
			  public virtual void OldProc()
			  {
			  }

			  [Procedure("changedProc")]
			  public virtual Stream<ChangedResults> ChangedProc()
			  {
					return Stream.of( new ChangedResults() );
			  }
		 }

		 private Matcher<Notification> _deprecatedFeatureWarning;

		 private Matcher<Notification> _deprecatedRulePlanner;

		 private Matcher<Notification> _deprecatedCompiledRuntime;

		 private Matcher<Notification> _deprecatedStartWarning;

		 private Matcher<Notification> _deprecatedCreateUnique;

		 private Matcher<Notification> _deprecatedProcedureWarning;

		 private Matcher<Notification> _deprecatedProcedureReturnFieldWarning;

		 private Matcher<Notification> _deprecatedBindingWarning;

		 private Matcher<Notification> _deprecatedSeparatorWarning;

		 private Matcher<Notification> Deprecation( string message )
		 {
			  return Notification( "Neo.ClientNotification.Statement.FeatureDeprecationWarning", containsString( message ), any( typeof( InputPosition ) ), SeverityLevel.WARNING );
		 }
	}

}