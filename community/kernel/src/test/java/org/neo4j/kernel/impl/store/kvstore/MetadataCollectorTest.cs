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
namespace Org.Neo4j.Kernel.impl.store.kvstore
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;

	public class MetadataCollectorTest
	{
		 private readonly BigEndianByteArrayBuffer _key = new BigEndianByteArrayBuffer( new sbyte[4] );
		 private readonly BigEndianByteArrayBuffer _value = new BigEndianByteArrayBuffer( new sbyte[4] );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComputePageCatalogue()
		 public virtual void ShouldComputePageCatalogue()
		 {
			  // given
			  StubCollector collector = new StubCollector( 4 );
			  collector.Visit( _key, _value );
			  collector.Visit( _key, _value );

			  // when
			  _key.putInt( 0, 24 );
			  collector.Visit( _key, _value );
			  _key.putInt( 0, 62 );
			  collector.Visit( _key, _value );

			  _key.putInt( 0, 78 );
			  collector.Visit( _key, _value );
			  _key.putInt( 0, 84 );
			  collector.Visit( _key, _value );
			  _key.putInt( 0, 96 );
			  collector.Visit( _key, _value );

			  // then
			  assertArrayEquals( new sbyte[]{ 0, 0, 0, 24, 0, 0, 0, 62, 0, 0, 0, 78, 0, 0, 0, 96 }, collector.PageCatalogue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComputePageCatalogueOverThreePages()
		 public virtual void ShouldComputePageCatalogueOverThreePages()
		 {
			  // given
			  StubCollector collector = new StubCollector( 4 );
			  collector.Visit( _key, _value );
			  collector.Visit( _key, _value );

			  // when
			  _key.putInt( 0, 24 );
			  collector.Visit( _key, _value );
			  _key.putInt( 0, 62 );
			  collector.Visit( _key, _value );

			  _key.putInt( 0, 78 );
			  collector.Visit( _key, _value );
			  _key.putInt( 0, 84 );
			  collector.Visit( _key, _value );
			  _key.putInt( 0, 96 );
			  collector.Visit( _key, _value );
			  _key.putInt( 0, 128 );
			  collector.Visit( _key, _value );

			  _key.putInt( 0, 133 );
			  collector.Visit( _key, _value );

			  // then
			  assertArrayEquals( new sbyte[]{ 0, 0, 0, 24, 0, 0, 0, 62, 0, 0, 0, 78, 0, 0, 0, unchecked( ( sbyte )128 ), 0, 0, 0, unchecked( ( sbyte )133 ), 0, 0, 0, unchecked( ( sbyte )133 ) }, collector.PageCatalogue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComputePageCatalogueWhenHeaderCoversEntireFirstPage()
		 public virtual void ShouldComputePageCatalogueWhenHeaderCoversEntireFirstPage()
		 {
			  // given
			  StubCollector collector = new StubCollector( 4, "a", "b", "c" );
			  _value.putInt( 0, -1 );
			  collector.Visit( _key, _value );
			  collector.Visit( _key, _value );
			  collector.Visit( _key, _value );
			  collector.Visit( _key, _value );

			  // when
			  _key.putInt( 0, 16 );
			  collector.Visit( _key, _value );
			  _key.putInt( 0, 32 );
			  collector.Visit( _key, _value );

			  // then
			  assertArrayEquals( new sbyte[]{ 0, 0, 0, 16, 0, 0, 0, 32 }, collector.PageCatalogue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComputePageCatalogueWhenHeaderExceedsFirstPage()
		 public virtual void ShouldComputePageCatalogueWhenHeaderExceedsFirstPage()
		 {
			  // given
			  StubCollector collector = new StubCollector( 4, "a", "b", "c", "d" );
			  _value.putInt( 0, -1 );
			  collector.Visit( _key, _value );
			  collector.Visit( _key, _value );
			  collector.Visit( _key, _value );
			  collector.Visit( _key, _value );
			  collector.Visit( _key, _value );

			  // when
			  _key.putInt( 0, 16 );
			  collector.Visit( _key, _value );
			  _key.putInt( 0, 32 );
			  collector.Visit( _key, _value );

			  // then
			  assertArrayEquals( new sbyte[]{ 0, 0, 0, 16, 0, 0, 0, 32 }, collector.PageCatalogue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComputeCatalogueWhenSingleDataEntryInPage()
		 public virtual void ShouldComputeCatalogueWhenSingleDataEntryInPage()
		 {
			  // given
			  StubCollector collector = new StubCollector( 4, "a", "b" );
			  _value.putInt( 0, -1 );
			  collector.Visit( _key, _value );
			  collector.Visit( _key, _value );
			  collector.Visit( _key, _value );

			  // when
			  _key.putInt( 0, 16 );
			  collector.Visit( _key, _value );
			  _key.putInt( 0, 32 );
			  collector.Visit( _key, _value );

			  // then
			  assertArrayEquals( new sbyte[]{ 0, 0, 0, 16, 0, 0, 0, 16, 0, 0, 0, 32, 0, 0, 0, 32 }, collector.PageCatalogue() );
		 }
	}

}