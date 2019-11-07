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
//	import static Neo4Net.index.Internal.gbptree.GenerationSafePointer.EMPTY_GENERATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.GenerationSafePointer.MIN_GENERATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.GenerationSafePointer.checksumOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.GenerationSafePointer.readChecksum;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.GenerationSafePointer.readGeneration;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.GenerationSafePointer.readPointer;

	/// <summary>
	/// Two <seealso cref="GenerationSafePointer"/> forming the basis for a B+tree becoming generate-aware.
	/// <para>
	/// Generally a GSP fall into one out of these categories:
	/// <ul>
	/// <li>STABLE: generation made durable and safe by a checkpoint</li>
	/// <li>UNSTABLE: generation which is currently under evolution and isn't safe until next checkpoint</li>
	/// <li>EMPTY: have never been written</li>
	/// </ul>
	/// There are variations of pointers written in UNSTABLE generation:
	/// <ul>
	/// <li>BROKEN: written during a concurrent page cache flush and wasn't flushed after that point before crash</li>
	/// <li>CRASH: pointers written as UNSTABLE before a crash happened (non-clean shutdown) are seen as
	/// CRASH during recovery</li>
	/// </ul>
	/// </para>
	/// <para>
	/// Combinations of above mentioned states of the two pointers dictates which, if any, to read from or write to.
	/// From the perspective of callers there's only "read" and "write", the two pointers are hidden.
	/// </para>
	/// <para>
	/// All methods are static and all interaction is made with primitives.
	/// </para>
	/// <para>
	/// Flags in results from read/write method calls. Pointer is 6B so msb 2B can be used for flags,
	/// although the most common case (successful read) has its flag zeros so a successful read doesn't need
	/// any masking to extract pointer.
	/// <pre>
	///     WRITE
	/// [_1__,____][___ ,    ][ ... 6B pointer data ... ]
	///  ▲ ▲▲ ▲▲▲▲  ▲▲▲
	///  │ ││ ││││  │││
	///  │ ││ ││││  └└└────────────────────────────────────── POINTER STATE B (on failure)
	///  │ ││ │└└└─────────────────────────────────────────── POINTER STATE A (on failure)
	///  │ │└─└────────────────────────────────────────────── GENERATION COMPARISON (on failure):<seealso cref="FLAG_GENERATION_B_BIG"/>,
	///  │ │                                                  <seealso cref="FLAG_GENERATION_EQUAL"/>, <seealso cref="FLAG_GENERATION_A_BIG"/>
	///  │ └───────────────────────────────────────────────── 0:<seealso cref="FLAG_SLOT_A"/>/1:<seealso cref="FLAG_SLOT_B"/> (on success)
	///  └─────────────────────────────────────────────────── 0:<seealso cref="FLAG_SUCCESS"/>/1:<seealso cref="FLAG_FAIL"/>
	/// </pre>
	/// <pre>
	///     READ failure
	/// [10__,____][__  ,    ][ ... 6B pointer data ... ]
	///    ▲▲ ▲▲▲▲  ▲▲
	///    ││ ││││  ││
	///    ││ │││└──└└─────────────────────────────────────── POINTER STATE B
	///    ││ └└└──────────────────────────────────────────── POINTER STATE A
	///    └└──────────────────────────────────────────────── GENERATION COMPARISON:
	///                                                       <seealso cref="FLAG_GENERATION_B_BIG"/>, <seealso cref="FLAG_GENERATION_EQUAL"/>,
	///                                                       <seealso cref="FLAG_GENERATION_A_BIG"/>
	/// </pre>
	/// <pre>
	///     READ success
	/// [00_ ,    ][    ,    ][ ... 6B pointer data ... ]
	///    ▲
	///    └───────────────────────────────────────────────── 0:<seealso cref="FLAG_SLOT_A"/>/1:<seealso cref="FLAG_SLOT_B"/>
	/// </pre>
	/// </para>
	/// </summary>
	internal class GenerationSafePointerPair
	{
		 internal static readonly int Size = GenerationSafePointer.Size * 2;
		 internal const string GENERATION_COMPARISON_NAME_B_BIG = "A < B";
		 internal const string GENERATION_COMPARISON_NAME_A_BIG = "A > B";
		 internal const string GENERATION_COMPARISON_NAME_EQUAL = "A == B";

		 // Pointer states
		 internal const sbyte STABLE = 0; // any previous generation made safe by a checkpoint
		 internal const sbyte UNSTABLE = 1; // current generation, generation under evolution until next checkpoint
		 internal const sbyte CRASH = 2; // pointer written as unstable and didn't make it to checkpoint before crashing
		 internal const sbyte BROKEN = 3; // mismatching checksum
		 internal const sbyte EMPTY = 4; // generation and pointer all zeros

		 // Flags and failure information
		 internal const long FLAG_SUCCESS = 0x00000000_00000000L;
		 internal const long FLAG_FAIL = unchecked( ( long )0x80000000_00000000L );
		 internal const long FLAG_READ = 0x00000000_00000000L;
		 internal const long FLAG_WRITE = 0x40000000_00000000L;
		 internal const long FLAG_GENERATION_EQUAL = 0x00000000_00000000L;
		 internal const long FLAG_GENERATION_A_BIG = 0x08000000_00000000L;
		 internal const long FLAG_GENERATION_B_BIG = 0x10000000_00000000L;
		 internal const long FLAG_SLOT_A = 0x00000000_00000000L;
		 internal const long FLAG_SLOT_B = 0x20000000_00000000L;
		 internal const int SHIFT_STATE_A = 56;
		 internal const int SHIFT_STATE_B = 53;

		 // Aggregations
		 internal const long SUCCESS_WRITE_TO_B = FLAG_SUCCESS | FLAG_WRITE | FLAG_SLOT_B;
		 internal const long SUCCESS_WRITE_TO_A = FLAG_SUCCESS | FLAG_WRITE | FLAG_SLOT_A;

		 // Masks
		 internal const long SUCCESS_MASK = FLAG_SUCCESS | FLAG_FAIL;
		 internal const long READ_OR_WRITE_MASK = FLAG_READ | FLAG_WRITE;
		 internal const long SLOT_MASK = FLAG_SLOT_A | FLAG_SLOT_B;
		 internal const long STATE_MASK = 0x7; // After shift
		 internal const long GENERATION_COMPARISON_MASK = FLAG_GENERATION_EQUAL | FLAG_GENERATION_A_BIG | FLAG_GENERATION_B_BIG;
		 internal const long POINTER_MASK = 0x0000FFFF_FFFFFFFFL;

		 private GenerationSafePointerPair()
		 {
		 }

		 /// <summary>
		 /// Reads a GSPP, returning the read pointer or a failure. Check success/failure using <seealso cref="isSuccess(long)"/>
		 /// and if failure extract more information using <seealso cref="failureDescription(long)"/>.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to read from, placed at the beginning of the GSPP. </param>
		 /// <param name="stableGeneration"> stable index generation. </param>
		 /// <param name="unstableGeneration"> unstable index generation. </param>
		 /// <param name="generationTarget"> target to write the generation of the selected pointer. </param>
		 /// <returns> most recent readable pointer, or failure. Check result using <seealso cref="isSuccess(long)"/>. </returns>
		 public static long Read( PageCursor cursor, long stableGeneration, long unstableGeneration, GBPTreeGenerationTarget generationTarget )
		 {
			  // Try A
			  long generationA = readGeneration( cursor );
			  long pointerA = readPointer( cursor );
			  short readChecksumA = readChecksum( cursor );
			  short checksumA = checksumOf( generationA, pointerA );
			  bool correctChecksumA = readChecksumA == checksumA;

			  // Try B
			  long generationB = readGeneration( cursor );
			  long pointerB = readPointer( cursor );
			  short readChecksumB = readChecksum( cursor );
			  short checksumB = checksumOf( generationB, pointerB );
			  bool correctChecksumB = readChecksumB == checksumB;

			  sbyte pointerStateA = PointerState( stableGeneration, unstableGeneration, generationA, pointerA, correctChecksumA );
			  sbyte pointerStateB = PointerState( stableGeneration, unstableGeneration, generationB, pointerB, correctChecksumB );

			  if ( pointerStateA == UNSTABLE )
			  {
					if ( pointerStateB == STABLE || pointerStateB == EMPTY )
					{
						 return BuildSuccessfulReadResult( FLAG_SLOT_A, generationA, pointerA, generationTarget );
					}
			  }
			  else if ( pointerStateB == UNSTABLE )
			  {
					if ( pointerStateA == STABLE || pointerStateA == EMPTY )
					{
						 return BuildSuccessfulReadResult( FLAG_SLOT_B, generationB, pointerB, generationTarget );
					}
			  }
			  else if ( pointerStateA == STABLE && pointerStateB == STABLE )
			  {
					// compare generation
					if ( generationA > generationB )
					{
						 return BuildSuccessfulReadResult( FLAG_SLOT_A, generationA, pointerA, generationTarget );
					}
					else if ( generationB > generationA )
					{
						 return BuildSuccessfulReadResult( FLAG_SLOT_B, generationB, pointerB, generationTarget );
					}
			  }
			  else if ( pointerStateA == STABLE )
			  {
					return BuildSuccessfulReadResult( FLAG_SLOT_A, generationA, pointerA, generationTarget );
			  }
			  else if ( pointerStateB == STABLE )
			  {
					return BuildSuccessfulReadResult( FLAG_SLOT_B, generationB, pointerB, generationTarget );
			  }

			  generationTarget( EMPTY_GENERATION );
			  return FLAG_FAIL | FLAG_READ | GenerationState( generationA, generationB ) | ( ( long ) pointerStateA ) << SHIFT_STATE_A | ( ( long ) pointerStateB ) << SHIFT_STATE_B;
		 }

		 private static long BuildSuccessfulReadResult( long slot, long generation, long pointer, GBPTreeGenerationTarget generationTarget )
		 {
			  generationTarget( generation );
			  return FLAG_SUCCESS | FLAG_READ | slot | pointer;
		 }

		 /// <summary>
		 /// Writes a GSP at one of the GSPP slots A/B, returning the result.
		 /// Check success/failure using <seealso cref="isSuccess(long)"/> and if failure extract more information using
		 /// <seealso cref="failureDescription(long)"/>.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to write to, placed at the beginning of the GSPP. </param>
		 /// <param name="pointer"> pageId to write. </param>
		 /// <param name="stableGeneration"> stable index generation. </param>
		 /// <param name="unstableGeneration"> unstable index generation, which will be the generation to write in the slot. </param>
		 /// <returns> {@code true} on success, otherwise {@code false} on failure. </returns>
		 public static long Write( PageCursor cursor, long pointer, long stableGeneration, long unstableGeneration )
		 {
			  // Later there will be a selection which "slot" of GSP out of the two to write into.
			  int offset = cursor.Offset;
			  pointer = pointer( pointer );

			  // Try A
			  long generationA = readGeneration( cursor );
			  long pointerA = readPointer( cursor );
			  short readChecksumA = readChecksum( cursor );
			  short checksumA = checksumOf( generationA, pointerA );
			  bool correctChecksumA = readChecksumA == checksumA;

			  // Try B
			  long generationB = readGeneration( cursor );
			  long pointerB = readPointer( cursor );
			  short readChecksumB = readChecksum( cursor );
			  short checksumB = checksumOf( generationB, pointerB );
			  bool correctChecksumB = readChecksumB == checksumB;

			  sbyte pointerStateA = PointerState( stableGeneration, unstableGeneration, generationA, pointerA, correctChecksumA );
			  sbyte pointerStateB = PointerState( stableGeneration, unstableGeneration, generationB, pointerB, correctChecksumB );

			  long writeResult = writeResult( pointerStateA, pointerStateB, generationA, generationB );

			  if ( IsSuccess( writeResult ) )
			  {
					bool WriteToA = ( writeResult & SLOT_MASK ) == FLAG_SLOT_A;
					int writeOffset = WriteToA ? offset : offset + GenerationSafePointer.Size;
					cursor.Offset = writeOffset;
					GenerationSafePointer.Write( cursor, unstableGeneration, pointer );
			  }
			  return writeResult;
		 }

		 private static long WriteResult( sbyte pointerStateA, sbyte pointerStateB, long generationA, long generationB )
		 {
			  if ( pointerStateA == STABLE )
			  {
					if ( pointerStateB == STABLE )
					{
						 if ( generationA > generationB )
						 {
							  // Write to slot B
							  return SUCCESS_WRITE_TO_B;
						 }
						 else if ( generationB > generationA )
						 {
							  // Write to slot A
							  return SUCCESS_WRITE_TO_A;
						 }
					}
					else
					{
						 // Write to slot B
						 return SUCCESS_WRITE_TO_B;
					}
			  }
			  else if ( pointerStateB == STABLE )
			  {
					// write to slot A
					return SUCCESS_WRITE_TO_A;
			  }
			  else if ( pointerStateA == UNSTABLE )
			  {
					if ( pointerStateB == EMPTY )
					{
						 // write to slot A
						 return SUCCESS_WRITE_TO_A;
					}
			  }
			  else if ( pointerStateB == UNSTABLE )
			  {
					if ( pointerStateA == EMPTY )
					{
						 // write to slot B
						 return SUCCESS_WRITE_TO_B;
					}
			  }
			  else if ( pointerStateA == EMPTY && pointerStateB == EMPTY )
			  {
					// write to slot A
					return SUCCESS_WRITE_TO_A;
			  }

			  // Encode error
			  return FLAG_FAIL | FLAG_WRITE | GenerationState( generationA, generationB ) | ( ( long ) pointerStateA ) << SHIFT_STATE_A | ( ( long ) pointerStateB ) << SHIFT_STATE_B;
		 }

		 private static long GenerationState( long generationA, long generationB )
		 {
			  return generationA > generationB ? FLAG_GENERATION_A_BIG : generationB > generationA ? FLAG_GENERATION_B_BIG : FLAG_GENERATION_EQUAL;
		 }

		 /// <summary>
		 /// Pointer state of a GSP (generation, pointer, checksum). Can be any of:
		 /// <ul>
		 /// <li><seealso cref="STABLE"/></li>
		 /// <li><seealso cref="UNSTABLE"/></li>
		 /// <li><seealso cref="CRASH"/></li>
		 /// <li><seealso cref="BROKEN"/></li>
		 /// <li><seealso cref="EMPTY"/></li>
		 /// </ul>
		 /// </summary>
		 /// <param name="stableGeneration"> stable generation. </param>
		 /// <param name="unstableGeneration"> unstable generation. </param>
		 /// <param name="generation"> GSP generation. </param>
		 /// <param name="pointer"> GSP pointer. </param>
		 /// <param name="checksumIsCorrect"> whether or not GSP checksum matches checksum of {@code generation} and {@code pointer}. </param>
		 /// <returns> one of the available pointer states. </returns>
		 internal static sbyte PointerState( long stableGeneration, long unstableGeneration, long generation, long pointer, bool checksumIsCorrect )
		 {
			  if ( GenerationSafePointer.IsEmpty( generation, pointer ) )
			  {
					return EMPTY;
			  }
			  if ( !checksumIsCorrect )
			  {
					return BROKEN;
			  }
			  if ( generation < MIN_GENERATION )
			  {
					return BROKEN;
			  }
			  if ( generation <= stableGeneration )
			  {
					return STABLE;
			  }
			  if ( generation == unstableGeneration )
			  {
					return UNSTABLE;
			  }
			  return CRASH;
		 }

		 /// <summary>
		 /// Checks to see if a result from read/write was successful. If not more failure information can be extracted
		 /// using <seealso cref="failureDescription(long)"/>.
		 /// </summary>
		 /// <param name="result"> result from <seealso cref="read(PageCursor, long, long, GBPTreeGenerationTarget)"/> or <seealso cref="write(PageCursor, long, long, long)"/>. </param>
		 /// <returns> {@code true} if successful read/write, otherwise {@code false}. </returns>
		 internal static bool IsSuccess( long result )
		 {
			  return ( result & SUCCESS_MASK ) == FLAG_SUCCESS;
		 }

		 /// <param name="readResult"> whole read result from <seealso cref="read(PageCursor, long, long, GBPTreeGenerationTarget)"/>, containing both
		 /// pointer as well as header information about the pointer. </param>
		 /// <returns> the pointer-part of {@code readResult}. </returns>
		 internal static long Pointer( long readResult )
		 {
			  return readResult & POINTER_MASK;
		 }

		 /// <summary>
		 /// Calling <seealso cref="read(PageCursor, long, long, GBPTreeGenerationTarget)"/> (potentially also <seealso cref="write(PageCursor, long, long, long)"/>)
		 /// can fail due to seeing an unexpected state of the two GSPs. Failing right there and then isn't an option
		 /// due to how the page cache works and that something read from a <seealso cref="PageCursor"/> must not be interpreted
		 /// until after passing a <seealso cref="PageCursor.shouldRetry()"/> returning {@code false}. This creates a need for
		 /// including failure information in result returned from these methods so that, if failed, can have
		 /// the caller which interprets the result fail in a proper place. That place can make use of this method
		 /// by getting a human-friendly description about the failure.
		 /// </summary>
		 /// <param name="result"> result from <seealso cref="read(PageCursor, long, long, GBPTreeGenerationTarget)"/> or
		 /// <seealso cref="write(PageCursor, long, long, long)"/>. </param>
		 /// <returns> a human-friendly description of the failure. </returns>
		 internal static string FailureDescription( long result )
		 {
			  return "GSPP " + ( IsRead( result ) ? "READ" : "WRITE" ) + " failure" + format( "%n  Pointer state A: %s", PointerStateName( PointerStateFromResult( result, SHIFT_STATE_A ) ) ) + format( "%n  Pointer state B: %s", PointerStateName( PointerStateFromResult( result, SHIFT_STATE_B ) ) ) + format( "%n  Generations: " + GenerationComparisonFromResult( result ) );
		 }

		 /// <summary>
		 /// Asserts that a result is <seealso cref="isSuccess(long) successful"/>, otherwise throws <seealso cref="System.InvalidOperationException"/>.
		 /// </summary>
		 /// <param name="result"> result returned from <seealso cref="read(PageCursor, long, long, GBPTreeGenerationTarget)"/> or
		 /// <seealso cref="write(PageCursor, long, long, long)"/> </param>
		 /// <returns> {@code true} if <seealso cref="isSuccess(long) successful"/>, for interoperability with {@code assert}. </returns>
		 internal static bool AssertSuccess( long result )
		 {
			  if ( !IsSuccess( result ) )
			  {
					throw new TreeInconsistencyException( FailureDescription( result ) );
			  }
			  return true;
		 }

		 private static string GenerationComparisonFromResult( long result )
		 {
			  long bits = result & GENERATION_COMPARISON_MASK;
			  if ( bits == FLAG_GENERATION_EQUAL )
			  {
					return GENERATION_COMPARISON_NAME_EQUAL;
			  }
			  else if ( bits == FLAG_GENERATION_A_BIG )
			  {
					return GENERATION_COMPARISON_NAME_A_BIG;
			  }
			  else if ( bits == FLAG_GENERATION_B_BIG )
			  {
					return GENERATION_COMPARISON_NAME_B_BIG;
			  }
			  else
			  {
					return "Unknown[" + bits + "]";
			  }
		 }

		 /// <summary>
		 /// Name of the provided {@code pointerState} gotten from <seealso cref="pointerState(long, long, long, long, bool)"/>.
		 /// </summary>
		 /// <param name="pointerState"> pointer state to get name for. </param>
		 /// <returns> name of {@code pointerState}. </returns>
		 internal static string PointerStateName( sbyte pointerState )
		 {
			  switch ( pointerState )
			  {
			  case STABLE:
				  return "STABLE";
			  case UNSTABLE:
				  return "UNSTABLE";
			  case CRASH:
				  return "CRASH";
			  case BROKEN:
				  return "BROKEN";
			  case EMPTY:
				  return "EMPTY";
			  default:
				  return "Unknown[" + pointerState + "]";
			  }
		 }

		 internal static sbyte PointerStateFromResult( long result, int shift )
		 {
			  return ( sbyte )( ( ( long )( ( ulong )result >> shift ) ) & STATE_MASK );
		 }

		 internal static bool IsRead( long result )
		 {
			  return ( result & READ_OR_WRITE_MASK ) == FLAG_READ;
		 }

		 internal static bool ResultIsFromSlotA( long result )
		 {
			  return ( result & SLOT_MASK ) == FLAG_SLOT_A;
		 }
	}

}