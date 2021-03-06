﻿/*
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
namespace Org.Neo4j.Kernel.impl.coreapi.schema
{

	using Label = Org.Neo4j.Graphdb.Label;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using ConstraintDefinition = Org.Neo4j.Graphdb.schema.ConstraintDefinition;
	using IndexDefinition = Org.Neo4j.Graphdb.schema.IndexDefinition;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;

	/// <summary>
	/// Implementations are used to configure <seealso cref="IndexCreatorImpl"/> and <seealso cref="BaseNodeConstraintCreator"/> for re-use
	/// by both the graph database and the batch inserter.
	/// </summary>
	public interface InternalSchemaActions
	{
		 IndexDefinition CreateIndexDefinition( Label label, Optional<string> indexName, params string[] propertyKey );

		 void DropIndexDefinitions( IndexDefinition indexDefinition );

		 ConstraintDefinition CreatePropertyUniquenessConstraint( IndexDefinition indexDefinition );

		 ConstraintDefinition CreateNodeKeyConstraint( IndexDefinition indexDefinition );

		 ConstraintDefinition CreatePropertyExistenceConstraint( Label label, params string[] propertyKey );

		 ConstraintDefinition CreatePropertyExistenceConstraint( RelationshipType type, string propertyKey );

		 void DropPropertyUniquenessConstraint( Label label, string[] properties );

		 void DropNodeKeyConstraint( Label label, string[] properties );

		 void DropNodePropertyExistenceConstraint( Label label, string[] properties );

		 void DropRelationshipPropertyExistenceConstraint( RelationshipType type, string propertyKey );

		 string GetUserMessage( KernelException e );

		 void AssertInOpenTransaction();
	}

}