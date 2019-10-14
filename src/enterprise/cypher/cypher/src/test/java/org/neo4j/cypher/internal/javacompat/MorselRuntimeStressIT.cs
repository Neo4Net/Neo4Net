using System;
using System.Collections.Generic;
using System.Threading;

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
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Result = Neo4Net.Graphdb.Result;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using EnterpriseDatabaseRule = Neo4Net.Test.rule.EnterpriseDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsNull.notNullValue;

	public class MorselRuntimeStressIT
	{
		 private const int N_THREADS = 10;
		 private const int ITERATIONS = 10;
		 private const int CHUNKS = 100;
		 private const int N_NODES = 100;
		 private static readonly Label _label = Label.label( "LABEL" );
		 private const string EXPAND_QUERY = "CYPHER runtime=morsel MATCH (:LABEL)-->(n:LABEL) RETURN n";
		 private const string MATCH_NODE_QUERY = "CYPHER runtime=morsel MATCH (n:LABEL) RETURN n";
		 private const string SYNTAX_ERROR_QUERY = "CYPHER runtime=morsel MATHC (n) RETURN n";
		 private const string RUNTIME_ERROR_QUERY = "CYPHER runtime=morsel MATCH (n) RETURN size($a)";
		 private static readonly IDictionary<string, object> @params = new Dictionary<string, object>();

		 static MorselRuntimeStressIT()
		 {
			  @params["a"] = 42;
		 }

		 private static readonly RelationshipType _r = RelationshipType.withName( "R" );

		 private static readonly Neo4Net.Graphdb.Result_ResultVisitor<Exception> _checkingVisitor = row =>
		 {
		  assertThat( row.get( "n" ), notNullValue() );
		  return true;
		 };
		 private static readonly Neo4Net.Graphdb.Result_ResultVisitor<Exception> _throwingVisitor = row =>
		 {
		  throw new Exception( "WHERE IS YOUR GOD NOW" );
		 };

		 private AtomicInteger _counter = new AtomicInteger( 0 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.EnterpriseDatabaseRule db = new org.neo4j.test.rule.EnterpriseDatabaseRule();
		 public readonly EnterpriseDatabaseRule Db = new EnterpriseDatabaseRule();

		 private ExecutorService _service = Executors.newFixedThreadPool( N_THREADS );
		 private ThreadStart _task = () =>
		 {

		  for ( int i = 0; i < ITERATIONS; i++ )
		  {
				try
				{
					 Db.execute( Query(), @params ).accept(Visitor());
				}
				catch ( Exception )
				{
					 //ignore
				}
		  }
		  _counter.incrementAndGet();
		 };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void runTest() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RunTest()
		 {
			  for ( int i = 0; i < N_THREADS; i++ )
			  {
					_service.submit( _task );
			  }
			  _service.awaitTermination( 10, TimeUnit.SECONDS );
			  assertThat( _counter.get(), equalTo(N_THREADS) );
		 }

		 private Neo4Net.Graphdb.Result_ResultVisitor<Exception> Visitor()
		 {
			  ThreadLocalRandom random = ThreadLocalRandom.current();
			  switch ( random.Next( 2 ) )
			  {
			  case 0:
					return _checkingVisitor;
			  case 1:
					return _throwingVisitor;
			  default:
					throw new System.InvalidOperationException( "this is not a valid state" );
			  }
		 }

		 private string Query()
		 {
			  ThreadLocalRandom random = ThreadLocalRandom.current();
			  switch ( random.Next( 4 ) )
			  {
			  case 0:
					return EXPAND_QUERY;
			  case 1:
					return MATCH_NODE_QUERY;
			  case 2:
					return SYNTAX_ERROR_QUERY;
			  case 3:
					return RUNTIME_ERROR_QUERY;
			  default:
					throw new System.InvalidOperationException( "this is not a valid state" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  Transaction tx = null;

			  Node previous = null;
			  for ( int i = 0; i < N_NODES; i++ )
			  {
					if ( i % CHUNKS == 0 )
					{
						 if ( tx != null )
						 {
							  tx.Success();
							  tx.Close();
						 }
						 tx = Db.beginTx();
					}
					Node node = Db.createNode( _label );
					if ( previous != null )
					{
						 previous.CreateRelationshipTo( node, _r );
					}
					previous = node;
			  }
		 }
	}

}