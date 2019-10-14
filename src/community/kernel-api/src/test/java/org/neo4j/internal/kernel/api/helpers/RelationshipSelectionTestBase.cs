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
namespace Neo4Net.Internal.Kernel.Api.helpers
{
	using Neo4Net.Graphdb;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	internal class RelationshipSelectionTestBase
	{

		 internal readonly int TypeA = 100;
		 internal readonly int TypeB = 101;
		 internal readonly int TypeC = 102;

		 internal virtual void AssertOutgoing( ResourceIterator<R> iterator, int targetNode, int type )
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( "has next", iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  R r = iterator.next();
			  assertEquals( "expected type", type, r.Type );
			  assertEquals( "expected target", targetNode, r.TargetNode );
		 }

		 internal virtual void AssertIncoming( ResourceIterator<R> iterator, int sourceNode, int type )
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( "has next", iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  R r = iterator.next();
			  assertEquals( "expected type", type, r.Type );
			  assertEquals( "expected source", sourceNode, r.SourceNode );
		 }

		 internal virtual void AssertLoop( ResourceIterator<R> iterator, int type )
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( "has next", iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  R r = iterator.next();
			  assertEquals( "expected type", type, r.Type );
			  assertEquals( "expected loop", r.SourceNode, r.TargetNode );
		 }

		 internal virtual void AssertEmpty( ResourceIterator<R> iterator )
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( "no more", iterator.hasNext() );
		 }

		 internal virtual void AssertOutgoing( RelationshipSelectionCursor cursor, int targetNode, int type )
		 {
			  assertTrue( "has next", cursor.Next() );
			  assertEquals( "expected type", type, cursor.Type() );
			  assertEquals( "expected target", targetNode, cursor.TargetNodeReference() );
		 }

		 internal virtual void AssertIncoming( RelationshipSelectionCursor cursor, int sourceNode, int type )
		 {
			  assertTrue( "has next", cursor.Next() );
			  assertEquals( "expected type", type, cursor.Type() );
			  assertEquals( "expected source", sourceNode, cursor.SourceNodeReference() );
		 }

		 internal virtual void AssertLoop( RelationshipSelectionCursor cursor, int type )
		 {
			  assertTrue( "has next", cursor.Next() );
			  assertEquals( "expected type", type, cursor.Type() );
			  assertEquals( "expected loop", cursor.SourceNodeReference(), cursor.TargetNodeReference() );
		 }

		 internal virtual void AssertEmpty( RelationshipSelectionCursor cursor )
		 {
			  assertFalse( "no more", cursor.Next() );
		 }

		 internal virtual int[] Types( params int[] types )
		 {
			  return types;
		 }

		 internal class R
		 {
			  internal readonly long Relationship;
			  internal readonly long SourceNode;
			  internal readonly int Type;
			  internal readonly long TargetNode;

			  internal R( long relationship, long sourceNode, int type, long targetNode )
			  {
					this.Relationship = relationship;
					this.SourceNode = sourceNode;
					this.Type = type;
					this.TargetNode = targetNode;
			  }
		 }
	}

}