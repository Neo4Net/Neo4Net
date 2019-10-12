using System;
using System.Diagnostics;
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


	[Obsolete]
	public sealed class TimeUtil
	{
		 [Obsolete]
		 public static readonly TimeUnit DefaultTimeUnit = TimeUnit.SECONDS;

		 [Obsolete]
		 public const string VALID_TIME_DESCRIPTION = "Valid units are: 'ms', 's', 'm' and 'h'; default unit is 's'";

		 [Obsolete]
		 public static readonly System.Func<string, long> ParseTimeMillis = timeWithOrWithoutUnit =>
		 {
		  int unitIndex = -1;
		  for ( int i = 0; i < timeWithOrWithoutUnit.length(); i++ )
		  {
				char ch = timeWithOrWithoutUnit.charAt( i );
				if ( !char.IsDigit( ch ) )
				{
					 unitIndex = i;
					 break;
				}
		  }
		  if ( unitIndex == -1 )
		  {
				return DefaultTimeUnit.toMillis( int.Parse( timeWithOrWithoutUnit ) );
		  }

		  string unit = timeWithOrWithoutUnit.substring( unitIndex ).ToLower();
		  if ( unitIndex == 0 )
		  {
				throw new System.ArgumentException( "Missing numeric value" );
		  }

		  // We have digits
		  int amount = int.Parse( timeWithOrWithoutUnit.substring( 0, unitIndex ) );
		  switch ( unit )
		  {
		  case "ms":
				return TimeUnit.MILLISECONDS.toMillis( amount );
		  case "s":
				return TimeUnit.SECONDS.toMillis( amount );
		  case "m":
				return TimeUnit.MINUTES.toMillis( amount );
		  case "h":
				return TimeUnit.HOURS.toMillis( amount );
		  default:
				throw new System.ArgumentException( "Unrecognized unit '" + unit + "'. " + VALID_TIME_DESCRIPTION );
		  }
		 };

		 [Obsolete]
		 public static string NanosToString( long nanos )
		 {
			  Debug.Assert( nanos >= 0 );
			  long nanoSeconds = nanos;
			  StringBuilder timeString = new StringBuilder();

			  long days = DAYS.convert( nanoSeconds, NANOSECONDS );
			  if ( days > 0 )
			  {
					nanoSeconds -= DAYS.toNanos( days );
					timeString.Append( days ).Append( 'd' );
			  }
			  long hours = HOURS.convert( nanoSeconds, NANOSECONDS );
			  if ( hours > 0 )
			  {
					nanoSeconds -= HOURS.toNanos( hours );
					timeString.Append( hours ).Append( 'h' );
			  }
			  long minutes = MINUTES.convert( nanoSeconds, NANOSECONDS );
			  if ( minutes > 0 )
			  {
					nanoSeconds -= MINUTES.toNanos( minutes );
					timeString.Append( minutes ).Append( 'm' );
			  }
			  long seconds = SECONDS.convert( nanoSeconds, NANOSECONDS );
			  if ( seconds > 0 )
			  {
					nanoSeconds -= SECONDS.toNanos( seconds );
					timeString.Append( seconds ).Append( 's' );
			  }
			  long milliseconds = MILLISECONDS.convert( nanoSeconds, NANOSECONDS );
			  if ( milliseconds > 0 )
			  {
					nanoSeconds -= MILLISECONDS.toNanos( milliseconds );
					timeString.Append( milliseconds ).Append( "ms" );
			  }
			  long microseconds = MICROSECONDS.convert( nanoSeconds, NANOSECONDS );
			  if ( microseconds > 0 )
			  {
					nanoSeconds -= MICROSECONDS.toNanos( microseconds );
					timeString.Append( microseconds ).Append( "μs" );
			  }
			  if ( nanoSeconds > 0 || timeString.Length == 0 )
			  {
					timeString.Append( nanoSeconds ).Append( "ns" );
			  }
			  return timeString.ToString();
		 }

		 private TimeUtil()
		 {
			  throw new AssertionError(); // no instances
		 }
	}

}