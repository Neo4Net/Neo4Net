﻿/*
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
namespace Org.Neo4j.Cypher.@internal.javacompat
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using QueryExecutionEngine = Org.Neo4j.Kernel.impl.query.QueryExecutionEngine;
	using EmbeddedDatabaseRule = Org.Neo4j.Test.rule.EmbeddedDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class ServerExecutionEngineTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.EmbeddedDatabaseRule rule = new org.neo4j.test.rule.EmbeddedDatabaseRule();
		 public EmbeddedDatabaseRule Rule = new EmbeddedDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectPeriodicCommitQueries()
		 public virtual void ShouldDetectPeriodicCommitQueries()
		 {
			  // GIVEN
			  QueryExecutionEngine engine = Rule.GraphDatabaseAPI.DependencyResolver.resolveDependency( typeof( QueryExecutionEngine ) );

			  // WHEN
			  bool result = engine.IsPeriodicCommit( "USING PERIODIC COMMIT LOAD CSV FROM 'file:///tmp/foo.csv' AS line CREATE ()" );

			  // THEN
			  assertTrue( "Did not detect periodic commit query", result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDetectNonPeriodicCommitQueriesAsPeriodicCommitQueries()
		 public virtual void ShouldNotDetectNonPeriodicCommitQueriesAsPeriodicCommitQueries()
		 {
			  // GIVEN
			  QueryExecutionEngine engine = Rule.GraphDatabaseAPI.DependencyResolver.resolveDependency( typeof( QueryExecutionEngine ) );

			  // WHEN
			  bool result = engine.IsPeriodicCommit( "CREATE ()" );

			  // THEN
			  assertFalse( "Did detect non-periodic commit query as periodic commit query", result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDetectInvalidQueriesAsPeriodicCommitQueries()
		 public virtual void ShouldNotDetectInvalidQueriesAsPeriodicCommitQueries()
		 {
			  // GIVEN
			  QueryExecutionEngine engine = Rule.GraphDatabaseAPI.DependencyResolver.resolveDependency( typeof( QueryExecutionEngine ) );

			  // WHEN
			  bool result = engine.IsPeriodicCommit( "MATCH n RETURN m" );

			  // THEN
			  assertFalse( "Did detect an invalid query as periodic commit query", result );
		 }
	}

}