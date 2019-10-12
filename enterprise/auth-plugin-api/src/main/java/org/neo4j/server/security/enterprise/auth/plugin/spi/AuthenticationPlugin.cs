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
namespace Org.Neo4j.Server.security.enterprise.auth.plugin.spi
{
	using AuthProviderOperations = Org.Neo4j.Server.security.enterprise.auth.plugin.api.AuthProviderOperations;
	using AuthToken = Org.Neo4j.Server.security.enterprise.auth.plugin.api.AuthToken;
	using AuthenticationException = Org.Neo4j.Server.security.enterprise.auth.plugin.api.AuthenticationException;

	/// <summary>
	/// An authentication provider plugin for the Neo4j enterprise security module.
	/// 
	/// <para>If the configuration setting {@code dbms.security.plugin.authentication_enabled} is set to {@code true},
	/// all objects that implements this interface that exists in the class path at Neo4j startup, will be
	/// loaded as services.
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= AuthPlugin </seealso>
	/// <seealso cref= AuthorizationPlugin </seealso>
	public interface AuthenticationPlugin : AuthProviderLifecycle
	{
		 /// <summary>
		 /// The name of this authentication provider.
		 /// 
		 /// <para>This name, prepended with the prefix "plugin-", can be used by a client to direct an auth token directly
		 /// to this authentication provider.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> the name of this authentication provider </returns>
		 string Name();

		 /// <summary>
		 /// Should perform authentication of the identity in the given auth token and return an
		 /// <seealso cref="AuthenticationInfo"/> result if successful.
		 /// If authentication failed, either {@code null} should be returned,
		 /// or an <seealso cref="AuthenticationException"/> should be thrown.
		 /// <para>
		 /// If authentication caching is enabled, either a <seealso cref="CacheableAuthenticationInfo"/> or a
		 /// <seealso cref="CustomCacheableAuthenticationInfo"/> should be returned.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> an <seealso cref="AuthenticationInfo"/> object if authentication was successful, otherwise {@code null} </returns>
		 /// <exception cref="AuthenticationException"> if authentication failed
		 /// </exception>
		 /// <seealso cref= org.neo4j.server.security.enterprise.auth.plugin.api.AuthToken </seealso>
		 /// <seealso cref= AuthProviderOperations#setAuthenticationCachingEnabled(boolean) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: AuthenticationInfo authenticate(org.neo4j.server.security.enterprise.auth.plugin.api.AuthToken authToken) throws org.neo4j.server.security.enterprise.auth.plugin.api.AuthenticationException;
		 AuthenticationInfo Authenticate( AuthToken authToken );
	}

	 public abstract class AuthenticationPlugin_Adapter : AuthProviderLifecycle_Adapter, AuthenticationPlugin
	 {
		 public abstract AuthenticationInfo Authenticate( AuthToken authToken );
		  public override string Name()
		  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				return this.GetType().FullName;
		  }
	 }

	 public abstract class AuthenticationPlugin_CachingEnabledAdapter : AuthProviderLifecycle_Adapter, AuthenticationPlugin
	 {
		 public abstract AuthenticationInfo Authenticate( AuthToken authToken );
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