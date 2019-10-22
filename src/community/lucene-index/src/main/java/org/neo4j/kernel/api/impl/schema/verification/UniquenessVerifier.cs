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
namespace Neo4Net.Kernel.Api.Impl.Schema.verification
{

	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using Value = Neo4Net.Values.Storable.Value;

	/// <summary>
	/// A component that verifies uniqueness of values in a lucene index.
	/// During uniqueness constraint creation we ensure that already existing data is unique using
	/// <seealso cref="verify(NodePropertyAccessor, int[])"/>.
	/// Since updates can be applied while index is being populated we need to verify them as well.
	/// Verification does not handle that automatically. They need to be collected in some way and then checked by
	/// <seealso cref="verify(NodePropertyAccessor, int[], System.Collections.IList)"/>.
	/// </summary>
	public interface UniquenessVerifier : System.IDisposable
	{
		 /// <summary>
		 /// Verifies uniqueness of existing data.
		 /// </summary>
		 /// <param name="accessor"> the accessor to retrieve actual property values from the store. </param>
		 /// <param name="propKeyIds"> the ids of the properties to verify. </param>
		 /// <exception cref="IndexEntryConflictException"> if there are duplicates. </exception>
		 /// <exception cref="IOException"> when Lucene throws <seealso cref="IOException"/>. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void verify(org.Neo4Net.storageengine.api.NodePropertyAccessor accessor, int[] propKeyIds) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException, java.io.IOException;
		 void Verify( NodePropertyAccessor accessor, int[] propKeyIds );

		 /// <summary>
		 /// Verifies uniqueness of given values and existing data.
		 /// </summary>
		 /// <param name="accessor"> the accessor to retrieve actual property values from the store. </param>
		 /// <param name="propKeyIds"> the ids of the properties to verify. </param>
		 /// <param name="updatedValueTuples"> the values to check uniqueness for. </param>
		 /// <exception cref="IndexEntryConflictException"> if there are duplicates. </exception>
		 /// <exception cref="IOException"> when Lucene throws <seealso cref="IOException"/>. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void verify(org.Neo4Net.storageengine.api.NodePropertyAccessor accessor, int[] propKeyIds, java.util.List<org.Neo4Net.values.storable.Value[]> updatedValueTuples) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException, java.io.IOException;
		 void Verify( NodePropertyAccessor accessor, int[] propKeyIds, IList<Value[]> updatedValueTuples );
	}

}