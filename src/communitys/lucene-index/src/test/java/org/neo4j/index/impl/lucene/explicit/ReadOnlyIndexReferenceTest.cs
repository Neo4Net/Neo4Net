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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class ReadOnlyIndexReferenceTest
	{
		private bool InstanceFieldsInitialized = false;

		public ReadOnlyIndexReferenceTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_indexReference = new ReadOnlyIndexReference( _identifier, _searcher );
		}


		 private IndexIdentifier _identifier = mock( typeof( IndexIdentifier ) );
		 private IndexSearcher _searcher = mock( typeof( IndexSearcher ) );
		 private CloseTrackingIndexReader _reader = new CloseTrackingIndexReader();
		 private ReadOnlyIndexReference _indexReference;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  when( _searcher.IndexReader ).thenReturn( _reader );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void obtainingWriterIsUnsupported()
		 internal virtual void ObtainingWriterIsUnsupported()
		 {
			  System.NotSupportedException uoe = assertThrows( typeof( System.NotSupportedException ), () => _indexReference.Writer );
			  assertEquals( uoe.Message, "Read only indexes do not have index writers." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void markAsStaleIsUnsupported()
		 internal virtual void MarkAsStaleIsUnsupported()
		 {
			  System.NotSupportedException uoe = assertThrows( typeof( System.NotSupportedException ), () => _indexReference.setStale() );
			  assertEquals( uoe.Message, "Read only indexes can't be marked as stale." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkAndClearStaleAlwaysFalse()
		 internal virtual void CheckAndClearStaleAlwaysFalse()
		 {
			  assertFalse( _indexReference.checkAndClearStale() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void disposeClosingSearcherAndMarkAsClosed() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void DisposeClosingSearcherAndMarkAsClosed()
		 {
			  _indexReference.dispose();

			  assertTrue( _reader.Closed );
			  assertTrue( _indexReference.Closed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void detachIndexReferenceWhenSomeReferencesExist() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void DetachIndexReferenceWhenSomeReferencesExist()
		 {
			  _indexReference.incRef();
			  _indexReference.detachOrClose();

			  assertTrue( _indexReference.Detached, "Should leave index in detached state." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void closeIndexReferenceWhenNoReferenceExist() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CloseIndexReferenceWhenNoReferenceExist()
		 {
			  _indexReference.detachOrClose();

			  assertFalse( _indexReference.Detached, "Should leave index in closed state." );
			  assertTrue( _reader.Closed );
			  assertTrue( _indexReference.Closed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void doNotCloseInstanceWhenSomeReferenceExist()
		 internal virtual void DoNotCloseInstanceWhenSomeReferenceExist()
		 {
			  _indexReference.incRef();
			  assertFalse( _indexReference.close() );

			  assertFalse( _indexReference.Closed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void closeDetachedIndexReferencedOnlyOnce() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CloseDetachedIndexReferencedOnlyOnce()
		 {
			  _indexReference.incRef();
			  _indexReference.detachOrClose();

			  assertTrue( _indexReference.Detached, "Should leave index in detached state." );

			  assertTrue( _indexReference.close() );
			  assertTrue( _reader.Closed );
			  assertTrue( _indexReference.Closed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void doNotCloseDetachedIndexReferencedMoreThenOnce() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void DoNotCloseDetachedIndexReferencedMoreThenOnce()
		 {
			  _indexReference.incRef();
			  _indexReference.incRef();
			  _indexReference.detachOrClose();

			  assertTrue( _indexReference.Detached, "Should leave index in detached state." );

			  assertFalse( _indexReference.close() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void doNotCloseReferencedIndex()
		 internal virtual void DoNotCloseReferencedIndex()
		 {
			  _indexReference.incRef();
			  assertFalse( _indexReference.close() );
			  assertFalse( _indexReference.Closed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void closeNotReferencedIndex()
		 internal virtual void CloseNotReferencedIndex()
		 {
			  assertTrue( _indexReference.close() );
		 }
	}

}