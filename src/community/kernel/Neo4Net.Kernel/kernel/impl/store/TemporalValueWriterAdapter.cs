using System;

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
namespace Neo4Net.Kernel.impl.store
{

	using Neo4Net.Values.Storable;
	using TemporalUtil = Neo4Net.Values.utils.TemporalUtil;

	/// <summary>
	/// A <seealso cref="ValueWriter"/> that defines format for all temporal types, except duration.
	/// Subclasses will not be able to override methods like <seealso cref="writeDate(LocalDate)"/>. They should instead override <seealso cref="writeDate(long)"/> that
	/// defines how <seealso cref="LocalDate"/> is serialized.
	/// <para>
	/// Primary purpose of this class is to share serialization format between property store writer and schema indexes.
	/// 
	/// </para>
	/// </summary>
	/// @param <E> the error type. </param>
	public abstract class TemporalValueWriterAdapter<E> : Neo4Net.Values.Storable.ValueWriter_Adapter<E> where E : Exception
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final void writeDate(java.time.LocalDate localDate) throws E
		 public override void WriteDate( LocalDate localDate )
		 {
			  writeDate( localDate.toEpochDay() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final void writeLocalTime(java.time.LocalTime localTime) throws E
		 public override void WriteLocalTime( LocalTime localTime )
		 {
			  writeLocalTime( localTime.toNanoOfDay() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final void writeTime(java.time.OffsetTime offsetTime) throws E
		 public override void WriteTime( OffsetTime offsetTime )
		 {
			  long nanosOfDayUTC = TemporalUtil.getNanosOfDayUTC( offsetTime );
			  int offsetSeconds = offsetTime.Offset.TotalSeconds;
			  WriteTime( nanosOfDayUTC, offsetSeconds );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final void writeLocalDateTime(java.time.LocalDateTime localDateTime) throws E
		 public override void WriteLocalDateTime( DateTime localDateTime )
		 {
			  long epochSecond = localDateTime.toEpochSecond( UTC );
			  int nano = localDateTime.Nano;
			  WriteLocalDateTime( epochSecond, nano );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final void writeDateTime(java.time.ZonedDateTime zonedDateTime) throws E
		 public override void WriteDateTime( ZonedDateTime zonedDateTime )
		 {
			  long epochSecondUTC = zonedDateTime.toEpochSecond();
			  int nano = zonedDateTime.Nano;

			  ZoneId zone = zonedDateTime.Zone;
			  if ( zone is ZoneOffset )
			  {
					int offsetSeconds = ( ( ZoneOffset ) zone ).TotalSeconds;
					WriteDateTime( epochSecondUTC, nano, offsetSeconds );
			  }
			  else
			  {
					string zoneId = zone.Id;
					WriteDateTime( epochSecondUTC, nano, zoneId );
			  }
		 }

		 /// <summary>
		 /// Write date value obtained from <seealso cref="LocalDate"/> in <seealso cref="writeDate(LocalDate)"/>.
		 /// </summary>
		 /// <param name="epochDay"> the epoch day. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void writeDate(long epochDay) throws E
		 protected internal virtual void WriteDate( long epochDay )
		 {
		 }

		 /// <summary>
		 /// Write local time value obtained from <seealso cref="LocalTime"/> in <seealso cref="writeLocalTime(LocalTime)"/>.
		 /// </summary>
		 /// <param name="nanoOfDay"> the nanosecond of the day. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void writeLocalTime(long nanoOfDay) throws E
		 protected internal virtual void WriteLocalTime( long nanoOfDay )
		 {
		 }

		 /// <summary>
		 /// Write time value obtained from <seealso cref="OffsetTime"/> in <seealso cref="writeTime(OffsetTime)"/>.
		 /// </summary>
		 /// <param name="nanosOfDayUTC"> nanoseconds of day in UTC. will be between -18h and +42h </param>
		 /// <param name="offsetSeconds"> time zone offset in seconds </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void writeTime(long nanosOfDayUTC, int offsetSeconds) throws E
		 protected internal virtual void WriteTime( long nanosOfDayUTC, int offsetSeconds )
		 {
		 }

		 /// <summary>
		 /// Write local date-time value obtained from <seealso cref="System.DateTime"/> in <seealso cref="writeLocalDateTime(System.DateTime)"/>.
		 /// </summary>
		 /// <param name="epochSecond"> the epoch second in UTC. </param>
		 /// <param name="nano"> the nanosecond. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void writeLocalDateTime(long epochSecond, int nano) throws E
		 protected internal virtual void WriteLocalDateTime( long epochSecond, int nano )
		 {
		 }

		 /// <summary>
		 /// Write zoned date-time value obtained from <seealso cref="ZonedDateTime"/> in <seealso cref="writeDateTime(ZonedDateTime)"/>.
		 /// </summary>
		 /// <param name="epochSecondUTC"> the epoch second in UTC (no offset). </param>
		 /// <param name="nano"> the nanosecond. </param>
		 /// <param name="offsetSeconds"> the offset in seconds. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void writeDateTime(long epochSecondUTC, int nano, int offsetSeconds) throws E
		 protected internal virtual void WriteDateTime( long epochSecondUTC, int nano, int offsetSeconds )
		 {
		 }

		 /// <summary>
		 /// Write zoned date-time value obtained from <seealso cref="ZonedDateTime"/> in <seealso cref="writeDateTime(ZonedDateTime)"/>.
		 /// </summary>
		 /// <param name="epochSecondUTC"> the epoch second in UTC (no offset). </param>
		 /// <param name="nano"> the nanosecond. </param>
		 /// <param name="zoneId"> the timezone id. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void writeDateTime(long epochSecondUTC, int nano, String zoneId) throws E
		 protected internal virtual void WriteDateTime( long epochSecondUTC, int nano, string zoneId )
		 {
		 }
	}

}