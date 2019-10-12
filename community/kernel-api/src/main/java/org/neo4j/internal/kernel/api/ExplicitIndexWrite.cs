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
namespace Org.Neo4j.@internal.Kernel.Api
{

	using EntityNotFoundException = Org.Neo4j.@internal.Kernel.Api.exceptions.EntityNotFoundException;
	using ExplicitIndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;

	/// <summary>
	/// Operations for creating and modifying explicit indexes.
	/// </summary>
	public interface ExplicitIndexWrite
	{
		 /// <summary>
		 /// Adds node to explicit index.
		 /// </summary>
		 /// <param name="indexName"> The name of the index </param>
		 /// <param name="node"> The id of the node to add </param>
		 /// <param name="key"> The key to associate with the node </param>
		 /// <param name="value"> The value to associate with the node an key </param>
		 /// <exception cref="ExplicitIndexNotFoundKernelException"> If there is no explicit index with the given name </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void nodeAddToExplicitIndex(String indexName, long node, String key, Object value) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 void NodeAddToExplicitIndex( string indexName, long node, string key, object value );

		 /// <summary>
		 /// Removes a node from an explicit index
		 /// </summary>
		 /// <param name="indexName"> The name of the index </param>
		 /// <param name="node"> The id of the node to remove </param>
		 /// <param name="key"> The key associated with the node </param>
		 /// <param name="value"> The value associated with the node and key </param>
		 /// <exception cref="ExplicitIndexNotFoundKernelException"> If there is no explicit index with the given name </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void nodeRemoveFromExplicitIndex(String indexName, long node, String key, Object value) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 void NodeRemoveFromExplicitIndex( string indexName, long node, string key, object value );

		 /// <summary>
		 /// Removes a node from an explicit index
		 /// </summary>
		 /// <param name="indexName"> The name of the index </param>
		 /// <param name="node"> The id of the node to remove </param>
		 /// <param name="key"> The key associated with the node </param>
		 /// <exception cref="ExplicitIndexNotFoundKernelException"> If there is no explicit index with the given name </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void nodeRemoveFromExplicitIndex(String indexName, long node, String key) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 void NodeRemoveFromExplicitIndex( string indexName, long node, string key );

		 /// <summary>
		 /// Removes a given node from an explicit index
		 /// </summary>
		 /// <param name="indexName"> The name of the index from which the node is to be removed. </param>
		 /// <param name="node"> The node id of the node to remove </param>
		 /// <exception cref="ExplicitIndexNotFoundKernelException"> If there is no explicit index with the given name </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void nodeRemoveFromExplicitIndex(String indexName, long node) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 void NodeRemoveFromExplicitIndex( string indexName, long node );

		 /// <summary>
		 /// Drops the explicit index with the given name </summary>
		 /// <param name="indexName"> the index to drop </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void nodeExplicitIndexDrop(String indexName) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 void NodeExplicitIndexDrop( string indexName );

		 /// <summary>
		 /// Updates configuration of the given index </summary>
		 /// <param name="indexName"> the name of the index </param>
		 /// <param name="key"> the configuration key </param>
		 /// <param name="value"> the value to be associated with the key </param>
		 /// <returns> The old value associated with the key or <tt>null</tt> if nothing associated with the key. </returns>
		 /// <exception cref="ExplicitIndexNotFoundKernelException"> if no such index exists </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: String nodeExplicitIndexSetConfiguration(String indexName, String key, String value) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 string NodeExplicitIndexSetConfiguration( string indexName, string key, string value );

		 /// <summary>
		 /// Remove a configuration of the given index </summary>
		 /// <param name="indexName"> the name of the index </param>
		 /// <param name="key"> the configuration key </param>
		 /// <returns> The old value associated with the key or <tt>null</tt> if nothing associated with the key. </returns>
		 /// <exception cref="ExplicitIndexNotFoundKernelException"> if no such index exists </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: String nodeExplicitIndexRemoveConfiguration(String indexName, String key) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 string NodeExplicitIndexRemoveConfiguration( string indexName, string key );

		 /// <summary>
		 /// Adds relationship to explicit index.
		 /// </summary>
		 /// <param name="indexName"> The name of the index </param>
		 /// <param name="relationship"> The id of the relationship to add </param>
		 /// <param name="key"> The key to associate with the relationship </param>
		 /// <param name="value"> The value to associate with the relationship and key </param>
		 /// <exception cref="ExplicitIndexNotFoundKernelException"> If there is no explicit index with the given name </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void relationshipAddToExplicitIndex(String indexName, long relationship, String key, Object value) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException, org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException;
		 void RelationshipAddToExplicitIndex( string indexName, long relationship, string key, object value );

