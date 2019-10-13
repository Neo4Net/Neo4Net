using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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

	using AuthorizationExpiredException = Neo4Net.Server.security.enterprise.auth.plugin.api.AuthorizationExpiredException;

	/// <summary>
	/// An authorization provider plugin for the Neo4j enterprise security module.
	/// 
	/// <para>If the configuration setting {@code dbms.security.plugin.authorization_enabled} is set to {@code true},
	/// all objects that implements this interface that exists in the class path at Neo4j startup, will be
	/// loaded as services.
	/// 
	/// </para>
	/// <para>NOTE: If the same object also implements <seealso cref="AuthenticationPlugin"/>, it will not be loaded twice.
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= AuthPlugin </seealso>
	/// <seealso cref= AuthorizationExpiredException </seealso>
	public interface AuthorizationPlugin : AuthProviderLifecycle
	{
		 /// <summary>
		 /// An object containing a principal and its corresponding authentication provider.
		 /// </summary>

		 /// <summary>
		 /// The name of this authorization provider.
		 /// 
		 /// <para>This name, prepended with the prefix "plugin-", can be used by a client to direct an auth token directly
		 /// to this authorization provider.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> the name of this authorization provider </returns>
		 string Name();

		 /// <summary>
		 /// Should perform authorization of the given collection of principals and their corresponding authentication
		 /// providers, and return an <seealso cref="AuthorizationInfo"/> result that contains a collection of roles
		 /// that are assigned to the given principals.
		 /// </summary>
		 /// <param name="principals"> a collection of principals and their corresponding authentication providers
		 /// </param>
		 /// <returns> an <seealso cref="AuthorizationInfo"/> result that contains the roles that are assigned to the given principals </returns>
		 AuthorizationInfo Authorize( ICollection<AuthorizationPlugin_PrincipalAndProvider> principals );
	}

	 public sealed class AuthorizationPlugin_PrincipalAndProvider
	 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly object PrincipalConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly string ProviderConflict;

		  public AuthorizationPlugin_PrincipalAndProvider( object principal, string provider )
		  {
				this.PrincipalConflict = principal;
				this.ProviderConflict = provider;
		  }

		  public object Principal()
		  {
				return PrincipalConflict;
		  }

		  public string Provider()
		  {
				return ProviderConflict;
		  }
	 }

	 public class AuthorizationPlugin_Adapter : AuthProviderLifecycle_Adapter, AuthorizationPlugin
	 {
		  public override string Name()
		  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				return this.GetType().FullName;
		  }

		  public override AuthorizationInfo Authorize( ICollection<AuthorizationPlugin_PrincipalAndProvider> principals )
		  {
				return null;
		  }
	 }

}