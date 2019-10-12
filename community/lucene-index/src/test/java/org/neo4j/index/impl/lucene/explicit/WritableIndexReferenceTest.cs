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
namespace Org.Neo4j.Index.impl.lucene.@explicit
{
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class WritableIndexReferenceTest
	{
		private bool InstanceFieldsInitialized = false;

		public WritableIndexReferenceTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_indexReference = new WritableIndexReference( _identifier, _searcher, _indexWriter );
		}

		 private IndexIdentifier _identifier = mock( typeof( IndexIdentifier ) );
		 private IndexSearcher _searcher = mock( typeof( IndexSearcher ) );
		 private IndexWriter _indexWriter = mock( typeof( IndexWriter ) );
		 private CloseTrackingIndexReader _reader = new CloseTrackingIndexReader();
		 private WritableIndexReference _indexReference;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  when( _searcher.IndexReader ).thenReturn( _reader );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void useProvidedWriterAsIndexWriter()
		 internal virtual void UseProvidedWriterAsIndexWriter()
		 {
			  assertSame( _indexWriter, _indexReference.Writer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stalingWritableIndex()
		 internal virtual void StalingWritableIndex()
		 {
			  assertFalse( _indexReference.checkAndClearStale(), "Index is not stale by default." );
			  _indexReference.setStale();
			  assertTrue( _indexReference.checkAndClearStale(), "We should be able to reset stale index state." );
			  assertFalse( _indexReference.checkAndClearStale(), "Index is not stale anymore." );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void disposeWritableIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void DisposeWritableIndex()
		 {
			  _indexReference.dispose();
			  assertTrue( _reader.Closed, "Reader should be closed." );
			  assertTrue( _indexReference.WriterClosed, "Reader should be closed." );
		 }

	}

}