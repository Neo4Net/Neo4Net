﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{
	using MutableLong = org.apache.commons.lang3.mutable.MutableLong;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using SimpleLongLayout = Org.Neo4j.Index.@internal.gbptree.SimpleLongLayout;
	using ByteArrayPageCursor = Org.Neo4j.Io.pagecache.ByteArrayPageCursor;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using RandomExtension = Org.Neo4j.Test.extension.RandomExtension;
	using RandomRule = Org.Neo4j.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(RandomExtension.class) class BlockEntryTest
	internal class BlockEntryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject RandomRule rnd;
		 internal RandomRule Rnd;

		 private static readonly PageCursor _pageCursor = ByteArrayPageCursor.wrap( 1000 );
		 private static SimpleLongLayout _layout;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setup()
		 internal virtual void Setup()
		 {
			  _layout = SimpleLongLayout.longLayout().withFixedSize(Rnd.nextBoolean()).withKeyPadding(Rnd.Next(10)).build();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReadWriteSingleEntry()
		 internal virtual void ShouldReadWriteSingleEntry()
		 {
			  // given
			  MutableLong writeKey = _layout.key( Rnd.nextLong() );
			  MutableLong writeValue = _layout.value( Rnd.nextLong() );
			  int offset = _pageCursor.Offset;
			  BlockEntry.Write( _pageCursor, _layout, writeKey, writeValue );

			  // when
			  MutableLong readKey = _layout.newKey();
			  MutableLong readValue = _layout.newValue();
			  _pageCursor.Offset = offset;
			  BlockEntry.Read( _pageCursor, _layout, readKey, readValue );

			  // then
			  assertEquals( 0, _layout.Compare( writeKey, readKey ) );
			  assertEquals( 0, _layout.Compare( writeValue, readValue ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReadWriteMultipleEntries()
		 internal virtual void ShouldReadWriteMultipleEntries()
		 {
			  IList<BlockEntry<MutableLong, MutableLong>> expectedEntries = new List<BlockEntry<MutableLong, MutableLong>>();
			  int nbrOfEntries = 10;
			  int offset = _pageCursor.Offset;
			  for ( int i = 0; i < nbrOfEntries; i++ )
			  {
					BlockEntry<MutableLong, MutableLong> entry = new BlockEntry<MutableLong, MutableLong>( _layout.key( Rnd.nextLong() ), _layout.value(Rnd.nextLong()) );
					BlockEntry.Write( _pageCursor, _layout, entry );
					expectedEntries.Add( entry );
			  }

			  _pageCursor.Offset = offset;
			  foreach ( BlockEntry<MutableLong, MutableLong> expectedEntry in expectedEntries )
			  {
					BlockEntry<MutableLong, MutableLong> actualEntry = BlockEntry.Read( _pageCursor, _layout );
					AssertBlockEquals( expectedEntry, actualEntry );
			  }
		 }

		 private static void AssertBlockEquals( BlockEntry<MutableLong, MutableLong> expected, BlockEntry<MutableLong, MutableLong> actual )
		 {
			  assertEquals( 0, _layout.Compare( expected.Key(), actual.Key() ) );
			  assertEquals( 0, _layout.Compare( expected.Value(), actual.Value() ) );
		 }
	}

}