		 /// <summary>
		 /// Removes relationship from explicit index.
		 /// </summary>
		 /// <param name="indexName"> The name of the index </param>
		 /// <param name="relationship"> The id of the relationship to remove </param>
		 /// <param name="key"> The key associated with the relationship </param>
		 /// <param name="value"> The value associated with the relationship and key </param>
		 /// <exception cref="ExplicitIndexNotFoundKernelException"> If there is no explicit index with the given name </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void relationshipRemoveFromExplicitIndex(String indexName, long relationship, String key, Object value) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 void RelationshipRemoveFromExplicitIndex( string indexName, long relationship, string key, object value );

		 /// <summary>
		 /// Removes relationship to explicit index.
		 /// </summary>
		 /// <param name="indexName"> The name of the index </param>
		 /// <param name="relationship"> The id of the relationship to remove </param>
		 /// <param name="key"> The key associated with the relationship </param>
		 /// <exception cref="ExplicitIndexNotFoundKernelException"> If there is no explicit index with the given name </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void relationshipRemoveFromExplicitIndex(String indexName, long relationship, String key) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 void RelationshipRemoveFromExplicitIndex( string indexName, long relationship, string key );

		 /// <summary>
		 /// Removes relationship to explicit index.
		 /// </summary>
		 /// <param name="indexName"> The name of the index </param>
		 /// <param name="relationship"> The id of the relationship to remove </param>
		 /// <exception cref="ExplicitIndexNotFoundKernelException"> If there is no explicit index with the given name </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void relationshipRemoveFromExplicitIndex(String indexName, long relationship) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 void RelationshipRemoveFromExplicitIndex( string indexName, long relationship );

		 /// <summary>
		 /// Creates an explicit index in a separate transaction if not yet available.
		 /// </summary>
		 /// <param name="indexName"> The name of the index to create. </param>
		 /// <param name="customConfig"> The configuration of the explicit index. </param>
		 void NodeExplicitIndexCreateLazily( string indexName, IDictionary<string, string> customConfig );

		 /// <summary>
		 /// Creates an explicit index.
		 /// </summary>
		 /// <param name="indexName"> The name of the index to create. </param>
		 /// <param name="customConfig"> The configuration of the explicit index. </param>
		 void NodeExplicitIndexCreate( string indexName, IDictionary<string, string> customConfig );

		 /// <summary>
		 /// Creates an explicit index in a separate transaction if not yet available.
		 /// </summary>
		 /// <param name="indexName"> The name of the index to create. </param>
		 /// <param name="customConfig"> The configuration of the explicit index. </param>
		 void RelationshipExplicitIndexCreateLazily( string indexName, IDictionary<string, string> customConfig );

		 /// <summary>
		 /// Creates an explicit index.
		 /// </summary>
		 /// <param name="indexName"> The name of the index to create. </param>
		 /// <param name="customConfig"> The configuration of the explicit index. </param>
		 void RelationshipExplicitIndexCreate( string indexName, IDictionary<string, string> customConfig );

		 /// <summary>
		 /// Drops the explicit index with the given name </summary>
		 /// <param name="indexName"> the index to drop </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void relationshipExplicitIndexDrop(String indexName) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 void RelationshipExplicitIndexDrop( string indexName );

		 /// <summary>
		 /// Updates configuration of the given index </summary>
		 /// <param name="indexName"> the name of the index </param>
		 /// <param name="key"> the configuration key </param>
		 /// <param name="value"> the value to be associated with the key </param>
		 /// <returns> The old value associated with the key or <tt>null</tt> if nothing associated with the key. </returns>
		 /// <exception cref="ExplicitIndexNotFoundKernelException"> if no such index exists </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: String relationshipExplicitIndexSetConfiguration(String indexName, String key, String value) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 string RelationshipExplicitIndexSetConfiguration( string indexName, string key, string value );

		 /// <summary>
		 /// Remove a configuration of the given index </summary>
		 /// <param name="indexName"> the name of the index </param>
		 /// <param name="key"> the configuration key </param>
		 /// <returns> The old value associated with the key or <tt>null</tt> if nothing associated with the key. </returns>
		 /// <exception cref="ExplicitIndexNotFoundKernelException"> if no such index exists </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: String relationshipExplicitIndexRemoveConfiguration(String indexName, String key) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 string RelationshipExplicitIndexRemoveConfiguration( string indexName, string key );

	}

}