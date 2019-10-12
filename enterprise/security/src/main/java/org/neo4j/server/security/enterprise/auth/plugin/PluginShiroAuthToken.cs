using System.Collections.Generic;

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
	using AuthenticationToken = org.apache.shiro.authc.AuthenticationToken;


	/// <summary>
	/// Version of ShiroAuthToken that returns credentials as a char array
	/// so that it is compatible for credentials matching with the
	/// cacheable auth info results returned by the plugin API
	/// </summary>
	public class PluginShiroAuthToken : ShiroAuthToken
	{
		 private readonly char[] _credentials;

		 private PluginShiroAuthToken( IDictionary<string, object> authTokenMap ) : base( authTokenMap )
		 {
			  // Convert credentials UTF8 byte[] to char[] (this should not create any intermediate copies)
			  sbyte[] credentialsBytes = ( sbyte[] ) base.Credentials;
			  _credentials = credentialsBytes != null ? StandardCharsets.UTF_8.decode( ByteBuffer.wrap( credentialsBytes ) ).array() : null;
		 }

		 public override object Credentials
		 {
			 get
			 {
				  return _credentials;
			 }
		 }

		 internal virtual void ClearCredentials()
		 {
			  if ( _credentials != null )
			  {
					Arrays.fill( _credentials, ( char ) 0 );
			  }
		 }

		 public static PluginShiroAuthToken Of( ShiroAuthToken shiroAuthToken )
		 {
			  return new PluginShiroAuthToken( shiroAuthToken.AuthTokenMap );
		 }

		 public static PluginShiroAuthToken Of( AuthenticationToken authenticationToken )
		 {
			  ShiroAuthToken shiroAuthToken = ( ShiroAuthToken ) authenticationToken;
			  return PluginShiroAuthToken.Of( shiroAuthToken );
		 }
	}

}