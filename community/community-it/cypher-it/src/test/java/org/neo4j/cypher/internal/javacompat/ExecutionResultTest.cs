﻿using System;

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
namespace Org.Neo4j.Cypher.@internal.javacompat
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Node = Org.Neo4j.Graphdb.Node;
	using QueryExecutionException = Org.Neo4j.Graphdb.QueryExecutionException;
	using Org.Neo4j.Graphdb;
	using Result = Org.Neo4j.Graphdb.Result;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Point = Org.Neo4j.Graphdb.spatial.Point;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using TopLevelTransaction = Org.Neo4j.Kernel.impl.coreapi.TopLevelTransaction;
	using QueryExecutionKernelException = Org.Neo4j.Kernel.impl.query.QueryExecutionKernelException;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsNull.notNullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsNull.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;

	public class ExecutionResultTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.ImpermanentDatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public readonly ImpermanentDatabaseRule Db = new ImpermanentDatabaseRule();

		 //TODO this test is not valid for compiled runtime as the transaction will be closed when the iterator was created
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseTransactionsWhenIteratingResults()
		 public virtual void ShouldCloseTransactionsWhenIteratingResults()
		 {
			  // Given an execution result that has been started but not exhausted
			  CreateNode();
			  CreateNode();
			  Result executionResult = Db.execute( "CYPHER runtime=interpreted MATCH (n) RETURN n" );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  executionResult.Next();
			  assertThat( ActiveTransaction(), @is(notNullValue()) );

			  // When
			  executionResult.Close();

			  // Then
			  assertThat( ActiveTransaction(), @is(nullValue()) );
		 }

		 //TODO this test is not valid for compiled runtime as the transaction will be closed when the iterator was created
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseTransactionsWhenIteratingOverSingleColumn()
		 public virtual void ShouldCloseTransactionsWhenIteratingOverSingleColumn()
		 {
			  // Given an execution result that has been started but not exhausted
			  CreateNode();
			  CreateNode();
			  Result executionResult = Db.execute( "CYPHER runtime=interpreted MATCH (n) RETURN n" );
			  ResourceIterator<Node> resultIterator = executionResult.ColumnAs( "n" );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  resultIterator.next();
			  assertThat( ActiveTransaction(), @is(notNullValue()) );

			  // When
			  resultIterator.Close();

			  // Then
			  assertThat( ActiveTransaction(), @is(nullValue()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowAppropriateException()
		 public virtual void ShouldThrowAppropriateException()
		 {
			  try
			  {
					Db.execute( "RETURN rand()/0" ).next();
			  }
			  catch ( QueryExecutionException ex )
			  {
					assertThat( ex.InnerException, instanceOf( typeof( QueryExecutionKernelException ) ) );
					assertThat( ex.InnerException.InnerException, instanceOf( typeof( ArithmeticException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.neo4j.cypher.ArithmeticException.class) public void shouldThrowAppropriateExceptionAlsoWhenVisiting()
		 public virtual void ShouldThrowAppropriateExceptionAlsoWhenVisiting()
		 {
			  Db.execute( "RETURN rand()/0" ).accept( row => true );
		 }

		 private void CreateNode()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode();
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleListsOfPointsAsInput()
		 public virtual void ShouldHandleListsOfPointsAsInput()
		 {
			  // Given
			  Point point1 = ( Point ) Db.execute( "RETURN point({latitude: 12.78, longitude: 56.7}) as point" ).next().get("point");
			  Point point2 = ( Point ) Db.execute( "RETURN point({latitude: 12.18, longitude: 56.2}) as point" ).next().get("point");

			  // When
			  double distance = ( double ) Db.execute( "RETURN distance({points}[0], {points}[1]) as dist", map( "points", asList( point1, point2 ) ) ).next().get("dist");
			  // Then
			  assertThat( ( long )Math.Round( distance, MidpointRounding.AwayFromZero ), equalTo( 86107L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMapWithPointsAsInput()
		 public virtual void ShouldHandleMapWithPointsAsInput()
		 {
			  // Given
			  Point point1 = ( Point ) Db.execute( "RETURN point({latitude: 12.78, longitude: 56.7}) as point" ).next().get("point");
			  Point point2 = ( Point ) Db.execute( "RETURN point({latitude: 12.18, longitude: 56.2}) as point" ).next().get("point");

			  // When
			  double distance = ( double ) Db.execute( "RETURN distance({points}['p1'], {points}['p2']) as dist", map( "points", map( "p1", point1, "p2", point2 ) ) ).next().get("dist");
			  // Then
			  assertThat( ( long )Math.Round( distance, MidpointRounding.AwayFromZero ), equalTo( 86107L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleColumnAsWithNull()
		 public virtual void ShouldHandleColumnAsWithNull()
		 {
			  assertThat( Db.execute( "RETURN toLower(null) AS lower" ).columnAs<string>( "lower" ).next(), nullValue() );
		 }

		 private TopLevelTransaction ActiveTransaction()
		 {
			  ThreadToStatementContextBridge bridge = Db.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) );
			  KernelTransaction kernelTransaction = bridge.GetKernelTransactionBoundToThisThread( false );
			  return kernelTransaction == null ? null : new TopLevelTransaction( kernelTransaction );
		 }
	}

}