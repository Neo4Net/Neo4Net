using System;

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
namespace Neo4Net.Index.@internal.gbptree
{

	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;

	/// <summary>
	/// The @Documented annotations are used for error messages in consistency checker.
	/// </summary>
	public interface GBPTreeConsistencyCheckVisitor<KEY>
	{

		 [Documented("Index inconsistency: " + "Page: %d is not a tree node page. " + GBPTreeConsistencyCheckVisitor_Fields.indexFile)]
		 void NotATreeNode( long pageId, File file );

		 [Documented("Index inconsistency: " + "Page: %d has an unknown tree node type: %d.%n" + GBPTreeConsistencyCheckVisitor_Fields.indexFile)]
		 void UnknownTreeNodeType( long pageId, sbyte treeNodeType, File file );

		 [Documented("Index inconsistency: " + "Sibling pointers misaligned.%n" + "Left siblings view:  {%d(%d)}-(%d)->{%d},%n" + "Right siblings view: {%d}<-(%d)-{%d(%d)}.%n" + GBPTreeConsistencyCheckVisitor_Fields.indexFile)]
		 void SiblingsDontPointToEachOther( long leftNode, long leftNodeGeneration, long leftRightSiblingPointerGeneration, long leftRightSiblingPointer, long rightLeftSiblingPointer, long rightLeftSiblingPointerGeneration, long rightNode, long rightNodeGeneration, File file );

		 [Documented("Index inconsistency: " + "Expected rightmost node to have no right sibling but was %d. Current rightmost node is %d.%n" + GBPTreeConsistencyCheckVisitor_Fields.indexFile)]
		 void RightmostNodeHasRightSibling( long rightSiblingPointer, long rightmostNode, File file );

		 [Documented("Index inconsistency: " + "We ended up on tree node %d which has a newer generation, successor is: %d.%n" + GBPTreeConsistencyCheckVisitor_Fields.indexFile)]
		 void PointerToOldVersionOfTreeNode( long pageId, long successorPointer, File file );

		 [Documented("Index inconsistency: " + "Pointer (%s) in tree node %d has pointer generation %d, but target node %d has a higher generation %d.%n" + GBPTreeConsistencyCheckVisitor_Fields.indexFile)]
		 void PointerHasLowerGenerationThanNode( GBPTreePointerType pointerType, long sourceNode, long pointerGeneration, long pointer, long targetNodeGeneration, File file );

		 [Documented("Index inconsistency: " + "Keys in tree node %d are out of order.%n" + GBPTreeConsistencyCheckVisitor_Fields.indexFile)]
		 void KeysOutOfOrderInNode( long pageId, File file );

		 [Documented("Index inconsistency: " + "Expected range for this tree node is %n%s%n but found %s in position %d, with keyCount %d on page %d.%n" + GBPTreeConsistencyCheckVisitor_Fields.indexFile)]
		 void KeysLocatedInWrongNode( KeyRange<KEY> range, KEY key, int pos, int keyCount, long pageId, File file );

		 [Documented("Index inconsistency: " + "Index has a leaked page that will never be reclaimed, pageId=%d.%n" + GBPTreeConsistencyCheckVisitor_Fields.indexFile)]
		 void UnusedPage( long pageId, File file );

		 [Documented("Index inconsistency: " + "Tree node has page id larger than registered last id, lastId=%d, pageId=%d.%n" + GBPTreeConsistencyCheckVisitor_Fields.indexFile)]
		 void PageIdExceedLastId( long lastId, long pageId, File file );

		 [Documented("Index inconsistency: " + "Tree node %d has inconsistent meta data: %s.%n" + GBPTreeConsistencyCheckVisitor_Fields.indexFile)]
		 void NodeMetaInconsistency( long pageId, string message, File file );

		 [Documented("Index inconsistency: " + "Page id seen multiple times, this means either active tree node is present in freelist or pointers in tree create a loop, pageId=%d.%n" + GBPTreeConsistencyCheckVisitor_Fields.indexFile)]
		 void PageIdSeenMultipleTimes( long pageId, File file );

