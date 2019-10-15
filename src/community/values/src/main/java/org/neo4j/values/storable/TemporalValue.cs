using System;
using System.Collections.Generic;
using System.Diagnostics;

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

	using IHashFunction = Neo4Net.Hashing.HashFunction;
	using Neo4Net.Helpers.Collections;
	using Neo4Net.Values;
	using InvalidValuesArgumentException = Neo4Net.Values.utils.InvalidValuesArgumentException;
	using TemporalArithmeticException = Neo4Net.Values.utils.TemporalArithmeticException;
	using TemporalParseException = Neo4Net.Values.utils.TemporalParseException;
	using UnsupportedTemporalUnitException = Neo4Net.Values.utils.UnsupportedTemporalUnitException;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateTimeValue.datetime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateTimeValue.parseZoneName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.IntegralValue.safeCastIntegral;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.LocalDateTimeValue.localDateTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.NumberType.NO_NUMBER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.TimeValue.time;

	public abstract class TemporalValue<T, V> : ScalarValue, Temporal where T : java.time.temporal.Temporal where V : TemporalValue<T,V>
	{
		 internal TemporalValue()
		 {
			  // subclasses are confined to this package,
			  // but type-checking is valuable to be able to do outside
			  // (therefore the type itself is public)
		 }

		 public abstract TemporalValue Add( DurationValue duration );

		 public abstract TemporalValue Sub( DurationValue duration );

		 internal abstract T Temporal();

		 /// <returns> the date part of this temporal, if date is supported. </returns>
		 internal abstract LocalDate DatePart { get; }

		 /// <returns> the local time part of this temporal, if time is supported. </returns>
		 internal abstract LocalTime LocalTimePart { get; }

		 /// <returns> the time part of this temporal, if time is supported. </returns>
		 internal abstract OffsetTime GetTimePart( System.Func<ZoneId> defaultZone );

		 /// <returns> the zone id, if time is supported. If time is supported, but no timezone, the defaultZone will be used. </returns>
		 /// <exception cref="UnsupportedTemporalUnitException"> if time is not supported </exception>
		 internal abstract ZoneId GetZoneId( System.Func<ZoneId> defaultZone );

		 /// <returns> the zone id, if this temporal has a timezone. </returns>
		 /// <exception cref="UnsupportedTemporalUnitException"> if this does not have a timezone </exception>
		 internal abstract ZoneId ZoneId { get; }

		 /// <returns> the zone offset, if this temporal has a zone offset. </returns>
		 /// <exception cref="UnsupportedTemporalUnitException"> if this does not have a offset </exception>
		 internal abstract ZoneOffset ZoneOffset { get; }

		 internal abstract bool SupportsTimeZone();

		 internal abstract bool HasTime();

		 internal abstract V Replacement( T temporal );

		 public override T AsObjectCopy()
		 {
			  return Temporal();
		 }

		 public override long UpdateHash( IHashFunction hashFunction, long hash )
		 {
			  // todo Good enough? Or do subclasses need to implement each their own?
			  return hashFunction.Update( hash, GetHashCode() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public final V with(java.time.temporal.TemporalAdjuster adjuster)
		 public override V With( TemporalAdjuster adjuster )
		 {
			  return Replacement( ( T ) Temporal().with(adjuster) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public final V plus(java.time.temporal.TemporalAmount amount)
		 public override V Plus( TemporalAmount amount )
		 {
			  return Replacement( ( T ) Temporal().plus(amount) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public final V minus(java.time.temporal.TemporalAmount amount)
		 public override V Minus( TemporalAmount amount )
		 {
			  return Replacement( ( T ) Temporal().minus(amount) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public final V minus(long amountToSubtract, java.time.temporal.TemporalUnit unit)
		 public override V Minus( long amountToSubtract, TemporalUnit unit )
		 {
			  return Replacement( ( T ) Temporal().minus(amountToSubtract, unit) );
		 }

		 public override bool IsSupported( TemporalUnit unit )
		 {
			  return Temporal().isSupported(unit);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public final V with(java.time.temporal.TemporalField field, long newValue)
		 public override V With( TemporalField field, long newValue )
		 {
			  return Replacement( ( T ) Temporal().with(field, newValue) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public final V plus(long amountToAdd, java.time.temporal.TemporalUnit unit)
		 public override V Plus( long amountToAdd, TemporalUnit unit )
		 {
			  return Replacement( ( T ) Temporal().plus(amountToAdd, unit) );
		 }

		 public override long Until( Temporal endExclusive, TemporalUnit unit )
		 {
			  if ( !( endExclusive is TemporalValue ) )
			  {
					throw new InvalidValuesArgumentException( "Can only compute durations between TemporalValues." );
			  }
			  TemporalValue from = this;
			  TemporalValue to = ( TemporalValue ) endExclusive;

			  from = AttachTime( from );
			  to = AttachTime( to );

			  if ( from.IsSupported( ChronoField.MONTH_OF_YEAR ) && !to.IsSupported( ChronoField.MONTH_OF_YEAR ) )
			  {
					to = AttachDate( to, from.DatePart );
			  }
			  else if ( to.IsSupported( ChronoField.MONTH_OF_YEAR ) && !from.IsSupported( ChronoField.MONTH_OF_YEAR ) )
			  {
					from = AttachDate( from, to.DatePart );
			  }

			  if ( from.SupportsTimeZone() && !to.SupportsTimeZone() )
			  {
					to = AttachTimeZone( to, from.GetZoneId( from.getZoneOffset ) );
			  }
			  else if ( to.SupportsTimeZone() && !from.SupportsTimeZone() )
			  {
					from = AttachTimeZone( from, to.GetZoneId( to.getZoneOffset ) );
			  }
			  long until;
			  try
			  {
					until = from.Temporal().until(to, unit);
			  }
			  catch ( UnsupportedTemporalTypeException e )
			  {
					throw new UnsupportedTemporalUnitException( e.Message, e );
			  }
			  return until;
		 }

		 private TemporalValue AttachTime( TemporalValue temporal )
		 {
			  bool supportsTime = temporal.IsSupported( ChronoField.SECOND_OF_DAY );

			  if ( supportsTime )
			  {
					return temporal;
			  }
			  else
			  {
					LocalDate datePart = temporal.DatePart;
					LocalTime timePart = LocalTimeValue.DefaultLocalTime;
					return localDateTime( new DateTime( datePart, timePart ) );
			  }
		 }

		 private TemporalValue AttachDate( TemporalValue temporal, LocalDate dateToAttach )
		 {
			  LocalTime timePart = temporal.LocalTimePart;

			  if ( temporal.SupportsTimeZone() )
			  {
					// turn time into date time
					return datetime( ZonedDateTime.of( dateToAttach, timePart, temporal.ZoneOffset ) );
			  }
			  else
			  {
					// turn local time into local date time
					return localDateTime( new DateTime( dateToAttach, timePart ) );
			  }
		 }

		 private TemporalValue AttachTimeZone( TemporalValue temporal, ZoneId zoneIdToAttach )
		 {
			  if ( temporal.IsSupported( ChronoField.MONTH_OF_YEAR ) )
			  {
					// turn local date time into date time
					return datetime( ZonedDateTime.of( temporal.DatePart, temporal.LocalTimePart, zoneIdToAttach ) );
			  }
			  else
			  {
					// turn local time into time
					if ( zoneIdToAttach is ZoneOffset )
					{
						 return time( OffsetTime.of( temporal.LocalTimePart, ( ZoneOffset ) zoneIdToAttach ) );
					}
					else
					{
						 throw new System.InvalidOperationException( "Should only attach offsets to local times, not zone ids." );
					}
			  }
		 }

		 public override ValueRange Range( TemporalField field )
		 {
			  return Temporal().range(field);
		 }

		 public override int Get( TemporalField field )
		 {
			  int accessor;
			  try
			  {
				accessor = Temporal().get(field);
			  }
			  catch ( UnsupportedTemporalTypeException e )
			  {
					throw new UnsupportedTemporalUnitException( e.Message, e );
			  }
			  return accessor;
		 }

		 public AnyValue Get( string fieldName )
		 {
			  TemporalFields field = TemporalFields.fields.get( fieldName.ToLower() );
			  if ( field == TemporalFields.EpochSeconds || field == TemporalFields.EpochMillis )
			  {
					T temp = Temporal();
					if ( temp is ChronoZonedDateTime )
					{
						 ChronoZonedDateTime zdt = ( ChronoZonedDateTime ) temp;
						 if ( field == TemporalFields.EpochSeconds )
						 {
							  return Values.LongValue( zdt.toInstant().toEpochMilli() / 1000 );
						 }
						 else
						 {
							  return Values.LongValue( zdt.toInstant().toEpochMilli() );
						 }
					}
					else
					{
						 throw new UnsupportedTemporalUnitException( "Epoch not supported." );
					}
			  }
			  if ( field == TemporalFields.Timezone )
			  {
					return Values.StringValue( GetZoneId( this.getZoneOffset ).ToString() );
			  }
			  if ( field == TemporalFields.Offset )
			  {
					return Values.StringValue( ZoneOffset.ToString() );
			  }
			  if ( field == TemporalFields.OffsetMinutes )
			  {
					return Values.IntValue( ZoneOffset.TotalSeconds / 60 );
			  }
			  if ( field == TemporalFields.OffsetSeconds )
			  {
					return Values.IntValue( ZoneOffset.TotalSeconds );
			  }
			  if ( field == null || field.field == null )
			  {
					throw new UnsupportedTemporalUnitException( "No such field: " + fieldName );
			  }
			  return Values.IntValue( get( field.field ) );
		 }

		 public override R Query<R>( TemporalQuery<R> query )
		 {
			  return Temporal().query(query);
		 }

		 public override bool IsSupported( TemporalField field )
		 {
			  return Temporal().isSupported(field);
		 }

		 public override long GetLong( TemporalField field )
		 {
			  return Temporal().getLong(field);
		 }

		 public override NumberType NumberType()
		 {
			  return NO_NUMBER;
		 }

		 public override sealed bool Equals( bool x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( long x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( double x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( char x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( string x )
		 {
			  return false;
		 }

		 public override string ToString()
		 {
			  return prettyPrint();
		 }

		 internal static VALUE Parse<VALUE>( Type type, Pattern pattern, System.Func<Matcher, VALUE> parser, CharSequence text )
		 {
				 type = typeof( VALUE );
			  Matcher matcher = pattern.matcher( text );
			  VALUE result = matcher.matches() ? parser(matcher) : default(VALUE);
			  if ( result == default( VALUE ) )
			  {
					throw new TemporalParseException( "Text cannot be parsed to a " + ValueName( type ), text.ToString(), 0 );
			  }
			  return result;
		 }

		 internal static VALUE Parse<VALUE>( Type type, Pattern pattern, System.Func<Matcher, VALUE> parser, TextValue text )
		 {
				 type = typeof( VALUE );
			  Matcher matcher = text.Matcher( pattern );
			  VALUE result = matcher != null && matcher.matches() ? parser(matcher) : default(VALUE);
			  if ( result == default( VALUE ) )
			  {
					throw new TemporalParseException( "Text cannot be parsed to a " + ValueName( type ), text.StringValue(), 0 );
			  }
			  return result;
		 }

		 internal static VALUE Parse<VALUE>( Type type, Pattern pattern, System.Func<Matcher, System.Func<ZoneId>, VALUE> parser, CharSequence text, System.Func<ZoneId> defaultZone )
		 {
				 type = typeof( VALUE );
			  Matcher matcher = pattern.matcher( text );
			  VALUE result = matcher.matches() ? parser(matcher, defaultZone) : default(VALUE);
			  if ( result == default( VALUE ) )
			  {
					throw new TemporalParseException( "Text cannot be parsed to a " + ValueName( type ), text.ToString(), 0 );
			  }
			  return result;
		 }

		 internal static VALUE Parse<VALUE>( Type type, Pattern pattern, System.Func<Matcher, System.Func<ZoneId>, VALUE> parser, TextValue text, System.Func<ZoneId> defaultZone )
		 {
				 type = typeof( VALUE );
			  Matcher matcher = text.Matcher( pattern );
			  VALUE result = matcher != null && matcher.matches() ? parser(matcher, defaultZone) : default(VALUE);
			  if ( result == default( VALUE ) )
			  {
					throw new TemporalParseException( "Text cannot be parsed to a " + ValueName( type ), text.StringValue(), 0 );
			  }
			  return result;
		 }

		 private static string ValueName<VALUE>( Type type )
		 {
				 type = typeof( VALUE );
			  string name = type.Name;
			  return name.Substring( 0, name.Length - 5 );
		 }

		 public static TimeCSVHeaderInformation ParseHeaderInformation( string text )
		 {
			  TimeCSVHeaderInformation fields = new TimeCSVHeaderInformation();
			  Value.ParseHeaderInformation( text, "time/datetime", fields );
			  return fields;
		 }

		 internal abstract class Builder<Result> : StructureBuilder<AnyValue, Result>
		 {
			 public abstract T Build( StructureBuilder<AnyValue, T> builder, IEnumerable<KeyValuePair<string, AnyValue>> entries );
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public abstract T build(final org.neo4j.values.StructureBuilder<org.neo4j.values.AnyValue, T> builder, org.neo4j.values.virtual.MapValue map);
			 public abstract T Build( StructureBuilder<AnyValue, T> builder, MapValue map );
			 public abstract StructureBuilder<Input, Result> Add( string field, Input value );
			  internal readonly System.Func<ZoneId> DefaultZone;
			  internal DateTimeBuilder State;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  protected internal AnyValue TimezoneConflict;

			  protected internal IDictionary<TemporalFields, AnyValue> Fields = new Dictionary<TemporalFields, AnyValue>( typeof( TemporalFields ) );

			  internal Builder( System.Func<ZoneId> defaultZone )
			  {
					this.DefaultZone = defaultZone;
			  }

			  public override Result Build()
			  {
					if ( State == null )
					{
						 throw new InvalidValuesArgumentException( "Builder state empty" );
					}
					State.checkAssignments( this.SupportsDate() );
					try
					{
						 return BuildInternal();
					}
					catch ( DateTimeException e )
					{
						 throw new InvalidValuesArgumentException( e.Message, e );
					}
			  }

			  internal virtual Temp AssignAllFields<Temp>( Temp temp ) where Temp : java.time.temporal.Temporal
			  {
					Temp result = temp;
					foreach ( KeyValuePair<TemporalFields, AnyValue> entry in Fields.SetOfKeyValuePairs() )
					{
						 TemporalFields f = entry.Key;
						 if ( f == TemporalFields.Year && Fields.ContainsKey( TemporalFields.Week ) )
						 {
							  // Year can mean week-based year, if a week is specified.
							  result = ( Temp ) result.with( IsoFields.WEEK_BASED_YEAR, safeCastIntegral( f.name(), entry.Value, f.defaultValue ) );
						 }
						 else if ( !f.GroupSelector && f != TemporalFields.Timezone && f != TemporalFields.Millisecond && f != TemporalFields.Microsecond && f != TemporalFields.Nanosecond )
						 {
							  TemporalField temporalField = f.field;
							  result = ( Temp ) result.with( temporalField, safeCastIntegral( f.name(), entry.Value, f.defaultValue ) );
						 }
					}
					// Assign all sub-second parts in one step
					if ( SupportsTime() && (Fields.ContainsKey(TemporalFields.Millisecond) || Fields.ContainsKey(TemporalFields.Microsecond) || Fields.ContainsKey(TemporalFields.Nanosecond)) )
					{
						 result = ( Temp ) result.with( TemporalFields.Nanosecond.field, ValidNano( Fields[TemporalFields.Millisecond], Fields[TemporalFields.Microsecond], Fields[TemporalFields.Nanosecond] ) );
					}
					return result;
			  }

			  public override StructureBuilder<AnyValue, Result> Add( string fieldName, AnyValue value )
			  {
					TemporalFields field = TemporalFields.fields.get( fieldName.ToLower() );
					if ( field == null )
					{
						 throw new InvalidValuesArgumentException( "No such field: " + fieldName );
					}
					// Change state
					field.assign( this, value );

					// Set field for this builder
					Fields[field] = value;
					return this;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("BooleanMethodIsAlwaysInverted") private boolean supports(java.time.temporal.TemporalField field)
			  internal virtual bool Supports( TemporalField field )
			  {
					if ( field.DateBased )
					{
						 return SupportsDate();
					}
					if ( field.TimeBased )
					{
						 return SupportsTime();
					}
					throw new System.InvalidOperationException( "Fields should be either date based or time based" );
			  }

			  protected internal abstract bool SupportsDate();

			  protected internal abstract bool SupportsTime();

			  protected internal abstract bool SupportsTimeZone();

			  protected internal abstract bool SupportsEpoch();

			  protected internal virtual ZoneId Timezone( AnyValue timezone )
			  {
					return timezone == null ? DefaultZone.get() : TimezoneOf(timezone);
			  }

			  // Construction

			  protected internal abstract Result BuildInternal();

			  // Timezone utilities

			  protected internal ZoneId OptionalTimezone()
			  {
					return TimezoneConflict == null ? null : TimezoneConflict();
			  }

			  protected internal ZoneId Timezone()
			  {
					return TimezoneConflict( TimezoneConflict );
			  }

		 }

		 /// <summary>
		 /// All fields that can be a asigned to or read from temporals.
		 /// Make sure that writable fields defined in "decreasing" order between year and nanosecond.
		 /// </summary>
		 public sealed class TemporalFields
		 {
			  public static readonly TemporalFields Year = new TemporalFields( "Year", InnerEnum.Year, java.time.temporal.ChronoField.YEAR, 0 );
			  public static readonly TemporalFields Quarter = new TemporalFields( "Quarter", InnerEnum.Quarter, java.time.temporal.IsoFields.QUARTER_OF_YEAR, 1 );
			  public static readonly TemporalFields Month = new TemporalFields( "Month", InnerEnum.Month, java.time.temporal.ChronoField.MONTH_OF_YEAR, 1 );
			  public static readonly TemporalFields Week = new TemporalFields( "Week", InnerEnum.Week, java.time.temporal.IsoFields.WEEK_OF_WEEK_BASED_YEAR, 1 );
			  public static readonly TemporalFields OrdinalDay = new TemporalFields( "OrdinalDay", InnerEnum.OrdinalDay, java.time.temporal.ChronoField.DAY_OF_YEAR, 1 );
			  public static readonly TemporalFields DayOfQuarter = new TemporalFields( "DayOfQuarter", InnerEnum.DayOfQuarter, java.time.temporal.IsoFields.DAY_OF_QUARTER, 1 );
			  public static readonly TemporalFields DayOfWeek = new TemporalFields( "DayOfWeek", InnerEnum.DayOfWeek, java.time.temporal.ChronoField.DAY_OF_WEEK, 1 );
			  public static readonly TemporalFields Day = new TemporalFields( "Day", InnerEnum.Day, java.time.temporal.ChronoField.DAY_OF_MONTH, 1 );
			  public static readonly TemporalFields Hour = new TemporalFields( "Hour", InnerEnum.Hour, java.time.temporal.ChronoField.HOUR_OF_DAY, 0 );
			  public static readonly TemporalFields Minute = new TemporalFields( "Minute", InnerEnum.Minute, java.time.temporal.ChronoField.MINUTE_OF_HOUR, 0 );
			  public static readonly TemporalFields Second = new TemporalFields( "Second", InnerEnum.Second, java.time.temporal.ChronoField.SECOND_OF_MINUTE, 0 );
			  public static readonly TemporalFields Millisecond = new TemporalFields( "Millisecond", InnerEnum.Millisecond, java.time.temporal.ChronoField.MILLI_OF_SECOND, 0 );
			  public static readonly TemporalFields Microsecond = new TemporalFields( "Microsecond", InnerEnum.Microsecond, java.time.temporal.ChronoField.MICRO_OF_SECOND, 0 );
			  public static readonly TemporalFields Nanosecond = new TemporalFields( "Nanosecond", InnerEnum.Nanosecond, java.time.temporal.ChronoField.NANO_OF_SECOND, 0 );
			  // Read only accessors (not assignable)
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           weekYear(java.time.temporal.IsoFields.WEEK_BASED_YEAR, 0) { void assign(Builder<JavaToDotNetGenericWildcard> builder, org.neo4j.values.AnyValue value) { throw new org.neo4j.values.utils.UnsupportedTemporalUnitException("Not supported: " + name()); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           offset { void assign(Builder<JavaToDotNetGenericWildcard> builder, org.neo4j.values.AnyValue value) { throw new org.neo4j.values.utils.UnsupportedTemporalUnitException("Not supported: " + name()); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           offsetMinutes { void assign(Builder<JavaToDotNetGenericWildcard> builder, org.neo4j.values.AnyValue value) { throw new org.neo4j.values.utils.UnsupportedTemporalUnitException("Not supported: " + name()); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           offsetSeconds { void assign(Builder<JavaToDotNetGenericWildcard> builder, org.neo4j.values.AnyValue value) { throw new org.neo4j.values.utils.UnsupportedTemporalUnitException("Not supported: " + name()); } },
			  // time zone
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           timezone { void assign(Builder<JavaToDotNetGenericWildcard> builder, org.neo4j.values.AnyValue value) { if(!builder.supportsTimeZone()) { throw new org.neo4j.values.utils.UnsupportedTemporalUnitException("Cannot assign time zone if also assigning other fields."); } if(builder.timezone != null) { throw new org.neo4j.values.utils.InvalidValuesArgumentException("Cannot assign timezone twice."); } builder.timezone = value; } },
			  // group selectors
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           date { void assign(Builder<JavaToDotNetGenericWildcard> builder, org.neo4j.values.AnyValue value) { if(!builder.supportsDate()) { throw new org.neo4j.values.utils.UnsupportedTemporalUnitException("Not supported: " + name()); } if(builder.state == null) { builder.state = new DateTimeBuilder(); } builder.state = builder.state.assign(this, value); } boolean isGroupSelector() { return true; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           time { void assign(Builder<JavaToDotNetGenericWildcard> builder, org.neo4j.values.AnyValue value) { if(!builder.supportsTime()) { throw new org.neo4j.values.utils.UnsupportedTemporalUnitException("Not supported: " + name()); } if(builder.state == null) { builder.state = new DateTimeBuilder(); } builder.state = builder.state.assign(this, value); } boolean isGroupSelector() { return true; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           datetime { void assign(Builder<JavaToDotNetGenericWildcard> builder, org.neo4j.values.AnyValue value) { if(!builder.supportsDate() || !builder.supportsTime()) { throw new org.neo4j.values.utils.UnsupportedTemporalUnitException("Not supported: " + name()); } if(builder.state == null) { builder.state = new DateTimeBuilder(); } builder.state = builder.state.assign(this, value); } boolean isGroupSelector() { return true; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           epochSeconds { void assign(Builder<JavaToDotNetGenericWildcard> builder, org.neo4j.values.AnyValue value) { if(!builder.supportsEpoch()) { throw new org.neo4j.values.utils.UnsupportedTemporalUnitException("Not supported: " + name()); } if(builder.state == null) { builder.state = new DateTimeBuilder(); } builder.state = builder.state.assign(this, value); } boolean isGroupSelector() { return true; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           epochMillis { void assign(Builder<JavaToDotNetGenericWildcard> builder, org.neo4j.values.AnyValue value) { if(!builder.supportsEpoch()) { throw new org.neo4j.values.utils.UnsupportedTemporalUnitException("Not supported: " + name()); } if(builder.state == null) { builder.state = new DateTimeBuilder(); } builder.state = builder.state.assign(this, value); } boolean isGroupSelector() { return true; } };

			  private static readonly IList<TemporalFields> valueList = new List<TemporalFields>();

			  public enum InnerEnum
			  {
				  Year,
				  Quarter,
				  Month,
				  Week,
				  OrdinalDay,
				  DayOfQuarter,
				  DayOfWeek,
				  Day,
				  Hour,
				  Minute,
				  Second,
				  Millisecond,
				  Microsecond,
				  Nanosecond,
				  weekYear,
				  offset,
				  offsetMinutes,
				  offsetSeconds,
				  timezone,
				  date,
				  time,
				  datetime,
				  epochSeconds,
				  epochMillis
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;
			  internal static readonly IDictionary<string, TemporalFields> fields = new Dictionary<string, TemporalFields>();

			  static TemporalFields()
			  {
					foreach ( TemporalFields field in values() )
					{
						 _fields.put( field.name().ToLower(), field );
					}
					// aliases
					_fields.put( "weekday", dayOfWeek );
					_fields.put( "quarterday", dayOfQuarter );

				  valueList.Add( Year );
				  valueList.Add( Quarter );
				  valueList.Add( Month );
				  valueList.Add( Week );
				  valueList.Add( OrdinalDay );
				  valueList.Add( DayOfQuarter );
				  valueList.Add( DayOfWeek );
				  valueList.Add( Day );
				  valueList.Add( Hour );
				  valueList.Add( Minute );
				  valueList.Add( Second );
				  valueList.Add( Millisecond );
				  valueList.Add( Microsecond );
				  valueList.Add( Nanosecond );
				  valueList.Add( weekYear );
				  valueList.Add( offset );
				  valueList.Add( offsetMinutes );
				  valueList.Add( offsetSeconds );
				  valueList.Add( timezone );
				  valueList.Add( date );
				  valueList.Add( time );
				  valueList.Add( datetime );
				  valueList.Add( epochSeconds );
				  valueList.Add( epochMillis );
			  }

			  internal readonly java.time.temporal.TemporalField field;
			  internal readonly int defaultValue;

			  internal TemporalFields( string name, InnerEnum innerEnum, java.time.temporal.TemporalField field, int defaultValue )
			  {
					this.Field = field;
					this.DefaultValue = defaultValue;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal TemporalFields( string name, InnerEnum innerEnum )
			  {
					this.Field = null;
					this.DefaultValue = -1;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal bool GroupSelector
			  {
				  get
				  {
						return false;
				  }
			  }

			  internal void Assign<T1>( Builder<T1> builder, Neo4Net.Values.AnyValue value )
			  {
					Debug.Assert( Field != null, "method should have been overridden" );
					if ( !builder.Supports( Field ) )
					{
						 throw new UnsupportedTemporalUnitException( "Not supported: " + name() );
					}
					if ( builder.State == null )
					{
						 builder.State = new DateTimeBuilder();
					}
					builder.State = builder.State.assign( this, value );
			  }

			  public static ISet<string> AllFields()
			  {
					return _fields.Keys;
			  }

			 public static IList<TemporalFields> values()
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

			 public static TemporalFields valueOf( string name )
			 {
				 foreach ( TemporalFields enumInstance in TemporalFields.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private class DateTimeBuilder
		 {
			  protected internal DateBuilder Date;
			  protected internal ConstructTime Time;

			  internal DateTimeBuilder()
			  {
			  }

			  internal DateTimeBuilder( DateBuilder date, ConstructTime time )
			  {
					this.Date = date;
					this.Time = time;
			  }

			  internal virtual void CheckAssignments( bool requiresDate )
			  {
					if ( Date != null )
					{
						 Date.checkAssignments();
					}
					if ( Time != null )
					{
						 if ( requiresDate )
						 {
							  if ( Date != null )
							  {
									Date.assertFullyAssigned();
							  }
							  else
							  {
									throw new InvalidValuesArgumentException( TemporalFields.Year.name() + " must be specified" );
							  }
						 }
						 Time.checkAssignments();
					}
			  }

			  internal virtual DateTimeBuilder Assign( TemporalFields field, AnyValue value )
			  {
					if ( field == TemporalFields.Datetime || field == TemporalFields.EpochSeconds || field == TemporalFields.EpochMillis )
					{
						 return ( new SelectDateTimeDTBuilder( Date, Time ) ).assign( field, value );
					}
					else if ( field == TemporalFields.Time || field == TemporalFields.Date )
					{
						 return ( new SelectDateOrTimeDTBuilder( Date, Time ) ).assign( field, value );
					}
					else
					{
						 return AssignToSubBuilders( field, value );
					}
			  }

			  internal virtual DateTimeBuilder AssignToSubBuilders( TemporalFields field, AnyValue value )
			  {
					if ( field == TemporalFields.Date || field.field != null && field.field.DateBased )
					{
						 if ( Date == null )
						 {
							  Date = new ConstructDate();
						 }
						 Date = Date.assign( field, value );
					}
					else if ( field == TemporalFields.Time || field.field != null && field.field.TimeBased )
					{
						 if ( Time == null )
						 {
							  Time = new ConstructTime();
						 }
						 Time.assign( field, value );
					}
					else
					{
						 throw new System.InvalidOperationException( "This method should not be used for any fields the DateBuilder or TimeBuilder can't handle" );
					}
					return this;
			  }
		 }

		 private class SelectDateTimeDTBuilder : DateTimeBuilder
		 {
			  internal AnyValue Datetime;
			  internal AnyValue EpochSeconds;
			  internal AnyValue EpochMillis;

			  internal SelectDateTimeDTBuilder( DateBuilder date, ConstructTime time ) : base( date, time )
			  {
			  }

			  internal override void CheckAssignments( bool requiresDate )
			  {
					// Nothing to do
			  }

			  internal override DateTimeBuilder Assign( TemporalFields field, AnyValue value )
			  {
					if ( field == TemporalFields.Date || field == TemporalFields.Time )
					{
						 throw new InvalidValuesArgumentException( field.name() + " cannot be selected together with datetime or epochSeconds or epochMillis." );
					}
					else if ( field == TemporalFields.Datetime )
					{
						 if ( EpochSeconds != null )
						 {
							  throw new InvalidValuesArgumentException( field.name() + " cannot be selected together with epochSeconds." );
						 }
						 else if ( EpochMillis != null )
						 {
							  throw new InvalidValuesArgumentException( field.name() + " cannot be selected together with epochMillis." );
						 }
						 Datetime = Assignment( TemporalFields.Datetime, Datetime, value );
					}
					else if ( field == TemporalFields.EpochSeconds )
					{
						 if ( EpochMillis != null )
						 {
							  throw new InvalidValuesArgumentException( field.name() + " cannot be selected together with epochMillis." );
						 }
						 else if ( Datetime != null )
						 {
							  throw new InvalidValuesArgumentException( field.name() + " cannot be selected together with datetime." );
						 }
						 EpochSeconds = Assignment( TemporalFields.EpochSeconds, EpochSeconds, value );
					}
					else if ( field == TemporalFields.EpochMillis )
					{
						 if ( EpochSeconds != null )
						 {
							  throw new InvalidValuesArgumentException( field.name() + " cannot be selected together with epochSeconds." );
						 }
						 else if ( Datetime != null )
						 {
							  throw new InvalidValuesArgumentException( field.name() + " cannot be selected together with datetime." );
						 }
						 EpochMillis = Assignment( TemporalFields.EpochMillis, EpochMillis, value );
					}
					else
					{
						 return AssignToSubBuilders( field, value );
					}
					return this;
			  }
		 }

		 private class SelectDateOrTimeDTBuilder : DateTimeBuilder
		 {
			  internal SelectDateOrTimeDTBuilder( DateBuilder date, ConstructTime time ) : base( date, time )
			  {
			  }

			  internal override DateTimeBuilder Assign( TemporalFields field, AnyValue value )
			  {
					if ( field == TemporalFields.Datetime || field == TemporalFields.EpochSeconds || field == TemporalFields.EpochMillis )
					{
						 throw new InvalidValuesArgumentException( field.name() + " cannot be selected together with date or time." );
					}
					else
					{
						 return AssignToSubBuilders( field, value );
					}
			  }
		 }

		 private abstract class DateBuilder
		 {
			  internal abstract DateBuilder Assign( TemporalFields field, AnyValue value );

			  internal abstract void CheckAssignments();

			  internal abstract void AssertFullyAssigned();
		 }

		 private sealed class ConstructTime
		 {
			  internal AnyValue Hour;
			  internal AnyValue Minute;
			  internal AnyValue Second;
			  internal AnyValue Millisecond;
			  internal AnyValue Microsecond;
			  internal AnyValue Nanosecond;
			  internal AnyValue Time;

			  internal ConstructTime()
			  {
			  }

			  internal void Assign( TemporalFields field, AnyValue value )
			  {
					switch ( field.innerEnumValue )
					{
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.hour:
						 Hour = Assignment( field, Hour, value );
						 break;
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.minute:
						 Minute = Assignment( field, Minute, value );
						 break;
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.second:
						 Second = Assignment( field, Second, value );
						 break;
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.millisecond:
						 Millisecond = Assignment( field, Millisecond, value );
						 break;
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.microsecond:
						 Microsecond = Assignment( field, Microsecond, value );
						 break;
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.nanosecond:
						 Nanosecond = Assignment( field, Nanosecond, value );
						 break;
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.time:
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.datetime:
						 Time = Assignment( field, Time, value );
						 break;
					default:
						 throw new System.InvalidOperationException( "Not a time field: " + field );
					}
			  }

			  internal void CheckAssignments()
			  {
					if ( Time == null )
					{
						 AssertDefinedInOrder( Pair.of( Hour, "hour" ), Pair.of( Minute, "minute" ), Pair.of( Second, "second" ), Pair.of( OneOf( Millisecond, Microsecond, Nanosecond ), "subsecond" ) );
					}
			  }
		 }

		 private class ConstructDate : DateBuilder
		 {
			  internal AnyValue Year;
			  internal AnyValue Date;

			  internal ConstructDate()
			  {
			  }

			  internal ConstructDate( AnyValue date )
			  {
					this.Date = date;
			  }

			  internal override ConstructDate Assign( TemporalFields field, AnyValue value )
			  {
					switch ( field.innerEnumValue )
					{
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.year:
						 Year = Assignment( field, Year, value );
						 return this;
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.quarter:
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.dayOfQuarter:
						 return ( new QuarterDate( Year, Date ) ).assign( field, value );
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.month:
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.day:
						 return ( new CalendarDate( Year, Date ) ).assign( field, value );
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.week:
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.dayOfWeek:
						 return ( new WeekDate( Year, Date ) ).assign( field, value );
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.ordinalDay:
						 return ( new OrdinalDate( Year, Date ) ).assign( field, value );
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.date:
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.datetime:
						 Date = Assignment( field, Date, value );
						 return this;
					default:
						 throw new System.InvalidOperationException( "Not a date field: " + field );
					}
			  }

			  internal override void CheckAssignments()
			  {
					// Nothing to do
			  }

			  internal override void AssertFullyAssigned()
			  {
					if ( Date == null )
					{
						 throw new InvalidValuesArgumentException( TemporalFields.Month.name() + " must be specified" );
					}
			  }
		 }

		 private sealed class CalendarDate : ConstructDate
		 {
			  internal AnyValue Month;
			  internal AnyValue Day;

			  internal CalendarDate( AnyValue year, AnyValue date )
			  {
					this.Year = year;
					this.Date = date;
			  }

			  internal override ConstructDate Assign( TemporalFields field, AnyValue value )
			  {
					switch ( field.innerEnumValue )
					{
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.year:
						 Year = Assignment( field, Year, value );
						 return this;
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.month:
						 Month = Assignment( field, Month, value );
						 return this;
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.day:
						 Day = Assignment( field, Day, value );
						 return this;
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.date:
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.datetime:
						 Date = Assignment( field, Date, value );
						 return this;
					default:
						 throw new UnsupportedTemporalUnitException( "Cannot assign " + field + " to calendar date." );
					}
			  }

			  internal override void CheckAssignments()
			  {
					if ( Date == null )
					{
						 AssertDefinedInOrder( Pair.of( Year, "year" ), Pair.of( Month, "month" ), Pair.of( Day, "day" ) );
					}
			  }

			  internal override void AssertFullyAssigned()
			  {
					if ( Date == null )
					{
						 AssertAllDefined( Pair.of( Year, "year" ), Pair.of( Month, "month" ), Pair.of( Day, "day" ) );
					}
			  }
		 }

		 private sealed class WeekDate : ConstructDate
		 {
			  internal AnyValue Week;
			  internal AnyValue DayOfWeek;

			  internal WeekDate( AnyValue year, AnyValue date )
			  {
					this.Year = year;
					this.Date = date;
			  }

			  internal override ConstructDate Assign( TemporalFields field, AnyValue value )
			  {
					switch ( field.innerEnumValue )
					{
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.year:
						 Year = Assignment( field, Year, value );
						 return this;
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.week:
						 Week = Assignment( field, Week, value );
						 return this;
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.dayOfWeek:
						 DayOfWeek = Assignment( field, DayOfWeek, value );
						 return this;
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.date:
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.datetime:
						 Date = Assignment( field, Date, value );
						 return this;
					default:
						 throw new UnsupportedTemporalUnitException( "Cannot assign " + field + " to week date." );
					}
			  }

			  internal override void CheckAssignments()
			  {
					if ( Date == null )
					{
						 AssertDefinedInOrder( Pair.of( Year, "year" ), Pair.of( Week, "week" ), Pair.of( DayOfWeek, "dayOfWeek" ) );
					}
			  }

			  internal override void AssertFullyAssigned()
			  {
					if ( Date == null )
					{
						 AssertAllDefined( Pair.of( Year, "year" ), Pair.of( Week, "week" ), Pair.of( DayOfWeek, "dayOfWeek" ) );
					}
			  }
		 }

		 private sealed class QuarterDate : ConstructDate
		 {
			  internal AnyValue Quarter;
			  internal AnyValue DayOfQuarter;

			  internal QuarterDate( AnyValue year, AnyValue date )
			  {
					this.Year = year;
					this.Date = date;
			  }

			  internal override ConstructDate Assign( TemporalFields field, AnyValue value )
			  {
					switch ( field.innerEnumValue )
					{
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.year:
						 Year = Assignment( field, Year, value );
						 return this;
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.quarter:
						 Quarter = Assignment( field, Quarter, value );
						 return this;
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.dayOfQuarter:
						 DayOfQuarter = Assignment( field, DayOfQuarter, value );
						 return this;
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.date:
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.datetime:
						 Date = Assignment( field, Date, value );
						 return this;
					default:
						 throw new UnsupportedTemporalUnitException( "Cannot assign " + field + " to quarter date." );
					}
			  }

			  internal override void CheckAssignments()
			  {
					if ( Date == null )
					{
						 AssertDefinedInOrder( Pair.of( Year, "year" ), Pair.of( Quarter, "quarter" ), Pair.of( DayOfQuarter, "dayOfQuarter" ) );
					}
			  }

			  internal override void AssertFullyAssigned()
			  {
					if ( Date == null )
					{
						 AssertAllDefined( Pair.of( Year, "year" ), Pair.of( Quarter, "quarter" ), Pair.of( DayOfQuarter, "dayOfQuarter" ) );
					}
			  }
		 }

		 private sealed class OrdinalDate : ConstructDate
		 {
			  internal AnyValue OrdinalDay;

			  internal OrdinalDate( AnyValue year, AnyValue date )
			  {
					this.Year = year;
					this.Date = date;
			  }

			  internal override ConstructDate Assign( TemporalFields field, AnyValue value )
			  {
					switch ( field.innerEnumValue )
					{
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.year:
						 Year = Assignment( field, Year, value );
						 return this;
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.ordinalDay:
						 OrdinalDay = Assignment( field, OrdinalDay, value );
						 return this;
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.date:
					case Neo4Net.Values.Storable.TemporalValue.TemporalFields.InnerEnum.datetime:
						 Date = Assignment( field, Date, value );
						 return this;
					default:
						 throw new UnsupportedTemporalUnitException( "Cannot assign " + field + " to ordinal date." );
					}
			  }

			  internal override void AssertFullyAssigned()
			  {
					if ( Date == null )
					{
						 AssertAllDefined( Pair.of( Year, "year" ), Pair.of( OrdinalDay, "ordinalDay" ) );
					}
			  }
		 }

		 private static AnyValue Assignment( TemporalFields field, AnyValue oldValue, AnyValue newValue )
		 {
			  if ( oldValue != null )
			  {
					throw new InvalidValuesArgumentException( "cannot re-assign " + field );
			  }
			  return newValue;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs static void assertDefinedInOrder(org.neo4j.helpers.collection.Pair<org.neo4j.values.AnyValue, String>... values)
		 internal static void AssertDefinedInOrder( params Pair<AnyValue, string>[] values )
		 {
			  if ( values[0].First() == null )
			  {
					throw new InvalidValuesArgumentException( values[0].Other() + " must be specified" );
			  }

			  string firstNotAssigned = null;

			  foreach ( Pair<AnyValue, string> value in values )
			  {
					if ( value.First() == null )
					{
						 if ( string.ReferenceEquals( firstNotAssigned, null ) )
						 {
							  firstNotAssigned = value.Other();
						 }
					}
					else if ( !string.ReferenceEquals( firstNotAssigned, null ) )
					{
						 throw new InvalidValuesArgumentException( value.Other() + " cannot be specified without " + firstNotAssigned );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs static void assertAllDefined(org.neo4j.helpers.collection.Pair<org.neo4j.values.AnyValue, String>... values)
		 internal static void AssertAllDefined( params Pair<AnyValue, string>[] values )
		 {
			  foreach ( Pair<AnyValue, string> value in values )
			  {
					if ( value.First() == null )
					{
						 throw new InvalidValuesArgumentException( value.Other() + " must be specified" );
					}
			  }
		 }

		 internal static AnyValue OneOf( AnyValue a, AnyValue b, AnyValue c )
		 {
			  return a != null ? a : b != null ? b : c;
		 }

		 internal static ZoneId TimezoneOf( AnyValue timezone )
		 {
			  if ( timezone is TextValue )
			  {
					return parseZoneName( ( ( TextValue ) timezone ).StringValue() );
			  }
			  throw new System.NotSupportedException( "Cannot convert to ZoneId: " + timezone );
		 }

		 internal static int ValidNano( AnyValue millisecond, AnyValue microsecond, AnyValue nanosecond )
		 {
			  long ms = safeCastIntegral( "millisecond", millisecond, TemporalFields.Millisecond.defaultValue );
			  long us = safeCastIntegral( "microsecond", microsecond, TemporalFields.Microsecond.defaultValue );
			  long ns = safeCastIntegral( "nanosecond", nanosecond, TemporalFields.Nanosecond.defaultValue );
			  if ( ms < 0 || ms >= 1000 )
			  {
					throw new InvalidValuesArgumentException( "Invalid value for Millisecond: " + ms );
			  }
			  if ( us < 0 || us >= ( millisecond != null ? 1000 : 1000_000 ) )
			  {
					throw new InvalidValuesArgumentException( "Invalid value for Microsecond: " + us );
			  }
			  if ( ns < 0 || ns >= ( microsecond != null ? 1000 : millisecond != null ? 1000_000 : 1000_000_000 ) )
			  {
					throw new InvalidValuesArgumentException( "Invalid value for Nanosecond: " + ns );
			  }
			  return ( int )( ms * 1000_000 + us * 1000 + ns );
		 }

		 internal static VALUE UpdateFieldMapWithConflictingSubseconds<TEMP, VALUE>( MapValue fields, TemporalUnit unit, TEMP temporal, System.Func<MapValue, TEMP, VALUE> mapFunction ) where TEMP : java.time.temporal.Temporal
		 {
			  bool conflictingMilliSeconds = unit == ChronoUnit.MILLIS && ( fields.ContainsKey( "microsecond" ) || fields.ContainsKey( "nanosecond" ) );
			  bool conflictingMicroSeconds = unit == ChronoUnit.MICROS && fields.ContainsKey( "nanosecond" );

			  if ( conflictingMilliSeconds )
			  {
					AnyValue millis = Values.IntValue( temporal.get( ChronoField.MILLI_OF_SECOND ) );
					AnyValue micros = fields.Get( "microsecond" );
					AnyValue nanos = fields.Get( "nanosecond" );

					int newNanos = ValidNano( millis, micros, nanos );
					TEMP newTemporal = ( TEMP ) temporal.with( ChronoField.NANO_OF_SECOND, newNanos );
					MapValue filtered = fields.Filter( ( k, ignore ) => !k.Equals( "microsecond" ) && !k.Equals( "nanosecond" ) );
					return mapFunction( filtered, newTemporal );
			  }
			  else if ( conflictingMicroSeconds )
			  {
					AnyValue micros = Values.IntValue( temporal.get( ChronoField.MICRO_OF_SECOND ) );
					AnyValue nanos = fields.Get( "nanosecond" );
					int newNanos = ValidNano( null, micros, nanos );
					TEMP newTemporal = ( TEMP ) temporal.with( ChronoField.NANO_OF_SECOND, newNanos );
					MapValue filtered = fields.Filter( ( k, ignore ) => !k.Equals( "nanosecond" ) );

					return mapFunction( filtered, newTemporal );
			  }
			  else
			  {
					return mapFunction( fields, temporal );
			  }
		 }

		 internal static TEMP AssertValidArgument<TEMP>( System.Func<TEMP> func ) where TEMP : java.time.temporal.Temporal
		 {
			  try
			  {
					return func();
			  }
			  catch ( DateTimeException e )
			  {
					throw new InvalidValuesArgumentException( e.Message, e );
			  }
		 }

		 internal static TEMP AssertValidUnit<TEMP>( System.Func<TEMP> func ) where TEMP : java.time.temporal.Temporal
		 {
			  try
			  {
					return func();
			  }
			  catch ( DateTimeException e )
			  {
					throw new UnsupportedTemporalUnitException( e.Message, e );
			  }
		 }

		 internal static OFFSET AssertValidZone<OFFSET>( System.Func<OFFSET> func ) where OFFSET : java.time.ZoneId
		 {
			  try
			  {
					return func();
			  }
			  catch ( DateTimeException e )
			  {
					throw new InvalidValuesArgumentException( e.Message, e );
			  }
		 }

		 internal static TEMP AssertParsable<TEMP>( System.Func<TEMP> func ) where TEMP : java.time.temporal.Temporal
		 {
			  try
			  {
					return func();
			  }
			  catch ( DateTimeException e )
			  {
					throw new TemporalParseException( e.Message, e );
			  }
		 }

		 internal static string AssertPrintable( System.Func<string> func )
		 {
			  try
			  {
					return func();
			  }
			  catch ( DateTimeException e )
			  {
					throw new TemporalParseException( e.Message, e );
			  }
		 }

		 internal static TEMP AssertValidArithmetic<TEMP>( System.Func<TEMP> func ) where TEMP : java.time.temporal.Temporal
		 {
			  try
			  {
					return func();
			  }
			  catch ( Exception e ) when ( e is DateTimeException || e is ArithmeticException )
			  {
					throw new TemporalArithmeticException( e.Message, e );
			  }
		 }

		 internal static Pair<LocalDate, LocalTime> GetTruncatedDateAndTime( TemporalUnit unit, TemporalValue input, string type )
		 {
			  if ( unit.TimeBased && !( input is DateTimeValue || input is LocalDateTimeValue ) )
			  {
					throw new UnsupportedTemporalUnitException( string.Format( "Cannot truncate {0} to {1} with a time based unit.", input, type ) );
			  }
			  LocalDate localDate = input.DatePart;
			  LocalTime localTime = input.HasTime() ? input.LocalTimePart : LocalTimeValue.DefaultLocalTime;

			  LocalTime truncatedTime;
			  LocalDate truncatedDate;
			  if ( unit.DateBased )
			  {
					truncatedDate = DateValue.TruncateTo( localDate, unit );
					truncatedTime = LocalTimeValue.DefaultLocalTime;
			  }
			  else
			  {
					truncatedDate = localDate;
					truncatedTime = localTime.truncatedTo( unit );
			  }
			  return Pair.of( truncatedDate, truncatedTime );
		 }

		 internal class TimeCSVHeaderInformation : CSVHeaderInformation
		 {
			  internal string Timezone;

			  public override void Assign( string key, object valueObj )
			  {
					if ( !( valueObj is string ) )
					{
						 throw new InvalidValuesArgumentException( string.Format( "Cannot assign {0} to field {1}", valueObj, key ) );
					}
					string value = ( string ) valueObj;
					if ( "timezone".Equals( key.ToLower() ) )
					{
						 if ( string.ReferenceEquals( Timezone, null ) )
						 {
							  Timezone = value;
						 }
						 else
						 {
							  throw new InvalidValuesArgumentException( "Cannot set timezone twice" );
						 }
					}
					else
					{
						 throw new InvalidValuesArgumentException( "Unsupported header field: " + value );
					}
			  }

			  internal virtual System.Func<ZoneId> ZoneSupplier( System.Func<ZoneId> defaultSupplier )
			  {
					if ( !string.ReferenceEquals( Timezone, null ) )
					{
						 ZoneId tz = DateTimeValue.ParseZoneName( Timezone );
						 // Override defaultZone
						 return () => tz;
					}
					return defaultSupplier;
			  }
		 }
	}

}