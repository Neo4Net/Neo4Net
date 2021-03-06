﻿using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.Locking
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Org.Neo4j.Function;
	using Org.Neo4j.Function;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using ConfigBuilder = Org.Neo4j.Test.ConfigBuilder;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;
	using ThreadingRule = Org.Neo4j.Test.rule.concurrent.ThreadingRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.lock_manager;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.ConfigBuilder.configure;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.concurrent.ThreadingRule.await;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class MergeLockConcurrencyTest
	public class MergeLockConcurrencyTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule Db = new ImpermanentDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.concurrent.ThreadingRule threads = new org.neo4j.test.rule.concurrent.ThreadingRule();
		 public readonly ThreadingRule Threads = new ThreadingRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static Iterable<Object[]> configurations()
		 public static IEnumerable<object[]> Configurations()
		 {
			  return Arrays.asList( configure( lock_manager, "community" ).asParameters(), configure(lock_manager, "forseti").asParameters() );
		 }

		 public MergeLockConcurrencyTest( ConfigBuilder config )
		 {
			  Db.withSettings( config.Configuration() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDeadlockOnMergeFollowedByPropertyAssignment() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotDeadlockOnMergeFollowedByPropertyAssignment()
		 {
			  WithConstraint( MergeThen( this.reassignProperties ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDeadlockOnMergeFollowedByLabelReAddition() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotDeadlockOnMergeFollowedByLabelReAddition()
		 {
			  WithConstraint( MergeThen( this.reassignLabels ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void withConstraint(org.neo4j.function.ThrowingFunction<java.util.concurrent.CyclicBarrier,org.neo4j.graphdb.Node,Exception> action) throws Exception
		 private void WithConstraint( ThrowingFunction<CyclicBarrier, Node, Exception> action )
		 {
			  // given
			  Db.execute( "CREATE CONSTRAINT ON (foo:Foo) ASSERT foo.bar IS UNIQUE" );
			  CyclicBarrier barrier = new CyclicBarrier( 2 );
			  Node node = MergeNode();

			  // when
			  IList<Node> result = await( Threads.multiple( barrier.Parties, action, barrier ) );

			  // then
			  assertEquals( "size of result", 2, result.Count );
			  assertEquals( node, result[0] );
			  assertEquals( node, result[1] );
		 }

		 private ThrowingFunction<CyclicBarrier, Node, Exception> MergeThen<T1>( ThrowingConsumer<T1> action ) where T1 : Exception
		 {
			  return barrier =>
			  {
				using ( Transaction tx = Db.beginTx() )
				{
					 Node node = MergeNode();

					 barrier.await();

					 action.Accept( node );

					 tx.success();
					 return node;
				}
			  };
		 }

		 private Node MergeNode()
		 {
			  return ( Node ) single( Db.execute( "MERGE (foo:Foo{bar:'baz'}) RETURN foo" ) ).get( "foo" );
		 }

		 private void ReassignProperties( Node node )
		 {
			  foreach ( KeyValuePair<string, object> property in node.AllProperties.SetOfKeyValuePairs() )
			  {
					node.SetProperty( property.Key, property.Value );
			  }
		 }

		 private void ReassignLabels( Node node )
		 {
			  foreach ( Label label in node.Labels )
			  {
					node.AddLabel( label );
			  }
		 }
	}

}