		 [Documented("Index inconsistency: " + "Crashed pointer found in tree node %d, pointerType='%s',%n" + "slotA[generation=%d, readPointer=%d, pointer=%d, state=%s],%n" + "slotB[generation=%d, readPointer=%d, pointer=%d, state=%s].%n" + GBPTreeConsistencyCheckVisitor_Fields.indexFile)]
		 void CrashedPointer( long pageId, GBPTreePointerType pointerType, long generationA, long readPointerA, long pointerA, sbyte stateA, long generationB, long readPointerB, long pointerB, sbyte stateB, File file );

		 [Documented("Index inconsistency: " + "Broken pointer found in tree node %d, pointerType='%s',%n" + "slotA[generation=%d, readPointer=%d, pointer=%d, state=%s],%n" + "slotB[generation=%d, readPointer=%d, pointer=%d, state=%s].%n" + GBPTreeConsistencyCheckVisitor_Fields.indexFile)]
		 void BrokenPointer( long pageId, GBPTreePointerType pointerType, long generationA, long readPointerA, long pointerA, sbyte stateA, long generationB, long readPointerB, long pointerB, sbyte stateB, File file );

		 [Documented("Index inconsistency: " + "Unexpected keyCount on pageId %d, keyCount=%d.%n" + GBPTreeConsistencyCheckVisitor_Fields.indexFile)]
		 void UnreasonableKeyCount( long pageId, int keyCount, File file );

		 [Documented("Index inconsistency: " + "Circular reference, child tree node found among parent nodes. Parents:%n" + "%s,%n" + "level: %d, pageId: %d.%n" + GBPTreeConsistencyCheckVisitor_Fields.indexFile)]
		 void ChildNodeFoundAmongParentNodes( KeyRange<KEY> superRange, int level, long pageId, File file );

		 [Documented("Index inconsistency: " + "Caught exception during consistency check: %s")]
		 void Exception( Exception e );
	}

	public static class GBPTreeConsistencyCheckVisitor_Fields
	{
		 public const string INDEX_FILE = "Index file: %s.";
	}

	 public class GBPTreeConsistencyCheckVisitor_Adaptor<KEY> : GBPTreeConsistencyCheckVisitor<KEY>
	 {
		  public override void NotATreeNode( long pageId, File file )
		  {
		  }

		  public override void UnknownTreeNodeType( long pageId, sbyte treeNodeType, File file )
		  {
		  }

		  public override void SiblingsDontPointToEachOther( long leftNode, long leftNodeGeneration, long leftRightSiblingPointerGeneration, long leftRightSiblingPointer, long rightLeftSiblingPointer, long rightLeftSiblingPointerGeneration, long rightNode, long rightNodeGeneration, File file )
		  {
		  }

		  public override void RightmostNodeHasRightSibling( long rightSiblingPointer, long rightmostNode, File file )
		  {
		  }

		  public override void PointerToOldVersionOfTreeNode( long pageId, long successorPointer, File file )
		  {
		  }

		  public override void PointerHasLowerGenerationThanNode( GBPTreePointerType pointerType, long sourceNode, long pointerGeneration, long pointer, long targetNodeGeneration, File file )
		  {
		  }

		  public override void KeysOutOfOrderInNode( long pageId, File file )
		  {
		  }

		  public override void KeysLocatedInWrongNode( KeyRange<KEY> range, KEY key, int pos, int keyCount, long pageId, File file )
		  {
		  }

		  public override void UnusedPage( long pageId, File file )
		  {
		  }

		  public override void PageIdExceedLastId( long lastId, long pageId, File file )
		  {
		  }

		  public override void NodeMetaInconsistency( long pageId, string message, File file )
		  {
		  }

		  public override void PageIdSeenMultipleTimes( long pageId, File file )
		  {
		  }

		  public override void CrashedPointer( long pageId, GBPTreePointerType pointerType, long generationA, long readPointerA, long pointerA, sbyte stateA, long generationB, long readPointerB, long pointerB, sbyte stateB, File file )
		  {
		  }

		  public override void BrokenPointer( long pageId, GBPTreePointerType pointerType, long generationA, long readPointerA, long pointerA, sbyte stateA, long generationB, long readPointerB, long pointerB, sbyte stateB, File file )
		  {
		  }

		  public override void UnreasonableKeyCount( long pageId, int keyCount, File file )
		  {
		  }

		  public override void ChildNodeFoundAmongParentNodes( KeyRange<KEY> superRange, int level, long pageId, File file )
		  {
		  }

		  public override void Exception( Exception e )
		  {
		  }
	 }

}