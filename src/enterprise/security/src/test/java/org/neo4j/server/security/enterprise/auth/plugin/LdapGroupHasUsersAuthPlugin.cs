using System;
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

	using AuthToken = Neo4Net.Server.security.enterprise.auth.plugin.api.AuthToken;
	using AuthenticationException = Neo4Net.Server.security.enterprise.auth.plugin.api.AuthenticationException;
	using PredefinedRoles = Neo4Net.Server.security.enterprise.auth.plugin.api.PredefinedRoles;
	using AuthInfo = Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthInfo;
	using AuthPlugin = Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthPlugin;

	public class LdapGroupHasUsersAuthPlugin : Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthPlugin_Adapter
	{
		 private const string GROUP_SEARCH_BASE = "ou=groups,dc=example,dc=com";
		 private const string GROUP_SEARCH_FILTER = "(&(objectClass=posixGroup)(memberUid={0}))";
		 public const string GROUP_ID = "gidNumber";

		 public override string Name()
		 {
			  return "ldap-alternative-groups";
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.server.security.enterprise.auth.plugin.spi.AuthInfo authenticateAndAuthorize(org.Neo4Net.server.security.enterprise.auth.plugin.api.AuthToken authToken) throws org.Neo4Net.server.security.enterprise.auth.plugin.api.AuthenticationException
		 public override AuthInfo AuthenticateAndAuthorize( AuthToken authToken )
		 {
			  try
			  {
					string username = authToken.Principal();
					char[] password = authToken.Credentials();

					LdapContext ctx = Authenticate( username, password );
					ISet<string> roles = Authorize( ctx, username );

					return AuthInfo.of( username, roles );
			  }
			  catch ( NamingException e )
			  {
					throw new AuthenticationException( e.Message );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private javax.naming.ldap.LdapContext authenticate(String username, char[] password) throws javax.naming.NamingException
		 private LdapContext Authenticate( string username, char[] password )
		 {
			  Dictionary<string, object> env = new Dictionary<string, object>();
			  env[Context.INITIAL_CONTEXT_FACTORY] = "com.sun.jndi.ldap.LdapCtxFactory";
			  env[Context.PROVIDER_URL] = "ldap://0.0.0.0:10389";

			  env[Context.SECURITY_PRINCIPAL] = string.Format( "cn={0},ou=users,dc=example,dc=com", username );
			  env[Context.SECURITY_CREDENTIALS] = password;

			  return new InitialLdapContext( env, null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.Set<String> authorize(javax.naming.ldap.LdapContext ctx, String username) throws javax.naming.NamingException
		 private ISet<string> Authorize( LdapContext ctx, string username )
		 {
			  ISet<string> roleNames = new LinkedHashSet<string>();

			  // Setup our search controls
			  SearchControls searchCtls = new SearchControls();
			  searchCtls.SearchScope = SearchControls.SUBTREE_SCOPE;
			  searchCtls.ReturningAttributes = new string[]{ GROUP_ID };

			  // Use a search argument to prevent potential code injection
			  object[] searchArguments = new object[]{ username };

			  // Search for groups that has the user as a member
			  NamingEnumeration result = ctx.search( GROUP_SEARCH_BASE, GROUP_SEARCH_FILTER, searchArguments, searchCtls );

			  if ( result.hasMoreElements() )
			  {
					SearchResult searchResult = ( SearchResult ) result.next();

					Attributes attributes = searchResult.Attributes;
					if ( attributes != null )
					{
						 NamingEnumeration attributeEnumeration = attributes.All;
						 while ( attributeEnumeration.hasMore() )
						 {
							  Attribute attribute = ( Attribute ) attributeEnumeration.next();
							  string attributeId = attribute.ID;
							  if ( attributeId.Equals( GROUP_ID, StringComparison.OrdinalIgnoreCase ) )
							  {
									// We found a group that the user is a member of. See if it has a role mapped to it
									string groupId = ( string ) attribute.get();
									string Neo4NetGroup = GetNeo4NetRoleForGroupId( groupId );
									if ( !string.ReferenceEquals( Neo4NetGroup, null ) )
									{
										 // Yay! Add it to our set of roles
										 roleNames.Add( Neo4NetGroup );
									}
							  }
						 }
					}
			  }
			  return roleNames;
		 }

		 private string GetNeo4NetRoleForGroupId( string groupId )
		 {
			  if ( "500".Equals( groupId ) )
			  {
					return PredefinedRoles.READER;
			  }
			  if ( "501".Equals( groupId ) )
			  {
					return PredefinedRoles.PUBLISHER;
			  }
			  if ( "502".Equals( groupId ) )
			  {
					return PredefinedRoles.ARCHITECT;
			  }
			  if ( "503".Equals( groupId ) )
			  {
					return PredefinedRoles.ADMIN;
			  }
			  return null;
		 }
	}

}