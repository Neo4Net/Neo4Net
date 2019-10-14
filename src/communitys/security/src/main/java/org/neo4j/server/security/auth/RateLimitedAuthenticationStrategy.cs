using System.Collections.Concurrent;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Server.Security.Auth
{

	using AuthenticationResult = Neo4Net.@internal.Kernel.Api.security.AuthenticationResult;
	using Config = Neo4Net.Kernel.configuration.Config;
	using User = Neo4Net.Kernel.impl.security.User;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.auth_lock_time;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.auth_max_failed_attempts;

	public class RateLimitedAuthenticationStrategy : AuthenticationStrategy
	{
		 private readonly Clock _clock;
		 private readonly long _lockDurationMs;
		 private readonly int _maxFailedAttempts;

		 private class AuthenticationMetadata
		 {
			 private readonly RateLimitedAuthenticationStrategy _outerInstance;

			 public AuthenticationMetadata( RateLimitedAuthenticationStrategy outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal readonly AtomicInteger FailedAuthAttempts = new AtomicInteger();
			  internal long LastFailedAttemptTime;

			  public virtual bool AuthenticationPermitted()
			  {
					return outerInstance.maxFailedAttempts <= 0 || FailedAuthAttempts.get() < outerInstance.maxFailedAttempts || outerInstance.clock.millis() >= LastFailedAttemptTime + outerInstance.lockDurationMs; // auth lock duration expired
			  }

			  public virtual void AuthSuccess()
			  {
					FailedAuthAttempts.set( 0 );
			  }

			  public virtual void AuthFailed()
			  {
					FailedAuthAttempts.incrementAndGet();
					LastFailedAttemptTime = outerInstance.clock.millis();
			  }
		 }

		 /// <summary>
		 /// Tracks authentication state for each user
		 /// </summary>
		 private readonly ConcurrentMap<string, AuthenticationMetadata> _authenticationData = new ConcurrentDictionary<string, AuthenticationMetadata>();

		 public RateLimitedAuthenticationStrategy( Clock clock, Config config ) : this( clock, config.Get( auth_lock_time ), config.Get( auth_max_failed_attempts ) )
		 {
		 }

		 internal RateLimitedAuthenticationStrategy( Clock clock, Duration lockDuration, int maxFailedAttempts )
		 {
			  this._clock = clock;
			  this._lockDurationMs = lockDuration.toMillis();
			  this._maxFailedAttempts = maxFailedAttempts;
		 }

		 public override AuthenticationResult Authenticate( User user, sbyte[] password )
		 {
			  AuthenticationMetadata authMetadata = AuthMetadataFor( user.Name() );

			  if ( !authMetadata.AuthenticationPermitted() )
			  {
					return AuthenticationResult.TOO_MANY_ATTEMPTS;
			  }

			  if ( user.Credentials().matchesPassword(password) )
			  {
					authMetadata.AuthSuccess();
					return AuthenticationResult.SUCCESS;
			  }
			  else
			  {
					authMetadata.AuthFailed();
					return AuthenticationResult.FAILURE;
			  }
		 }

		 private AuthenticationMetadata AuthMetadataFor( string username )
		 {
			  AuthenticationMetadata authMeta = _authenticationData.get( username );

			  if ( authMeta == null )
			  {
					authMeta = new AuthenticationMetadata( this );
					AuthenticationMetadata preExisting = _authenticationData.putIfAbsent( username, authMeta );
					if ( preExisting != null )
					{
						 authMeta = preExisting;
					}
			  }

			  return authMeta;
		 }

	}

}