﻿/*
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
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using AutoIndexingKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.explicitindex.AutoIndexingKernelException;
	using ConstraintValidationException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.ConstraintValidationException;
	using Value = Org.Neo4j.Values.Storable.Value;

	/// <summary>
	/// Defines the write operations of the Kernel API.
	/// </summary>
	public interface Write
	{
		 /// <summary>
		 /// Create a node.
		 /// </summary>
		 /// <returns> The internal id of the created node </returns>
		 long NodeCreate();

		 /// <summary>
		 /// Create a node, and assign it the given array of labels.
		 /// <para>
		 /// This method differs from a <seealso cref="nodeCreate()"/> and <seealso cref="nodeAddLabel(long, int)"/> sequence, in that we will
		 /// avoid taking the "unlabelled node lock" of the {@code nodeCreate}, and we will avoid taking the exclusive node
		 /// lock in the {@code nodeAddLabel} method.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="labels"> The labels to assign to the newly created node. </param>
		 /// <returns> The internal id of the created node. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long nodeCreateWithLabels(int[] labels) throws org.neo4j.internal.kernel.api.exceptions.schema.ConstraintValidationException;
		 long NodeCreateWithLabels( int[] labels );

		 /// <summary>
		 /// Delete a node.
		 /// </summary>
		 /// <param name="node"> the internal id of the node to delete </param>
		 /// <returns> returns true if it deleted a node or false if no node was found for this id </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean nodeDelete(long node) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException;
		 bool NodeDelete( long node );

		 /// <summary>
		 /// Deletes the node and all relationships connecting the node
		 /// </summary>
		 /// <param name="node"> the node to delete </param>
		 /// <returns> the number of deleted relationships </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int nodeDetachDelete(long node) throws org.neo4j.internal.kernel.api.exceptions.KernelException;
		 int NodeDetachDelete( long node );

		 /// <summary>
		 /// Create a relationship between two nodes.
		 /// </summary>
		 /// <param name="sourceNode"> the source internal node id </param>
		 /// <param name="relationshipType"> the type of the relationship to create </param>
		 /// <param name="targetNode"> the target internal node id </param>
		 /// <returns> the internal id of the created relationship </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long relationshipCreate(long sourceNode, int relationshipType, long targetNode) throws org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException;
		 long RelationshipCreate( long sourceNode, int relationshipType, long targetNode );

		 /// <summary>
		 /// Delete a relationship
		 /// </summary>
		 /// <param name="relationship"> the internal id of the relationship to delete </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean relationshipDelete(long relationship) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException;
		 bool RelationshipDelete( long relationship );

		 /// <summary>
		 /// Add a label to a node
		 /// </summary>
		 /// <param name="node"> the internal node id </param>
		 /// <param name="nodeLabel"> the internal id of the label to add </param>
		 /// <returns> {@code true} if a label was added otherwise {@code false} </returns>
		 /// <exception cref="ConstraintValidationException"> if adding the label to node breaks a constraint </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean nodeAddLabel(long node, int nodeLabel) throws org.neo4j.internal.kernel.api.exceptions.KernelException;
		 bool NodeAddLabel( long node, int nodeLabel );

		 /// <summary>
		 /// Remove a label from a node
		 /// </summary>
		 /// <param name="node"> the internal node id </param>
		 /// <param name="nodeLabel"> the internal id of the label to remove </param>
		 /// <returns> {@code true} if node was removed otherwise {@code false} </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean nodeRemoveLabel(long node, int nodeLabel) throws org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException;
		 bool NodeRemoveLabel( long node, int nodeLabel );

		 /// <summary>
		 /// Set a property on a node
		 /// </summary>
		 /// <param name="node"> the internal node id </param>
		 /// <param name="propertyKey"> the property key id </param>
		 /// <param name="value"> the value to set </param>
		 /// <returns> The replaced value, or Values.NO_VALUE if the node did not have the property before </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.values.storable.Value nodeSetProperty(long node, int propertyKey, org.neo4j.values.storable.Value value) throws org.neo4j.internal.kernel.api.exceptions.KernelException;
		 Value NodeSetProperty( long node, int propertyKey, Value value );

		 /// <summary>
		 /// Remove a property from a node
		 /// </summary>
		 /// <param name="node"> the internal node id </param>
		 /// <param name="propertyKey"> the property key id </param>
		 /// <returns> The removed value, or Values.NO_VALUE if the node did not have the property before </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.values.storable.Value nodeRemoveProperty(long node, int propertyKey) throws org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException, org.neo4j.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException;
		 Value NodeRemoveProperty( long node, int propertyKey );

		 /// <summary>
		 /// Set a property on a relationship
		 /// </summary>
		 /// <param name="relationship"> the internal relationship id </param>
		 /// <param name="propertyKey"> the property key id </param>
		 /// <param name="value"> the value to set </param>
		 /// <returns> The replaced value, or Values.NO_VALUE if the relationship did not have the property before </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.values.storable.Value relationshipSetProperty(long relationship, int propertyKey, org.neo4j.values.storable.Value value) throws org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException, org.neo4j.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException;
		 Value RelationshipSetProperty( long relationship, int propertyKey, Value value );

		 /// <summary>
		 /// Remove a property from a relationship
		 /// </summary>
		 /// <param name="relationship"> the internal relationship id </param>
		 /// <param name="propertyKey"> the property key id </param>
		 /// <returns> The removed value, or Values.NO_VALUE if the relationship did not have the property before </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.values.storable.Value relationshipRemoveProperty(long relationship, int propertyKey) throws org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException, org.neo4j.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException;
		 Value RelationshipRemoveProperty( long relationship, int propertyKey );

		 /// <summary>
		 /// Set a property on the graph
		 /// </summary>
		 /// <param name="propertyKey"> the property key id </param>
		 /// <param name="value"> the value to set </param>
		 /// <returns> The replaced value, or Values.NO_VALUE if the graph did not have the property before </returns>
		 Value GraphSetProperty( int propertyKey, Value value );

		 /// <summary>
		 /// Remove a property from the graph
		 /// </summary>
		 /// <param name="propertyKey"> the property key id </param>
		 /// <returns> The removed value, or Values.NO_VALUE if the graph did not have the property before </returns>
		 Value GraphRemoveProperty( int propertyKey );
	}

}