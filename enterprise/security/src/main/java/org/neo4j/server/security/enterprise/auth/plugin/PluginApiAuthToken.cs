﻿using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.Server.security.enterprise.auth.plugin
{

	using InvalidAuthTokenException = Org.Neo4j.Kernel.api.security.exception.InvalidAuthTokenException;
	using AuthToken = Org.Neo4j.Server.security.enterprise.auth.plugin.api.AuthToken;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.security.AuthToken_Fields.PRINCIPAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.security.AuthToken_Fields.CREDENTIALS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.security.AuthToken_Fields.PARAMETERS;

	public class PluginApiAuthToken : AuthToken
	{
		 private readonly string _principal;
		 private readonly char[] _credentials;
		 private readonly IDictionary<string, object> _parameters;

		 private PluginApiAuthToken( string principal, char[] credentials, IDictionary<string, object> parameters )
		 {
			  this._principal = principal;
			  this._credentials = credentials;
			  this._parameters = parameters;
		 }

		 public override string Principal()
		 {
			  return _principal;
		 }

		 public override char[] Credentials()
		 {
			  return _credentials;
		 }

		 public override IDictionary<string, object> Parameters()
		 {
			  return _parameters;
		 }

		 internal virtual void ClearCredentials()
		 {
			  if ( _credentials != null )
			  {
					Arrays.fill( _credentials, ( char ) 0 );
			  }
		 }

		 public static PluginApiAuthToken Of( string principal, char[] credentials, IDictionary<string, object> parameters )
		 {
			  return new PluginApiAuthToken( principal, credentials, parameters );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static PluginApiAuthToken createFromMap(java.util.Map<String,Object> authTokenMap) throws org.neo4j.kernel.api.security.exception.InvalidAuthTokenException
		 public static PluginApiAuthToken CreateFromMap( IDictionary<string, object> authTokenMap )
		 {
			  string scheme = Org.Neo4j.Kernel.api.security.AuthToken.safeCast( Org.Neo4j.Kernel.api.security.AuthToken_Fields.SCHEME_KEY, authTokenMap );

			  // Always require principal
			  string principal = Org.Neo4j.Kernel.api.security.AuthToken.safeCast( PRINCIPAL, authTokenMap );

			  sbyte[] credentials = null;
			  if ( scheme.Equals( Org.Neo4j.Kernel.api.security.AuthToken_Fields.BASIC_SCHEME ) )
			  {
					// Basic scheme requires credentials
					credentials = Org.Neo4j.Kernel.api.security.AuthToken.safeCastCredentials( CREDENTIALS, authTokenMap );
			  }
			  else
			  {
					// Otherwise credentials are optional
					object credentialsObject = authTokenMap[CREDENTIALS];
					if ( credentialsObject is sbyte[] )
					{
						 credentials = ( sbyte[] ) credentialsObject;
					}
			  }
			  IDictionary<string, object> parameters = Org.Neo4j.Kernel.api.security.AuthToken.safeCastMap( PARAMETERS, authTokenMap );

			  return PluginApiAuthToken.Of( principal, credentials != null ? StandardCharsets.UTF_8.decode( ByteBuffer.wrap( credentials ) ).array() : null, parameters );
		 }
	}

}