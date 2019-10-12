using System;

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
namespace Neo4Net.Server.rest.repr.util
{

	public sealed class RFC1123
	{
		 private static readonly ThreadLocal<RFC1123> _instances = new ThreadLocal<RFC1123>();

		 public static readonly TimeZone Gmt = TimeZone.getTimeZone( "GMT" );

		 private static readonly DateTime _y2kStartDate;

		 static RFC1123()
		 {
			  DateTime calendar = new DateTime();
			  calendar.TimeZone = Gmt;
			  calendar = new DateTime( 2000, 1, 1, 0, 0, 0 );
			  calendar.set( DateTime.MILLISECOND, 0 );
			  _y2kStartDate = calendar;
		 }

		 private readonly SimpleDateFormat _format;

		 private RFC1123()
		 {
			  _format = new SimpleDateFormat( "EEE, dd MMM yyyy HH:mm:ss Z", Locale.US );
			  _format.TimeZone = Gmt;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Date parse(String input) throws java.text.ParseException
		 public DateTime Parse( string input )
		 {
			  _format.set2DigitYearStart( _y2kStartDate );
			  return _format.parse( input );
		 }

		 public string Format( DateTime date )
		 {
			  if ( null == date )
			  {
					throw new System.ArgumentException( "Date is null" );
			  }

			  return _format.format( date );
		 }

		 internal static RFC1123 Instance()
		 {
			  RFC1123 instance = _instances.get();
			  if ( null == instance )
			  {
					instance = new RFC1123();
					_instances.set( instance );
			  }
			  return instance;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.util.Date parseTimestamp(String input) throws java.text.ParseException
		 public static DateTime ParseTimestamp( string input )
		 {
			  return Instance().Parse(input);
		 }

		 public static string FormatDate( DateTime date )
		 {
			  return Instance().Format(date);
		 }
	}

}