﻿using System.Collections.Generic;

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
namespace Common
{

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;

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

			  public virtual Relationship Create( GraphDatabaseService graphdb, IDictionary<string, Node> nodes )
			  {
					Node startNode = GetNode( graphdb, nodes, Start );
					Node endNode = GetNode( graphdb, nodes, End );
					return startNode.CreateRelationshipTo( endNode, Type );
			  }

			  internal virtual Node GetNode( GraphDatabaseService graphdb, IDictionary<string, Node> nodes, string name )
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

		 public override Node Create( GraphDatabaseService graphdb )
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