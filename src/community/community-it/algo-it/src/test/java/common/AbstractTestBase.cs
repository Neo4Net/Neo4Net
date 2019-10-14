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
namespace Common
{
	using AfterClass = org.junit.AfterClass;
	using BeforeClass = org.junit.BeforeClass;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public abstract class AbstractTestBase
	{
		 private static GraphDatabaseService _graphdb;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void beforeSuite()
		 public static void BeforeSuite()
		 {
			  _graphdb = ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void afterSuite()
		 public static void AfterSuite()
		 {
			  _graphdb.shutdown();
			  _graphdb = null;
		 }

		 protected internal static Node GetNode( long id )
		 {
			  return _graphdb.getNodeById( id );
		 }

		 protected internal static Transaction BeginTx()
		 {
			  return _graphdb.beginTx();
		 }

		 protected internal interface Representation<T>
		 {
			  string Represent( T item );
		 }

		 protected internal sealed class RelationshipRepresentation : Representation<Relationship>
		 {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private final Representation<? super org.neo4j.graphdb.Node> nodes;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  internal readonly Representation<object> Nodes;
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private final Representation<? super org.neo4j.graphdb.Relationship> rel;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  internal readonly Representation<object> Rel;

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public RelationshipRepresentation(Representation<? super org.neo4j.graphdb.Node> nodes, Representation<? super org.neo4j.graphdb.Relationship> rel)
			  public RelationshipRepresentation<T1, T2>( Representation<T1> nodes, Representation<T2> rel )
			  {
					this.Nodes = nodes;
					this.Rel = rel;
			  }

			  public override string Represent( Relationship item )
			  {
					return Nodes.represent( item.StartNode ) + " "
							 + Rel.represent( item ) + " "
							 + Nodes.represent( item.EndNode );
			  }
		 }

		 protected internal static void Expect<T, T1>( IEnumerable<T1> items, Representation<T> representation, params string[] expected ) where T1 : T
		 {
			  Expect( items, representation, new HashSet<string>( Arrays.asList( expected ) ) );
		 }

		 protected internal static void Expect<T, T1>( IEnumerable<T1> items, Representation<T> representation, ISet<string> expected ) where T1 : T
		 {
			  using ( Transaction tx = BeginTx() )
			  {
					foreach ( T item in items )
					{
						 string repr = representation.Represent( item );
						 assertTrue( repr + " not expected ", expected.remove( repr ) );
					}
					tx.Success();
			  }

			  if ( expected.Count > 0 )
			  {
					fail( "The expected elements " + expected + " were not returned." );
			  }
		 }
	}

}