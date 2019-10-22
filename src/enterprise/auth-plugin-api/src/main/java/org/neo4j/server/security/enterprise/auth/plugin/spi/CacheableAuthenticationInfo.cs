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
	/// <para>This is an alternative to <seealso cref="CustomCacheableAuthenticationInfo"/> if you want Neo4Net to manage secure
	/// hashing and matching of cached credentials.
	/// 
	/// </para>
	/// <para>NOTE: Caching only occurs if it is explicitly enabled by the plugin.
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= AuthenticationPlugin#authenticate(AuthToken) </seealso>
	/// <seealso cref= AuthProviderOperations#setAuthenticationCachingEnabled(boolean) </seealso>
	/// <seealso cref= CustomCacheableAuthenticationInfo </seealso>
	public interface CacheableAuthenticationInfo : AuthenticationInfo
	{
		 /// <summary>
		 /// Should return a principal that uniquely identifies the authenticated subject within this authentication provider.
		 /// This will be used as the cache key, and needs to be matcheable against a principal within the auth token map.
		 /// 
		 /// <para>Typically this is the same as the principal within the auth token map.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> a principal that uniquely identifies the authenticated subject within this authentication provider
		 /// </returns>
		 /// <seealso cref= AuthToken#principal() </seealso>
		 object Principal();

		 /// <summary>
		 /// Should return credentials that can be cached, so that successive authentication attempts could be performed
		 /// against the cached authentication info from a previous successful authentication attempt.
		 /// 
		 /// <para>NOTE: The returned credentials will be hashed using a cryptographic hash function together
		 /// with a random salt (generated with a secure random number generator) before being stored.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> credentials that can be cached
		 /// </returns>
		 /// <seealso cref= AuthToken#credentials() </seealso>
		 /// <seealso cref= AuthenticationPlugin#authenticate(AuthToken) </seealso>
		 sbyte[] Credentials();

		 /// <summary>
		 /// Creates a new <seealso cref="CacheableAuthenticationInfo"/>
		 /// </summary>
		 /// <param name="principal"> a principal that uniquely identifies the authenticated subject within this authentication
		 ///                  provider </param>
		 /// <param name="credentials"> credentials that can be cached </param>
		 /// <returns> a new <seealso cref="CacheableAuthenticationInfo"/> containing the given parameters </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static CacheableAuthenticationInfo of(Object principal, byte[] credentials)
	//	 {
	//		  return new CacheableAuthenticationInfo()
	//		  {
	//				@@Override public Object principal()
	//				{
	//					 return principal;
	//				}
	//
	//				@@Override public byte[] credentials()
	//				{
	//					 return credentials;
	//				}
	//		  };
	//	 }
	}

}