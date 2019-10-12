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
	using QueryStatistics = Org.Neo4j.Graphdb.QueryStatistics;

	public class CypherStatisticsRepresentation : MappingRepresentation
	{
		 private readonly QueryStatistics _stats;

		 public CypherStatisticsRepresentation( QueryStatistics stats ) : base( "stats" )
		 {
			  this._stats = stats;
		 }

		 protected internal override void Serialize( MappingSerializer serializer )
		 {
			  serializer.PutBoolean( "contains_updates", _stats.containsUpdates() );
			  serializer.PutNumber( "nodes_created", _stats.NodesCreated );
			  serializer.PutNumber( "nodes_deleted", _stats.NodesDeleted );
			  serializer.PutNumber( "properties_set", _stats.PropertiesSet );
			  serializer.PutNumber( "relationships_created", _stats.RelationshipsCreated );
			  serializer.PutNumber( "relationship_deleted", _stats.RelationshipsDeleted );
			  serializer.PutNumber( "labels_added", _stats.LabelsAdded );
			  serializer.PutNumber( "labels_removed", _stats.LabelsRemoved );
			  serializer.PutNumber( "indexes_added", _stats.IndexesAdded );
			  serializer.PutNumber( "indexes_removed", _stats.IndexesRemoved );
			  serializer.PutNumber( "constraints_added", _stats.ConstraintsAdded );
			  serializer.PutNumber( "constraints_removed", _stats.ConstraintsRemoved );
		 }
	}

}