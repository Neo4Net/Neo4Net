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
namespace Neo4Net.Kernel.Api.Impl.Schema
{

	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Neo4Net.Kernel.Api.Impl.Index;
	using UniquenessVerifier = Neo4Net.Kernel.Api.Impl.Schema.verification.UniquenessVerifier;
	using NodePropertyAccessor = Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor;
	using IndexReader = Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader;
	using Value = Neo4Net.Values.Storable.Value;

	/// <summary>
	/// Partitioned lucene schema index.
	/// </summary>
	public interface ISchemaIndex : DatabaseIndex<IndexReader>
	{

		 /// <summary>
		 /// Verifies uniqueness of property values present in this index.
		 /// </summary>
		 /// <param name="accessor"> the accessor to retrieve actual property values from the store. </param>
		 /// <param name="propertyKeyIds"> the ids of the properties to verify. </param>
		 /// <exception cref="IndexEntryConflictException"> if there are duplicates. </exception>
		 /// <exception cref="IOException"> </exception>
		 /// <seealso cref= UniquenessVerifier#verify(NodePropertyAccessor, int[]) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void verifyUniqueness(org.Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor accessor, int[] propertyKeyIds) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException;
		 void VerifyUniqueness( NodePropertyAccessor accessor, int[] propertyKeyIds );

		 /// <summary>
		 /// Verifies uniqueness of updated property values.
		 /// </summary>
		 /// <param name="accessor"> the accessor to retrieve actual property values from the store. </param>
		 /// <param name="propertyKeyIds"> the ids of the properties to verify. </param>
		 /// <param name="updatedValueTuples"> the values to check uniqueness for. </param>
		 /// <exception cref="IndexEntryConflictException"> if there are duplicates. </exception>
		 /// <exception cref="IOException"> </exception>
		 /// <seealso cref= UniquenessVerifier#verify(NodePropertyAccessor, int[], List) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void verifyUniqueness(org.Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor accessor, int[] propertyKeyIds, java.util.List<org.Neo4Net.values.storable.Value[]> updatedValueTuples) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException;
		 void VerifyUniqueness( NodePropertyAccessor accessor, int[] propertyKeyIds, IList<Value[]> updatedValueTuples );
	}

}