using System.Collections.Generic;

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
namespace Org.Neo4j.Storageengine.Api
{

	using InternalIndexState = Org.Neo4j.@internal.Kernel.Api.InternalIndexState;
	using IndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using ConstraintDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using CapableIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.CapableIndexDescriptor;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using PopulationProgress = Org.Neo4j.Storageengine.Api.schema.PopulationProgress;

	public interface StorageSchemaReader
	{
		 /// <summary>
		 /// Looks for a stored index by given {@code descriptor}
		 /// </summary>
		 /// <param name="descriptor"> a description of the index. </param>
		 /// <returns> <seealso cref="CapableIndexDescriptor"/> for matching index, or {@code null} if not found. </returns>
		 CapableIndexDescriptor IndexGetForSchema( SchemaDescriptor descriptor );

		 /// <param name="labelId"> label to list indexes for. </param>
		 /// <returns> <seealso cref="IndexDescriptor"/> associated with the given {@code labelId}. </returns>
		 IEnumerator<CapableIndexDescriptor> IndexesGetForLabel( int labelId );

		 /// <param name="relationshipType"> relationship type to list indexes for. </param>
		 /// <returns> <seealso cref="IndexDescriptor"/> associated with the given {@code relationshipType}. </returns>
		 IEnumerator<CapableIndexDescriptor> IndexesGetForRelationshipType( int relationshipType );

		 /// <returns> all <seealso cref="CapableIndexDescriptor"/> in storage. </returns>
		 IEnumerator<CapableIndexDescriptor> IndexesGetAll();

		 /// <summary>
		 /// Returns state of a stored index.
		 /// </summary>
		 /// <param name="descriptor"> <seealso cref="IndexDescriptor"/> to get state for. </param>
		 /// <returns> <seealso cref="InternalIndexState"/> for index. </returns>
		 /// <exception cref="IndexNotFoundKernelException"> if index not found. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.InternalIndexState indexGetState(org.neo4j.storageengine.api.schema.IndexDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException;
		 InternalIndexState IndexGetState( IndexDescriptor descriptor );

		 /// <param name="descriptor"> <seealso cref="SchemaDescriptor"/> to get population progress for. </param>
		 /// <returns> progress of index population, which is the initial state of an index when it's created. </returns>
		 /// <exception cref="IndexNotFoundKernelException"> if index not found. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.storageengine.api.schema.PopulationProgress indexGetPopulationProgress(org.neo4j.internal.kernel.api.schema.SchemaDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException;
		 PopulationProgress IndexGetPopulationProgress( SchemaDescriptor descriptor );

		 /// <summary>
		 /// Returns any failure that happened during population or operation of an index. Such failures
		 /// are persisted and can be accessed even after restart.
		 /// </summary>
		 /// <param name="descriptor"> <seealso cref="SchemaDescriptor"/> to get failure for. </param>
		 /// <returns> failure of an index, or {@code null} if index is working as it should. </returns>
		 /// <exception cref="IndexNotFoundKernelException"> if index not found. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: String indexGetFailure(org.neo4j.internal.kernel.api.schema.SchemaDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException;
		 string IndexGetFailure( SchemaDescriptor descriptor );

		 /// <param name="labelId"> label token id. </param>
		 /// <returns> node property constraints associated with the label token id. </returns>
		 IEnumerator<ConstraintDescriptor> ConstraintsGetForLabel( int labelId );

		 /// <param name="typeId"> relationship type token id . </param>
		 /// <returns> relationship property constraints associated with the relationship type token id. </returns>
		 IEnumerator<ConstraintDescriptor> ConstraintsGetForRelationshipType( int typeId );

		 /// <returns> all stored property constraints. </returns>
		 IEnumerator<ConstraintDescriptor> ConstraintsGetAll();
	}

}