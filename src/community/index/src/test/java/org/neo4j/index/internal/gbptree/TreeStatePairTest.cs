using System.Collections.Generic;

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
	using Pair = org.apache.commons.lang3.tuple.Pair;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using ParameterizedTest = org.junit.jupiter.@params.ParameterizedTest;
	using MethodSource = org.junit.jupiter.@params.provider.MethodSource;


	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;

	internal class TreeStatePairTest
	{
		 private const long PAGE_A = 1;
		 private const long PAGE_B = 2;

		 private PageAwareByteArrayCursor _cursor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  _cursor = new PageAwareByteArrayCursor( 256 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource(value = "parameters") void shouldCorrectSelectNewestAndOldestState(State stateA, State stateB, Selected expectedNewest, Selected expectedOldest) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCorrectSelectNewestAndOldestState( State stateA, State stateB, Selected expectedNewest, Selected expectedOldest )
		 {
			  // GIVEN
			  _cursor.next( PAGE_A );
			  stateA.write( _cursor );
			  _cursor.next( PAGE_B );
			  stateB.write( _cursor );

			  // WHEN
			  Pair<TreeState, TreeState> states = TreeStatePair.ReadStatePages( _cursor, PAGE_A, PAGE_B );

			  // THEN
			  expectedNewest.verify( states, SelectionUseCase.Newest );
			  expectedOldest.verify( states, SelectionUseCase.Oldest );
		 }

		 private static ICollection<object[]> Parameters()
		 {
			  ICollection<object[]> variants = new List<object[]>();

			  //               ┌──────────────-───-────┬──────────────────────────┬───────────────┬───────────────┐
			  //               │ State A               │ State B                  │ Select newest │ Select oldest │
			  //               └───────────────────────┴──────────────────────────┴───────────────┴───────────────┘
			  Variant( variants, State.Empty, State.Empty, Selected.Fail, Selected.A );
			  Variant( variants, State.Empty, State.Broken, Selected.Fail, Selected.A );
			  Variant( variants, State.Empty, State.Valid, Selected.B, Selected.A );

			  Variant( variants, State.Broken, State.Empty, Selected.Fail, Selected.A );
			  Variant( variants, State.Broken, State.Broken, Selected.Fail, Selected.A );
			  Variant( variants, State.Broken, State.Valid, Selected.B, Selected.A );

			  Variant( variants, State.Valid, State.Empty, Selected.A, Selected.B );
			  Variant( variants, State.Valid, State.Broken, Selected.A, Selected.B );

			  Variant( variants, State.Valid, State.OldValid, Selected.A, Selected.B );
			  Variant( variants, State.Valid, State.OldValidDirty, Selected.A, Selected.B );
			  Variant( variants, State.ValidDirty, State.OldValid, Selected.A, Selected.B );

			  Variant( variants, State.Valid, State.Valid, Selected.Fail, Selected.A );
			  Variant( variants, State.Valid, State.ValidDirty, Selected.A, Selected.B );
			  Variant( variants, State.ValidDirty, State.Valid, Selected.B, Selected.A );

			  Variant( variants, State.OldValid, State.Valid, Selected.B, Selected.A );
			  Variant( variants, State.OldValidDirty, State.Valid, Selected.B, Selected.A );
			  Variant( variants, State.OldValid, State.ValidDirty, Selected.B, Selected.A );

			  Variant( variants, State.CrashValid, State.Valid, Selected.A, Selected.B );
			  Variant( variants, State.CrashValidDirty, State.Valid, Selected.A, Selected.B );
			  Variant( variants, State.CrashValid, State.ValidDirty, Selected.A, Selected.B );

			  Variant( variants, State.Valid, State.CrashValid, Selected.B, Selected.A );
			  Variant( variants, State.ValidDirty, State.CrashValid, Selected.B, Selected.A );
			  Variant( variants, State.Valid, State.CrashValidDirty, Selected.B, Selected.A );

			  Variant( variants, State.WideValid, State.CrashValid, Selected.Fail, Selected.A );
			  Variant( variants, State.WideValidDirty, State.CrashValid, Selected.Fail, Selected.A );
			  Variant( variants, State.WideValid, State.CrashValidDirty, Selected.Fail, Selected.A );

			  Variant( variants, State.CrashValid, State.WideValid, Selected.Fail, Selected.A );
			  Variant( variants, State.CrashValidDirty, State.WideValid, Selected.Fail, Selected.A );
			  Variant( variants, State.CrashValid, State.WideValidDirty, Selected.Fail, Selected.A );

			  return variants;
		 }

		 private static void Variant( ICollection<object[]> variants, State stateA, State stateB, Selected newest, Selected oldest )
		 {
			  variants.Add( new object[] { stateA, stateB, newest, oldest } );
		 }

		 internal abstract class SelectionUseCase
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           NEWEST { TreeState select(org.apache.commons.lang3.tuple.Pair<TreeState, TreeState> states) { return TreeStatePair.selectNewestValidState(states); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           OLDEST { TreeState select(org.apache.commons.lang3.tuple.Pair<TreeState, TreeState> states) { return TreeStatePair.selectOldestOrInvalid(states); } };

			  private static readonly IList<SelectionUseCase> valueList = new List<SelectionUseCase>();

			  static SelectionUseCase()
			  {
				  valueList.Add( NEWEST );
				  valueList.Add( OLDEST );
			  }

			  public enum InnerEnum
			  {
				  NEWEST,
				  OLDEST
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private SelectionUseCase( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal abstract TreeState select( org.apache.commons.lang3.tuple.Pair<TreeState, TreeState> states );

			 public static IList<SelectionUseCase> values()
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

			 public static SelectionUseCase ValueOf( string name )
			 {
				 foreach ( SelectionUseCase enumInstance in SelectionUseCase.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 internal abstract class State
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           EMPTY { void write(Neo4Net.io.pagecache.PageCursor cursor) { } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           BROKEN { void write(Neo4Net.io.pagecache.PageCursor cursor) { TreeState.write(cursor, 1, 2, 3, 4, 5, 6, 7, 8, 9, true); cursor.rewind(); long someOfTheBits = cursor.getLong(cursor.getOffset()); cursor.putLong(cursor.getOffset(), ~someOfTheBits); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           VALID { void write(Neo4Net.io.pagecache.PageCursor cursor) { TreeState.write(cursor, 5, 6, 7, 8, 9, 10, 11, 12, 13, true); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           CRASH_VALID { void write(Neo4Net.io.pagecache.PageCursor cursor) { TreeState.write(cursor, 5, 7, 7, 8, 9, 10, 11, 12, 13, true); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           WIDE_VALID { void write(Neo4Net.io.pagecache.PageCursor cursor) { TreeState.write(cursor, 4, 8, 9, 10, 11, 12, 13, 14, 15, true); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           OLD_VALID { void write(Neo4Net.io.pagecache.PageCursor cursor) { TreeState.write(cursor, 2, 3, 4, 5, 6, 7, 8, 9, 10, true); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           VALID_DIRTY { void write(Neo4Net.io.pagecache.PageCursor cursor) { TreeState.write(cursor, 5, 6, 7, 8, 9, 10, 11, 12, 13, false); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           CRASH_VALID_DIRTY { void write(Neo4Net.io.pagecache.PageCursor cursor) { TreeState.write(cursor, 5, 7, 7, 8, 9, 10, 11, 12, 13, false); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           WIDE_VALID_DIRTY { void write(Neo4Net.io.pagecache.PageCursor cursor) { TreeState.write(cursor, 4, 8, 9, 10, 11, 12, 13, 14, 15, false); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           OLD_VALID_DIRTY { void write(Neo4Net.io.pagecache.PageCursor cursor) { TreeState.write(cursor, 2, 3, 4, 5, 6, 7, 8, 9, 10, false); } };

			  private static readonly IList<State> valueList = new List<State>();

			  static State()
			  {
				  valueList.Add( EMPTY );
				  valueList.Add( BROKEN );
				  valueList.Add( VALID );
				  valueList.Add( CRASH_VALID );
				  valueList.Add( WIDE_VALID );
				  valueList.Add( OLD_VALID );
				  valueList.Add( VALID_DIRTY );
				  valueList.Add( CRASH_VALID_DIRTY );
				  valueList.Add( WIDE_VALID_DIRTY );
				  valueList.Add( OLD_VALID_DIRTY );
			  }

			  public enum InnerEnum
			  {
				  EMPTY,
				  BROKEN,
				  VALID,
				  CRASH_VALID,
				  WIDE_VALID,
				  OLD_VALID,
				  VALID_DIRTY,
				  CRASH_VALID_DIRTY,
				  WIDE_VALID_DIRTY,
				  OLD_VALID_DIRTY
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

			  internal abstract void write( Neo4Net.Io.pagecache.PageCursor cursor );

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

			 public static State ValueOf( string name )
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

		 internal abstract class Selected
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           FAIL { void verify(org.apache.commons.lang3.tuple.Pair<TreeState, TreeState> states, SelectionUseCase selection) { assertThrows(TreeInconsistencyException.class, () -> selection.select(states)); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           A { void verify(org.apache.commons.lang3.tuple.Pair<TreeState, TreeState> states, SelectionUseCase selection) { assertSame(states.getLeft(), selection.select(states)); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           B { void verify(org.apache.commons.lang3.tuple.Pair<TreeState, TreeState> states, SelectionUseCase selection) { assertSame(states.getRight(), selection.select(states)); } };

			  private static readonly IList<Selected> valueList = new List<Selected>();

			  static Selected()
			  {
				  valueList.Add( FAIL );
				  valueList.Add( A );
				  valueList.Add( B );
			  }

			  public enum InnerEnum
			  {
				  FAIL,
				  A,
				  B
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private Selected( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal abstract void verify( org.apache.commons.lang3.tuple.Pair<TreeState, TreeState> states, SelectionUseCase selection );

			 public static IList<Selected> values()
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

			 public static Selected ValueOf( string name )
			 {
				 foreach ( Selected enumInstance in Selected.valueList )
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