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
namespace Org.Neo4j.Kernel.impl.util
{
	using Node = Org.Neo4j.Graphdb.Node;
	using Path = Org.Neo4j.Graphdb.Path;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using NodeValue = Org.Neo4j.Values.@virtual.NodeValue;
	using PathValue = Org.Neo4j.Values.@virtual.PathValue;
	using RelationshipValue = Org.Neo4j.Values.@virtual.RelationshipValue;

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