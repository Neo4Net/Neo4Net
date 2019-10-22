using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Server.security.enterprise.auth.plugin
{

	using InvalidAuthTokenException = Neo4Net.Kernel.api.security.exception.InvalidAuthTokenException;
	using AuthToken = Neo4Net.Server.security.enterprise.auth.plugin.api.AuthToken;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.security.AuthToken_Fields.PRINCIPAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.security.AuthToken_Fields.CREDENTIALS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.security.AuthToken_Fields.PARAMETERS;

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
//ORIGINAL LINE: public static PluginApiAuthToken createFromMap(java.util.Map<String,Object> authTokenMap) throws org.Neo4Net.kernel.api.security.exception.InvalidAuthTokenException
		 public static PluginApiAuthToken CreateFromMap( IDictionary<string, object> authTokenMap )
		 {
			  string scheme = Neo4Net.Kernel.api.security.AuthToken.safeCast( Neo4Net.Kernel.api.security.AuthToken_Fields.SCHEME_KEY, authTokenMap );

			  // Always require principal
			  string principal = Neo4Net.Kernel.api.security.AuthToken.safeCast( PRINCIPAL, authTokenMap );

			  sbyte[] credentials = null;
			  if ( scheme.Equals( Neo4Net.Kernel.api.security.AuthToken_Fields.BASIC_SCHEME ) )
			  {
					// Basic scheme requires credentials
					credentials = Neo4Net.Kernel.api.security.AuthToken.safeCastCredentials( CREDENTIALS, authTokenMap );
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
			  IDictionary<string, object> parameters = Neo4Net.Kernel.api.security.AuthToken.safeCastMap( PARAMETERS, authTokenMap );

			  return PluginApiAuthToken.Of( principal, credentials != null ? StandardCharsets.UTF_8.decode( ByteBuffer.wrap( credentials ) ).array() : null, parameters );
		 }
	}

}