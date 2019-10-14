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
namespace Neo4Net.Helpers
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.parseInt;

	public class SocketAddressParser
	{
		 private static readonly Pattern _hostnamePortPatternExt = Pattern.compile( "\\[(?<hostname>[^\\s]+)]:(?<port>\\d+)" );
		 private static readonly Pattern _hostnamePortPattern = Pattern.compile( "(?<hostname>[^\\s]+):(?<port>\\d+)" );
		 private static readonly Pattern _portPattern = Pattern.compile( ":(?<port>\\d+)" );

		 public static T DeriveSocketAddress<T>( string settingName, string settingValue, string defaultHostname, int defaultPort, System.Func<string, int, T> constructor ) where T : SocketAddress
		 {
			  if ( string.ReferenceEquals( settingValue, null ) )
			  {
					return constructor( defaultHostname, defaultPort );
			  }

			  settingValue = settingValue.Trim();

			  T socketAddress;
			  if ( ( socketAddress = MatchHostnamePort( settingValue, constructor ) ) != null )
			  {
					return socketAddress;
			  }

			  if ( ( socketAddress = MatchPort( settingValue, defaultHostname, constructor ) ) != null )
			  {
					return socketAddress;
			  }

			  throw new System.ArgumentException( format( "Setting \"%s\" must be in the format " + "\"hostname:port\" or \":port\". \"%s\" does not conform to these formats", settingName, settingValue ) );
		 }

		 public static T SocketAddress<T>( string settingValue, System.Func<string, int, T> constructor ) where T : SocketAddress
		 {
			  if ( string.ReferenceEquals( settingValue, null ) )
			  {
					throw new System.ArgumentException( "Cannot parse socket address from null" );
			  }

			  settingValue = settingValue.Trim();

			  T socketAddress;
			  if ( ( socketAddress = MatchHostnamePort( settingValue, constructor ) ) != null )
			  {
					return socketAddress;
			  }

			  throw new System.ArgumentException( format( "Configured socket address must be in the format " + "\"hostname:port\". \"%s\" does not conform to this format", settingValue ) );
		 }

		 private static T MatchHostnamePort<T>( string settingValue, System.Func<string, int, T> constructor ) where T : SocketAddress
		 {
			  Matcher hostnamePortWithBracketsMatcher = _hostnamePortPatternExt.matcher( settingValue );
			  if ( hostnamePortWithBracketsMatcher.matches() )
			  {
					string hostname = hostnamePortWithBracketsMatcher.group( "hostname" );
					int port = parseInt( hostnamePortWithBracketsMatcher.group( "port" ) );
					return constructor( hostname, port );
			  }

			  Matcher hostnamePortMatcher = _hostnamePortPattern.matcher( settingValue );
			  if ( hostnamePortMatcher.matches() )
			  {
					string hostname = hostnamePortMatcher.group( "hostname" );
					int port = parseInt( hostnamePortMatcher.group( "port" ) );
					return constructor( hostname, port );
			  }

			  return default( T );
		 }

		 private static T MatchPort<T>( string settingValue, string defaultHostname, System.Func<string, int, T> constructor ) where T : SocketAddress
		 {
			  Matcher portMatcher = _portPattern.matcher( settingValue );
			  if ( portMatcher.matches() )
			  {
					int port = parseInt( portMatcher.group( "port" ) );
					return constructor( defaultHostname, port );
			  }

			  return default( T );
		 }
	}

}