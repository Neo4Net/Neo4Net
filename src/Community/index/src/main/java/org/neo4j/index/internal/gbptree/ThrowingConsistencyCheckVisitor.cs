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

	using Exceptions = Neo4Net.Helpers.Exceptions;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.TreeNode.NO_NODE_FLAG;

	public class ThrowingConsistencyCheckVisitor<KEY> : GBPTreeConsistencyCheckVisitor<KEY>
	{
		 private const string TREE_STRUCTURE_INCONSISTENCY = "Tree structure inconsistency: ";
		 private const string KEY_ORDER_INCONSISTENCY = "Key order inconsistency: ";
		 private const string NODE_META_INCONSISTENCY = "Node meta inconsistency: ";
		 private const string TREE_META_INCONSISTENCY = "Tree meta inconsistency: ";
		 private const string UNEXPECTED_EXCEPTION_INCONSISTENCY = "Unexpected exception inconsistency: ";

		 public override void NotATreeNode( long pageId, File file )
		 {
			  ThrowTreeStructureInconsistency( "Page: %d is not a tree node page.", pageId );
		 }

		 public override void UnknownTreeNodeType( long pageId, sbyte treeNodeType, File file )
		 {
			  ThrowTreeStructureInconsistency( "Page: %d has an unknown tree node type: %d.", pageId, treeNodeType );
		 }

		 public override void SiblingsDontPointToEachOther( long leftNode, long leftNodeGeneration, long leftRightSiblingPointerGeneration, long leftRightSiblingPointer, long rightLeftSiblingPointer, long rightLeftSiblingPointerGeneration, long rightNode, long rightNodeGeneration, File file )
		 {
			  ThrowTreeStructureInconsistency( "Sibling pointers misaligned.%n" + "  Left siblings view:  %s%n" + "  Right siblings view: %s%n", LeftPattern( leftNode, leftNodeGeneration, leftRightSiblingPointerGeneration, leftRightSiblingPointer ), RightPattern( rightNode, rightNodeGeneration, rightLeftSiblingPointerGeneration, rightLeftSiblingPointer ) );
		 }

		 public override void RightmostNodeHasRightSibling( long rightSiblingPointer, long rightmostNode, File file )
		 {
			  ThrowTreeStructureInconsistency( "Expected rightmost right sibling to be %d but was %d. Current rightmost node is %d.", NO_NODE_FLAG, rightSiblingPointer, rightmostNode );
		 }

		 public override void PointerToOldVersionOfTreeNode( long pageId, long successorPointer, File file )
		 {
			  ThrowTreeStructureInconsistency( "We ended up on tree node %d which has a newer generation, successor is: %d", pageId, successorPointer );
		 }

		 public override void PointerHasLowerGenerationThanNode( GBPTreePointerType pointerType, long sourceNode, long pointerGeneration, long pointer, long targetNodeGeneration, File file )
		 {
			  ThrowTreeStructureInconsistency( "Pointer (%s) in tree node %d has pointer generation %d, but target node %d has a higher generation %d.", pointerType.ToString(), sourceNode, pointerGeneration, pointer, targetNodeGeneration );
		 }

		 public override void KeysOutOfOrderInNode( long pageId, File file )
		 {
			  ThrowKeyOrderInconsistency( "Keys in tree node %d are out of order.", pageId );
		 }

		 public override void KeysLocatedInWrongNode( KeyRange<KEY> range, KEY key, int pos, int keyCount, long pageId, File file )
		 {
			  ThrowKeyOrderInconsistency( "Expected range for this tree node is %n%s%n but found %s in position %d, with keyCount %d on page %d.", range, key, pos, keyCount, pageId );
		 }

		 public override void UnusedPage( long pageId, File file )
		 {
			  ThrowTreeMetaInconsistency( "Index has a leaked page that will never be reclaimed, pageId=%d.", pageId );
		 }

		 public override void PageIdExceedLastId( long lastId, long pageId, File file )
		 {
			  ThrowTreeMetaInconsistency( "Tree node has page id larger than registered last id, lastId=%d, pageId=%d.", lastId, pageId );
		 }

		 public override void NodeMetaInconsistency( long pageId, string message, File file )
		 {
			  ThrowNodeMetaInconsistency( "Tree node %d has inconsistent meta data: %s.", pageId, message );
		 }

		 public override void PageIdSeenMultipleTimes( long pageId, File file )
		 {
			  ThrowTreeStructureInconsistency( "Page id seen multiple times, this means either active tree node is present in freelist or pointers in tree create a loop, pageId=%d.", pageId );
		 }

		 public override void CrashedPointer( long pageId, GBPTreePointerType pointerType, long generationA, long readPointerA, long pointerA, sbyte stateA, long generationB, long readPointerB, long pointerB, sbyte stateB, File file )
		 {
			  ThrowTreeStructureInconsistency( "Crashed pointer found in tree node %d, pointer: %s%n  slotA[%s]%n  slotB[%s]", pageId, pointerType.ToString(), StateToString(generationA, readPointerA, pointerA, stateA), StateToString(generationB, readPointerB, pointerB, stateB) );
		 }

		 public override void BrokenPointer( long pageId, GBPTreePointerType pointerType, long generationA, long readPointerA, long pointerA, sbyte stateA, long generationB, long readPointerB, long pointerB, sbyte stateB, File file )
		 {
			  ThrowTreeStructureInconsistency( "Broken pointer found in tree node %d, pointer: %s%n  slotA[%s]%n  slotB[%s]", pageId, pointerType.ToString(), StateToString(generationA, readPointerA, pointerA, stateA), StateToString(generationB, readPointerB, pointerB, stateB) );
		 }

		 public override void UnreasonableKeyCount( long pageId, int keyCount, File file )
		 {
			  ThrowTreeMetaInconsistency( "Unexpected keyCount on pageId %d, keyCount=%d", pageId, keyCount );
		 }

		 public override void ChildNodeFoundAmongParentNodes( KeyRange<KEY> parentRange, int level, long pageId, File file )
		 {
			  ThrowTreeStructureInconsistency( "Circular reference, child tree node found among parent nodes. Parents:%n%s%nlevel: %d, pageId: %d", parentRange, level, pageId );
		 }

		 public override void Exception( Exception e )
		 {
			  ThrowUnexpectedExceptionInconsistency( "%s", Exceptions.stringify( e ) );
		 }

		 private static string StateToString( long generation, long readPointer, long pointer, sbyte stateA )
		 {
			  return format( "generation=%d, readPointer=%d, pointer=%d, state=%s", generation, readPointer, pointer, GenerationSafePointerPair.PointerStateName( stateA ) );
		 }

		 private string LeftPattern( long actualLeftSibling, long actualLeftSiblingGeneration, long expectedRightSiblingGeneration, long expectedRightSibling )
		 {
			  return format( "{%d(%d)}-(%d)->{%d}", actualLeftSibling, actualLeftSiblingGeneration, expectedRightSiblingGeneration, expectedRightSibling );
		 }

		 private string RightPattern( long actualRightSibling, long actualRightSiblingGeneration, long expectedLeftSiblingGeneration, long expectedLeftSibling )
		 {
			  return format( "{%d}<-(%d)-{%d(%d)}", expectedLeftSibling, expectedLeftSiblingGeneration, actualRightSibling, actualRightSiblingGeneration );
		 }

		 private void ThrowKeyOrderInconsistency( string format, params object[] args )
		 {
			  ThrowWithPrefix( KEY_ORDER_INCONSISTENCY, format, args );
		 }

		 private void ThrowTreeStructureInconsistency( string format, params object[] args )
		 {
			  ThrowWithPrefix( TREE_STRUCTURE_INCONSISTENCY, format, args );
		 }

		 private void ThrowNodeMetaInconsistency( string format, params object[] args )
		 {
			  ThrowWithPrefix( NODE_META_INCONSISTENCY, format, args );
		 }

		 private void ThrowTreeMetaInconsistency( string format, params object[] args )
		 {
			  ThrowWithPrefix( TREE_META_INCONSISTENCY, format, args );
		 }

		 private void ThrowUnexpectedExceptionInconsistency( string format, params object[] args )
		 {
			  ThrowWithPrefix( UNEXPECTED_EXCEPTION_INCONSISTENCY, format, args );
		 }

		 private void ThrowWithPrefix( string prefix, string format, object[] args )
		 {
			  throw new TreeInconsistencyException( string.format( prefix + format, args ) );
		 }
	}

}