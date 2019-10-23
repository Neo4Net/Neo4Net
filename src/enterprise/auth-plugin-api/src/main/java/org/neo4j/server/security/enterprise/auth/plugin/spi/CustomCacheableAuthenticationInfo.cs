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
namespace Neo4Net.Server.security.enterprise.auth.plugin.spi
{
	using AuthProviderOperations = Neo4Net.Server.security.enterprise.auth.plugin.api.AuthProviderOperations;
	using AuthToken = Neo4Net.Server.security.enterprise.auth.plugin.api.AuthToken;

	/// <summary>
	/// A cacheable object that can be returned as the result of successful authentication by an
	/// <seealso cref="AuthenticationPlugin"/>.
	/// 
	/// <para>This object can be cached by the Neo4Net authentication cache.
	/// 
	/// </para>
	/// <para>This is an alternative to <seealso cref="CacheableAuthenticationInfo"/> to use if you want to manage your own way of
	/// hashing and matching credentials. On authentication, when a cached authentication info from a previous successful
	/// authentication attempt is found for the principal within the auth token map, then
	/// <seealso cref="CredentialsMatcher.doCredentialsMatch(AuthToken)"/> of the <seealso cref="CredentialsMatcher"/> returned by
	/// <seealso cref="credentialsMatcher()"/> will be called to determine if the credentials match.
	/// 
	/// </para>
	/// <para>NOTE: Caching only occurs if it is explicitly enabled by the plugin.
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= AuthenticationPlugin#authenticate(AuthToken) </seealso>
	/// <seealso cref= AuthProviderOperations#setAuthenticationCachingEnabled(boolean) </seealso>
	public interface ICustomCacheableAuthenticationInfo : AuthenticationInfo
	{

		 /// <summary>
		 /// Returns the credentials matcher that will be used to verify the credentials of an auth token against the
		 /// cached credentials in this object.
		 /// 
		 /// <para>NOTE: The returned object implementing the <seealso cref="CredentialsMatcher"/> interface need to have a
		 /// reference to the actual credentials in a matcheable form within its context in order to benefit from caching,
		 /// so it is typically stateful. The simplest way is to return a lambda from this method.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> the credentials matcher that will be used to verify the credentials of an auth token against the
		 ///         cached credentials in this object </returns>
		 CustomCacheableAuthenticationInfo_CredentialsMatcher CredentialsMatcher();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static CustomCacheableAuthenticationInfo of(Object principal, CustomCacheableAuthenticationInfo_CredentialsMatcher credentialsMatcher)
	//	 {
	//		  return new CustomCacheableAuthenticationInfo()
	//		  {
	//				@@Override public Object principal()
	//				{
	//					 return principal;
	//				}
	//
	//				@@Override public CredentialsMatcher credentialsMatcher()
	//				{
	//					 return credentialsMatcher;
	//				}
	//		  };
	//	 }
	}

	 public interface ICustomCacheableAuthenticationInfo_CredentialsMatcher
	 {
		  /// <summary>
		  /// Returns true if the credentials of the given <seealso cref="AuthToken"/> matches the credentials of the cached
		  /// <seealso cref="CustomCacheableAuthenticationInfo"/> that is the owner of this <seealso cref="CredentialsMatcher"/>.
		  /// </summary>
		  /// <returns> true if the credentials of the given auth token matches the credentials of this cached
		  ///         authentication info, otherwise false </returns>
		  bool DoCredentialsMatch( AuthToken authToken );
	 }

}