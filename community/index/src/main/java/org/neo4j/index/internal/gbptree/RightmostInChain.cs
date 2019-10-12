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
namespace Org.Neo4j.Index.@internal.gbptree
{

	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.TreeNode.NO_NODE_FLAG;

	/// <summary>
	/// Used to verify a chain of siblings starting with leftmost node.
	/// Call <seealso cref="assertNext(PageCursor, long, long, long, long, long, GBPTreeConsistencyCheckVisitor)"/> with cursor pointing at sibling expected
	/// to be right sibling to previous call to verify that they are indeed linked together correctly.
	/// <para>
	/// When assertNext has been called on node that is expected to be last in chain, use <seealso cref="assertLast(GBPTreeConsistencyCheckVisitor)"/> to verify.
	/// </para>
	/// </summary>
	internal class RightmostInChain
	{
		 private readonly File _file;
		 private long _currentRightmostNode = TreeNode.NO_NODE_FLAG;
		 private long _currentRightmostRightSiblingPointer = TreeNode.NO_NODE_FLAG;
		 private long _currentRightmostRightSiblingPointerGeneration;
		 private long _currentRightmostNodeGeneration;

		 internal RightmostInChain( File file )
		 {
			  this._file = file;
		 }

		 internal virtual void AssertNext( PageCursor cursor, long newRightmostNodeGeneration, long newRightmostLeftSiblingPointer, long newRightmostLeftSiblingPointerGeneration, long newRightmostRightSiblingPointer, long newRightmostRightSiblingPointerGeneration, GBPTreeConsistencyCheckVisitor visitor )
		 {
			  long newRightmostNode = cursor.CurrentPageId;

			  // Assert we have reached expected node and that we agree about being siblings
			  AssertSiblingsAgreeOnBeingSiblings( _currentRightmostNode, _currentRightmostNodeGeneration, _currentRightmostRightSiblingPointer, _currentRightmostRightSiblingPointerGeneration, newRightmostNode, newRightmostNodeGeneration, newRightmostLeftSiblingPointer, newRightmostLeftSiblingPointerGeneration, visitor );
			  // Assert that both sibling pointers have reasonable generations
			  AssertSiblingPointerGeneration( _currentRightmostNode, _currentRightmostNodeGeneration, _currentRightmostRightSiblingPointer, _currentRightmostRightSiblingPointerGeneration, newRightmostNode, newRightmostNodeGeneration, newRightmostLeftSiblingPointer, newRightmostLeftSiblingPointerGeneration, visitor );

			  // Update currentRightmostNode = newRightmostNode;
			  _currentRightmostNode = newRightmostNode;
			  _currentRightmostNodeGeneration = newRightmostNodeGeneration;
			  _currentRightmostRightSiblingPointer = newRightmostRightSiblingPointer;
			  _currentRightmostRightSiblingPointerGeneration = newRightmostRightSiblingPointerGeneration;
		 }

		 private void AssertSiblingPointerGeneration( long currentRightmostNode, long currentRightmostNodeGeneration, long currentRightmostRightSiblingPointer, long currentRightmostRightSiblingPointerGeneration, long newRightmostNode, long newRightmostNodeGeneration, long newRightmostLeftSiblingPointer, long newRightmostLeftSiblingPointerGeneration, GBPTreeConsistencyCheckVisitor visitor )
		 {
			  if ( currentRightmostNodeGeneration > newRightmostLeftSiblingPointerGeneration && currentRightmostNode != NO_NODE_FLAG )
			  {
					// Generation of left sibling is larger than that of the pointer from right sibling
					// Left siblings view:  {_(9)}-(_)->{_}
					// Right siblings view: {_}<-(5)-{_(_)}
					visitor.pointerHasLowerGenerationThanNode( GBPTreePointerType.leftSibling(), newRightmostNode, newRightmostLeftSiblingPointerGeneration, newRightmostLeftSiblingPointer, currentRightmostNodeGeneration, _file );
			  }
			  if ( currentRightmostRightSiblingPointerGeneration < newRightmostNodeGeneration && currentRightmostRightSiblingPointer != NO_NODE_FLAG )
			  {
					// Generation of right sibling is larger than that of the pointer from left sibling
					// Left siblings view:  {_(_)}-(5)->{_}
					// Right siblings view: {_}<-(_)-{_(9)}
					visitor.pointerHasLowerGenerationThanNode( GBPTreePointerType.rightSibling(), currentRightmostNode, currentRightmostRightSiblingPointerGeneration, currentRightmostRightSiblingPointer, newRightmostNodeGeneration, _file );
			  }
		 }

		 private void AssertSiblingsAgreeOnBeingSiblings( long currentRightmostNode, long currentRightmostNodeGeneration, long currentRightmostRightSiblingPointer, long currentRightmostRightSiblingPointerGeneration, long newRightmostNode, long newRightmostNodeGeneration, long newRightmostLeftSiblingPointer, long newRightmostLeftSiblingPointerGeneration, GBPTreeConsistencyCheckVisitor visitor )
		 {
			  bool siblingsPointToEachOther = true;
			  if ( newRightmostLeftSiblingPointer != currentRightmostNode )
			  {
					// Right sibling does not point to left sibling
					// Left siblings view:  {2(_)}-(_)->{_}
					// Right siblings view: {1}<-(_)-{_(_)}
					siblingsPointToEachOther = false;
			  }
			  if ( newRightmostNode != currentRightmostRightSiblingPointer && ( currentRightmostRightSiblingPointer != NO_NODE_FLAG || currentRightmostNode != NO_NODE_FLAG ) )
			  {
					// Left sibling does not point to right sibling
					// Left siblings view:  {_(_)}-(_)->{1}
					// Right siblings view: {_}<-(_)-{2(_)}
					siblingsPointToEachOther = false;
			  }
			  if ( !siblingsPointToEachOther )
			  {
					visitor.siblingsDontPointToEachOther( currentRightmostNode, currentRightmostNodeGeneration, currentRightmostRightSiblingPointerGeneration, currentRightmostRightSiblingPointer, newRightmostLeftSiblingPointer, newRightmostLeftSiblingPointerGeneration, newRightmostNode, newRightmostNodeGeneration, _file );
			  }
		 }

		 internal virtual void AssertLast( GBPTreeConsistencyCheckVisitor visitor )
		 {
			  if ( _currentRightmostRightSiblingPointer != NO_NODE_FLAG )
			  {
					visitor.rightmostNodeHasRightSibling( _currentRightmostRightSiblingPointer, _currentRightmostNode, _file );
			  }
		 }
	}

}