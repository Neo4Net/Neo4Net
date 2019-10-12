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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using Test = org.junit.jupiter.api.Test;

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using RelationshipGroupStore = Neo4Net.Kernel.impl.store.RelationshipGroupStore;
	using RelationshipStore = Neo4Net.Kernel.impl.store.RelationshipStore;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class RecordCursorsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nodeCursorShouldClosePageCursor()
		 internal virtual void NodeCursorShouldClosePageCursor()
		 {
			  NodeStore store = mock( typeof( NodeStore ) );
			  PageCursor pageCursor = mock( typeof( PageCursor ) );
			  when( store.OpenPageCursorForReading( anyLong() ) ).thenReturn(pageCursor);

			  using ( RecordNodeCursor cursor = new RecordNodeCursor( store ) )
			  {
					cursor.Single( 0 );
			  }
			  verify( pageCursor ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void relationshipScanCursorShouldClosePageCursor()
		 internal virtual void RelationshipScanCursorShouldClosePageCursor()
		 {
			  RelationshipStore store = mock( typeof( RelationshipStore ) );
			  PageCursor pageCursor = mock( typeof( PageCursor ) );
			  when( store.OpenPageCursorForReading( anyLong() ) ).thenReturn(pageCursor);

			  using ( RecordRelationshipScanCursor cursor = new RecordRelationshipScanCursor( store ) )
			  {
					cursor.Single( 0 );
			  }
			  verify( pageCursor ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void relationshipTraversalCursorShouldClosePageCursor()
		 internal virtual void RelationshipTraversalCursorShouldClosePageCursor()
		 {
			  RelationshipStore store = mock( typeof( RelationshipStore ) );
			  PageCursor pageCursor = mock( typeof( PageCursor ) );
			  when( store.OpenPageCursorForReading( anyLong() ) ).thenReturn(pageCursor);
			  RelationshipGroupStore groupStore = mock( typeof( RelationshipGroupStore ) );
			  PageCursor groupPageCursor = mock( typeof( PageCursor ) );
			  when( store.OpenPageCursorForReading( anyLong() ) ).thenReturn(pageCursor);

			  using ( RecordRelationshipTraversalCursor cursor = new RecordRelationshipTraversalCursor( store, groupStore ) )
			  {
					cursor.Init( 0, 0 );
			  }
			  verify( pageCursor ).close();
			  verifyZeroInteractions( groupPageCursor, groupStore );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void relationshipGroupCursorShouldClosePageCursor()
		 internal virtual void RelationshipGroupCursorShouldClosePageCursor()
		 {
			  RelationshipStore relationshipStore = mock( typeof( RelationshipStore ) );
			  PageCursor relationshipPageCursor = mock( typeof( PageCursor ) );
			  when( relationshipStore.OpenPageCursorForReading( anyLong() ) ).thenReturn(relationshipPageCursor);
			  RelationshipGroupStore store = mock( typeof( RelationshipGroupStore ) );
			  PageCursor pageCursor = mock( typeof( PageCursor ) );
			  when( store.OpenPageCursorForReading( anyLong() ) ).thenReturn(pageCursor);

			  using ( RecordRelationshipGroupCursor cursor = new RecordRelationshipGroupCursor( relationshipStore, store ) )
			  {
					cursor.Init( 0, 0 );
			  }
			  verify( pageCursor ).close();
			  verifyZeroInteractions( relationshipStore, relationshipPageCursor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void propertyCursorShouldClosePageCursor()
		 internal virtual void PropertyCursorShouldClosePageCursor()
		 {
			  PropertyStore store = mock( typeof( PropertyStore ) );
			  PageCursor pageCursor = mock( typeof( PageCursor ) );
			  when( store.OpenPageCursorForReading( anyLong() ) ).thenReturn(pageCursor);

			  using ( RecordPropertyCursor cursor = new RecordPropertyCursor( store ) )
			  {
					cursor.Init( 0 );
			  }
			  verify( pageCursor ).close();
		 }
	}

}