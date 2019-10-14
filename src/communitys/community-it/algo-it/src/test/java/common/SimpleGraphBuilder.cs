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
namespace Common
{

	using PathImpl = Neo4Net.Graphalgo.impl.util.PathImpl;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Neo4Net.Graphdb;
	using Neo4Net.Graphdb;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;

	public class SimpleGraphBuilder
	{
		 public const string KEY_ID = "name";

		 internal GraphDatabaseService GraphDb;
		 internal Dictionary<string, Node> Nodes;
		 internal Dictionary<Node, string> NodeNames;
		 internal ISet<Relationship> Edges;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal RelationshipType CurrentRelTypeConflict;

		 public SimpleGraphBuilder( GraphDatabaseService graphDb, RelationshipType relationshipType ) : base()
		 {
			  this.GraphDb = graphDb;
			  Nodes = new Dictionary<string, Node>();
			  NodeNames = new Dictionary<Node, string>();
			  Edges = new HashSet<Relationship>();
			  CurrentRelType = relationshipType;
		 }

		 public virtual void Clear()
		 {
			  foreach ( Node node in Nodes.Values )
			  {
					foreach ( Relationship relationship in node.Relationships )
					{
						 relationship.Delete();
					}
					node.Delete();
			  }
			  Nodes.Clear();
			  NodeNames.Clear();
			  Edges.Clear();
		 }

		 public virtual ISet<Relationship> AllEdges
		 {
			 get
			 {
				  return Edges;
			 }
		 }

		 public virtual ISet<Node> AllNodes
		 {
			 get
			 {
				  return NodeNames.Keys;
			 }
		 }

		 public virtual RelationshipType CurrentRelType
		 {
			 set
			 {
				  this.CurrentRelTypeConflict = value;
			 }
		 }

		 public virtual Node MakeNode( string id )
		 {
			  return MakeNode( id, Collections.emptyMap() );
		 }

		 public virtual Node MakeNode( string id, params object[] keyValuePairs )
		 {
			  return MakeNode( id, ToMap( keyValuePairs ) );
		 }

		 private IDictionary<string, object> ToMap( object[] keyValuePairs )
		 {
			  IDictionary<string, object> map = new Dictionary<string, object>();
			  for ( int i = 0; i < keyValuePairs.Length; i++ )
			  {
					map[keyValuePairs[i++].ToString()] = keyValuePairs[i];
			  }
			  return map;
		 }

		 public virtual Node MakeNode( string id, IDictionary<string, object> properties )
		 {
			  Node node = GraphDb.createNode();
			  Nodes[id] = node;
			  NodeNames[node] = id;
			  node.SetProperty( KEY_ID, id );
			  foreach ( KeyValuePair<string, object> property in properties.SetOfKeyValuePairs() )
			  {
					if ( property.Key.Equals( KEY_ID ) )
					{
						 throw new Exception( "Can't use '" + property.Key + "'" );
					}
					node.SetProperty( property.Key, property.Value );
			  }
			  return node;
		 }

		 public virtual Node GetNode( string id )
		 {
			  return GetNode( id, false );
		 }

		 public virtual Node GetNode( string id, bool force )
		 {
			  Node node = Nodes[id];
			  if ( node == null && force )
			  {
					node = MakeNode( id );
			  }
			  return node;
		 }

		 public virtual string GetNodeId( Node node )
		 {
			  return NodeNames[node];
		 }

		 public virtual Relationship MakeEdge( string node1, string node2 )
		 {
			  return MakeEdge( node1, node2, Collections.emptyMap() );
		 }

		 public virtual Relationship MakeEdge( string node1, string node2, IDictionary<string, object> edgeProperties )
		 {
			  Node n1 = GetNode( node1, true );
			  Node n2 = GetNode( node2, true );
			  Relationship relationship = n1.CreateRelationshipTo( n2, CurrentRelTypeConflict );
			  foreach ( KeyValuePair<string, object> property in edgeProperties.SetOfKeyValuePairs() )
			  {
					relationship.SetProperty( property.Key, property.Value );
			  }
			  Edges.Add( relationship );
			  return relationship;
		 }

