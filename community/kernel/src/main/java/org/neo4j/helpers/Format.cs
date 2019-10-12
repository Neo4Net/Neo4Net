using System;
using System.Text;

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
namespace Org.Neo4j.Helpers
{

	using ByteUnit = Org.Neo4j.Io.ByteUnit;

	[Obsolete]
	public class Format
	{
		 /// <summary>
		 /// Default time zone is UTC (+00:00) so that comparing timestamped logs from different
		 /// sources is an easier task.
		 /// </summary>
		 public static readonly TimeZone DefaultTimeZone = TimeZone.getTimeZone( "UTC" );

		 private static readonly string[] _byteSizes = new string[] { "B", "kB", "MB", "GB" };
		 private static readonly string[] _countSizes = new string[] { "", "k", "M", "G", "T" };

		 public const string DATE_FORMAT = "yyyy-MM-dd HH:mm:ss.SSSZ";
		 public const string TIME_FORMAT = "HH:mm:ss.SSS";

		 private static readonly ThreadLocalFormat _date = new ThreadLocalFormat( DATE_FORMAT );
		 private static readonly ThreadLocalFormat _time = new ThreadLocalFormat( TIME_FORMAT );
		 private static int _kb = ( int ) ByteUnit.kibiBytes( 1 );

		 public static string Date()
		 {
			  return Date( DefaultTimeZone );
		 }

		 public static string Date( TimeZone timeZone )
		 {
			  return date( DateTime.Now, timeZone );
		 }

		 public static string Date( long millis )
		 {
			  return Date( millis, DefaultTimeZone );
		 }

		 public static string Date( long millis, TimeZone timeZone )
		 {
			  return Date( new DateTime( millis ), timeZone );
		 }

		 public static string Date( DateTime date )
		 {
			  return date( date, DefaultTimeZone );
		 }

		 public static string Date( DateTime date, TimeZone timeZone )
		 {
			  return _date.format( date, timeZone );
		 }

		 public static string Time()
		 {
			  return Time( DefaultTimeZone );
		 }

		 public static string Time( TimeZone timeZone )
		 {
			  return time( DateTime.Now, timeZone );
		 }

		 public static string Time( long millis )
		 {
			  return Time( millis, DefaultTimeZone );
		 }

		 public static string Time( long millis, TimeZone timeZone )
		 {
			  return Time( new DateTime( millis ), timeZone );
		 }

		 public static string Time( DateTime date )
		 {
			  return Time( date, DefaultTimeZone );
		 }

		 public static string Time( DateTime date, TimeZone timeZone )
		 {
			  return _time.format( date, timeZone );
		 }

		 public static string Bytes( long bytes )
		 {
			  return SuffixCount( bytes, _byteSizes, _kb );
		 }

		 public static string Count( long count )
		 {
			  return SuffixCount( count, _countSizes, 1_000 );
		 }

		 private static string SuffixCount( long value, string[] sizes, int stride )
		 {
			  double size = value;
			  foreach ( string suffix in sizes )
			  {
					if ( size < stride )
					{
						 return string.format( Locale.ROOT, "%.2f %s", size, suffix );
					}
					size /= stride;
			  }
			  return string.format( Locale.ROOT, "%.2f TB", size );
		 }

		 public static string Duration( long durationMillis )
		 {
			  return Duration( durationMillis, TimeUnit.DAYS, TimeUnit.MILLISECONDS );
		 }

		 public static string Duration( long durationMillis, TimeUnit highestGranularity, TimeUnit lowestGranularity )
		 {
			  StringBuilder builder = new StringBuilder();

			  TimeUnit[] units = TimeUnit.values();
			  Reverse( units );
			  bool use = false;
			  foreach ( TimeUnit unit in units )
			  {
					if ( unit == highestGranularity )
					{
						 use = true;
					}

					if ( use )
					{
						 durationMillis = ExtractFromDuration( durationMillis, unit, builder );
						 if ( unit == lowestGranularity )
						 {
							  break;
						 }
					}
			  }

			  if ( builder.Length == 0 )
			  {
					// The value is too low to extract any meaningful numbers with the given unit brackets.
					// So we append a zero of the lowest unit.
					builder.Append( '0' ).Append( ShortName( lowestGranularity ) );
			  }

			  return builder.ToString();
		 }

		 private static void Reverse<T>( T[] array )
		 {
			  int half = array.Length >> 1;
			  for ( int i = 0; i < half; i++ )
			  {
					T temp = array[i];
					int highIndex = array.Length - 1 - i;
					array[i] = array[highIndex];
					array[highIndex] = temp;
			  }
		 }

		 private static string ShortName( TimeUnit unit )
		 {
			  switch ( unit )
			  {
			  case NANOSECONDS:
				  return "ns";
			  case MICROSECONDS:
				  return "μs";
			  case MILLISECONDS:
				  return "ms";
			  default:
				  return unit.name().substring(0, 1).ToLower();
			  }
		 }

		 private static long ExtractFromDuration( long durationMillis, TimeUnit unit, StringBuilder target )
		 {
			  int count = 0;
			  long millisPerUnit = unit.toMillis( 1 );
			  while ( durationMillis >= millisPerUnit )
			  {
					count++;
					durationMillis -= millisPerUnit;
			  }
			  if ( count > 0 )
			  {
					target.Append( target.Length > 0 ? " " : "" ).Append( count ).Append( ShortName( unit ) );
			  }
			  return durationMillis;
		 }

		 private Format()
		 {
			  // No instances
		 }

		 private class ThreadLocalFormat : ThreadLocal<DateFormat>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string FormatConflict;

			  internal ThreadLocalFormat( string format )
			  {
					this.FormatConflict = format;
			  }

			  internal virtual string Format( DateTime date, TimeZone timeZone )
			  {
					DateFormat dateFormat = get();
					dateFormat.TimeZone = timeZone;
					return dateFormat.format( date );
			  }

			  protected internal override DateFormat InitialValue()
			  {
					return new SimpleDateFormat( FormatConflict );
			  }
		 }
	}

}