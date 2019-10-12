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
namespace Org.Neo4j.@internal.Kernel.Api.helpers
{

	/// <summary>
	/// Helper methods for working with nodes
	/// </summary>
	public sealed class Nodes
	{
		 private Nodes()
		 {
			  throw new System.NotSupportedException( "Do not instantiate" );
		 }

		 /// <summary>
		 /// Counts the number of outgoing relationships from node where the cursor is positioned.
		 /// <para>
		 /// NOTE: The number of outgoing relationships also includes eventual loops.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="nodeCursor"> a cursor positioned at the node whose relationships we're counting </param>
		 /// <param name="cursors"> a factory for cursors </param>
		 /// <returns> the number of outgoing - including loops - relationships from the node </returns>
		 public static int CountOutgoing( NodeCursor nodeCursor, CursorFactory cursors )
		 {
			  if ( nodeCursor.Dense )
			  {
					using ( RelationshipGroupCursor group = cursors.AllocateRelationshipGroupCursor() )
					{
						 nodeCursor.Relationships( group );
						 int count = 0;
						 while ( group.next() )
						 {
							  count += group.OutgoingCount() + group.LoopCount();
						 }
						 return count;
					}
			  }
			  else
			  {
					using ( RelationshipTraversalCursor traversal = cursors.AllocateRelationshipTraversalCursor() )
					{
						 int count = 0;
						 nodeCursor.AllRelationships( traversal );
						 while ( traversal.next() )
						 {
							  if ( traversal.SourceNodeReference() == nodeCursor.NodeReference() )
							  {
									count++;
							  }
						 }
						 return count;
					}
			  }
		 }

		 /// <summary>
		 /// Counts the number of incoming relationships from node where the cursor is positioned.
		 /// <para>
		 /// NOTE: The number of incoming relationships also includes eventual loops.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="nodeCursor"> a cursor positioned at the node whose relationships we're counting </param>
		 /// <param name="cursors"> a factory for cursors </param>
		 /// <returns> the number of incoming - including loops - relationships from the node </returns>
		 public static int CountIncoming( NodeCursor nodeCursor, CursorFactory cursors )
		 {
			  if ( nodeCursor.Dense )
			  {
					using ( RelationshipGroupCursor group = cursors.AllocateRelationshipGroupCursor() )
					{
						 nodeCursor.Relationships( group );
						 int count = 0;
						 while ( group.next() )
						 {
							  count += group.IncomingCount() + group.LoopCount();
						 }
						 return count;
					}
			  }
			  else
			  {
					using ( RelationshipTraversalCursor traversal = cursors.AllocateRelationshipTraversalCursor() )
					{
						 int count = 0;
						 nodeCursor.AllRelationships( traversal );
						 while ( traversal.next() )
						 {
							  if ( traversal.TargetNodeReference() == nodeCursor.NodeReference() )
							  {
									count++;
							  }
						 }
						 return count;
					}
			  }
		 }

		 /// <summary>
		 /// Counts all the relationships from node where the cursor is positioned.
		 /// </summary>
		 /// <param name="nodeCursor"> a cursor positioned at the node whose relationships we're counting </param>
		 /// <param name="cursors"> a factory for cursors </param>
		 /// <returns> the number of relationships from the node </returns>
		 public static int CountAll( NodeCursor nodeCursor, CursorFactory cursors )
		 {
			  if ( nodeCursor.Dense )
			  {
					using ( RelationshipGroupCursor group = cursors.AllocateRelationshipGroupCursor() )
					{
						 nodeCursor.Relationships( group );
						 int count = 0;
						 while ( group.next() )
						 {
							  count += group.TotalCount();
						 }
						 return count;
					}
			  }
			  else
			  {
					using ( RelationshipTraversalCursor traversal = cursors.AllocateRelationshipTraversalCursor() )
					{
						 int count = 0;
						 nodeCursor.AllRelationships( traversal );
						 while ( traversal.next() )
						 {
							  count++;
						 }
						 return count;
					}
			  }
		 }

