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
namespace Neo4Net.Kernel.impl.util
{
	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using NodeValue = Neo4Net.Values.@virtual.NodeValue;
	using PathValue = Neo4Net.Values.@virtual.PathValue;
	using RelationshipValue = Neo4Net.Values.@virtual.RelationshipValue;

	public class PathWrappingPathValue : PathValue
	{
		 private readonly Path _path;

		 internal PathWrappingPathValue( Path path )
		 {
			  this._path = path;
		 }

		 public override NodeValue StartNode()
		 {
			  return ValueUtils.FromNodeProxy( _path.startNode() );
		 }

		 public override NodeValue EndNode()
		 {
			  return ValueUtils.FromNodeProxy( _path.endNode() );
		 }

		 public override RelationshipValue LastRelationship()
		 {
			  return ValueUtils.FromRelationshipProxy( _path.lastRelationship() );
		 }

		 public override NodeValue[] Nodes()
		 {
			  int length = _path.length() + 1;
			  NodeValue[] values = new NodeValue[length];
			  int i = 0;
			  foreach ( Node node in _path.nodes() )
			  {
					values[i++] = ValueUtils.FromNodeProxy( node );
			  }
			  return values;
		 }

		 public override RelationshipValue[] Relationships()
		 {
			  int length = _path.length();
			  RelationshipValue[] values = new RelationshipValue[length];
			  int i = 0;
			  foreach ( Relationship relationship in _path.relationships() )
			  {
					values[i++] = ValueUtils.FromRelationshipProxy( relationship );
			  }
			  return values;
		 }

		 public virtual Path Path()
		 {
			  return _path;
		 }
	}

}