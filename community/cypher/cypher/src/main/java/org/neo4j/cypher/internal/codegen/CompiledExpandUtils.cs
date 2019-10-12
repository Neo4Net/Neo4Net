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
	using Read = Org.Neo4j.@internal.Kernel.Api.Read;
	using EntityNotFoundException = Org.Neo4j.@internal.Kernel.Api.exceptions.EntityNotFoundException;
	using RelationshipSelectionCursor = Org.Neo4j.@internal.Kernel.Api.helpers.RelationshipSelectionCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.helpers.Nodes.countAll;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.helpers.Nodes.countIncoming;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.helpers.Nodes.countOutgoing;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public abstract class CompiledExpandUtils
	public abstract class CompiledExpandUtils
	{
		 private const int NOT_DENSE_DEGREE = -1;

		 public static RelationshipSelectionCursor ConnectingRelationships( Read read, CursorFactory cursors, NodeCursor nodeCursor, long fromNode, Direction direction, long toNode )
		 {
			  //Check from
			  int fromDegree = NodeGetDegreeIfDense( read, fromNode, nodeCursor, cursors, direction );
			  if ( fromDegree == 0 )
			  {
					return Org.Neo4j.@internal.Kernel.Api.helpers.RelationshipSelectionCursor_EMPTY;
			  }
			  bool fromNodeIsDense = fromDegree != NOT_DENSE_DEGREE;

			  //Check to
			  read.SingleNode( toNode, nodeCursor );
			  if ( !nodeCursor.Next() )
			  {
					return Org.Neo4j.@internal.Kernel.Api.helpers.RelationshipSelectionCursor_EMPTY;
			  }
			  bool toNodeIsDense = nodeCursor.Dense;

			  //Both are dense, start with the one with the lesser degree
			  if ( fromNodeIsDense && toNodeIsDense )
			  {
					//Note that we have already position the cursor at toNode
					int toDegree = NodeGetDegree( nodeCursor, cursors, direction );
					long startNode;
					long endNode;
					Direction relDirection;
					if ( fromDegree < toDegree )
					{
						 startNode = fromNode;
						 endNode = toNode;
						 relDirection = direction;
					}
					else
					{
						 startNode = toNode;
						 endNode = fromNode;
						 relDirection = direction.reverse();
					}

					return ConnectingRelationshipsIterator( CompiledCursorUtils.NodeGetRelationships( read, cursors, nodeCursor, startNode, relDirection ), endNode );
			  }
			  else if ( fromNodeIsDense )
			  {
					return ConnectingRelationshipsIterator( CompiledCursorUtils.NodeGetRelationships( read, cursors, nodeCursor, toNode, direction.reverse() ), fromNode );
			  }
			  else
			  { //either only toNode is dense or none of them, just go with what we got
					return ConnectingRelationshipsIterator( CompiledCursorUtils.NodeGetRelationships( read, cursors, nodeCursor, fromNode, direction ), toNode );
			  }
		 }

		 public static RelationshipSelectionCursor ConnectingRelationships( Read read, CursorFactory cursors, NodeCursor nodeCursor, long fromNode, Direction direction, long toNode, int[] relTypes )
		 {
			  //Check from
			  int fromDegree = CalculateTotalDegreeIfDense( read, fromNode, nodeCursor, direction, relTypes, cursors );
			  if ( fromDegree == 0 )
			  {
					return Org.Neo4j.@internal.Kernel.Api.helpers.RelationshipSelectionCursor_EMPTY;
			  }
			  bool fromNodeIsDense = fromDegree != NOT_DENSE_DEGREE;

			  //Check to
			  read.SingleNode( toNode, nodeCursor );
			  if ( !nodeCursor.Next() )
			  {
					return Org.Neo4j.@internal.Kernel.Api.helpers.RelationshipSelectionCursor_EMPTY;
			  }
			  bool toNodeIsDense = nodeCursor.Dense;

			  //Both are dense, start with the one with the lesser degree
			  if ( fromNodeIsDense && toNodeIsDense )
			  {
					//Note that we have already position the cursor at toNode
					int toDegree = CalculateTotalDegree( nodeCursor, direction, relTypes, cursors );
					long startNode;
					long endNode;
					Direction relDirection;
					if ( fromDegree < toDegree )
					{
						 startNode = fromNode;
						 endNode = toNode;
						 relDirection = direction;
					}
					else
					{
						 startNode = toNode;
						 endNode = fromNode;
						 relDirection = direction.reverse();
					}

					return ConnectingRelationshipsIterator( CompiledCursorUtils.NodeGetRelationships( read, cursors, nodeCursor, startNode, relDirection, relTypes ), endNode );
			  }
			  else if ( fromNodeIsDense )
			  {
					return ConnectingRelationshipsIterator( CompiledCursorUtils.NodeGetRelationships( read, cursors, nodeCursor, toNode, direction.reverse(), relTypes ), fromNode );
			  }
			  else
			  { //either only toNode is dense or none of them, just go with what we got
					return ConnectingRelationshipsIterator( CompiledCursorUtils.NodeGetRelationships( read, cursors, nodeCursor, fromNode, direction, relTypes ), toNode );
			  }
		 }

		 internal static int NodeGetDegreeIfDense( Read read, long node, NodeCursor nodeCursor, CursorFactory cursors, Direction direction )
		 {
			  read.SingleNode( node, nodeCursor );
			  if ( !nodeCursor.Next() )
			  {
					return 0;
			  }
			  if ( !nodeCursor.Dense )
			  {
					return NOT_DENSE_DEGREE;
			  }

			  return NodeGetDegree( nodeCursor, cursors, direction );
		 }

		 private static int NodeGetDegree( NodeCursor nodeCursor, CursorFactory cursors, Direction direction )
		 {
			  switch ( direction.innerEnumValue )
			  {
			  case Direction.InnerEnum.OUTGOING:
					return countOutgoing( nodeCursor, cursors );
			  case Direction.InnerEnum.INCOMING:
					return countIncoming( nodeCursor, cursors );
			  case Direction.InnerEnum.BOTH:
					return countAll( nodeCursor, cursors );
			  default:
					throw new System.InvalidOperationException( "Unknown direction " + direction );
			  }
		 }

		 internal static int NodeGetDegreeIfDense( Read read, long node, NodeCursor nodeCursor, CursorFactory cursors, Direction direction, int type )
		 {
			  read.SingleNode( node, nodeCursor );
			  if ( !nodeCursor.Next() )
			  {
					return 0;
			  }
			  if ( !nodeCursor.Dense )
			  {
					return NOT_DENSE_DEGREE;
			  }

			  return NodeGetDegree( nodeCursor, cursors, direction, type );
		 }

		 private static int NodeGetDegree( NodeCursor nodeCursor, CursorFactory cursors, Direction direction, int type )
		 {
			  switch ( direction.innerEnumValue )
			  {
			  case Direction.InnerEnum.OUTGOING:
					return countOutgoing( nodeCursor, cursors, type );
			  case Direction.InnerEnum.INCOMING:
					return countIncoming( nodeCursor, cursors, type );
			  case Direction.InnerEnum.BOTH:
					return countAll( nodeCursor, cursors, type );
			  default:
					throw new System.InvalidOperationException( "Unknown direction " + direction );
			  }
		 }

		 private static int CalculateTotalDegreeIfDense( Read read, long node, NodeCursor nodeCursor, Direction direction, int[] relTypes, CursorFactory cursors )
		 {
			  read.SingleNode( node, nodeCursor );
			  if ( !nodeCursor.Next() )
			  {
					return 0;
			  }
			  if ( !nodeCursor.Dense )
			  {
					return NOT_DENSE_DEGREE;
			  }
			  return CalculateTotalDegree( nodeCursor, direction, relTypes, cursors );
		 }

		 private static int CalculateTotalDegree( NodeCursor nodeCursor, Direction direction, int[] relTypes, CursorFactory cursors )
		 {
			  int degree = 0;
			  foreach ( int relType in relTypes )
			  {
					degree += NodeGetDegree( nodeCursor, cursors, direction, relType );
			  }

			  return degree;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.neo4j.internal.kernel.api.helpers.RelationshipSelectionCursor connectingRelationshipsIterator(final org.neo4j.internal.kernel.api.helpers.RelationshipSelectionCursor allRelationships, final long toNode)
		 private static RelationshipSelectionCursor ConnectingRelationshipsIterator( RelationshipSelectionCursor allRelationships, long toNode )
		 {
			  return new RelationshipSelectionCursorAnonymousInnerClass( allRelationships, toNode );
		 }

		 private class RelationshipSelectionCursorAnonymousInnerClass : RelationshipSelectionCursor
		 {
			 private RelationshipSelectionCursor _allRelationships;
			 private long _toNode;

			 public RelationshipSelectionCursorAnonymousInnerClass( RelationshipSelectionCursor allRelationships, long toNode )
			 {
				 this._allRelationships = allRelationships;
				 this._toNode = toNode;
			 }

			 public void close()
			 {
				  _allRelationships.close();
			 }

			 public long relationshipReference()
			 {
				  return _allRelationships.relationshipReference();
			 }

			 public int type()
			 {
				  return _allRelationships.type();
			 }

			 public long otherNodeReference()
			 {
				  return _allRelationships.otherNodeReference();
			 }

			 public long sourceNodeReference()
			 {
				  return _allRelationships.sourceNodeReference();
			 }

			 public long targetNodeReference()
			 {
				  return _allRelationships.targetNodeReference();
			 }

			 public long propertiesReference()
			 {
				  return _allRelationships.propertiesReference();
			 }

			 public bool next()
			 {
				  while ( _allRelationships.next() )
				  {
						if ( _allRelationships.otherNodeReference() == _toNode )
						{
							 return true;
						}
				  }

				  return false;
			 }
		 }
	}

}