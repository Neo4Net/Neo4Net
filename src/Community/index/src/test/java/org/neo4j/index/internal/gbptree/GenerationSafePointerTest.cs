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
namespace Neo4Net.Index.@internal.gbptree
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using ByteArrayPageCursor = Neo4Net.Io.pagecache.ByteArrayPageCursor;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using Inject = Neo4Net.Test.extension.Inject;
	using RandomExtension = Neo4Net.Test.extension.RandomExtension;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(RandomExtension.class) class GenerationSafePointerTest
	internal class GenerationSafePointerTest
	{
		 private static readonly int _pageSize = GenerationSafePointer.Size * 2;
		 private readonly PageCursor _cursor = ByteArrayPageCursor.wrap( new sbyte[_pageSize] );
		 private readonly GSP _read = new GSP();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.RandomRule random;
		 private RandomRule _random;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWriteAndReadGsp()
		 internal virtual void ShouldWriteAndReadGsp()
		 {
			  // GIVEN
			  int offset = 3;
			  GSP expected = Gsp( 10, 110 );

			  // WHEN
			  Write( _cursor, offset, expected );

			  // THEN
			  bool matches = _read( _cursor, offset, _read );
			  assertTrue( matches );
			  assertEquals( expected, _read );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReadGspWithZeroValues()
		 internal virtual void ShouldReadGspWithZeroValues()
		 {
			  // GIVEN
			  int offset = 3;
			  GSP expected = Gsp( 0, 0 );

			  // THEN
			  bool matches = _read( _cursor, offset, _read );
			  assertTrue( matches );
			  assertEquals( expected, _read );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDetectInvalidChecksumOnReadDueToChangedGeneration()
		 internal virtual void ShouldDetectInvalidChecksumOnReadDueToChangedGeneration()
		 {
			  // GIVEN
			  int offset = 0;
			  GSP initial = Gsp( 123, 456 );
			  Write( _cursor, offset, initial );

			  // WHEN
			  _cursor.putInt( offset, ( int )( initial.Generation + 5 ) );

			  // THEN
			  bool matches = _read( _cursor, offset, _read );
			  assertFalse( matches );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDetectInvalidChecksumOnReadDueToChangedChecksum()
		 internal virtual void ShouldDetectInvalidChecksumOnReadDueToChangedChecksum()
		 {
			  // GIVEN
			  int offset = 0;
			  GSP initial = Gsp( 123, 456 );
			  Write( _cursor, offset, initial );

			  // WHEN
			  _cursor.putShort( offset + GenerationSafePointer.Size - GenerationSafePointer.CHECKSUM_SIZE, ( short )( ChecksumOf( initial ) - 2 ) );

			  // THEN
			  bool matches = _read( _cursor, offset, _read );
			  assertFalse( matches );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWriteAndReadGspCloseToGenerationMax()
		 internal virtual void ShouldWriteAndReadGspCloseToGenerationMax()
		 {
			  // GIVEN
			  long generation = GenerationSafePointer.MAX_GENERATION;
			  GSP expected = Gsp( generation, 12345 );
			  Write( _cursor, 0, expected );

			  // WHEN
			  GSP read = new GSP();
			  bool matches = read( _cursor, 0, read );

			  // THEN
			  assertTrue( matches );
			  assertEquals( expected, read );
			  assertEquals( generation, read.Generation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWriteAndReadGspCloseToPointerMax()
		 internal virtual void ShouldWriteAndReadGspCloseToPointerMax()
		 {
			  // GIVEN
			  long pointer = GenerationSafePointer.MAX_POINTER;
			  GSP expected = Gsp( 12345, pointer );
			  Write( _cursor, 0, expected );

			  // WHEN
			  GSP read = new GSP();
			  bool matches = read( _cursor, 0, read );

			  // THEN
			  assertTrue( matches );
			  assertEquals( expected, read );
			  assertEquals( pointer, read.Pointer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWriteAndReadGspCloseToGenerationAndPointerMax()
		 internal virtual void ShouldWriteAndReadGspCloseToGenerationAndPointerMax()
		 {
			  // GIVEN
			  long generation = GenerationSafePointer.MAX_GENERATION;
			  long pointer = GenerationSafePointer.MAX_POINTER;
			  GSP expected = Gsp( generation, pointer );
			  Write( _cursor, 0, expected );

			  // WHEN
			  GSP read = new GSP();
			  bool matches = read( _cursor, 0, read );

			  // THEN
			  assertTrue( matches );
			  assertEquals( expected, read );
			  assertEquals( generation, read.Generation );
			  assertEquals( pointer, read.Pointer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowIfPointerToLarge()
		 internal virtual void ShouldThrowIfPointerToLarge()
		 {
			  long generation = GenerationSafePointer.MIN_GENERATION;
			  long pointer = GenerationSafePointer.MAX_POINTER + 1;
			  GSP broken = Gsp( generation, pointer );

			  assertThrows( typeof( System.ArgumentException ), () => write(_cursor, 0, broken) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowIfPointerToSmall()
		 internal virtual void ShouldThrowIfPointerToSmall()
		 {
			  long generation = GenerationSafePointer.MIN_GENERATION;
			  long pointer = GenerationSafePointer.MinPointer - 1;
			  GSP broken = Gsp( generation, pointer );

			  assertThrows( typeof( System.ArgumentException ), () => write(_cursor, 0, broken) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowIfGenerationToLarge()
		 internal virtual void ShouldThrowIfGenerationToLarge()
		 {
			  long generation = GenerationSafePointer.MAX_GENERATION + 1;
			  long pointer = GenerationSafePointer.MinPointer;
			  GSP broken = Gsp( generation, pointer );

			  assertThrows( typeof( System.ArgumentException ), () => write(_cursor, 0, broken) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowIfGenerationToSmall()
		 internal virtual void ShouldThrowIfGenerationToSmall()
		 {
			  long generation = GenerationSafePointer.MIN_GENERATION - 1;
			  long pointer = GenerationSafePointer.MinPointer;
			  GSP broken = Gsp( generation, pointer );

			  assertThrows( typeof( System.ArgumentException ), () => write(_cursor, 0, broken) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHaveLowAccidentalChecksumCollision()
		 internal virtual void ShouldHaveLowAccidentalChecksumCollision()
		 {
			  // GIVEN
			  int count = 100_000;

			  // WHEN
			  GSP gsp = new GSP();
			  int collisions = 0;
			  short reference = 0;
			  for ( int i = 0; i < count; i++ )
			  {
					gsp.Generation = _random.nextLong( GenerationSafePointer.MAX_GENERATION );
					gsp.Pointer = _random.nextLong( GenerationSafePointer.MAX_POINTER );
					short checksum = ChecksumOf( gsp );
					if ( i == 0 )
					{
						 reference = checksum;
					}
					else
					{
						 bool unique = checksum != reference;
						 collisions += unique ? 0 : 1;
					}
			  }

			  // THEN
			  assertTrue( ( double ) collisions / count < 0.0001 );
		 }

		 private GSP Gsp( long generation, long pointer )
		 {
			  GSP gsp = new GSP();
			  gsp.Generation = generation;
			  gsp.Pointer = pointer;
			  return gsp;
		 }

		 private bool Read( PageCursor cursor, int offset, GSP into )
		 {
			  cursor.Offset = offset;
			  into.Generation = GenerationSafePointer.ReadGeneration( cursor );
			  into.Pointer = GenerationSafePointer.ReadPointer( cursor );
			  return GenerationSafePointer.VerifyChecksum( cursor, into.Generation, into.Pointer );
		 }

		 private void Write( PageCursor cursor, int offset, GSP gsp )
		 {
			  cursor.Offset = offset;
			  GenerationSafePointer.Write( cursor, gsp.Generation, gsp.Pointer );
		 }

		 private static short ChecksumOf( GSP gsp )
		 {
			  return GenerationSafePointer.ChecksumOf( gsp.Generation, gsp.Pointer );
		 }

		 /// <summary>
		 /// Data for a GSP, i.e. generation and pointer. Checksum is generated from those two fields and
		 /// so isn't a field in this struct - ahem class. The reason this class exists is that we, when reading,
		 /// want to read two fields and a checksum and match the two fields with the checksum. This class
		 /// is designed to be mutable and should be reused in as many places as possible.
		 /// </summary>
		 private class GSP
		 {
			  internal long Generation; // unsigned int
			  internal long Pointer;

			  public override int GetHashCode()
			  {
					const int prime = 31;
					int result = 1;
					result = prime * result + ( int )( Generation ^ ( ( long )( ( ulong )Generation >> 32 ) ) );
					result = prime * result + ( int )( Pointer ^ ( ( long )( ( ulong )Pointer >> 32 ) ) );
					return result;
			  }
			  public override bool Equals( object obj )
			  {
					if ( this == obj )
					{
						 return true;
					}
					if ( obj == null )
					{
						 return false;
					}
					if ( this.GetType() != obj.GetType() )
					{
						 return false;
					}
					GSP other = ( GSP ) obj;
					if ( Generation != other.Generation )
					{
						 return false;
					}
					return Pointer == other.Pointer;
			  }

			  public override string ToString()
			  {
					return "[generation:" + Generation + ",p:" + Pointer + "]";
			  }
		 }
	}

}