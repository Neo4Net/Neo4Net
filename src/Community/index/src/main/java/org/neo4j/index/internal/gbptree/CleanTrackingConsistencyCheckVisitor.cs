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
	using MutableBoolean = org.apache.commons.lang3.mutable.MutableBoolean;

	internal class CleanTrackingConsistencyCheckVisitor<KEY> : GBPTreeConsistencyCheckVisitor<KEY>
	{
		 private readonly MutableBoolean _clean = new MutableBoolean( true );
		 private readonly GBPTreeConsistencyCheckVisitor<KEY> @delegate;

		 internal CleanTrackingConsistencyCheckVisitor( GBPTreeConsistencyCheckVisitor<KEY> @delegate )
		 {
			  this.@delegate = @delegate;
		 }

		 internal virtual bool Clean()
		 {
			  return _clean.booleanValue();
		 }

		 public override void NotATreeNode( long pageId, File file )
		 {
			  _clean.setFalse();
			  @delegate.NotATreeNode( pageId, file );
		 }

		 public override void UnknownTreeNodeType( long pageId, sbyte treeNodeType, File file )
		 {
			  _clean.setFalse();
			  @delegate.UnknownTreeNodeType( pageId, treeNodeType, file );
		 }

		 public override void SiblingsDontPointToEachOther( long leftNode, long leftNodeGeneration, long leftRightSiblingPointerGeneration, long leftRightSiblingPointer, long rightLeftSiblingPointer, long rightLeftSiblingPointerGeneration, long rightNode, long rightNodeGeneration, File file )
		 {
			  _clean.setFalse();
			  @delegate.SiblingsDontPointToEachOther( leftNode, leftNodeGeneration, leftRightSiblingPointerGeneration, leftRightSiblingPointer, rightLeftSiblingPointer, rightLeftSiblingPointerGeneration, rightNode, rightNodeGeneration, file );
		 }

		 public override void RightmostNodeHasRightSibling( long rightSiblingPointer, long rightmostNode, File file )
		 {
			  _clean.setFalse();
			  @delegate.RightmostNodeHasRightSibling( rightSiblingPointer, rightmostNode, file );
		 }

		 public override void PointerToOldVersionOfTreeNode( long pageId, long successorPointer, File file )
		 {
			  _clean.setFalse();
			  @delegate.PointerToOldVersionOfTreeNode( pageId, successorPointer, file );
		 }

		 public override void PointerHasLowerGenerationThanNode( GBPTreePointerType pointerType, long sourceNode, long pointerGeneration, long pointer, long targetNodeGeneration, File file )
		 {
			  _clean.setFalse();
			  @delegate.PointerHasLowerGenerationThanNode( pointerType, sourceNode, pointerGeneration, pointer, targetNodeGeneration, file );
		 }

		 public override void KeysOutOfOrderInNode( long pageId, File file )
		 {
			  _clean.setFalse();
			  @delegate.KeysOutOfOrderInNode( pageId, file );
		 }

		 public override void KeysLocatedInWrongNode( KeyRange<KEY> range, KEY key, int pos, int keyCount, long pageId, File file )
		 {
			  _clean.setFalse();
			  @delegate.KeysLocatedInWrongNode( range, key, pos, keyCount, pageId, file );
		 }

		 public override void UnusedPage( long pageId, File file )
		 {
			  _clean.setFalse();
			  @delegate.UnusedPage( pageId, file );
		 }

		 public override void PageIdExceedLastId( long lastId, long pageId, File file )
		 {
			  _clean.setFalse();
			  @delegate.PageIdExceedLastId( lastId, pageId, file );
		 }

		 public override void NodeMetaInconsistency( long pageId, string message, File file )
		 {
			  _clean.setFalse();
			  @delegate.NodeMetaInconsistency( pageId, message, file );
		 }

		 public override void PageIdSeenMultipleTimes( long pageId, File file )
		 {
			  _clean.setFalse();
			  @delegate.PageIdSeenMultipleTimes( pageId, file );
		 }

		 public override void CrashedPointer( long pageId, GBPTreePointerType pointerType, long generationA, long readPointerA, long pointerA, sbyte stateA, long generationB, long readPointerB, long pointerB, sbyte stateB, File file )
		 {
			  _clean.setFalse();
			  @delegate.CrashedPointer( pageId, pointerType, generationA, readPointerA, pointerA, stateA, generationB, readPointerB, pointerB, stateB, file );
		 }

		 public override void BrokenPointer( long pageId, GBPTreePointerType pointerType, long generationA, long readPointerA, long pointerA, sbyte stateA, long generationB, long readPointerB, long pointerB, sbyte stateB, File file )
		 {
			  _clean.setFalse();
			  @delegate.BrokenPointer( pageId, pointerType, generationA, readPointerA, pointerA, stateA, generationB, readPointerB, pointerB, stateB, file );
		 }

		 public override void UnreasonableKeyCount( long pageId, int keyCount, File file )
		 {
			  _clean.setFalse();
			  @delegate.UnreasonableKeyCount( pageId, keyCount, file );
		 }

		 public override void ChildNodeFoundAmongParentNodes( KeyRange<KEY> superRange, int level, long pageId, File file )
		 {
			  _clean.setFalse();
			  @delegate.ChildNodeFoundAmongParentNodes( superRange, level, pageId, file );
		 }

		 public override void Exception( Exception e )
		 {
			  _clean.setFalse();
			  @delegate.Exception( e );
		 }
	}

}