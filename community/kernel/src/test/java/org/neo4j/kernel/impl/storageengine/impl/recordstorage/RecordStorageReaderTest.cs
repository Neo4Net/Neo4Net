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
namespace Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage
{
	using Test = org.junit.Test;

	using LabelScanReader = Org.Neo4j.Storageengine.Api.schema.LabelScanReader;
	using MockedNeoStores = Org.Neo4j.Test.MockedNeoStores;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class RecordStorageReaderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseOpenedLabelScanReader()
		 public virtual void ShouldCloseOpenedLabelScanReader()
		 {
			  // given
			  System.Func<LabelScanReader> scanStore = mock( typeof( System.Func ) );
			  LabelScanReader scanReader = mock( typeof( LabelScanReader ) );

			  when( scanStore() ).thenReturn(scanReader);
			  RecordStorageReader statement = new RecordStorageReader( null, null, MockedNeoStores.basicMockedNeoStores(), null, null, mock(typeof(System.Func)), scanStore, mock(typeof(RecordStorageCommandCreationContext)) );
			  statement.Acquire();

			  // when
			  LabelScanReader actualReader = statement.LabelScanReader;

			  // then
			  assertEquals( scanReader, actualReader );

			  // when
			  statement.Close();

			  // then
			  verify( scanStore ).get();
			  verifyNoMoreInteractions( scanStore );

			  verify( scanReader ).close();
			  verifyNoMoreInteractions( scanReader );
		 }
	}

}