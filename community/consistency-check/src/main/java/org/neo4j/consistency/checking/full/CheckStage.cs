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
namespace Org.Neo4j.Consistency.checking.full
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.cache.CacheSlots_Fields.ID_SLOT_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.cache.CacheSlots_Fields.LABELS_SLOT_SIZE;

	/// <summary>
	/// The different stages a consistency check goes through. A stage typically focuses one one store.
	/// </summary>
	public sealed class CheckStage : Stage
	{
		 public static readonly CheckStage Stage1NSPropsLabels = new CheckStage( "Stage1NSPropsLabels", InnerEnum.Stage1NSPropsLabels, false, true, "NodeStore pass - check its properties, " + "check labels and cache them, skip relationships", LABELS_SLOT_SIZE, 1 );
		 public static readonly CheckStage Stage2RSLabels = new CheckStage( "Stage2RSLabels", InnerEnum.Stage2RSLabels, false, true, "RelationshipStore pass - check label counts using cached labels, check properties, " + "skip nodes and relationships", LABELS_SLOT_SIZE, 1 );
		 public static readonly CheckStage Stage3NSNextRel = new CheckStage( "Stage3NSNextRel", InnerEnum.Stage3NSNextRel, false, true, "NodeStore pass - just cache nextRel and inUse", ID_SLOT_SIZE, 1, 1 );
		 public static readonly CheckStage Stage4RSNextRel = new CheckStage( "Stage4RSNextRel", InnerEnum.Stage4RSNextRel, true, true, "RelationshipStore pass - check nodes inUse, FirstInFirst, " + "FirstInSecond using cached info", ID_SLOT_SIZE, 1, 1 );
		 public static readonly CheckStage Stage5CheckNextRel = new CheckStage( "Stage5CheckNextRel", InnerEnum.Stage5CheckNextRel, false, true, "NodeRelationship cache pass - check nextRel", ID_SLOT_SIZE, 1, 1 );
		 public static readonly CheckStage Stage6RSForward = new CheckStage( "Stage6RSForward", InnerEnum.Stage6RSForward, true, true, "RelationshipStore pass - forward scan of source chain using the cache", ID_SLOT_SIZE, ID_SLOT_SIZE, 1, 1, 1 );
		 public static readonly CheckStage Stage7RSBackward = new CheckStage( "Stage7RSBackward", InnerEnum.Stage7RSBackward, true, false, "RelationshipStore pass - reverse scan of source chain using the cache", ID_SLOT_SIZE, ID_SLOT_SIZE, 1, 1, 1 );
		 public static readonly CheckStage Stage8PSProps = new CheckStage( "Stage8PSProps", InnerEnum.Stage8PSProps, true, true, "PropertyStore and Node to Index check pass" );
		 public static readonly CheckStage Stage9RSIndexes = new CheckStage( "Stage9RSIndexes", InnerEnum.Stage9RSIndexes, true, true, "Relationship to Index check pass" );
		 public static readonly CheckStage Stage10NSLabelCounts = new CheckStage( "Stage10NSLabelCounts", InnerEnum.Stage10NSLabelCounts, true, true, "NodeStore pass - Label counts" );
		 public static readonly CheckStage Stage11NSPropertyRelocator = new CheckStage( "Stage11NSPropertyRelocator", InnerEnum.Stage11NSPropertyRelocator, true, true, "Property store relocation" );

		 private static readonly IList<CheckStage> valueList = new List<CheckStage>();

		 static CheckStage()
		 {
			 valueList.Add( Stage1NSPropsLabels );
			 valueList.Add( Stage2RSLabels );
			 valueList.Add( Stage3NSNextRel );
			 valueList.Add( Stage4RSNextRel );
			 valueList.Add( Stage5CheckNextRel );
			 valueList.Add( Stage6RSForward );
			 valueList.Add( Stage7RSBackward );
			 valueList.Add( Stage8PSProps );
			 valueList.Add( Stage9RSIndexes );
			 valueList.Add( Stage10NSLabelCounts );
			 valueList.Add( Stage11NSPropertyRelocator );
		 }

		 public enum InnerEnum
		 {
			 Stage1NSPropsLabels,
			 Stage2RSLabels,
			 Stage3NSNextRel,
			 Stage4RSNextRel,
			 Stage5CheckNextRel,
			 Stage6RSForward,
			 Stage7RSBackward,
			 Stage8PSProps,
			 Stage9RSIndexes,
			 Stage10NSLabelCounts,
			 Stage11NSPropertyRelocator
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;
		 internal Private readonly;
		 internal Private readonly;
		 internal Private readonly;

		 internal CheckStage( string name, InnerEnum innerEnum, bool parallel, bool forward, string purpose, params int[] cacheFields )
		 {
			  this._parallel = parallel;
			  this._forward = forward;
			  this._purpose = purpose;
			  this._cacheSlotSizes = cacheFields;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public bool Parallel
		 {
			 get
			 {
				  return _parallel;
			 }
		 }

		 public bool Forward
		 {
			 get
			 {
				  return _forward;
			 }
		 }

		 public string Purpose
		 {
			 get
			 {
				  return _purpose;
			 }
		 }

		 public int[] CacheSlotSizes
		 {
			 get
			 {
				  return _cacheSlotSizes;
			 }
		 }

		public static IList<CheckStage> values()
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

		public static CheckStage valueOf( string name )
		{
			foreach ( CheckStage enumInstance in CheckStage.valueList )
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