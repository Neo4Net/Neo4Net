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
namespace Neo4Net.Index.Internal.gbptree
{
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.PageCursorUtil.get6BLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.PageCursorUtil.getUnsignedInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.PageCursorUtil.put6BLong;

	/// <summary>
	/// Provides static methods for getting and manipulating GSP (generation-safe pointer) data.
	/// All interaction is made using a <seealso cref="PageCursor"/>. These methods are about a single GSP,
	/// whereas the normal use case of a GSP is in pairs (<seealso cref="GenerationSafePointerPair GSPP"/>).
	/// <para>
	/// A GSP consists of [generation,pointer,checksum]. Checksum is calculated from generation and pointer.
	/// </para>
	/// <para>
	/// Due to how java has one a single return type and objects produce/is garbage
	/// the design of the methods below for reading GSP requires some documentation
	/// to be used properly:
	/// </para>
	/// <para>
	/// Caller is responsible for initially setting cursor offset at the start of
	/// the GSP to read, then follows a couple of calls, each advancing the cursor
	/// offset themselves:
	/// <ol>
	/// <li><seealso cref="readGeneration(PageCursor)"/></li>
	/// <li><seealso cref="readPointer(PageCursor)"/></li>
	/// <li><seealso cref="verifyChecksum(PageCursor, long, long)"/></li>
	/// </ol>
	/// </para>
	/// </summary>
	internal class GenerationSafePointer
	{
		 private const int EMPTY_POINTER = 0;
		 internal const int EMPTY_GENERATION = 0;

		 internal const long MIN_GENERATION = 1L;
		 // unsigned int
		 internal const long MAX_GENERATION = 0xFFFFFFFFL;
		 internal const long GENERATION_MASK = 0xFFFFFFFFL;
		 internal const long MinPointer = IdSpace.MIN_TREE_NODE_ID;
		 internal const long MAX_POINTER = 0xFFFF_FFFFFFFFL;
		 internal const int UNSIGNED_SHORT_MASK = 0xFFFF;

		 internal const int GENERATION_SIZE = 4;
		 internal const int POINTER_SIZE = 6;
		 internal const int CHECKSUM_SIZE = 2;
		 internal static readonly int Size = GENERATION_SIZE + POINTER_SIZE + CHECKSUM_SIZE;

		 private GenerationSafePointer()
		 {
		 }

		 /// <summary>
		 /// Writes GSP at the given {@code offset}, the two fields (generation, pointer) + a checksum will be written.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to write into. </param>
		 /// <param name="generation"> generation to write. </param>
		 /// <param name="pointer"> pointer to write. </param>
		 internal static void Write( PageCursor cursor, long generation, long pointer )
		 {
			  AssertGenerationOnWrite( generation );
			  AssertPointerOnWrite( pointer );
			  WriteGSP( cursor, generation, pointer );
		 }

		 private static void WriteGSP( PageCursor cursor, long generation, long pointer )
		 {
			  cursor.PutInt( ( int ) generation );
			  put6BLong( cursor, pointer );
			  cursor.PutShort( ChecksumOf( generation, pointer ) );
		 }

		 internal static void Clean( PageCursor cursor )
		 {
			  WriteGSP( cursor, EMPTY_GENERATION, EMPTY_POINTER );
		 }

		 internal static void AssertGenerationOnWrite( long generation )
		 {
			  if ( generation < MIN_GENERATION || generation > MAX_GENERATION )
			  {
					throw new System.ArgumentException( "Can not write pointer with generation " + generation + " because outside boundary for valid generation." );
			  }
		 }

		 private static void AssertPointerOnWrite( long pointer )
		 {
			  if ( ( pointer > MAX_POINTER || pointer < MinPointer ) && TreeNode.IsNode( pointer ) )
			  {
					throw new System.ArgumentException( "Can not write pointer " + pointer + " because outside boundary for valid pointer" );
			  }
		 }

		 internal static long ReadGeneration( PageCursor cursor )
		 {
			  return getUnsignedInt( cursor );
		 }

		 internal static long ReadPointer( PageCursor cursor )
		 {
			  return get6BLong( cursor );
		 }

		 internal static short ReadChecksum( PageCursor cursor )
		 {
			  return cursor.Short;
		 }

		 internal static bool VerifyChecksum( PageCursor cursor, long generation, long pointer )
		 {
			  short checksum = cursor.Short;
			  return checksum == ChecksumOf( generation, pointer );
		 }

		 /// <summary>
		 /// Calculates a 2-byte checksum from GSP data.
		 /// </summary>
		 /// <param name="generation"> generation of the pointer. </param>
		 /// <param name="pointer"> pointer itself.
		 /// </param>
		 /// <returns> a {@code short} which is the checksum of the generation-pointer. </returns>
		 internal static short ChecksumOf( long generation, long pointer )
		 {
			  short result = 0;
			  result ^= ( short )( ( ( short ) generation ) & UNSIGNED_SHORT_MASK );
			  result ^= ( short )( ( ( short )( ( long )( ( ulong )generation >> ( sizeof( short ) * 8 ) ) ) ) & UNSIGNED_SHORT_MASK );
			  result ^= ( short )( ( ( short ) pointer ) & UNSIGNED_SHORT_MASK );
			  result ^= ( short )( ( ( short )( ( long )( ( ulong )pointer >> ( sizeof( short ) * 8 ) ) ) ) & UNSIGNED_SHORT_MASK );
			  result ^= ( short )( ( ( short )( ( long )( ( ulong )pointer >> ( sizeof( int ) * 8 ) ) ) ) & UNSIGNED_SHORT_MASK );
			  return result;
		 }

		 public static bool IsEmpty( long generation, long pointer )
		 {
			  return generation == EMPTY_GENERATION && pointer == EMPTY_POINTER;
		 }
	}

}