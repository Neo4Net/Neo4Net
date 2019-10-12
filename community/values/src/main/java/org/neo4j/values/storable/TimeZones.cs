using System;
using System.Collections.Generic;
using System.IO;

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

	public class TimeZones
	{
		 /// <summary>
		 /// Prevent instance creation.
		 /// </summary>
		 private TimeZones()
		 {
		 }

		 private static readonly IList<string> _timeZoneShortToString = new List<string>( 1024 );
		 private static readonly IDictionary<string, short> _timeZoneStringToShort = new Dictionary<string, short>( 1024 );

		 private const long MIN_ZONE_OFFSET_SECONDS = -18 * 3600;
		 private const long MAX_ZONE_OFFSET_SECONDS = 18 * 3600;

		 public static bool ValidZoneOffset( int zoneOffsetSeconds )
		 {
			  return zoneOffsetSeconds >= MIN_ZONE_OFFSET_SECONDS && zoneOffsetSeconds <= MAX_ZONE_OFFSET_SECONDS;
		 }

		 public static bool ValidZoneId( short zoneId )
		 {
			  return zoneId >= 0 && zoneId < _timeZoneShortToString.Count;
		 }

		 public static readonly string LatestSupportedIanaVersion;

		 /// <exception cref="IllegalArgumentException"> if tzid is not in the file </exception>
		 public static short Map( string tzid )
		 {
			  if ( !_timeZoneStringToShort.ContainsKey( tzid ) )
			  {
					throw new System.ArgumentException( "tzid" );
			  }
			  return _timeZoneStringToShort[tzid];
		 }

		 public static string Map( short offset )
		 {
			  return _timeZoneShortToString[offset];
		 }

		 public static ISet<string> SupportedTimeZones()
		 {
			  return unmodifiableSet( _timeZoneStringToShort.Keys );
		 }

		 static TimeZones()
		 {
			  string latestVersion = "";
			  Pattern version = Pattern.compile( "# tzdata([0-9]{4}[a-z])" );
			  IDictionary<string, string> oldToNewName = new Dictionary<string, string>( 1024 );

			  try
			  {
					  using ( StreamReader reader = new StreamReader( typeof( TimeZones ).getResourceAsStream( "/TZIDS" ) ) )
					  {
						for ( string line; ( line = reader.ReadLine() ) != null; )
						{
							 if ( line.StartsWith( "//" ) || line.Trim().Empty )
							 {
								  continue;
							 }
							 else if ( line.StartsWith( "#" ) )
							 {
								  Matcher matcher = version.matcher( line );
								  if ( matcher.matches() )
								  {
										latestVersion = matcher.group( 1 );
								  }
								  continue;
							 }
							 int sep = line.IndexOf( ' ' );
							 if ( sep != -1 )
							 {
								  string oldName = line.substring( 0, sep );
								  string newName = line.substring( sep + 1 );
								  _timeZoneShortToString.Add( newName );
								  oldToNewName[oldName] = newName;
							 }
							 else
							 {
								  _timeZoneShortToString.Add( line );
								  _timeZoneStringToShort[line] = ( short )( _timeZoneShortToString.Count - 1 );
							 }
						}
						LatestSupportedIanaVersion = latestVersion;
					  }
			  }
			  catch ( IOException )
			  {
					throw new Exception( "Failed to read time zone id file." );
			  }

			  foreach ( KeyValuePair<string, string> entry in oldToNewName.SetOfKeyValuePairs() )
			  {
					string oldName = entry.Key;
					string newName = entry.Value;
					short? newNameId = _timeZoneStringToShort[newName];
					_timeZoneStringToShort[oldName] = newNameId.Value;
			  }
		 }
	}

}