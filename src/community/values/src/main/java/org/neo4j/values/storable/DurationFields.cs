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
	using UnsupportedTemporalUnitException = Neo4Net.Values.utils.UnsupportedTemporalUnitException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.utils.TemporalUtil.NANOS_PER_SECOND;

	/// <summary>
	/// Defines all valid field accessors for durations
	/// </summary>
	public abstract class DurationFields
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       YEARS("years") { public long asTimeStamp(long months, long days, long seconds, long nanos) { return months / 12; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       MONTHS("months") { public long asTimeStamp(long months, long days, long seconds, long nanos) { return months; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       MONTHS_OF_YEAR("monthsofyear") { public long asTimeStamp(long months, long days, long seconds, long nanos) { return months % 12; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       MONTHS_OF_QUARTER("monthsofquarter") { public long asTimeStamp(long months, long days, long seconds, long nanos) { return months % 3; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       QUARTERS("quarters") { public long asTimeStamp(long months, long days, long seconds, long nanos) { return months / 3; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       QUARTERS_OF_YEAR("quartersofyear") { public long asTimeStamp(long months, long days, long seconds, long nanos) { return(months / 3) % 4; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       WEEKS("weeks") { public long asTimeStamp(long months, long days, long seconds, long nanos) { return days / 7; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       DAYS_OF_WEEK("daysofweek") { public long asTimeStamp(long months, long days, long seconds, long nanos) { return days % 7; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       DAYS("days") { public long asTimeStamp(long months, long days, long seconds, long nanos) { return days; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       HOURS("hours") { public long asTimeStamp(long months, long days, long seconds, long nanos) { return seconds / 3600; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       MINUTES_OF_HOUR("minutesofhour") { public long asTimeStamp(long months, long days, long seconds, long nanos) { return(seconds / 60) % 60; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       MINUTES("minutes") { public long asTimeStamp(long months, long days, long seconds, long nanos) { return seconds / 60; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       SECONDS_OF_MINUTE("secondsofminute") { public long asTimeStamp(long months, long days, long seconds, long nanos) { return seconds % 60; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       SECONDS("seconds") { public long asTimeStamp(long months, long days, long seconds, long nanos) { return seconds; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       MILLISECONDS_OF_SECOND("millisecondsofsecond") { public long asTimeStamp(long months, long days, long seconds, long nanos) { return nanos / 1000_000; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       MILLISECONDS("milliseconds") { public long asTimeStamp(long months, long days, long seconds, long nanos) { return seconds * 1000 + nanos / 1000_000; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       MICROSECONDS_OF_SECOND("microsecondsofsecond") { public long asTimeStamp(long months, long days, long seconds, long nanos) { return nanos / 1000; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       MICROSECONDS("microseconds") { public long asTimeStamp(long months, long days, long seconds, long nanos) { return seconds * 1000_000 + nanos / 1000; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       NANOSECONDS_OF_SECOND("nanosecondsofsecond") { public long asTimeStamp(long months, long days, long seconds, long nanos) { return nanos; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       NANOSECONDS("nanoseconds") { public long asTimeStamp(long months, long days, long seconds, long nanos) { return seconds * NANOS_PER_SECOND + nanos; } };

		 private static readonly IList<DurationFields> valueList = new List<DurationFields>();

		 static DurationFields()
		 {
			 valueList.Add( YEARS );
			 valueList.Add( MONTHS );
			 valueList.Add( MONTHS_OF_YEAR );
			 valueList.Add( MONTHS_OF_QUARTER );
			 valueList.Add( QUARTERS );
			 valueList.Add( QUARTERS_OF_YEAR );
			 valueList.Add( WEEKS );
			 valueList.Add( DAYS_OF_WEEK );
			 valueList.Add( DAYS );
			 valueList.Add( HOURS );
			 valueList.Add( MINUTES_OF_HOUR );
			 valueList.Add( MINUTES );
			 valueList.Add( SECONDS_OF_MINUTE );
			 valueList.Add( SECONDS );
			 valueList.Add( MILLISECONDS_OF_SECOND );
			 valueList.Add( MILLISECONDS );
			 valueList.Add( MICROSECONDS_OF_SECOND );
			 valueList.Add( MICROSECONDS );
			 valueList.Add( NANOSECONDS_OF_SECOND );
			 valueList.Add( NANOSECONDS );
		 }

		 public enum InnerEnum
		 {
			 YEARS,
			 MONTHS,
			 MONTHS_OF_YEAR,
			 MONTHS_OF_QUARTER,
			 QUARTERS,
			 QUARTERS_OF_YEAR,
			 WEEKS,
			 DAYS_OF_WEEK,
			 DAYS,
			 HOURS,
			 MINUTES_OF_HOUR,
			 MINUTES,
			 SECONDS_OF_MINUTE,
			 SECONDS,
			 MILLISECONDS_OF_SECOND,
			 MILLISECONDS,
			 MICROSECONDS_OF_SECOND,
			 MICROSECONDS,
			 NANOSECONDS_OF_SECOND,
			 NANOSECONDS
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private DurationFields( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public string propertyKey;

		 DurationFields( string propertyKey ) { this.propertyKey = propertyKey; } public abstract long asTimeStamp( long months, long days, long seconds, long nanos );

		 public static readonly DurationFields public static DurationFields fromName( String fieldName )
		 {
			 switch ( fieldName.toLowerCase() ) { case "years": return YEARS; case "months": return MONTHS; case "monthsofyear": return MONTHS_OF_YEAR; case "monthsofquarter": return MONTHS_OF_QUARTER; case "quarters": return QUARTERS; case "quartersofyear": return QUARTERS_OF_YEAR; case "weeks": return WEEKS; case "daysofweek": return DAYS_OF_WEEK; case "days": return DAYS; case "hours": return HOURS; case "minutesofhour": return MINUTES_OF_HOUR; case "minutes": return MINUTES; case "secondsofminute": return SECONDS_OF_MINUTE; case "seconds": return SECONDS; case "millisecondsofsecond": return MILLISECONDS_OF_SECOND; case "milliseconds": return MILLISECONDS; case "microsecondsofsecond": return MICROSECONDS_OF_SECOND; case "microseconds": return MICROSECONDS; case "nanosecondsofsecond": return NANOSECONDS_OF_SECOND; case "nanoseconds": return NANOSECONDS; default: throw new Neo4Net.Values.utils.UnsupportedTemporalUnitException("No such field: " + fieldName); }
		 }
		 = new DurationFields("public static DurationFields fromName(String fieldName) { switch(fieldName.toLowerCase()) { case "years": return YEARS; case "months": return MONTHS; case "monthsofyear": return MONTHS_OF_YEAR; case "monthsofquarter": return MONTHS_OF_QUARTER; case "quarters": return QUARTERS; case "quartersofyear": return QUARTERS_OF_YEAR; case "weeks": return WEEKS; case "daysofweek": return DAYS_OF_WEEK; case "days": return DAYS; case "hours": return HOURS; case "minutesofhour": return MINUTES_OF_HOUR; case "minutes": return MINUTES; case "secondsofminute": return SECONDS_OF_MINUTE; case "seconds": return SECONDS; case "millisecondsofsecond": return MILLISECONDS_OF_SECOND; case "milliseconds": return MILLISECONDS; case "microsecondsofsecond": return MICROSECONDS_OF_SECOND; case "microseconds": return MICROSECONDS; case "nanosecondsofsecond": return NANOSECONDS_OF_SECOND; case "nanoseconds": return NANOSECONDS; default: throw new org.Neo4Net.values.utils.UnsupportedTemporalUnitException("No such field: " + fieldName); } }", InnerEnum.public static DurationFields fromName(String fieldName)
		 {
			 switch ( fieldName.toLowerCase() ) { case "years": return YEARS; case "months": return MONTHS; case "monthsofyear": return MONTHS_OF_YEAR; case "monthsofquarter": return MONTHS_OF_QUARTER; case "quarters": return QUARTERS; case "quartersofyear": return QUARTERS_OF_YEAR; case "weeks": return WEEKS; case "daysofweek": return DAYS_OF_WEEK; case "days": return DAYS; case "hours": return HOURS; case "minutesofhour": return MINUTES_OF_HOUR; case "minutes": return MINUTES; case "secondsofminute": return SECONDS_OF_MINUTE; case "seconds": return SECONDS; case "millisecondsofsecond": return MILLISECONDS_OF_SECOND; case "milliseconds": return MILLISECONDS; case "microsecondsofsecond": return MICROSECONDS_OF_SECOND; case "microseconds": return MICROSECONDS; case "nanosecondsofsecond": return NANOSECONDS_OF_SECOND; case "nanoseconds": return NANOSECONDS; default: throw new Neo4Net.Values.utils.UnsupportedTemporalUnitException("No such field: " + fieldName); }
		 });

		public static IList<DurationFields> values()
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

		public static DurationFields ValueOf( string name )
		{
			foreach ( DurationFields enumInstance in DurationFields.valueList )
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