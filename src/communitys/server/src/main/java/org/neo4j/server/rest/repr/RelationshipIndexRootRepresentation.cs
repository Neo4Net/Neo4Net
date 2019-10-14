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
namespace Neo4Net.Server.rest.repr
{
	using IndexManager = Neo4Net.Graphdb.index.IndexManager;
	using RelationshipIndex = Neo4Net.Graphdb.index.RelationshipIndex;

	public class RelationshipIndexRootRepresentation : MappingRepresentation
	{
		 private IndexManager _indexManager;

		 public RelationshipIndexRootRepresentation( IndexManager indexManager ) : base( "relationship-index" )
		 {
			  this._indexManager = indexManager;
		 }

		 protected internal override void Serialize( MappingSerializer serializer )
		 {
			  foreach ( string indexName in _indexManager.relationshipIndexNames() )
			  {
					RelationshipIndex index = _indexManager.forRelationships( indexName );
					serializer.PutMapping( indexName, new RelationshipIndexRepresentation( indexName, _indexManager.getConfiguration( index ) ) );
			  }
		 }
	}

}