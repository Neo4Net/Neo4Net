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
namespace Neo4Net.Internal.Kernel.Api
{
	/// <summary>
	/// Cursor for traversing the relationship groups of a node.
	/// </summary>
	public interface RelationshipGroupCursor : SuspendableCursor<RelationshipGroupCursor_Position>
	{

		 /// <summary>
		 /// Find the first relationship group with a label greater than or equal to the provided label.
		 /// <para>
		 /// Note that the default implementation of this method (and most likely any sane use of this method - regardless of
		 /// implementation) assumes that relationship groups are ordered by label.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="relationshipLabel"> the relationship label to search for. </param>
		 /// <returns> {@code true} if a matching relationship group was found, {@code false} if all relationship groups
		 /// within
		 /// reach
		 /// of
		 /// this
		 /// cursor were exhausted without finding a matching relationship group. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default boolean seek(int relationshipLabel)
	//	 {
	//		  while (next())
	//		  {
	//				if (relationshipLabel < type())
	//				{
	//					 return true;
	//				}
	//		  }
	//		  return false;
	//	 }

		 int Type();

		 int OutgoingCount();

		 int IncomingCount();

		 int LoopCount();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default int totalCount()
	//	 {
	//		  return outgoingCount() + incomingCount() + loopCount();
	//	 }

		 void Outgoing( RelationshipTraversalCursor cursor );

		 void Incoming( RelationshipTraversalCursor cursor );

		 void Loops( RelationshipTraversalCursor cursor );

		 long OutgoingReference();

		 long IncomingReference();

		 long LoopsReference();
	}

	 public abstract class RelationshipGroupCursor_Position : CursorPosition<RelationshipGroupCursor_Position>
	 {
	 }

}