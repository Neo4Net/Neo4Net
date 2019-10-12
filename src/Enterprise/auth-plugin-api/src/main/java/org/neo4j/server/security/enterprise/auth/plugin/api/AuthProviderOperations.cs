using System;

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
namespace Neo4Net.Server.security.enterprise.auth.plugin.api
{

	/// <summary>
	/// These are the methods that the plugin can perform on Neo4j.
	/// </summary>
	public interface AuthProviderOperations
	{
		 /// <summary>
		 /// Returns the path to the Neo4j home directory.
		 /// </summary>
		 /// <returns> the path to the Neo4j home directory </returns>
		 Path Neo4jHome();

		 /// <summary>
		 /// Returns the path to the Neo4j configuration file if one exists.
		 /// </summary>
		 /// <returns> the path to the Neo4j configuration file if one exists
		 /// 
		 /// @deprecated
		 /// Settings are recommended to be stored in a separate file. You can use <seealso cref="AuthProviderOperations.neo4jHome()"/>
		 /// to resolve your configuration file, e.g. {@code neo4jHome().resolve("conf/myPlugin.conf" );} </returns>
		 [Obsolete]
		 Optional<Path> Neo4jConfigFile();

		 /// <summary>
		 /// Returns the Neo4j version.
		 /// </summary>
		 /// <returns> the Neo4j version </returns>
		 string Neo4jVersion();

		 /// <summary>
		 /// Returns the clock that is used by the Neo4j security module within which this auth provider plugin is running.
		 /// </summary>
		 /// <returns> the clock that is used by the Neo4j security module </returns>
		 Clock Clock();

		 /// <summary>
		 /// Returns the security log that is used by the Neo4j security module within which this auth provider plugin is
		 /// running.
		 /// </summary>
		 /// <returns> the security log that is used by the Neo4j security module </returns>
		 AuthProviderOperations_Log Log();

		 /// <summary>
		 /// An interface to the security log that is used by the Neo4j security module.
		 /// </summary>

		 /// <summary>
		 /// If set to {@code true} the authentication information returned by the plugin will be cached.
		 /// The expiration time of the cached information is configured by the
		 /// {@code dbms.security.auth_cache_ttl} configuration setting.
		 /// 
		 /// <para>Since a principal can be authenticated against cached authentication information this requires
		 /// the capability of matching the credentials of an authentication token against the credentials of the
		 /// authentication information returned by the plugin.
		 /// 
		 /// </para>
		 /// <para>The default value is {@code false}.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="authenticationCachingEnabled"> if caching of authentication information should be enabled or not </param>
		 bool AuthenticationCachingEnabled { set; }

		 /// <summary>
		 /// If set to {@code true} the authorization information (i.e. the list of roles for a given principal)
		 /// returned by the plugin will be cached.
		 /// The expiration time of the cached information is configured by the
		 /// {@code dbms.security.auth_cache_ttl} configuration setting.
		 /// 
		 /// The default value is {@code true}.
		 /// </summary>
		 /// <param name="authorizationCachingEnabled"> if caching of authorization information should be enabled or not </param>
		 bool AuthorizationCachingEnabled { set; }
	}

	 public interface AuthProviderOperations_Log
	 {
		  /// <summary>
		  /// Writes to the security log at log level debug.
		  /// </summary>
		  /// <param name="message"> the message to write to the security log </param>
		  void Debug( string message );

		  /// <summary>
		  /// Writes to the security log at log level info.
		  /// </summary>
		  /// <param name="message"> the message to write to the security log </param>
		  void Info( string message );

		  /// <summary>
		  /// Writes to the security log at log level warning.
		  /// </summary>
		  /// <param name="message"> the message to write to the security log </param>
		  void Warn( string message );

		  /// <summary>
		  /// Writes to the security log at log level error.
		  /// </summary>
		  /// <param name="message"> the message to write to the security log </param>
		  void Error( string message );

		  /// <summary>
		  /// Returns {@code true} if log level debug is enabled.
		  /// </summary>
		  /// <returns> {@code true} if log level debug is enabled, otherwise {@code false} </returns>
		  bool DebugEnabled { get; }
	 }

}