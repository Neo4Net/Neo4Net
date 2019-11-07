﻿/*
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
namespace Neo4Net.Kernel.Api
{
	using ExplicitIndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException;

	/// <summary>
	/// The main way to access an explicit index. Even pure reads will need to get a hold of an object of this class
	/// and to a query on. Blending of transaction state must also be handled within this object.
	/// </summary>
	public interface ExplicitIndex
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ExplicitIndexHits get(String key, Object value) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 ExplicitIndexHits Get( string key, object value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ExplicitIndexHits query(String key, Object queryOrQueryObject) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 ExplicitIndexHits Query( string key, object queryOrQueryObject );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ExplicitIndexHits query(Object queryOrQueryObject) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 ExplicitIndexHits Query( object queryOrQueryObject );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void addNode(long IEntity, String key, Object value) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 void AddNode( long IEntity, string key, object value );

		 void Remove( long IEntity, string key, object value );

		 void Remove( long IEntity, string key );

		 void Remove( long IEntity );

		 void Drop();

		 // Relationship-index-specific accessors
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ExplicitIndexHits get(String key, Object value, long startNode, long endNode) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 ExplicitIndexHits Get( string key, object value, long startNode, long endNode );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ExplicitIndexHits query(String key, Object queryOrQueryObject, long startNode, long endNode) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 ExplicitIndexHits Query( string key, object queryOrQueryObject, long startNode, long endNode );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ExplicitIndexHits query(Object queryOrQueryObject, long startNode, long endNode) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 ExplicitIndexHits Query( object queryOrQueryObject, long startNode, long endNode );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void addRelationship(long IEntity, String key, Object value, long startNode, long endNode) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 void AddRelationship( long IEntity, string key, object value, long startNode, long endNode );

		 void RemoveRelationship( long IEntity, string key, object value, long startNode, long endNode );

		 void RemoveRelationship( long IEntity, string key, long startNode, long endNode );

		 void RemoveRelationship( long IEntity, long startNode, long endNode );
	}

}