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
namespace Org.Neo4j.Server.rest.repr
{
	using Node = Org.Neo4j.Graphdb.Node;
	using Org.Neo4j.Graphdb.index;
	using IndexManager = Org.Neo4j.Graphdb.index.IndexManager;

	public class NodeIndexRootRepresentation : MappingRepresentation
	{
		 private IndexManager _indexManager;

		 public NodeIndexRootRepresentation( IndexManager indexManager ) : base( "node-index" )
		 {
			  this._indexManager = indexManager;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected void serialize(final MappingSerializer serializer)
		 protected internal override void Serialize( MappingSerializer serializer )
		 {
			  _indexManager.nodeIndexNames();

			  foreach ( string indexName in _indexManager.nodeIndexNames() )
			  {
					Index<Node> index = _indexManager.forNodes( indexName );
					serializer.PutMapping( indexName, new NodeIndexRepresentation( indexName, _indexManager.getConfiguration( index ) ) );
			  }
		 }
	}

}