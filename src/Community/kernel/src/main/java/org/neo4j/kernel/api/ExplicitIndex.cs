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
namespace Neo4Net.Kernel.api
{
	using ExplicitIndexNotFoundKernelException = Neo4Net.@internal.Kernel.Api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;

	/// <summary>
	/// The main way to access an explicit index. Even pure reads will need to get a hold of an object of this class
	/// and to a query on. Blending of transaction state must also be handled within this object.
	/// </summary>
	public interface ExplicitIndex
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ExplicitIndexHits get(String key, Object value) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 ExplicitIndexHits Get( string key, object value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ExplicitIndexHits query(String key, Object queryOrQueryObject) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 ExplicitIndexHits Query( string key, object queryOrQueryObject );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ExplicitIndexHits query(Object queryOrQueryObject) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 ExplicitIndexHits Query( object queryOrQueryObject );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void addNode(long entity, String key, Object value) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 void AddNode( long entity, string key, object value );

		 void Remove( long entity, string key, object value );

		 void Remove( long entity, string key );

		 void Remove( long entity );

		 void Drop();

		 // Relationship-index-specific accessors
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ExplicitIndexHits get(String key, Object value, long startNode, long endNode) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 ExplicitIndexHits Get( string key, object value, long startNode, long endNode );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ExplicitIndexHits query(String key, Object queryOrQueryObject, long startNode, long endNode) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 ExplicitIndexHits Query( string key, object queryOrQueryObject, long startNode, long endNode );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ExplicitIndexHits query(Object queryOrQueryObject, long startNode, long endNode) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 ExplicitIndexHits Query( object queryOrQueryObject, long startNode, long endNode );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void addRelationship(long entity, String key, Object value, long startNode, long endNode) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 void AddRelationship( long entity, string key, object value, long startNode, long endNode );

		 void RemoveRelationship( long entity, string key, object value, long startNode, long endNode );

		 void RemoveRelationship( long entity, string key, long startNode, long endNode );

		 void RemoveRelationship( long entity, long startNode, long endNode );
	}

}