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
namespace Neo4Net.Kernel.impl.util.dbstructure
{
	using NodeExistenceConstraintDescriptor = Neo4Net.Kernel.Api.schema.constraints.NodeExistenceConstraintDescriptor;
	using NodeKeyConstraintDescriptor = Neo4Net.Kernel.Api.schema.constraints.NodeKeyConstraintDescriptor;
	using RelExistenceConstraintDescriptor = Neo4Net.Kernel.Api.schema.constraints.RelExistenceConstraintDescriptor;
	using UniquenessConstraintDescriptor = Neo4Net.Kernel.Api.schema.constraints.UniquenessConstraintDescriptor;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;

	public interface DbStructureVisitor
	{
		 void VisitLabel( int labelId, string labelName );
		 void VisitPropertyKey( int propertyKeyId, string propertyKeyName );
		 void VisitRelationshipType( int relTypeId, string relTypeName );

		 void VisitIndex( IndexDescriptor descriptor, string userDescription, double uniqueValuesPercentage, long size );
		 void VisitUniqueConstraint( UniquenessConstraintDescriptor constraint, string userDescription );
		 void VisitNodePropertyExistenceConstraint( NodeExistenceConstraintDescriptor constraint, string userDescription );
		 void VisitRelationshipPropertyExistenceConstraint( RelExistenceConstraintDescriptor constraint, string userDescription );
		 void VisitNodeKeyConstraint( NodeKeyConstraintDescriptor constraint, string userDescription );

		 void VisitAllNodesCount( long nodeCount );
		 void VisitNodeCount( int labelId, string labelName, long nodeCount );
		 void VisitRelCount( int startLabelId, int relTypeId, int endLabelId, string relCountQuery, long relCount );
	}

}