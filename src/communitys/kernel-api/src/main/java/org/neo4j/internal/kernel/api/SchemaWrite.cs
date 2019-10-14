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
namespace Neo4Net.@internal.Kernel.Api
{

	using SchemaKernelException = Neo4Net.@internal.Kernel.Api.exceptions.schema.SchemaKernelException;
	using LabelSchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using RelationTypeSchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.RelationTypeSchemaDescriptor;
	using SchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptor;
	using ConstraintDescriptor = Neo4Net.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;

	/// <summary>
	/// Surface for creating and dropping indexes and constraints.
	/// </summary>
	public interface SchemaWrite
	{
		 /// <summary>
		 /// Create index from schema descriptor
		 /// </summary>
		 /// <param name="descriptor"> description of the index </param>
		 /// <returns> the newly created index </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: IndexReference indexCreate(org.neo4j.internal.kernel.api.schema.SchemaDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException;
		 IndexReference IndexCreate( SchemaDescriptor descriptor );

		 /// <summary>
		 /// Create index from schema descriptor
		 /// </summary>
		 /// <param name="descriptor"> description of the index </param>
		 /// <param name="name"> name of the index </param>
		 /// <returns> the newly created index </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: IndexReference indexCreate(org.neo4j.internal.kernel.api.schema.SchemaDescriptor descriptor, java.util.Optional<String> name) throws org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException;
		 IndexReference IndexCreate( SchemaDescriptor descriptor, Optional<string> name );

		 /// <summary>
		 /// Create index from schema descriptor
		 /// </summary>
		 /// <param name="descriptor"> description of the index </param>
		 /// <param name="provider"> name of the desired index provider implementation </param>
		 /// <param name="name"> name of the index </param>
		 /// <returns> the newly created index </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: IndexReference indexCreate(org.neo4j.internal.kernel.api.schema.SchemaDescriptor descriptor, String provider, java.util.Optional<String> name) throws org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException;
		 IndexReference IndexCreate( SchemaDescriptor descriptor, string provider, Optional<string> name );

		 /// <summary>
		 /// Drop the given index
		 /// </summary>
		 /// <param name="index"> the index to drop </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void indexDrop(IndexReference index) throws org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException;
		 void IndexDrop( IndexReference index );

		 /// <summary>
		 /// Create unique property constraint
		 /// </summary>
		 /// <param name="descriptor"> description of the constraint </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.schema.constraints.ConstraintDescriptor uniquePropertyConstraintCreate(org.neo4j.internal.kernel.api.schema.SchemaDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException;
		 ConstraintDescriptor UniquePropertyConstraintCreate( SchemaDescriptor descriptor );

		 /// <summary>
		 /// Create unique property constraint
		 /// </summary>
		 /// <param name="descriptor"> description of the constraint </param>
		 /// <param name="provider"> name of the desired index provider implementation </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.schema.constraints.ConstraintDescriptor uniquePropertyConstraintCreate(org.neo4j.internal.kernel.api.schema.SchemaDescriptor descriptor, String provider) throws org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException;
		 ConstraintDescriptor UniquePropertyConstraintCreate( SchemaDescriptor descriptor, string provider );

		 /// <summary>
		 /// Create node key constraint
		 /// </summary>
		 /// <param name="descriptor"> description of the constraint </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.schema.constraints.ConstraintDescriptor nodeKeyConstraintCreate(org.neo4j.internal.kernel.api.schema.LabelSchemaDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException;
		 ConstraintDescriptor NodeKeyConstraintCreate( LabelSchemaDescriptor descriptor );

		 /// <summary>
		 /// Create node key constraint
		 /// </summary>
		 /// <param name="descriptor"> description of the constraint </param>
		 /// <param name="provider"> name of the desired index provider implementation </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.schema.constraints.ConstraintDescriptor nodeKeyConstraintCreate(org.neo4j.internal.kernel.api.schema.LabelSchemaDescriptor descriptor, String provider) throws org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException;
		 ConstraintDescriptor NodeKeyConstraintCreate( LabelSchemaDescriptor descriptor, string provider );

		 /// <summary>
		 /// Create node property existence constraint
		 /// </summary>
		 /// <param name="descriptor"> description of the constraint </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.schema.constraints.ConstraintDescriptor nodePropertyExistenceConstraintCreate(org.neo4j.internal.kernel.api.schema.LabelSchemaDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException;
		 ConstraintDescriptor NodePropertyExistenceConstraintCreate( LabelSchemaDescriptor descriptor );

		 /// <summary>
		 /// Create relationship property existence constraint
		 /// </summary>
		 /// <param name="descriptor"> description of the constraint </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.schema.constraints.ConstraintDescriptor relationshipPropertyExistenceConstraintCreate(org.neo4j.internal.kernel.api.schema.RelationTypeSchemaDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException;
		 ConstraintDescriptor RelationshipPropertyExistenceConstraintCreate( RelationTypeSchemaDescriptor descriptor );

		 /// <summary>
		 /// Drop constraint
		 /// </summary>
		 /// <param name="descriptor"> description of the constraint </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void constraintDrop(org.neo4j.internal.kernel.api.schema.constraints.ConstraintDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException;
		 void ConstraintDrop( ConstraintDescriptor descriptor );
	}

}