		 /// <summary>
		 /// Counts the number of outgoing relationships of the given type from node where the cursor is positioned.
		 /// <para>
		 /// NOTE: The number of outgoing relationships also includes eventual loops.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="nodeCursor"> a cursor positioned at the node whose relationships we're counting </param>
		 /// <param name="cursors"> a factory for cursors </param>
		 /// <param name="type"> the type of the relationship we're counting </param>
		 /// <returns> the number of outgoing - including loops - relationships from the node with the given type </returns>
		 public static int CountOutgoing( NodeCursor nodeCursor, CursorFactory cursors, int type )
		 {
			  if ( nodeCursor.Dense )
			  {
					using ( RelationshipGroupCursor group = cursors.AllocateRelationshipGroupCursor() )
					{
						 nodeCursor.Relationships( group );
						 while ( group.next() )
						 {
							  if ( group.Type() == type )
							  {
									return group.OutgoingCount() + group.LoopCount();
							  }
						 }
						 return 0;
					}
			  }
			  else
			  {
					using ( RelationshipTraversalCursor traversal = cursors.AllocateRelationshipTraversalCursor() )
					{
						 int count = 0;
						 nodeCursor.AllRelationships( traversal );
						 while ( traversal.next() )
						 {
							  if ( traversal.SourceNodeReference() == nodeCursor.NodeReference() && traversal.Type() == type )
							  {
									count++;
							  }
						 }
						 return count;
					}
			  }
		 }

		 /// <summary>
		 /// Counts the number of incoming relationships of the given type from node where the cursor is positioned.
		 /// <para>
		 /// NOTE: The number of incoming relationships also includes eventual loops.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="nodeCursor"> a cursor positioned at the node whose relationships we're counting </param>
		 /// <param name="cursors"> a factory for cursors </param>
		 /// <param name="type"> the type of the relationship we're counting </param>
		 /// <returns> the number of incoming - including loops - relationships from the node with the given type </returns>
		 public static int CountIncoming( NodeCursor nodeCursor, CursorFactory cursors, int type )
		 {
			  if ( nodeCursor.Dense )
			  {
					using ( RelationshipGroupCursor group = cursors.AllocateRelationshipGroupCursor() )
					{
						 nodeCursor.Relationships( group );
						 int count = 0;
						 while ( group.next() )
						 {
							  if ( group.Type() == type )
							  {
									return group.IncomingCount() + group.LoopCount();
							  }
						 }
						 return count;
					}
			  }
			  else
			  {
					using ( RelationshipTraversalCursor traversal = cursors.AllocateRelationshipTraversalCursor() )
					{
						 int count = 0;
						 nodeCursor.AllRelationships( traversal );
						 while ( traversal.next() )
						 {
							  if ( traversal.TargetNodeReference() == nodeCursor.NodeReference() && traversal.Type() == type )
							  {
									count++;
							  }
						 }
						 return count;
					}
			  }
		 }

		 /// <summary>
		 /// Counts all the relationships of the given type from node where the cursor is positioned.
		 /// </summary>
		 /// <param name="nodeCursor"> a cursor positioned at the node whose relationships we're counting </param>
		 /// <param name="cursors"> a factory for cursors </param>
		 /// <param name="type"> the type of the relationship we're counting </param>
		 /// <returns> the number relationships from the node with the given type </returns>
		 public static int CountAll( NodeCursor nodeCursor, CursorFactory cursors, int type )
		 {
			  if ( nodeCursor.Dense )
			  {
					using ( RelationshipGroupCursor group = cursors.AllocateRelationshipGroupCursor() )
					{
						 nodeCursor.Relationships( group );
						 int count = 0;
						 while ( group.next() )
						 {
							  if ( group.Type() == type )
							  {
									return group.TotalCount();
							  }
						 }
						 return count;
					}
			  }
			  else
			  {
					using ( RelationshipTraversalCursor traversal = cursors.AllocateRelationshipTraversalCursor() )
					{
						 int count = 0;
						 nodeCursor.AllRelationships( traversal );
						 while ( traversal.next() )
						 {
							  if ( traversal.Type() == type )
							  {
									count++;
							  }
						 }
						 return count;
					}
			  }
		 }
	}

}