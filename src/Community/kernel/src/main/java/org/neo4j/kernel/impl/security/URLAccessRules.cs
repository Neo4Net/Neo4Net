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
namespace Neo4Net.Kernel.impl.security
{

	using URLAccessRule = Neo4Net.Graphdb.security.URLAccessRule;
	using URLAccessValidationError = Neo4Net.Graphdb.security.URLAccessValidationError;

	public class URLAccessRules
	{
		 private static readonly URLAccessRule _alwaysPermitted = ( config, url ) => url;

		 private URLAccessRules()
		 {
		 }

		 public static URLAccessRule AlwaysPermitted()
		 {
			  return _alwaysPermitted;
		 }

		 private static readonly URLAccessRule _fileAccess = new FileURLAccessRule();

		 public static URLAccessRule FileAccess()
		 {
			  return _fileAccess;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.neo4j.graphdb.security.URLAccessRule combined(final java.util.Map<String,org.neo4j.graphdb.security.URLAccessRule> urlAccessRules)
		 public static URLAccessRule Combined( IDictionary<string, URLAccessRule> urlAccessRules )
		 {
			  return ( config, url ) =>
			  {
				string protocol = url.Protocol;
				URLAccessRule protocolRule = urlAccessRules[protocol];
				if ( protocolRule == null )
				{
					 throw new URLAccessValidationError( "loading resources via protocol '" + protocol + "' is not permitted" );
				}
				return protocolRule.validate( config, url );
			  };
		 }
	}

}