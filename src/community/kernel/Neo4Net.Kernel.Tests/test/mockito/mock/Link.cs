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
namespace Neo4Net.Test.mockito.mock
{
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;

	public class Link
	{
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static Link LinkConflict( Relationship relationship, Node node )
		 {
			  if ( relationship.StartNode.Id == node.Id )
			  {
					return new Link( node, relationship );
			  }
			  if ( relationship.EndNode.Id == node.Id )
			  {
					return new Link( relationship, node );
			  }
			  throw IllegalArgument( "%s is neither the start node nor the end node of %s", node, relationship );
		 }

		 internal readonly Relationship Relationship;
		 private readonly Node _node;
		 private readonly bool _isStartNode;

		 private Link( Node node, Relationship relationship )
		 {
			  this.Relationship = relationship;
			  this._node = node;
			  this._isStartNode = true;
		 }

		 private Link( Relationship relationship, Node node )
		 {
			  this.Relationship = relationship;
			  this._node = node;
			  this._isStartNode = false;
		 }

		 public virtual Node CheckNode( Node node )
		 {
			  if ( _isStartNode )
			  {
					if ( node.Id != Relationship.EndNode.Id )
					{
						 throw IllegalArgument( "%s is not the end node of %s", node, Relationship );
					}
			  }
			  else
			  {
					if ( node.Id != Relationship.StartNode.Id )
					{
						 throw IllegalArgument( "%s is not the start node of %s", node, Relationship );
					}
			  }
			  return this._node;
		 }

		 private static System.ArgumentException IllegalArgument( string message, params object[] parameters )
		 {
			  return new System.ArgumentException( string.format( message, parameters ) );
		 }
	}

}