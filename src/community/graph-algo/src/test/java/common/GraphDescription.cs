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

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;

	public class GraphDescription : GraphDefinition
	{

		 private class RelationshipDescription
		 {
			  internal readonly string End;
			  internal readonly string Start;
			  internal readonly RelationshipType Type;

			  internal RelationshipDescription( string rel )
			  {
					string[] parts = rel.Split( " ", true );
					if ( parts.Length != 3 )
					{
						 throw new System.ArgumentException( "syntax error: \"" + rel + "\"" );
					}
					Start = parts[0];
					Type = RelationshipType.withName( parts[1] );
					End = parts[2];
			  }

			  public virtual Relationship Create( IGraphDatabaseService graphdb, IDictionary<string, Node> nodes )
			  {
					Node startNode = GetNode( graphdb, nodes, Start );
					Node endNode = GetNode( graphdb, nodes, End );
					return startNode.CreateRelationshipTo( endNode, Type );
			  }

			  internal virtual Node GetNode( IGraphDatabaseService graphdb, IDictionary<string, Node> nodes, string name )
			  {
					Node node = nodes[name];
					if ( node == null )
					{
						 if ( nodes.Count == 0 )
						 {
							  node = graphdb.CreateNode();
						 }
						 else
						 {
							  node = graphdb.CreateNode();
						 }
						 node.SetProperty( "name", name );
						 nodes[name] = node;
					}
					return node;
			  }
		 }

		 private readonly RelationshipDescription[] _description;

		 public GraphDescription( params string[] description )
		 {
			  IList<RelationshipDescription> lines = new List<RelationshipDescription>();
			  foreach ( string part in description )
			  {
					foreach ( string line in part.Split( "\n", true ) )
					{
						 lines.Add( new RelationshipDescription( line ) );
					}
			  }
			  this._description = lines.ToArray();
		 }

		 public override Node Create( IGraphDatabaseService graphdb )
		 {
			  IDictionary<string, Node> nodes = new Dictionary<string, Node>();
			  Node node = null;
			  using ( Transaction tx = graphdb.BeginTx() )
			  {
					foreach ( RelationshipDescription rel in _description )
					{
						 node = rel.Create( graphdb, nodes ).EndNode;
					}
					tx.Success();
			  }
			  return node;
		 }
	}

}