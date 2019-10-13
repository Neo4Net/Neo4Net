using System.Collections.Generic;
using System.Text;

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
namespace Common
{
	using After = org.junit.After;
	using AfterClass = org.junit.AfterClass;
	using Before = org.junit.Before;
	using BeforeClass = org.junit.BeforeClass;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Neo4Net.Graphdb;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	/// <summary>
	/// Base class for test cases working on a NeoService. It sets up a NeoService
	/// and a transaction.
	/// @author Patrik Larsson
	/// </summary>
	public abstract class Neo4jAlgoTestCase
	{
		 protected internal static GraphDatabaseService GraphDb;
		 protected internal static SimpleGraphBuilder Graph;
		 protected internal Transaction Tx;

		 public enum MyRelTypes
		 {
			  R1,
			  R2,
			  R3
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setUpGraphDb()
		 public static void SetUpGraphDb()
		 {
			  GraphDb = ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
			  Graph = new SimpleGraphBuilder( GraphDb, MyRelTypes.R1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUpTransaction()
		 public virtual void SetUpTransaction()
		 {
			  Tx = GraphDb.beginTx();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void tearDownGraphDb()
		 public static void TearDownGraphDb()
		 {
			  GraphDb.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDownTransactionAndGraph()
		 public virtual void TearDownTransactionAndGraph()
		 {
			  Graph.clear();
			  Tx.success();
			  Tx.close();
		 }

		 protected internal virtual void AssertPathDef( Path path, params string[] names )
		 {
			  int i = 0;
			  foreach ( Node node in path.Nodes() )
			  {
					assertEquals( "Wrong node " + i + " in " + GetPathDef( path ), names[i++], node.GetProperty( SimpleGraphBuilder.KEY_ID ) );
			  }
			  assertEquals( names.Length, i );
		 }

		 protected internal virtual void AssertPath( Path path, string commaSeparatedNodePath )
		 {
			  string[] nodeIds = commaSeparatedNodePath.Split( ",", true );
			  Node[] nodes = new Node[nodeIds.Length];
			  int i = 0;
			  foreach ( string id in nodeIds )
			  {
					nodes[i] = Graph.getNode( id );
					i++;
			  }
			  AssertPath( path, nodes );
		 }

		 protected internal virtual void AssertPath( Path path, params Node[] nodes )
		 {
			  int i = 0;
			  foreach ( Node node in path.Nodes() )
			  {
					assertEquals( "Wrong node " + i + " in " + GetPathDef( path ), nodes[i++].GetProperty( SimpleGraphBuilder.KEY_ID ), node.GetProperty( SimpleGraphBuilder.KEY_ID ) );
			  }
			  assertEquals( nodes.Length, i );
		 }

		 protected internal virtual void AssertContains<E>( IEnumerable<E> actual, params E[] expected )
		 {
			  ISet<E> expectation = new HashSet<E>( Arrays.asList( expected ) );
			  foreach ( E element in actual )
			  {
					if ( !expectation.remove( element ) )
					{
						 fail( "unexpected element <" + element + ">" );
					}
			  }
			  if ( expectation.Count > 0 )
			  {
					fail( "the expected elements <" + expectation + "> were not contained" );
			  }
		 }

		 public virtual string GetPathDef( Path path )
		 {
			  StringBuilder builder = new StringBuilder();
			  foreach ( Node node in path.Nodes() )
			  {
					if ( builder.Length > 0 )
					{
						 builder.Append( "," );
					}
					builder.Append( node.GetProperty( SimpleGraphBuilder.KEY_ID ) );
			  }
			  return builder.ToString();
		 }

		 public virtual void AssertPaths<T1>( IEnumerable<T1> paths, IList<string> pathDefs ) where T1 : Neo4Net.Graphdb.Path
		 {
			  IList<string> unexpectedDefs = new List<string>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: try (org.neo4j.graphdb.ResourceIterator<? extends org.neo4j.graphdb.Path> iterator = org.neo4j.helpers.collection.Iterators.asResourceIterator(paths.iterator()))
			  using ( ResourceIterator<Path> iterator = Iterators.asResourceIterator( paths.GetEnumerator() ) )
			  {
					while ( iterator.MoveNext() )
					{
						 Path path = iterator.Current;

						 string pathDef = GetPathDef( path );
						 int index = pathDefs.IndexOf( pathDef );
						 if ( index != -1 )
						 {
							  pathDefs.RemoveAt( index );
						 }
						 else
						 {
							  unexpectedDefs.Add( GetPathDef( path ) );
						 }
					}
			  }
			  assertTrue( "These unexpected paths were found: " + unexpectedDefs + ". In addition these expected paths weren't found:" + pathDefs, unexpectedDefs.Count == 0 );
			  assertTrue( "These were expected, but not found: " + pathDefs.ToString(), pathDefs.Count == 0 );
		 }

		 public virtual void AssertPaths<T1>( IEnumerable<T1> paths, params string[] pathDefinitions ) where T1 : Neo4Net.Graphdb.Path
		 {
			  AssertPaths( paths, new List<string>( Arrays.asList( pathDefinitions ) ) );
		 }

		 public virtual void AssertPathsWithPaths<T1>( IEnumerable<T1> actualPaths, params Path[] expectedPaths ) where T1 : Neo4Net.Graphdb.Path
		 {
			  IList<string> pathDefs = new List<string>();
			  foreach ( Path path in expectedPaths )
			  {
					pathDefs.Add( GetPathDef( path ) );
			  }
			  AssertPaths( actualPaths, pathDefs );
		 }

		 public virtual void AssertPathDef( Path expected, Path actual )
		 {
			  int expectedLength = expected.Length();
			  int actualLength = actual.Length();
			  assertEquals( "Actual path length " + actualLength + " differ from expected path length " + expectedLength, expectedLength, actualLength );
			  IEnumerator<Node> expectedNodes = expected.Nodes().GetEnumerator();
			  IEnumerator<Node> actualNodes = actual.Nodes().GetEnumerator();
			  int position = 0;
			  while ( expectedNodes.MoveNext() && actualNodes.MoveNext() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( "Path differ on position " + position + ". Expected " + GetPathDef( expected ) + ", actual " + GetPathDef( actual ), expectedNodes.Current.getProperty( SimpleGraphBuilder.KEY_ID ), actualNodes.next().getProperty(SimpleGraphBuilder.KEY_ID) );
					position++;
			  }
		 }
	}

}