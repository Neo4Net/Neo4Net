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
namespace Neo4Net.Server.security.enterprise.auth.plugin.spi
{
	using AuthProviderOperations = Neo4Net.Server.security.enterprise.auth.plugin.api.AuthProviderOperations;
	using AuthToken = Neo4Net.Server.security.enterprise.auth.plugin.api.AuthToken;
	using AuthenticationException = Neo4Net.Server.security.enterprise.auth.plugin.api.AuthenticationException;

	/// <summary>
	/// A simplified combined authentication and authorization provider plugin for the Neo4j enterprise security module.
	/// 
	/// <para>If either the configuration setting {@code dbms.security.plugin.authentication_enabled} or
	/// {@code dbms.security.plugin.authorization_enabled} is set to {@code true},
	/// all objects that implements this interface that exists in the class path at Neo4j startup, will be
	/// loaded as services.
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= AuthenticationPlugin </seealso>
	/// <seealso cref= AuthorizationPlugin </seealso>
	public interface AuthPlugin : AuthProviderLifecycle
	{
		 /// <summary>
		 /// The name of this auth provider.
		 /// 
		 /// <para>This name, prepended with the prefix "plugin-", can be used by a client to direct an auth token directly
		 /// to this auth provider.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> the name of this auth provider </returns>
		 string Name();

		 /// <summary>
		 /// Should perform both authentication and authorization of the identity in the given auth token and return an
		 /// <seealso cref="AuthInfo"/> result if successful. The <seealso cref="AuthInfo"/> result can also contain a collection of roles
		 /// that are assigned to the given identity, which constitutes the authorization part.
		 /// 
		 /// If authentication failed, either {@code null} should be returned,
		 /// or an <seealso cref="AuthenticationException"/> should be thrown.
		 /// 
		 /// <para>If authentication caching is enabled, then a <seealso cref="CacheableAuthInfo"/> should be returned.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> an <seealso cref="AuthInfo"/> object if authentication was successful, otherwise {@code null}
		 /// </returns>
		 /// <seealso cref= org.neo4j.server.security.enterprise.auth.plugin.api.AuthToken </seealso>
		 /// <seealso cref= AuthenticationInfo </seealso>
		 /// <seealso cref= CacheableAuthenticationInfo </seealso>
		 /// <seealso cref= CustomCacheableAuthenticationInfo </seealso>
		 /// <seealso cref= AuthProviderOperations#setAuthenticationCachingEnabled(boolean) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: AuthInfo authenticateAndAuthorize(org.neo4j.server.security.enterprise.auth.plugin.api.AuthToken authToken) throws org.neo4j.server.security.enterprise.auth.plugin.api.AuthenticationException;
		 AuthInfo AuthenticateAndAuthorize( AuthToken authToken );
	}

	 public abstract class AuthPlugin_Adapter : AuthProviderLifecycle_Adapter, AuthPlugin
	 {
		 public abstract AuthInfo AuthenticateAndAuthorize( AuthToken authToken );
		  public override string Name()
		  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				return this.GetType().FullName;
		  }
	 }

	 public abstract class AuthPlugin_CachingEnabledAdapter : AuthProviderLifecycle_Adapter, AuthPlugin
	 {
		 public abstract AuthInfo AuthenticateAndAuthorize( AuthToken authToken );
		  public override string Name()
		  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				return this.GetType().FullName;
		  }

		  public override void Initialize( AuthProviderOperations authProviderOperations )
		  {
				authProviderOperations.AuthenticationCachingEnabled = true;
		  }
	 }

}