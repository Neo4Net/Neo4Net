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

namespace Neo4Net.Kernel.Api.Internal
{
   using ConstraintValidationException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.ConstraintValidationException;
   using Value = Neo4Net.Values.Storable.Value;

   /// <summary>
   /// Defines the write operations of the Kernel API.
   /// </summary>
   public interface IWrite
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
      //ORIGINAL LINE: long nodeCreateWithLabels(int[] labels) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.ConstraintValidationException;
      long NodeCreateWithLabels(int[] labels);

      /// <summary>
      /// Delete a node.
      /// </summary>
      /// <param name="node"> the internal id of the node to delete </param>
      /// <returns> returns true if it deleted a node or false if no node was found for this id </returns>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: boolean nodeDelete(long node) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.AutoIndexingKernelException;
      bool NodeDelete(long node);

      /// <summary>
      /// Deletes the node and all relationships connecting the node
      /// </summary>
      /// <param name="node"> the node to delete </param>
      /// <returns> the number of deleted relationships </returns>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: int nodeDetachDelete(long node) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
      int NodeDetachDelete(long node);

      /// <summary>
      /// Create a relationship between two nodes.
      /// </summary>
      /// <param name="sourceNode"> the source internal node id </param>
      /// <param name="relationshipType"> the type of the relationship to create </param>
      /// <param name="targetNode"> the target internal node id </param>
      /// <returns> the internal id of the created relationship </returns>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: long relationshipCreate(long sourceNode, int relationshipType, long targetNode) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.EntityNotFoundException;
      long RelationshipCreate(long sourceNode, int relationshipType, long targetNode);

      /// <summary>
      /// Delete a relationship
      /// </summary>
      /// <param name="relationship"> the internal id of the relationship to delete </param>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: boolean relationshipDelete(long relationship) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.AutoIndexingKernelException;
      bool RelationshipDelete(long relationship);

      /// <summary>
      /// Add a label to a node
      /// </summary>
      /// <param name="node"> the internal node id </param>
      /// <param name="nodeLabel"> the internal id of the label to add </param>
      /// <returns> {@code true} if a label was added otherwise {@code false} </returns>
      /// <exception cref="ConstraintValidationException"> if adding the label to node breaks a constraint </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: boolean nodeAddLabel(long node, int nodeLabel) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
      bool NodeAddLabel(long node, int nodeLabel);

      /// <summary>
      /// Remove a label from a node
      /// </summary>
      /// <param name="node"> the internal node id </param>
      /// <param name="nodeLabel"> the internal id of the label to remove </param>
      /// <returns> {@code true} if node was removed otherwise {@code false} </returns>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: boolean nodeRemoveLabel(long node, int nodeLabel) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.EntityNotFoundException;
      bool NodeRemoveLabel(long node, int nodeLabel);

      /// <summary>
      /// Set a property on a node
      /// </summary>
      /// <param name="node"> the internal node id </param>
      /// <param name="propertyKey"> the property key id </param>
      /// <param name="value"> the value to set </param>
      /// <returns> The replaced value, or Values.NO_VALUE if the node did not have the property before </returns>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.values.storable.Value nodeSetProperty(long node, int propertyKey, org.Neo4Net.values.storable.Value value) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
      Value NodeSetProperty(long node, int propertyKey, Value value);

      /// <summary>
      /// Remove a property from a node
      /// </summary>
      /// <param name="node"> the internal node id </param>
      /// <param name="propertyKey"> the property key id </param>
      /// <returns> The removed value, or Values.NO_VALUE if the node did not have the property before </returns>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.values.storable.Value nodeRemoveProperty(long node, int propertyKey) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.EntityNotFoundException, org.Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.AutoIndexingKernelException;
      Value NodeRemoveProperty(long node, int propertyKey);

      /// <summary>
      /// Set a property on a relationship
      /// </summary>
      /// <param name="relationship"> the internal relationship id </param>
      /// <param name="propertyKey"> the property key id </param>
      /// <param name="value"> the value to set </param>
      /// <returns> The replaced value, or Values.NO_VALUE if the relationship did not have the property before </returns>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.values.storable.Value relationshipSetProperty(long relationship, int propertyKey, org.Neo4Net.values.storable.Value value) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.EntityNotFoundException, org.Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.AutoIndexingKernelException;
      Value RelationshipSetProperty(long relationship, int propertyKey, Value value);

      /// <summary>
      /// Remove a property from a relationship
      /// </summary>
      /// <param name="relationship"> the internal relationship id </param>
      /// <param name="propertyKey"> the property key id </param>
      /// <returns> The removed value, or Values.NO_VALUE if the relationship did not have the property before </returns>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.values.storable.Value relationshipRemoveProperty(long relationship, int propertyKey) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.EntityNotFoundException, org.Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.AutoIndexingKernelException;
      Value RelationshipRemoveProperty(long relationship, int propertyKey);

      /// <summary>
      /// Set a property on the graph
      /// </summary>
      /// <param name="propertyKey"> the property key id </param>
      /// <param name="value"> the value to set </param>
      /// <returns> The replaced value, or Values.NO_VALUE if the graph did not have the property before </returns>
      Value GraphSetProperty(int propertyKey, Value value);

      /// <summary>
      /// Remove a property from the graph
      /// </summary>
      /// <param name="propertyKey"> the property key id </param>
      /// <returns> The removed value, or Values.NO_VALUE if the graph did not have the property before </returns>
      Value GraphRemoveProperty(int propertyKey);
   }
}