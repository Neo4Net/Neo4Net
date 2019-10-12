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
	using Matcher = org.hamcrest.Matcher;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.hasProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.inTx;

	public class CypherUpdateMapTest
	{
		 private GraphDatabaseService _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void updateNodeByMapParameter()
		 public virtual void UpdateNodeByMapParameter()
		 {
			  _db.execute( "CREATE (n:Reference) SET n = {data} RETURN n", map( "data", map( "key1", "value1", "key2", 1234 ) ) );

			  Node node1 = GetNodeByIdInTx( 0 );

			  assertThat( node1, InTxS( hasProperty( "key1" ).withValue( "value1" ) ) );
			  assertThat( node1, InTxS( hasProperty( "key2" ).withValue( 1234 ) ) );

			  _db.execute( "MATCH (n:Reference) SET n = {data} RETURN n", map( "data", map( "key1", null, "key3", 5678 ) ) );

			  Node node2 = GetNodeByIdInTx( 0 );

			  assertThat( node2, InTxS( not( hasProperty( "key1" ) ) ) );
			  assertThat( node2, InTxS( not( hasProperty( "key2" ) ) ) );
			  assertThat( node2, InTxS( hasProperty( "key3" ).withValue( 5678 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <T> org.hamcrest.Matcher<? super T> inTxS(final org.hamcrest.Matcher<T> inner)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public virtual Matcher<object> InTxS<T>( Matcher<T> inner )
		 {
			  return inTx( _db, inner, false );
		 }

		 private Node GetNodeByIdInTx( int nodeId )
		 {
			  using ( Transaction ignored = _db.beginTx() )
			  {
					return _db.getNodeById( nodeId );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _db = ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup()
		 public virtual void Cleanup()
		 {
			  _db.shutdown();
		 }
	}

}