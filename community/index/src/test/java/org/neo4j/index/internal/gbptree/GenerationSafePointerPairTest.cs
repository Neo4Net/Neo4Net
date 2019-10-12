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
namespace Org.Neo4j.Index.@internal.gbptree
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameter = org.junit.runners.Parameterized.Parameter;
	using Parameters = org.junit.runners.Parameterized.Parameters;


	using ByteArrayPageCursor = Org.Neo4j.Io.pagecache.ByteArrayPageCursor;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GenerationSafePointerPair.FLAG_READ;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GenerationSafePointerPair.FLAG_WRITE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GenerationSafePointerPair.GENERATION_COMPARISON_MASK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GenerationSafePointerPair.READ_OR_WRITE_MASK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GenerationSafePointerPair.SHIFT_STATE_A;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GenerationSafePointerPair.SHIFT_STATE_B;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GenerationSafePointerPair.failureDescription;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GenerationSafePointerPair.isRead;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GenerationSafePointerPair.pointerStateFromResult;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GenerationSafePointerPair.pointerStateName;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class GenerationSafePointerPairTest
	public class GenerationSafePointerPairTest
	{
		 private const int PAGE_SIZE = 128;
		 private const int OLD_STABLE_GENERATION = 1;
		 private const int STABLE_GENERATION = 2;
		 private const int OLD_CRASH_GENERATION = 3;
		 private const int CRASH_GENERATION = 4;
		 private const int UNSTABLE_GENERATION = 5;
		 private const long EMPTY_POINTER = 0L;

		 private const long POINTER_A = 5;
		 private const long POINTER_B = 6;
		 private const long WRITTEN_POINTER = 10;

		 private const int EXPECTED_GENERATION_DISREGARD = -2;
		 private const int EXPECTED_GENERATION_B_BIG = -1;
		 private const int EXPECTED_GENERATION_EQUAL = 0;
		 private const int EXPECTED_GENERATION_A_BIG = 1;

		 private const bool SLOT_A = true;
		 private const bool SLOT_B = false;
		 private const int GSPP_OFFSET = 5;
		 private const int SLOT_A_OFFSET = GSPP_OFFSET;
		 private static readonly int _slotBOffset = SLOT_A_OFFSET + GenerationSafePointer.Size;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters(name = "{0},{1},read {2},write {3}") public static java.util.Collection<Object[]> data()
		 public static ICollection<object[]> Data()
		 {
			  ICollection<object[]> data = new List<object[]>();

			  //             ┌─────────────────┬─────────────────┬─────────────────-------──┬───────------────────────────┐
			  //             │ State A         │ State B         │ Read outcome             │ Write outcome               │
			  //             └─────────────────┴─────────────────┴──────────────────-------─┴────────────────────------───┘
			  data.Add( Array( State.Empty, State.Empty, Fail.GenerationDisregard, Success.A ) );
			  data.Add( Array( State.Empty, State.Unstable, Success.B, Success.B ) );
			  data.Add( Array( State.Empty, State.Stable, Success.B, Success.A ) );
			  data.Add( Array( State.Empty, State.Crash, Fail.GenerationDisregard, Fail.GenerationDisregard ) );
			  data.Add( Array( State.Empty, State.Broken, Fail.GenerationDisregard, Fail.GenerationDisregard ) );
			  data.Add( Array( State.Unstable, State.Empty, Success.A, Success.A ) );
			  data.Add( Array( State.Unstable, State.Unstable, Fail.GenerationEqual, Fail.GenerationEqual ) );
			  data.Add( Array( State.Unstable, State.Stable, Success.A, Success.A ) );
			  data.Add( Array( State.Unstable, State.Crash, Fail.GenerationABig, Fail.GenerationABig ) );
			  data.Add( Array( State.Unstable, State.Broken, Fail.GenerationDisregard, Fail.GenerationDisregard ) );
			  data.Add( Array( State.Stable, State.Empty, Success.A, Success.B ) );
			  data.Add( Array( State.Stable, State.Unstable, Success.B, Success.B ) );
			  data.Add( Array( State.Stable, State.OldStable, Success.A, Success.B ) );
			  data.Add( Array( State.OldStable, State.Stable, Success.B, Success.A ) );
			  data.Add( Array( State.Stable, State.Stable, Fail.GenerationEqual, Fail.GenerationEqual ) );
			  data.Add( Array( State.Stable, State.Crash, Success.A, Success.B ) );
			  data.Add( Array( State.Stable, State.Broken, Success.A, Success.B ) );
			  data.Add( Array( State.Crash, State.Empty, Fail.GenerationDisregard, Fail.GenerationDisregard ) );
			  data.Add( Array( State.Crash, State.Unstable, Fail.GenerationBBig, Fail.GenerationBBig ) );
			  data.Add( Array( State.Crash, State.Stable, Success.B, Success.A ) );
			  data.Add( Array( State.Crash, State.OldCrash, Fail.GenerationABig, Fail.GenerationABig ) );
			  data.Add( Array( State.OldCrash, State.Crash, Fail.GenerationBBig, Fail.GenerationBBig ) );
			  data.Add( Array( State.Crash, State.Crash, Fail.GenerationEqual, Fail.GenerationEqual ) );
			  data.Add( Array( State.Crash, State.Broken, Fail.GenerationDisregard, Fail.GenerationDisregard ) );
			  data.Add( Array( State.Broken, State.Empty, Fail.GenerationDisregard, Fail.GenerationDisregard ) );
			  data.Add( Array( State.Broken, State.Unstable, Fail.GenerationDisregard, Fail.GenerationDisregard ) );
			  data.Add( Array( State.Broken, State.Stable, Success.B, Success.A ) );
			  data.Add( Array( State.Broken, State.Crash, Fail.GenerationDisregard, Fail.GenerationDisregard ) );
			  data.Add( Array( State.Broken, State.Broken, Fail.GenerationDisregard, Fail.GenerationDisregard ) );

			  return data;
		 }

		 [Parameter(0)]
		 public State StateA;
		 [Parameter(1)]
		 public State StateB;
		 [Parameter(2)]
		 public Slot ExpectedReadOutcome;
		 [Parameter(3)]
		 public Slot ExpectedWriteOutcome;

		 private readonly PageCursor _cursor = ByteArrayPageCursor.wrap( new sbyte[PAGE_SIZE] );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRead()
		 public virtual void ShouldRead()
		 {
			  // GIVEN
			  _cursor.Offset = SLOT_A_OFFSET;
			  long preStatePointerA = StateA.materialize( _cursor, POINTER_A );
			  _cursor.Offset = _slotBOffset;
			  long preStatePointerB = StateB.materialize( _cursor, POINTER_B );

			  // WHEN
			  _cursor.Offset = GSPP_OFFSET;
			  GenerationKeeper generationKeeper = new GenerationKeeper();
			  long result = GenerationSafePointerPair.Read( _cursor, STABLE_GENERATION, UNSTABLE_GENERATION, generationKeeper );

			  // THEN
			  ExpectedReadOutcome.verifyRead( _cursor, result, StateA, StateB, preStatePointerA, preStatePointerB, generationKeeper.Generation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWrite()
		 public virtual void ShouldWrite()
		 {
			  // GIVEN
			  _cursor.Offset = SLOT_A_OFFSET;
			  long preStatePointerA = StateA.materialize( _cursor, POINTER_A );
			  _cursor.Offset = _slotBOffset;
			  long preStatePointerB = StateB.materialize( _cursor, POINTER_B );

			  // WHEN
			  _cursor.Offset = GSPP_OFFSET;
			  long written = GenerationSafePointerPair.Write( _cursor, WRITTEN_POINTER, STABLE_GENERATION, UNSTABLE_GENERATION );

			  // THEN
			  ExpectedWriteOutcome.verifyWrite( _cursor, written, StateA, StateB, preStatePointerA, preStatePointerB );
		 }

		 private static void AssertFailure( long result, long readOrWrite, int generationComparison, sbyte pointerStateA, sbyte pointerStateB )
		 {
			  assertFalse( GenerationSafePointerPair.IsSuccess( result ) );

			  // Raw failure bits
			  assertEquals( readOrWrite, result & READ_OR_WRITE_MASK );
			  if ( generationComparison != EXPECTED_GENERATION_DISREGARD )
			  {
					assertEquals( GenerationComparisonBits( generationComparison ), result & GENERATION_COMPARISON_MASK );
			  }
			  assertEquals( pointerStateA, pointerStateFromResult( result, SHIFT_STATE_A ) );
			  assertEquals( pointerStateB, pointerStateFromResult( result, SHIFT_STATE_B ) );

			  // Failure description
			  string failureDescription = failureDescription( result );
			  assertThat( failureDescription, containsString( isRead( result ) ? "READ" : "WRITE" ) );
			  if ( generationComparison != EXPECTED_GENERATION_DISREGARD )
			  {
					assertThat( failureDescription, containsString( GenerationComparisonName( generationComparison ) ) );
			  }
			  assertThat( failureDescription, containsString( pointerStateName( pointerStateA ) ) );
			  assertThat( failureDescription, containsString( pointerStateName( pointerStateB ) ) );
		 }

		 private static string GenerationComparisonName( int generationComparison )
		 {
			  switch ( generationComparison )
			  {
			  case EXPECTED_GENERATION_B_BIG:
					return GenerationSafePointerPair.GENERATION_COMPARISON_NAME_B_BIG;
			  case EXPECTED_GENERATION_EQUAL:
					return GenerationSafePointerPair.GENERATION_COMPARISON_NAME_EQUAL;
			  case EXPECTED_GENERATION_A_BIG:
					return GenerationSafePointerPair.GENERATION_COMPARISON_NAME_A_BIG;
			  default:
					throw new System.NotSupportedException( generationComparison.ToString() );
			  }
		 }

		 private static long GenerationComparisonBits( int generationComparison )
		 {
			  switch ( generationComparison )
			  {
			  case EXPECTED_GENERATION_B_BIG:
					return GenerationSafePointerPair.FLAG_GENERATION_B_BIG;
			  case EXPECTED_GENERATION_EQUAL:
					return GenerationSafePointerPair.FLAG_GENERATION_EQUAL;
			  case EXPECTED_GENERATION_A_BIG:
					return GenerationSafePointerPair.FLAG_GENERATION_A_BIG;
			  default:
					throw new System.NotSupportedException( generationComparison.ToString() );
			  }
		 }

		 private static long ReadSlotA( PageCursor cursor )
		 {
			  cursor.Offset = SLOT_A_OFFSET;
			  return ReadSlot( cursor );
		 }

		 private static long ReadSlotB( PageCursor cursor )
		 {
			  cursor.Offset = _slotBOffset;
			  return ReadSlot( cursor );
		 }

		 private static long ReadSlot( PageCursor cursor )
		 {
			  long generation = GenerationSafePointer.ReadGeneration( cursor );
			  long pointer = GenerationSafePointer.ReadPointer( cursor );
			  short checksum = GenerationSafePointer.ReadChecksum( cursor );
			  assertEquals( GenerationSafePointer.ChecksumOf( generation, pointer ), checksum );
			  return pointer;
		 }

		 private static object[] Array( params object[] array )
		 {
			  return array;
		 }

		 internal abstract class State
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           EMPTY(GenerationSafePointerPair.EMPTY) { long materialize(org.neo4j.io.pagecache.PageCursor cursor, long pointer) { return EMPTY_POINTER; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           BROKEN(GenerationSafePointerPair.BROKEN) { long materialize(org.neo4j.io.pagecache.PageCursor cursor, long pointer) { int offset = cursor.getOffset(); GenerationSafePointer.write(cursor, 10, 20); cursor.setOffset(offset + GenerationSafePointer.SIZE - GenerationSafePointer.CHECKSUM_SIZE); short checksum = GenerationSafePointer.readChecksum(cursor); cursor.setOffset(offset + GenerationSafePointer.SIZE - GenerationSafePointer.CHECKSUM_SIZE); cursor.putShort((short) ~checksum); return pointer; } void verify(org.neo4j.io.pagecache.PageCursor cursor, long expectedPointer, boolean slotA) { cursor.setOffset(slotA ? SLOT_A_OFFSET : SLOT_B_OFFSET); long generation = GenerationSafePointer.readGeneration(cursor); long pointer = GenerationSafePointer.readPointer(cursor); short checksum = GenerationSafePointer.readChecksum(cursor); assertNotEquals(GenerationSafePointer.checksumOf(generation, pointer), checksum); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           OLD_CRASH(GenerationSafePointerPair.CRASH) { long materialize(org.neo4j.io.pagecache.PageCursor cursor, long pointer) { GenerationSafePointer.write(cursor, OLD_CRASH_GENERATION, pointer); return pointer; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           CRASH(GenerationSafePointerPair.CRASH) { long materialize(org.neo4j.io.pagecache.PageCursor cursor, long pointer) { GenerationSafePointer.write(cursor, CRASH_GENERATION, pointer); return pointer; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           OLD_STABLE(GenerationSafePointerPair.STABLE) { long materialize(org.neo4j.io.pagecache.PageCursor cursor, long pointer) { GenerationSafePointer.write(cursor, OLD_STABLE_GENERATION, pointer); return pointer; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           STABLE(GenerationSafePointerPair.STABLE) { long materialize(org.neo4j.io.pagecache.PageCursor cursor, long pointer) { GenerationSafePointer.write(cursor, STABLE_GENERATION, pointer); return pointer; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           UNSTABLE(GenerationSafePointerPair.UNSTABLE) { long materialize(org.neo4j.io.pagecache.PageCursor cursor, long pointer) { GenerationSafePointer.write(cursor, UNSTABLE_GENERATION, pointer); return pointer; } };

			  private static readonly IList<State> valueList = new List<State>();

			  static State()
			  {
				  valueList.Add( EMPTY );
				  valueList.Add( BROKEN );
				  valueList.Add( OLD_CRASH );
				  valueList.Add( CRASH );
				  valueList.Add( OLD_STABLE );
				  valueList.Add( STABLE );
				  valueList.Add( UNSTABLE );
			  }

			  public enum InnerEnum
			  {
				  EMPTY,
				  BROKEN,
				  OLD_CRASH,
				  CRASH,
				  OLD_STABLE,
				  STABLE,
				  UNSTABLE
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private State( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  /// <summary>
			  /// Actual <seealso cref="GenerationSafePointerPair"/> pointer state value.
			  /// </summary>
			  internal readonly sbyte byteValue;

			  internal State( string name, InnerEnum innerEnum, sbyte byteValue )
			  {
					this._byteValue = byteValue;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  /// <summary>
			  /// Writes this state onto cursor.
			  /// </summary>
			  /// <param name="cursor"> <seealso cref="PageCursor"/> to write pre-state to. </param>
			  /// <param name="pointer"> pointer to write in GSP. Generation is decided by the pre-state. </param>
			  /// <returns> written pointer. </returns>
			  internal abstract long materialize( Org.Neo4j.Io.pagecache.PageCursor cursor, long pointer );

			  /// <summary>
			  /// Verifies result after WHEN section in test.
			  /// </summary>
			  /// <param name="cursor"> <seealso cref="PageCursor"/> to read actual pointer from. </param>
			  /// <param name="expectedPointer"> expected pointer, as received from <seealso cref="materialize(PageCursor, long)"/>. </param>
			  /// <param name="slotA"> whether or not this is for slot A, otherwise B. </param>
			  internal void Verify( Org.Neo4j.Io.pagecache.PageCursor cursor, long expectedPointer, bool slotA )
			  {
					assertEquals( expectedPointer, slotA ? readSlotA( cursor ) : readSlotB( cursor ) );
			  }

			  public static long ReadGeneration( Org.Neo4j.Io.pagecache.PageCursor cursor, bool slotA )
			  {
					cursor.Offset = slotA ? SLOT_A_OFFSET : SLOT_B_OFFSET;
					return GenerationSafePointer.ReadGeneration( cursor );
			  }

			 public static IList<State> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static State valueOf( string name )
			 {
				 foreach ( State enumInstance in State.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 internal interface Slot
		 {
			  /// <param name="cursor"> <seealso cref="PageCursor"/> to read actual result from. </param>
			  /// <param name="result"> read-result from <seealso cref="GenerationSafePointerPair.read(PageCursor, long, long, GBPTreeGenerationTarget)"/>. </param>
			  /// <param name="stateA"> state of pointer A when read. </param>
			  /// <param name="stateB"> state of pointer B when read. </param>
			  /// <param name="preStatePointerA"> pointer A as it looked like in pre-state. </param>
			  /// <param name="preStatePointerB"> pointer B as it looked like in pre-state. </param>
			  /// <param name="generation"> read generation. </param>
			  void VerifyRead( PageCursor cursor, long result, State stateA, State stateB, long preStatePointerA, long preStatePointerB, long generation );

			  /// <param name="cursor"> <seealso cref="PageCursor"/> to read actual result from. </param>
			  /// <param name="result"> write-result from <seealso cref="GenerationSafePointerPair.write(PageCursor, long, long, long)"/>. </param>
			  /// <param name="stateA"> state of pointer A when written. </param>
			  /// <param name="stateB"> state of pointer B when written. </param>
			  /// <param name="preStatePointerA"> pointer A as it looked like in pre-state. </param>
			  /// <param name="preStatePointerB"> pointer B as it looked like in pre-state. </param>
			  void VerifyWrite( PageCursor cursor, long result, State stateA, State stateB, long preStatePointerA, long preStatePointerB );
		 }

		 internal sealed class Success : Slot
		 {
			  public static readonly Success A = new Success( "A", InnerEnum.A, POINTER_A, SLOT_A );
			  public static readonly Success B = new Success( "B", InnerEnum.B, POINTER_B, SLOT_B );

			  private static readonly IList<Success> valueList = new List<Success>();

			  static Success()
			  {
				  valueList.Add( A );
				  valueList.Add( B );
			  }

			  public enum InnerEnum
			  {
				  A,
				  B
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal Private readonly;
			  internal Private readonly;

			  internal Success( string name, InnerEnum innerEnum, long expectedPointer, bool expectedSlot )
			  {
					this._expectedPointer = expectedPointer;
					this._expectedSlot = expectedSlot;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  public void VerifyRead( Org.Neo4j.Io.pagecache.PageCursor cursor, long result, State stateA, State stateB, long preStatePointerA, long preStatePointerB, long generation )
			  {
					AssertSuccess( result );
					long pointer = GenerationSafePointerPair.Pointer( result );
					assertEquals( _expectedPointer, pointer );
					assertEquals( _expectedSlot == SLOT_A, GenerationSafePointerPair.ResultIsFromSlotA( result ) );
					stateA.verify( cursor, preStatePointerA, SLOT_A );
					stateB.verify( cursor, preStatePointerB, SLOT_B );
					assertEquals( State.readGeneration( cursor, _expectedSlot ), generation );
			  }

			  public void VerifyWrite( Org.Neo4j.Io.pagecache.PageCursor cursor, long result, State stateA, State stateB, long preStatePointerA, long preStatePointerB )
			  {
					AssertSuccess( result );
					bool actuallyWrittenSlot = ( result & GenerationSafePointerPair.SLOT_MASK ) == GenerationSafePointerPair.FLAG_SLOT_A ? SLOT_A : SLOT_B;
					assertEquals( _expectedSlot, actuallyWrittenSlot );

					if ( _expectedSlot == SLOT_A )
					{
						 // Expect slot A to have been written, B staying the same
						 assertEquals( WRITTEN_POINTER, ReadSlotA( cursor ) );
						 assertEquals( preStatePointerB, ReadSlotB( cursor ) );
					}
					else
					{
						 // Expect slot B to have been written, A staying the same
						 assertEquals( preStatePointerA, ReadSlotA( cursor ) );
						 assertEquals( WRITTEN_POINTER, ReadSlotB( cursor ) );
					}
			  }

			  internal static void AssertSuccess( long result )
			  {
					assertTrue( GenerationSafePointerPair.IsSuccess( result ) );
			  }

			 public static IList<Success> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static Success valueOf( string name )
			 {
				 foreach ( Success enumInstance in Success.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 internal sealed class Fail : Slot
		 {
			  public static readonly Fail GenerationDisregard = new Fail( "GenerationDisregard", InnerEnum.GenerationDisregard, EXPECTED_GENERATION_DISREGARD );
			  public static readonly Fail GenerationBBig = new Fail( "GenerationBBig", InnerEnum.GenerationBBig, EXPECTED_GENERATION_B_BIG );
			  public static readonly Fail GenerationEqual = new Fail( "GenerationEqual", InnerEnum.GenerationEqual, EXPECTED_GENERATION_EQUAL );
			  public static readonly Fail GenerationABig = new Fail( "GenerationABig", InnerEnum.GenerationABig, EXPECTED_GENERATION_A_BIG );

			  private static readonly IList<Fail> valueList = new List<Fail>();

			  static Fail()
			  {
				  valueList.Add( GenerationDisregard );
				  valueList.Add( GenerationBBig );
				  valueList.Add( GenerationEqual );
				  valueList.Add( GenerationABig );
			  }

			  public enum InnerEnum
			  {
				  GenerationDisregard,
				  GenerationBBig,
				  GenerationEqual,
				  GenerationABig
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal Private readonly;

			  internal Fail( string name, InnerEnum innerEnum, int generationComparison )
			  {
					this._generationComparison = generationComparison;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  public void VerifyRead( Org.Neo4j.Io.pagecache.PageCursor cursor, long result, State stateA, State stateB, long preStatePointerA, long preStatePointerB, long generation )
			  {
					AssertFailure( result, FLAG_READ, _generationComparison, stateA.byteValue, stateB.byteValue );
					stateA.verify( cursor, preStatePointerA, SLOT_A );
					stateB.verify( cursor, preStatePointerB, SLOT_B );
					assertEquals( GenerationSafePointer.EMPTY_GENERATION, generation );
			  }

			  public void VerifyWrite( Org.Neo4j.Io.pagecache.PageCursor cursor, long result, State stateA, State stateB, long preStatePointerA, long preStatePointerB )
			  {
					AssertFailure( result, FLAG_WRITE, _generationComparison, stateA.byteValue, stateB.byteValue );
					stateA.verify( cursor, preStatePointerA, SLOT_A );
					stateB.verify( cursor, preStatePointerB, SLOT_B );
			  }

			 public static IList<Fail> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static Fail valueOf( string name )
			 {
				 foreach ( Fail enumInstance in Fail.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }
	}

}