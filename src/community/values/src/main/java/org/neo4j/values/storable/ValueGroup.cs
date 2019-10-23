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
namespace Neo4Net.Values.Storable
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ValueCategory.NO_CATEGORY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ValueCategory.TEMPORAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.ValueCategory.TEMPORAL_ARRAY;

	/// <summary>
	/// The ValueGroup is the logical group or type of a Value. For example byte, short, int and long are all attempting
	/// to represent mathematical integers, meaning that for comparison purposes they should be treated the same.
	/// 
	/// The order here is defined in <a href="https://github.com/opencypher/openCypher/blob/master/cip/1.accepted/CIP2016-06-14-Define-comparability-and-equality-as-well-as-orderability-and-equivalence.adoc">
	///   The Cypher CIP defining orderability
	/// </a>
	/// 
	/// Each ValueGroup belong to some larger grouping called <seealso cref="ValueCategory"/>.
	/// </summary>
	public sealed class ValueGroup
	{
		 public static readonly ValueGroup Unknown = new ValueGroup( "Unknown", InnerEnum.Unknown, ValueCategory.Unknown );
		 public static readonly ValueGroup GeometryArray = new ValueGroup( "GeometryArray", InnerEnum.GeometryArray, ValueCategory.GeometryArray );
		 public static readonly ValueGroup ZonedDateTimeArray = new ValueGroup( "ZonedDateTimeArray", InnerEnum.ZonedDateTimeArray, TEMPORAL_ARRAY );
		 public static readonly ValueGroup LocalDateTimeArray = new ValueGroup( "LocalDateTimeArray", InnerEnum.LocalDateTimeArray, TEMPORAL_ARRAY );
		 public static readonly ValueGroup DateArray = new ValueGroup( "DateArray", InnerEnum.DateArray, TEMPORAL_ARRAY );
		 public static readonly ValueGroup ZonedTimeArray = new ValueGroup( "ZonedTimeArray", InnerEnum.ZonedTimeArray, TEMPORAL_ARRAY );
		 public static readonly ValueGroup LocalTimeArray = new ValueGroup( "LocalTimeArray", InnerEnum.LocalTimeArray, TEMPORAL_ARRAY );
		 public static readonly ValueGroup DurationArray = new ValueGroup( "DurationArray", InnerEnum.DurationArray, TEMPORAL_ARRAY );
		 public static readonly ValueGroup TextArray = new ValueGroup( "TextArray", InnerEnum.TextArray, ValueCategory.TextArray );
		 public static readonly ValueGroup BooleanArray = new ValueGroup( "BooleanArray", InnerEnum.BooleanArray, ValueCategory.BooleanArray );
		 public static readonly ValueGroup NumberArray = new ValueGroup( "NumberArray", InnerEnum.NumberArray, ValueCategory.NumberArray );
		 public static readonly ValueGroup Geometry = new ValueGroup( "Geometry", InnerEnum.Geometry, ValueCategory.Geometry );
		 public static readonly ValueGroup ZonedDateTime = new ValueGroup( "ZonedDateTime", InnerEnum.ZonedDateTime, TEMPORAL );
		 public static readonly ValueGroup LocalDateTime = new ValueGroup( "LocalDateTime", InnerEnum.LocalDateTime, TEMPORAL );
		 public static readonly ValueGroup Date = new ValueGroup( "Date", InnerEnum.Date, TEMPORAL );
		 public static readonly ValueGroup ZonedTime = new ValueGroup( "ZonedTime", InnerEnum.ZonedTime, TEMPORAL );
		 public static readonly ValueGroup LocalTime = new ValueGroup( "LocalTime", InnerEnum.LocalTime, TEMPORAL );
		 public static readonly ValueGroup Duration = new ValueGroup( "Duration", InnerEnum.Duration, TEMPORAL );
		 public static readonly ValueGroup Text = new ValueGroup( "Text", InnerEnum.Text, ValueCategory.Text );
		 public static readonly ValueGroup Boolean = new ValueGroup( "Boolean", InnerEnum.Boolean, ValueCategory.Boolean );
		 public static readonly ValueGroup Number = new ValueGroup( "Number", InnerEnum.Number, ValueCategory.Number );
		 public static readonly ValueGroup NoValue = new ValueGroup( "NoValue", InnerEnum.NoValue, NO_CATEGORY );

		 private static readonly IList<ValueGroup> valueList = new List<ValueGroup>();

		 static ValueGroup()
		 {
			 valueList.Add( Unknown );
			 valueList.Add( GeometryArray );
			 valueList.Add( ZonedDateTimeArray );
			 valueList.Add( LocalDateTimeArray );
			 valueList.Add( DateArray );
			 valueList.Add( ZonedTimeArray );
			 valueList.Add( LocalTimeArray );
			 valueList.Add( DurationArray );
			 valueList.Add( TextArray );
			 valueList.Add( BooleanArray );
			 valueList.Add( NumberArray );
			 valueList.Add( Geometry );
			 valueList.Add( ZonedDateTime );
			 valueList.Add( LocalDateTime );
			 valueList.Add( Date );
			 valueList.Add( ZonedTime );
			 valueList.Add( LocalTime );
			 valueList.Add( Duration );
			 valueList.Add( Text );
			 valueList.Add( Boolean );
			 valueList.Add( Number );
			 valueList.Add( NoValue );
		 }

		 public enum InnerEnum
		 {
			 Unknown,
			 GeometryArray,
			 ZonedDateTimeArray,
			 LocalDateTimeArray,
			 DateArray,
			 ZonedTimeArray,
			 LocalTimeArray,
			 DurationArray,
			 TextArray,
			 BooleanArray,
			 NumberArray,
			 Geometry,
			 ZonedDateTime,
			 LocalDateTime,
			 Date,
			 ZonedTime,
			 LocalTime,
			 Duration,
			 Text,
			 Boolean,
			 Number,
			 NoValue
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;

		 internal ValueGroup( string name, InnerEnum innerEnum, ValueCategory category )
		 {
			  this._category = category;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public ValueCategory Category()
		 {
			  return _category;
		 }

		public static IList<ValueGroup> values()
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

		public static ValueGroup ValueOf( string name )
		{
			foreach ( ValueGroup enumInstance in ValueGroup.valueList )
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