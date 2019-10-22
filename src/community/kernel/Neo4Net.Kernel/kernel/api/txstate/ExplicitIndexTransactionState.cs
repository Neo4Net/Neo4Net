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
namespace Neo4Net.Kernel.api.txstate
{

	using ExplicitIndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
	using AuxiliaryTransactionState = Neo4Net.Kernel.api.txstate.auxiliary.AuxiliaryTransactionState;
	using IndexEntityType = Neo4Net.Kernel.impl.index.IndexEntityType;

	/// <summary>
	/// Defines transactional state for explicit indexes. Since the implementation of this enlists another transaction
	/// management engine under the hood, these methods have been split out from
	/// <seealso cref="TransactionState the transaction state"/> in order to be able to keep the implementation of
	/// <seealso cref="org.Neo4Net.kernel.impl.api.state.TxState transaction state"/> simple with no dependencies.
	/// </summary>
	public interface ExplicitIndexTransactionState : AuxiliaryTransactionState
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.kernel.api.ExplicitIndex nodeChanges(String indexName) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 ExplicitIndex NodeChanges( string indexName );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.kernel.api.ExplicitIndex relationshipChanges(String indexName) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 ExplicitIndex RelationshipChanges( string indexName );

		 void CreateIndex( IndexEntityType IEntityType, string indexName, IDictionary<string, string> config );

		 void DeleteIndex( IndexEntityType IEntityType, string indexName );

		 /// <summary>
		 /// Checks whether or not index with specific {@code name} exists.
		 /// Optionally the specific {@code config} is verified to be matching.
		 /// 
		 /// This method can either return {@code boolean} or {@code throw} exception on:
		 /// <ul>
		 /// <li>index exists, config is provided and matching => {@code true}</li>
		 /// <li>index exists, config is provided and NOT matching => {@code throw exception}</li>
		 /// <li>index exists, config is NOT provided => {@code true}</li>
		 /// <li>index does NOT exist => {@code false}</li>
		 /// </ul>
		 /// </summary>
		 /// <param name="entityType"> <seealso cref="IndexEntityType"/> for the index. </param>
		 /// <param name="indexName"> name of the index. </param>
		 /// <param name="config"> configuration which must match the existing index, if it exists. {@code null} means
		 /// that the configuration doesn't need to be checked. </param>
		 /// <returns> {@code true} if the index with the specific {@code name} and {@code IEntityType} exists, otherwise {@code false}. </returns>
		 bool CheckIndexExistence( IndexEntityType IEntityType, string indexName, IDictionary<string, string> config );
	}

}