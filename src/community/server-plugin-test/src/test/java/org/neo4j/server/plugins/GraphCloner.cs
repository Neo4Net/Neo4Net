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
namespace Neo4Net.Server.plugins
{

	using Direction = Neo4Net.GraphDb.Direction;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using PathExpanders = Neo4Net.GraphDb.PathExpanders;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Evaluation = Neo4Net.GraphDb.Traversal.Evaluation;
	using TraversalDescription = Neo4Net.GraphDb.Traversal.TraversalDescription;
	using Traverser = Neo4Net.GraphDb.Traversal.Traverser;

	[Description("Clones a subgraph (an example taken from a community mailing list requirement)")]
	public class GraphCloner : ServerPlugin
	{

		 public GraphCloner() : base("GraphCloner")
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.Neo4Net.graphdb.Node.class) public org.Neo4Net.graphdb.Node clonedSubgraph(@Source Node startNode, @Parameter(name = "depth", optional = false) System.Nullable<int> depth)
		 [PluginTarget(Neo4Net.GraphDb.Node.class)]
		 public virtual Node ClonedSubgraph( Node startNode, int? depth )
		 {
			  IGraphDatabaseService graphDb = startNode.GraphDatabase;
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
//ORIGINAL LINE: private org.Neo4Net.graphdb.traversal.Traverser traverseToDepth(org.Neo4Net.graphdb.GraphDatabaseService graphDb, final org.Neo4Net.graphdb.Node startNode, final int depth)
		 private Traverser TraverseToDepth( IGraphDatabaseService graphDb, Node startNode, int depth )
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

		 private Node CloneNodeData( IGraphDatabaseService graphDb, Node node )
		 {
			  Node newNode = graphDb.CreateNode();
			  foreach ( KeyValuePair<string, object> property in node.AllProperties.SetOfKeyValuePairs() )
			  {
					newNode.SetProperty( property.Key, property.Value );
			  }
			  return newNode;
		 }

		 private Dictionary<Node, Node> CloneNodes( IGraphDatabaseService graphDb, IEnumerator<Node> nodes )
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