		 public virtual Relationship MakeEdge( string node1, string node2, params object[] keyValuePairs )
		 {
			  return MakeEdge( node1, node2, ToMap( keyValuePairs ) );
		 }

		 /// <summary>
		 /// This creates a chain by adding a number of edges. Example: The input
		 /// string "a,b,c,d,e" makes the chain a->b->c->d->e </summary>
		 /// <param name="commaSeparatedNodeNames">
		 ///            A string with the node names separated by commas. </param>
		 public virtual void MakeEdgeChain( string commaSeparatedNodeNames )
		 {
			  string[] nodeNames = commaSeparatedNodeNames.Split( ",", true );
			  for ( int i = 0; i < nodeNames.Length - 1; ++i )
			  {
					MakeEdge( nodeNames[i], nodeNames[i + 1] );
			  }
		 }

		 /// <summary>
		 /// Same as makeEdgeChain, but with some property set on all edges. </summary>
		 /// <param name="commaSeparatedNodeNames">
		 ///            A string with the node names separated by commas. </param>
		 /// <param name="propertyName"> </param>
		 /// <param name="propertyValue"> </param>
		 public virtual void MakeEdgeChain( string commaSeparatedNodeNames, string propertyName, object propertyValue )
		 {
			  string[] nodeNames = commaSeparatedNodeNames.Split( ",", true );
			  for ( int i = 0; i < nodeNames.Length - 1; ++i )
			  {
					MakeEdge( nodeNames[i], nodeNames[i + 1], propertyName, propertyValue );
			  }
		 }

		 /// <summary>
		 /// This creates a number of edges from a number of node names, pairwise.
		 /// Example: Input "a,b,c,d" gives a->b and c->d </summary>
		 /// <param name="commaSeparatedNodeNames"> </param>
		 public virtual void MakeEdges( string commaSeparatedNodeNames )
		 {
			  string[] nodeNames = commaSeparatedNodeNames.Split( ",", true );
			  for ( int i = 0; i < nodeNames.Length / 2; ++i )
			  {
					MakeEdge( nodeNames[i * 2], nodeNames[i * 2 + 1] );
			  }
		 }

		 /// <param name="node1Id"> </param>
		 /// <param name="node2Id"> </param>
		 /// <returns> One relationship between two given nodes, if there exists one,
		 ///         otherwise null. </returns>
		 public virtual Relationship GetRelationship( string node1Id, string node2Id )
		 {
			  Node node1 = GetNode( node1Id );
			  Node node2 = GetNode( node2Id );
			  if ( node1 == null || node2 == null )
			  {
					return null;
			  }
			  ResourceIterable<Relationship> relationships = Iterables.asResourceIterable( node1.Relationships );
			  using ( ResourceIterator<Relationship> resourceIterator = relationships.GetEnumerator() )
			  {
					while ( resourceIterator.MoveNext() )
					{
						 Relationship relationship = resourceIterator.Current;
						 if ( relationship.GetOtherNode( node1 ).Equals( node2 ) )
						 {
							  return relationship;
						 }
					}
			  }
			  return null;
		 }

		 // Syntax: makePathWithRelProperty( "weight", "a-4-b-2.3-c-3-d" )
		 public virtual Path MakePathWithRelProperty( string relPropertyName, string dashSeparatedNodeNamesAndRelationshipProperty )
		 {
			  string[] nodeNamesAndRelationshipProperties = dashSeparatedNodeNamesAndRelationshipProperty.Split( "-", true );
			  Node startNode = GetNode( nodeNamesAndRelationshipProperties[0], true );
			  PathImpl.Builder builder = new PathImpl.Builder( startNode );

			  if ( nodeNamesAndRelationshipProperties.Length < 1 )
			  {
					return builder.Build();
			  }

			  for ( int i = 0; i < nodeNamesAndRelationshipProperties.Length - 2; i += 2 )
			  {
					string from = nodeNamesAndRelationshipProperties[i];
					string to = nodeNamesAndRelationshipProperties[i + 2];
					string prop = nodeNamesAndRelationshipProperties[i + 1];
					Relationship relationship = MakeEdge( from, to, relPropertyName, prop );
					builder = builder.Push( relationship );
			  }
			  return builder.Build();
		 }
	}

}