using System.Collections.Generic;

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

	using IndexNotFoundKernelException = Neo4Net.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using SchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptor;
	using ConstraintDescriptor = Neo4Net.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using PopulationProgress = Neo4Net.Storageengine.Api.schema.PopulationProgress;

	/// <summary>
	/// The reduced core set of schema read methods
	/// </summary>
	public interface SchemaReadCore
	{
		 /// <summary>
		 /// Acquire a reference to the index mapping the given {@code SchemaDescriptor}.
		 /// </summary>
		 /// <param name="schema"> <seealso cref="SchemaDescriptor"/> for the index </param>
		 /// <returns> the IndexReference, or <seealso cref="IndexReference.NO_INDEX"/> if such an index does not exist. </returns>
		 IndexReference Index( SchemaDescriptor schema );

		 /// <summary>
		 /// Returns all indexes associated with the given label
		 /// </summary>
		 /// <param name="labelId"> The id of the label which associated indexes you are looking for </param>
		 /// <returns> The indexes associated with the given label </returns>
		 IEnumerator<IndexReference> IndexesGetForLabel( int labelId );

		 /// <summary>
		 /// Returns all indexes associated with the given relationship type
		 /// </summary>
		 /// <param name="relationshipType"> The id of the relationship type which associated indexes you are looking for </param>
		 /// <returns> The indexes associated with the given relationship type. </returns>
		 IEnumerator<IndexReference> IndexesGetForRelationshipType( int relationshipType );

		 /// <summary>
		 /// Returns all indexes used in the database
		 /// </summary>
		 /// <returns> all indexes used in the database </returns>
		 IEnumerator<IndexReference> IndexesGetAll();

		 /// <summary>
		 /// Retrieves the state of an index
		 /// </summary>
		 /// <param name="index"> the index which state to retrieve </param>
		 /// <returns> The state of the provided index </returns>
		 /// <exception cref="IndexNotFoundKernelException"> if the index was not found in the database </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: InternalIndexState indexGetState(IndexReference index) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException;
		 InternalIndexState IndexGetState( IndexReference index );

		 /// <summary>
		 /// Retrives the population progress of the index
		 /// </summary>
		 /// <param name="index"> The index whose progress to retrieve </param>
		 /// <returns> The population progress of the given index </returns>
		 /// <exception cref="IndexNotFoundKernelException"> if the index was not found in the database </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.storageengine.api.schema.PopulationProgress indexGetPopulationProgress(IndexReference index) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException;
		 PopulationProgress IndexGetPopulationProgress( IndexReference index );

		 /// <summary>
		 /// Returns the failure description of a failed index.
		 /// </summary>
		 /// <param name="index"> the failed index </param>
		 /// <returns> The failure message from the index </returns>
		 /// <exception cref="IndexNotFoundKernelException"> if the index was not found in the database </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: String indexGetFailure(IndexReference index) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException;
		 string IndexGetFailure( IndexReference index );

		 /// <summary>
		 /// Finds all constraints for the given label
		 /// </summary>
		 /// <param name="labelId"> The id of the label </param>
		 /// <returns> All constraints for the given label </returns>
		 IEnumerator<ConstraintDescriptor> ConstraintsGetForLabel( int labelId );

		 /// <summary>
		 /// Get all constraints applicable to relationship type.
		 /// </summary>
		 /// <param name="typeId"> the id of the relationship type </param>
		 /// <returns> An iterator of constraints associated with the given type. </returns>
		 IEnumerator<ConstraintDescriptor> ConstraintsGetForRelationshipType( int typeId );

		 /// <summary>
		 /// Find all constraints in the database
		 /// </summary>
		 /// <returns> An iterator of all the constraints in the database. </returns>
		 IEnumerator<ConstraintDescriptor> ConstraintsGetAll();
	}

}