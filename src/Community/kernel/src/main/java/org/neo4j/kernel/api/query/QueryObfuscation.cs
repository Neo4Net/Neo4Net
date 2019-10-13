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
namespace Neo4Net.Kernel.api.query
{

	using TextValue = Neo4Net.Values.Storable.TextValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

	public class QueryObfuscation
	{
		 private static readonly Pattern _passwordPattern = Pattern.compile( "(?:(?i)call)\\s+(?:dbms(?:\\.security)?\\.change(?:User)?Password|dbms\\.security\\.createUser)\\(\\s*" + "(?:\\s*(?:'(?:(?<=\\\\)'|[^'])*'|\"(?:(?<=\\\\)\"|[^\"])*\"|[^,]*)\\s*,)?" + "\\s*('(?:(?<=\\\\)'|[^'])*'|\"(?:(?<=\\\\)\"|[^\"])*\"|\\$\\w*|\\{\\w*})" );

		 internal static readonly TextValue Obfuscated = stringValue( "******" );
		 internal const string OBFUSCATED_LITERAL = "'******'";

		 public static string ObfuscateText( string queryText, ISet<string> passwordParams )
		 {
			  Matcher matcher = _passwordPattern.matcher( queryText );

			  while ( matcher.find() )
			  {
					string password = matcher.group( 1 ).Trim();
					if ( password[0] == '$' )
					{
						 passwordParams.Add( password.Substring( 1 ) );
					}
					else if ( password[0] == '{' )
					{
						 passwordParams.Add( password.Substring( 1, ( password.Length - 1 ) - 1 ) );
					}
					else
					{
						 queryText = queryText.Replace( password, OBFUSCATED_LITERAL );
					}
			  }

			  return queryText;
		 }

		 public static MapValue ObfuscateParams( MapValue queryParameters, ISet<string> passwordParams )
		 {
			  foreach ( string passwordKey in passwordParams )
			  {
					queryParameters = queryParameters.UpdatedWith( passwordKey, Obfuscated );
			  }
			  return queryParameters;
		 }
	}

}