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
namespace Neo4Net.Internal.Kernel.Api
{

	using ExplicitIndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;

	/// <summary>
	/// Operations for querying and seeking in explicit indexes.
	/// </summary>
	public interface ExplicitIndexRead
	{
		 /// <summary>
		 /// Finds item from explicit index
		 /// </summary>
		 /// <param name="cursor"> the cursor to use for consuming the result </param>
		 /// <param name="index"> the name of the explicit index </param>
		 /// <param name="key"> the key to find </param>
		 /// <param name="value"> the value corresponding to the key </param>
		 /// <exception cref="ExplicitIndexNotFoundKernelException"> if index is not there </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void nodeExplicitIndexLookup(NodeExplicitIndexCursor cursor, String index, String key, Object value) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 void NodeExplicitIndexLookup( NodeExplicitIndexCursor cursor, string index, string key, object value );

		 /// <summary>
		 /// Queries explicit index
		 /// </summary>
		 /// <param name="cursor"> the cursor to use for consuming the result </param>
		 /// <param name="index"> the name of the explicit index </param>
		 /// <param name="query"> the query object </param>
		 /// <exception cref="ExplicitIndexNotFoundKernelException"> if index is not there </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void nodeExplicitIndexQuery(NodeExplicitIndexCursor cursor, String index, Object query) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 void NodeExplicitIndexQuery( NodeExplicitIndexCursor cursor, string index, object query );

		 /// <summary>
		 /// Queries explicit index
		 /// </summary>
		 /// <param name="cursor"> the cursor to use for consuming the result </param>
		 /// <param name="index"> the name of the explicit index </param>
		 /// <param name="key"> the key to find </param>
		 /// <param name="query"> the query object </param>
		 /// <exception cref="ExplicitIndexNotFoundKernelException"> if index is not there </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void nodeExplicitIndexQuery(NodeExplicitIndexCursor cursor, String index, String key, Object query) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 void NodeExplicitIndexQuery( NodeExplicitIndexCursor cursor, string index, string key, object query );

		 /// <summary>
		 /// Check whether a node index with the given name exists.
		 /// </summary>
		 /// <param name="indexName"> name of node index to check for existence. </param>
		 /// <param name="customConfiguration"> if {@code null} the configuration of existing won't be matched, otherwise it will
		 /// be matched and a mismatch will throw <seealso cref="System.ArgumentException"/>. </param>
		 /// <returns> whether or not node explicit index with name {@code indexName} exists. </returns>
		 /// <exception cref="IllegalArgumentException"> on index existence with provided name, but mismatching {@code customConfiguration}. </exception>
		 bool NodeExplicitIndexExists( string indexName, IDictionary<string, string> customConfiguration );

		 /// <summary>
		 /// Return the configuration of the given index </summary>
		 /// <param name="indexName"> the name of the index </param>
		 /// <returns> the configuration of the index with the given name </returns>
		 /// <exception cref="ExplicitIndexNotFoundKernelException"> if the index is not there </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.Map<String, String> nodeExplicitIndexGetConfiguration(String indexName) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 IDictionary<string, string> NodeExplicitIndexGetConfiguration( string indexName );

		 /// <summary>
		 /// Finds item from explicit index
		 /// </summary>
		 /// <param name="cursor"> the cursor to use for consuming the result </param>
		 /// <param name="index"> the name of the explicit index </param>
		 /// <param name="key"> the key to find </param>
		 /// <param name="value"> the value corresponding to the key </param>
		 /// <exception cref="ExplicitIndexNotFoundKernelException"> if index is not there </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void relationshipExplicitIndexLookup(RelationshipExplicitIndexCursor cursor, String index, String key, Object value, long source, long target) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 void RelationshipExplicitIndexLookup( RelationshipExplicitIndexCursor cursor, string index, string key, object value, long source, long target );

		 /// <summary>
		 /// Queries explicit index
		 /// </summary>
		 /// <param name="cursor"> the cursor to use for consuming the result </param>
		 /// <param name="index"> the name of the explicit index </param>
		 /// <param name="query"> the query object </param>
		 /// <param name="source"> the source node or <code>-1</code> if any </param>
		 /// <param name="target"> the source node or <code>-1</code> if any </param>
		 /// <exception cref="ExplicitIndexNotFoundKernelException"> if index is not there </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void relationshipExplicitIndexQuery(RelationshipExplicitIndexCursor cursor, String index, Object query, long source, long target) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 void RelationshipExplicitIndexQuery( RelationshipExplicitIndexCursor cursor, string index, object query, long source, long target );

		 /// <summary>
		 /// Queries explicit index
		 /// </summary>
		 /// <param name="cursor"> the cursor to use for consuming the result </param>
		 /// <param name="index"> the name of the explicit index </param>
		 /// <param name="key"> the key to find </param>
		 /// <param name="query"> the query object </param>
		 /// <param name="source"> the source node or <code>-1</code> if any </param>
		 /// <param name="target"> the source node or <code>-1</code> if any </param>
		 /// <exception cref="ExplicitIndexNotFoundKernelException"> if index is not there </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void relationshipExplicitIndexQuery(RelationshipExplicitIndexCursor cursor, String index, String key, Object query, long source, long target) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 void RelationshipExplicitIndexQuery( RelationshipExplicitIndexCursor cursor, string index, string key, object query, long source, long target );

		 /// <summary>
		 /// Check whether a relationship index with the given name exists.
		 /// </summary>
		 /// <param name="indexName"> name of relationship index to check for existence. </param>
		 /// <param name="customConfiguration"> if {@code null} the configuration of existing won't be matched, otherwise it will
		 /// be matched and a mismatch will throw <seealso cref="System.ArgumentException"/>. </param>
		 /// <returns> whether or not relationship explicit index with name {@code indexName} exists. </returns>
		 /// <exception cref="IllegalArgumentException"> on index existence with provided name, but mismatching {@code customConfiguration}. </exception>
		 bool RelationshipExplicitIndexExists( string indexName, IDictionary<string, string> customConfiguration );

		 /// <summary>
		 /// Retrieve all node explicit indexes </summary>
		 /// <returns> the names of all node explicit indexes </returns>
		 string[] NodeExplicitIndexesGetAll();

		 /// <summary>
		 /// Retrieve all relationship explicit indexes </summary>
		 /// <returns> the names of all relationship explicit indexes </returns>
		 string[] RelationshipExplicitIndexesGetAll();

		 /// <summary>
		 /// Return the configuration of the given index </summary>
		 /// <param name="indexName"> the name of the index </param>
		 /// <returns> the configuration of the index with the given name </returns>
		 /// <exception cref="ExplicitIndexNotFoundKernelException"> if the index doesn't exist </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.Map<String, String> relationshipExplicitIndexGetConfiguration(String indexName) throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 IDictionary<string, string> RelationshipExplicitIndexGetConfiguration( string indexName );
	}

}