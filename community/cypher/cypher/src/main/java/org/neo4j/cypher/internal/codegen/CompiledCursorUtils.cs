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
namespace Org.Neo4j.Cypher.@internal.codegen
{
	using Direction = Org.Neo4j.Graphdb.Direction;
	using CursorFactory = Org.Neo4j.@internal.Kernel.Api.CursorFactory;
	using NodeCursor = Org.Neo4j.@internal.Kernel.Api.NodeCursor;
	using PropertyCursor = Org.Neo4j.@internal.Kernel.Api.PropertyCursor;
	using Read = Org.Neo4j.@internal.Kernel.Api.Read;
	using RelationshipScanCursor = Org.Neo4j.@internal.Kernel.Api.RelationshipScanCursor;
	using EntityNotFoundException = Org.Neo4j.@internal.Kernel.Api.exceptions.EntityNotFoundException;
	using RelationshipSelectionCursor = Org.Neo4j.@internal.Kernel.Api.helpers.RelationshipSelectionCursor;
	using RelationshipSelections = Org.Neo4j.@internal.Kernel.Api.helpers.RelationshipSelections;
	using StatementConstants = Org.Neo4j.Kernel.api.StatementConstants;
	using EntityType = Org.Neo4j.Storageengine.Api.EntityType;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

	/// <summary>
	/// Utilities for working with cursors from within generated code
	/// </summary>
	public sealed class CompiledCursorUtils
	{
		 /// <summary>
		 /// Do not instantiate this class
		 /// </summary>
		 private CompiledCursorUtils()
		 {
			  throw new System.NotSupportedException();
		 }

		 /// <summary>
		 /// Fetches a given property from a node
		 /// </summary>
		 /// <param name="read"> The current Read instance </param>
		 /// <param name="nodeCursor"> The node cursor to use </param>
		 /// <param name="node"> The id of the node </param>
		 /// <param name="propertyCursor"> The property cursor to use </param>
		 /// <param name="prop"> The id of the property to find </param>
		 /// <returns> The value of the given property </returns>
		 /// <exception cref="EntityNotFoundException"> If the node cannot be find. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.values.storable.Value nodeGetProperty(org.neo4j.internal.kernel.api.Read read, org.neo4j.internal.kernel.api.NodeCursor nodeCursor, long node, org.neo4j.internal.kernel.api.PropertyCursor propertyCursor, int prop) throws org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException
		 public static Value NodeGetProperty( Read read, NodeCursor nodeCursor, long node, PropertyCursor propertyCursor, int prop )
		 {
			  if ( prop == StatementConstants.NO_SUCH_PROPERTY_KEY )
			  {
					return Values.NO_VALUE;
			  }
			  SingleNode( read, nodeCursor, node );
			  nodeCursor.Properties( propertyCursor );
			  while ( propertyCursor.Next() )
			  {
					if ( propertyCursor.PropertyKey() == prop )
					{
						 return propertyCursor.PropertyValue();
					}
			  }

			  return Values.NO_VALUE;
		 }

		 /// <summary>
		 /// Checks if given node has a given label.
		 /// </summary>
		 /// <param name="read"> The current Read instance </param>
		 /// <param name="nodeCursor"> The node cursor to use </param>
		 /// <param name="node"> The id of the node </param>
		 /// <param name="label"> The id of the label </param>
		 /// <returns> {@code true} if the node has the label, otherwise {@code false} </returns>
		 /// <exception cref="EntityNotFoundException"> if the node is not there. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static boolean nodeHasLabel(org.neo4j.internal.kernel.api.Read read, org.neo4j.internal.kernel.api.NodeCursor nodeCursor, long node, int label) throws org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException
		 public static bool NodeHasLabel( Read read, NodeCursor nodeCursor, long node, int label )
		 {
			  if ( label == StatementConstants.NO_SUCH_LABEL )
			  {
					return false;
			  }
			  SingleNode( read, nodeCursor, node );

			  return nodeCursor.HasLabel( label );
		 }

		 public static RelationshipSelectionCursor NodeGetRelationships( Read read, CursorFactory cursors, NodeCursor node, long nodeId, Direction direction, int[] types )
		 {
			  read.SingleNode( nodeId, node );
			  if ( !node.Next() )
			  {
					return Org.Neo4j.@internal.Kernel.Api.helpers.RelationshipSelectionCursor_EMPTY;
			  }
			  switch ( direction.innerEnumValue )
			  {
			  case Direction.InnerEnum.OUTGOING:
					return RelationshipSelections.outgoingCursor( cursors, node, types );
			  case Direction.InnerEnum.INCOMING:
					return RelationshipSelections.incomingCursor( cursors, node, types );
			  case Direction.InnerEnum.BOTH:
					return RelationshipSelections.allCursor( cursors, node, types );
			  default:
					throw new System.InvalidOperationException( "Unknown direction " + direction );
			  }
		 }

		 /// <summary>
		 /// Fetches a given property from a relationship
		 /// </summary>
		 /// <param name="read"> The current Read instance </param>
		 /// <param name="relationship"> The node cursor to use </param>
		 /// <param name="node"> The id of the node </param>
		 /// <param name="propertyCursor"> The property cursor to use </param>
		 /// <param name="prop"> The id of the property to find </param>
		 /// <returns> The value of the given property </returns>
		 /// <exception cref="EntityNotFoundException"> If the node cannot be find. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.values.storable.Value relationshipGetProperty(org.neo4j.internal.kernel.api.Read read, org.neo4j.internal.kernel.api.RelationshipScanCursor relationship, long node, org.neo4j.internal.kernel.api.PropertyCursor propertyCursor, int prop) throws org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException
		 public static Value RelationshipGetProperty( Read read, RelationshipScanCursor relationship, long node, PropertyCursor propertyCursor, int prop )
		 {
			  if ( prop == StatementConstants.NO_SUCH_PROPERTY_KEY )
			  {
					return Values.NO_VALUE;
			  }
			  SingleRelationship( read, relationship, node );
			  relationship.Properties( propertyCursor );
			  while ( propertyCursor.Next() )
			  {
					if ( propertyCursor.PropertyKey() == prop )
					{
						 return propertyCursor.PropertyValue();
					}
			  }

			  return Values.NO_VALUE;
		 }

		 public static RelationshipSelectionCursor NodeGetRelationships( Read read, CursorFactory cursors, NodeCursor node, long nodeId, Direction direction )
		 {
			  return NodeGetRelationships( read, cursors, node, nodeId, direction, null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void singleNode(org.neo4j.internal.kernel.api.Read read, org.neo4j.internal.kernel.api.NodeCursor nodeCursor, long node) throws org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException
		 private static void SingleNode( Read read, NodeCursor nodeCursor, long node )
		 {
			  read.SingleNode( node, nodeCursor );
			  if ( !nodeCursor.Next() )
			  {
					throw new EntityNotFoundException( EntityType.NODE, node );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void singleRelationship(org.neo4j.internal.kernel.api.Read read, org.neo4j.internal.kernel.api.RelationshipScanCursor relationships, long relationship) throws org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException
		 private static void SingleRelationship( Read read, RelationshipScanCursor relationships, long relationship )
		 {
			  read.SingleRelationship( relationship, relationships );
			  if ( !relationships.Next() )
			  {
					throw new EntityNotFoundException( EntityType.NODE, relationship );
			  }
		 }
	}


}