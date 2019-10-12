using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.index.labelscan
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using Test = org.junit.Test;


	using Neo4Net.Cursors;
	using Neo4Net.Index.@internal.gbptree;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.LabelScanReader_Fields.NO_ID;

	public class LabelScanValueIteratorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseExhaustedCursors() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseExhaustedCursors()
		 {
			  // GIVEN
			  RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException> cursor = mock( typeof( RawCursor ) );
			  when( cursor.Next() ).thenReturn(false);
			  ICollection<RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException>> toRemoveFrom = new HashSet<RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException>>();
			  LabelScanValueIterator iterator = new LabelScanValueIterator( cursor, toRemoveFrom, NO_ID );
			  verify( cursor, never() ).close();

			  // WHEN
			  Exhaust( iterator );
			  verify( cursor, times( 1 ) ).close();

			  // retrying to get more items from the first one should not close it again
			  iterator.HasNext();
			  verify( cursor, times( 1 ) ).close();

			  // and set should be empty
			  assertTrue( toRemoveFrom.Count == 0 );
		 }

		 private void Exhaust( LongIterator iterator )
		 {
			  while ( iterator.hasNext() )
			  {
					iterator.next();
			  }
		 }
	}

}