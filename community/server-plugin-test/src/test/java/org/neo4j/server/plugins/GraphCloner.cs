using System.Collections.Generic;

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
namespace Org.Neo4j.Server.plugins
{

	using Direction = Org.Neo4j.Graphdb.Direction;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using PathExpanders = Org.Neo4j.Graphdb.PathExpanders;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Evaluation = Org.Neo4j.Graphdb.traversal.Evaluation;
	using TraversalDescription = Org.Neo4j.Graphdb.traversal.TraversalDescription;
	using Traverser = Org.Neo4j.Graphdb.traversal.Traverser;

	[Description("Clones a subgraph (an example taken from a community mailing list requirement)")]
	public class GraphCloner : ServerPlugin
	{

		 public GraphCloner() : base("GraphCloner")
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.Node.class) public org.neo4j.graphdb.Node clonedSubgraph(@Source Node startNode, @Parameter(name = "depth", optional = false) System.Nullable<int> depth)
		 [PluginTarget(Org.Neo4j.Graphdb.Node.class)]
		 public virtual Node ClonedSubgraph( Node startNode, int? depth )
		 {
			  GraphDatabaseService graphDb = startNode.GraphDatabase;
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					Traverser traverse = TraverseToDepth( graphDb, startNode, depth.Value );
					IEnumerator<Node> nodes = traverse.Nodes().GetEnumerator();

					Dictionary<Node, Node> clonedNodes = CloneNodes( graphDb, nodes );

					foreach ( Node oldNode in clonedNodes.Keys )
					{
						 // give me the matching new node
						 Node newStartNode = clonedNodes[oldNode];

						 // Now let's go through the relationships and copy them over
						 IEnumerator<Relationship> oldRelationships = oldNode.GetRelationships( Direction.OUTGOING ).GetEnumerator();
						 while ( oldRelationships.MoveNext() )
						 {
							  Relationship oldRelationship = oldRelationships.Current;

							  Node newEndNode = clonedNodes[oldRelationship.EndNode];
							  if ( newEndNode != null )
							  {
									Relationship newRelationship = newStartNode.CreateRelationshipTo( newEndNode, oldRelationship.Type );

									CloneProperties( oldRelationship, newRelationship );
							  }
						 }
					}

					tx.Success();

					return clonedNodes[startNode];

			  }
		 }

		 private void CloneProperties( Relationship oldRelationship, Relationship newRelationship )
		 {
			  foreach ( KeyValuePair<string, object> property in oldRelationship.AllProperties.SetOfKeyValuePairs() )
			  {
					newRelationship.SetProperty( property.Key, property.Value );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.graphdb.traversal.Traverser traverseToDepth(org.neo4j.graphdb.GraphDatabaseService graphDb, final org.neo4j.graphdb.Node startNode, final int depth)
		 private Traverser TraverseToDepth( GraphDatabaseService graphDb, Node startNode, int depth )
		 {

			  TraversalDescription traversalDescription = graphDb.TraversalDescription().expand(PathExpanders.allTypesAndDirections()).depthFirst().evaluator(path =>
			  {
						  if ( path.length() < depth )
						  {
								return Evaluation.INCLUDE_AND_CONTINUE;
						  }
						  else
						  {
								return Evaluation.INCLUDE_AND_PRUNE;
						  }
			  });

			  return traversalDescription.Traverse( startNode );

		 }

		 private Node CloneNodeData( GraphDatabaseService graphDb, Node node )
		 {
			  Node newNode = graphDb.CreateNode();
			  foreach ( KeyValuePair<string, object> property in node.AllProperties.SetOfKeyValuePairs() )
			  {
					newNode.SetProperty( property.Key, property.Value );
			  }
			  return newNode;
		 }

		 private Dictionary<Node, Node> CloneNodes( GraphDatabaseService graphDb, IEnumerator<Node> nodes )
		 {
			  Dictionary<Node, Node> result = new Dictionary<Node, Node>();

			  while ( nodes.MoveNext() )
			  {
					Node next = nodes.Current;
					result[next] = CloneNodeData( graphDb, next );
			  }

			  return result;
		 }
	}

}