using System;
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
namespace Neo4Net.Server.security.enterprise.auth
{
	using AuthenticationException = org.apache.shiro.authc.AuthenticationException;
	using AuthenticationInfo = org.apache.shiro.authc.AuthenticationInfo;
	using AuthenticationToken = org.apache.shiro.authc.AuthenticationToken;
	using AbstractAuthenticationStrategy = org.apache.shiro.authc.pam.AbstractAuthenticationStrategy;
	using Realm = org.apache.shiro.realm.Realm;

	public class ShiroAuthenticationStrategy : AbstractAuthenticationStrategy
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.apache.shiro.authc.AuthenticationInfo beforeAllAttempts(java.util.Collection<? extends org.apache.shiro.realm.Realm> realms, org.apache.shiro.authc.AuthenticationToken token) throws org.apache.shiro.authc.AuthenticationException
		 public override AuthenticationInfo BeforeAllAttempts<T1>( ICollection<T1> realms, AuthenticationToken token ) where T1 : org.apache.shiro.realm.Realm
		 {
			  return new ShiroAuthenticationInfo();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.apache.shiro.authc.AuthenticationInfo afterAttempt(org.apache.shiro.realm.Realm realm, org.apache.shiro.authc.AuthenticationToken token, org.apache.shiro.authc.AuthenticationInfo singleRealmInfo, org.apache.shiro.authc.AuthenticationInfo aggregateInfo, Throwable t) throws org.apache.shiro.authc.AuthenticationException
		 public override AuthenticationInfo AfterAttempt( Realm realm, AuthenticationToken token, AuthenticationInfo singleRealmInfo, AuthenticationInfo aggregateInfo, Exception t )
		 {
			  AuthenticationInfo info = base.AfterAttempt( realm, token, singleRealmInfo, aggregateInfo, t );
			  if ( t != null && info is ShiroAuthenticationInfo )
			  {
					// Save the throwable so we can use it for correct log messages later
					( ( ShiroAuthenticationInfo ) info ).AddThrowable( t );
			  }
			  return info;
		 }
	}

}