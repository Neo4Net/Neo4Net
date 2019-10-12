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
namespace Org.Neo4j.Values.Storable
{


	internal sealed class Neo4JTemporalField : TemporalField
	{
		 public static readonly Neo4JTemporalField YearOfDecade = new Neo4JTemporalField( "YearOfDecade", InnerEnum.YearOfDecade, "Year of decade", YEARS, DECADES, 10 );
		 public static readonly Neo4JTemporalField YearOfCentury = new Neo4JTemporalField( "YearOfCentury", InnerEnum.YearOfCentury, "Year of century", YEARS, CENTURIES, 100 );
		 public static readonly Neo4JTemporalField YearOfMillennium = new Neo4JTemporalField( "YearOfMillennium", InnerEnum.YearOfMillennium, "Millennium", YEARS, MILLENNIA, 1000 );

		 private static readonly IList<Neo4JTemporalField> valueList = new List<Neo4JTemporalField>();

		 static Neo4JTemporalField()
		 {
			 valueList.Add( YearOfDecade );
			 valueList.Add( YearOfCentury );
			 valueList.Add( YearOfMillennium );
		 }

		 public enum InnerEnum
		 {
			 YearOfDecade,
			 YearOfCentury,
			 YearOfMillennium
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;
		 internal Private readonly;
		 internal Private readonly;
		 internal Private readonly;
		 internal Private readonly;

		 internal Neo4JTemporalField( string name, InnerEnum innerEnum, string name, java.time.temporal.TemporalUnit baseUnit, java.time.temporal.TemporalUnit rangeUnit, int years )
		 {
			  this._name = name;
			  this._baseUnit = baseUnit;
			  this._rangeUnit = rangeUnit;
			  this._years = years;
			  this._range = ValueRange.of( Year.MIN_VALUE / years, Year.MAX_VALUE / years );

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public string GetDisplayName( java.util.Locale locale )
		 {
			  return _name;
		 }

		 public java.time.temporal.TemporalUnit BaseUnit
		 {
			 get
			 {
				  return _baseUnit;
			 }
		 }

		 public java.time.temporal.TemporalUnit RangeUnit
		 {
			 get
			 {
				  return _rangeUnit;
			 }
		 }

		 public java.time.temporal.ValueRange Range()
		 {
			  return _range;
		 }

		 public bool DateBased
		 {
			 get
			 {
				  return true;
			 }
		 }

		 public bool TimeBased
		 {
			 get
			 {
				  return false;
			 }
		 }

		 public bool IsSupportedBy( java.time.temporal.TemporalAccessor temporal )
		 {
			  return false;
		 }

		 public java.time.temporal.ValueRange RangeRefinedBy( java.time.temporal.TemporalAccessor temporal )
		 {
			  // Always identical
			  return Range();
		 }

		 public long GetFrom( java.time.temporal.TemporalAccessor temporal )
		 {
			  throw new System.NotSupportedException( "Getting a " + this._name + " from temporal values is not supported." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public <R extends java.time.temporal.Temporal> R adjustInto(R temporal, long newValue)
		 public R AdjustInto<R>( R temporal, long newValue )
		 {
			  int newVal = _range.checkValidIntValue( newValue, this );
			  int oldYear = temporal.get( ChronoField.YEAR );
			  return ( R ) temporal.with( ChronoField.YEAR, ( oldYear / _years ) * _years + newVal ).with( TemporalAdjusters.firstDayOfYear() );
		 }

		 public override string ToString()
		 {
			  return _name;
		 }


		public static IList<Neo4JTemporalField> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static Neo4JTemporalField valueOf( string name )
		{
			foreach ( Neo4JTemporalField enumInstance in Neo4JTemporalField.valueList )
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