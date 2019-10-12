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
	/// <summary>
	/// Means of communicating information about splits, caused by insertion, from lower levels of the tree up to parent
	/// and potentially all the way up to the root. The content of StructurePropagation is acted upon when we are traversing
	/// back up along the "traversal path" (see <seealso cref="org.neo4j.index.internal.gbptree"/>). Level where StructurePropagation
	/// is evaluated is called "current level". Point of reference is mid child in current level and this is the parts
	/// of interest:
	/// <pre>
	///        ┌─────────┬──────────┐
	///        │ leftKey │ rightKey │
	///        └─────────┴──────────┘
	///       ╱          │           ╲
	/// [leftChild]  [midChild]  [rightChild]
	///     ╱            │             ╲
	///    v             v              v
	/// </pre>
	/// <ul>
	///  <li> midChild - the child that was traversed to/through while traversing down the tree.
	///  <li> leftChild - left sibling to midChild.
	///  <li> rightChild - right sibling to midChild.
	///  <li> leftKey - if position of midChild in current level is {@code n} then leftKey is key at position {@code n-1}.
	///  If {@code n==0} then leftKey refer to leftKey in parent in a recursive manor.
	///  <li> rightKey - if position of midChild in current level is {@code n} then rightKey is the key at position {@code n}.
	///  If {@code n==keyCount} then rightKey to rightKey in parent in a recursive manor.
	/// </ul>
	/// 
	/// If position of <seealso cref="midChild"/> {@code n > 0}.
	/// <pre>
	/// Current level-> [...,leftKey,rightKey,...]
	///                    ╱        │         ╲
	///              leftChild  midChild  rightChild
	///                  ╱          │           ╲
	///                 v           v            v
	/// Child nodes-> [...] <───> [...] <────> [...]
	/// </pre>
	/// 
	/// If position of <seealso cref="midChild"/> {@code n == 0}.
	/// <pre>
	/// 
	/// Parent node->          [...,leftKey,...]
	///                   ┌────────┘       └────────┐
	///                   v                         v
	/// Current level-> [...] <───────────> [rightKey,...]
	///                     │               │        ╲
	///                 leftChild       midChild    rightChild
	///                     │               │          ╲
	///                     v               v           v
	/// Child nodes->     [...] <───────> [...] <───> [...]
	/// </pre>
	/// 
	/// * If position of <seealso cref="midChild"/> {@code n == keyCount}.
	/// <pre>
	/// 
	/// Parent node->                  [...,rightKey,...]
	///                           ┌────────┘       └───────┐
	///                           v                        v
	/// Current level->    [...,leftKey] <─────────────> [...]
	///                       /        │                 |
	///                 leftChild  midChild          rightChild
	///                     /          │                 |
	///                    v           v                 v
	/// Child nodes->   [...] <────> [...] <─────────> [...]
	/// </pre> </summary>
	/// @param <KEY> type of key. </param>
	internal class StructurePropagation<KEY>
	{
		 /* <CONTENT> */
		 // Below are the "content" of structure propagation
		 /// <summary>
		 /// See <seealso cref="keyReplaceStrategy"/>.
		 /// </summary>
		 internal readonly KEY LeftKey;

		 /// <summary>
		 /// See <seealso cref="keyReplaceStrategy"/>.
		 /// </summary>
		 internal readonly KEY RightKey;

		 /// <summary>
		 /// See <seealso cref="keyReplaceStrategy"/>.
		 /// </summary>
		 internal readonly KEY BubbleKey;

		 /// <summary>
		 /// New version of left sibling to mid child.
		 /// </summary>
		 internal long LeftChild;

		 /// <summary>
		 /// New version of the child that was traversed to/through while traversing down the tree.
		 /// </summary>
		 internal long MidChild;

		 /// <summary>
		 /// New right sibling to <seealso cref="midChild"/>, depending on <seealso cref="hasRightKeyInsert"/> this can be simple replace of an insert.
		 /// </summary>
		 internal long RightChild;
		 /* </CONTENT> */

		 /* <ACTIONS> */
		 // Below are the actions, deciding what the content of structure propagation should be used for.
		 /// <summary>
		 /// Left child pointer needs to be replaced by <seealso cref="leftChild"/>.
		 /// </summary>
		 internal bool HasLeftChildUpdate;

		 /// <summary>
		 /// Right child pointer needs to be replaced by <seealso cref="rightChild"/> OR, if <seealso cref="hasRightKeyInsert"/> is true
		 /// <seealso cref="rightChild"/> should be inserted as a completely new additional child, moving old right child to the right.
		 /// </summary>
		 internal bool HasRightChildUpdate;

		 /// <summary>
		 /// Mid child pointer needs to be replaced by <seealso cref="midChild"/>.
		 /// </summary>
		 internal bool HasMidChildUpdate;

		 /// <summary>
		 /// <seealso cref="rightKey"/> should be inserted at right keys position (not replacing old right key).
		 /// </summary>
		 internal bool HasRightKeyInsert;
		 /* </ACTIONS> */

		 /// <summary>
		 /// Depending on keyReplaceStrategy either <seealso cref="KeyReplaceStrategy.REPLACE replace"/> left / right key with
		 /// <seealso cref="leftKey"/> / <seealso cref="rightKey"/> or replace left / right key by <seealso cref="bubbleKey"/> (with strategy
		 /// <seealso cref="KeyReplaceStrategy.BUBBLE bubble"/> rightmost from subtree). In the case of bubble, <seealso cref="leftKey"/> / <seealso cref="rightKey"/>
		 /// is used to find "common ancestor" of leaves involved in merge. See <seealso cref="org.neo4j.index.internal.gbptree"/>.
		 /// </summary>
		 internal KeyReplaceStrategy KeyReplaceStrategy;
		 internal bool HasLeftKeyReplace;
		 internal bool HasRightKeyReplace;

		 internal StructurePropagation( KEY leftKey, KEY rightKey, KEY bubbleKey )
		 {
			  this.LeftKey = leftKey;
			  this.RightKey = rightKey;
			  this.BubbleKey = bubbleKey;
		 }

		 /// <summary>
		 /// Clear booleans indicating change has occurred.
		 /// </summary>
		 internal virtual void Clear()
		 {
			  HasLeftChildUpdate = false;
			  HasRightChildUpdate = false;
			  HasMidChildUpdate = false;
			  HasRightKeyInsert = false;
			  HasLeftKeyReplace = false;
			  HasRightKeyReplace = false;
		 }

		 internal interface StructureUpdate
		 {
			  void Update( StructurePropagation structurePropagation, long childId );
		 }

		 internal static readonly StructureUpdate UpdateLeftChild = ( sp, childId ) =>
		 {
		  sp.hasLeftChildUpdate = true;
		  sp.leftChild = childId;
		 };

		 internal static readonly StructureUpdate UpdateMidChild = ( sp, childId ) =>
		 {
		  sp.hasMidChildUpdate = true;
		  sp.midChild = childId;
		 };

		 internal static readonly StructureUpdate UpdateRightChild = ( sp, childId ) =>
		 {
		  sp.hasRightChildUpdate = true;
		  sp.rightChild = childId;
		 };

		 internal enum KeyReplaceStrategy
		 {
			  Replace,
			  Bubble
		 }
	}

}