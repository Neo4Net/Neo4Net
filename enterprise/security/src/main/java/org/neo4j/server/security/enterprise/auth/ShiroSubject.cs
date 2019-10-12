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
namespace Org.Neo4j.Server.security.enterprise.auth
{
	using SecurityManager = org.apache.shiro.mgt.SecurityManager;
	using Session = org.apache.shiro.session.Session;
	using PrincipalCollection = org.apache.shiro.subject.PrincipalCollection;
	using DelegatingSubject = org.apache.shiro.subject.support.DelegatingSubject;

	using AuthenticationResult = Org.Neo4j.@internal.Kernel.Api.security.AuthenticationResult;

	public class ShiroSubject : DelegatingSubject
	{
		 private AuthenticationResult _authenticationResult;
		 private ShiroAuthenticationInfo _authenticationInfo;

		 public ShiroSubject( SecurityManager securityManager, AuthenticationResult authenticationResult ) : base( securityManager )
		 {
			  this._authenticationResult = authenticationResult;
		 }

		 public ShiroSubject( PrincipalCollection principals, bool authenticated, string host, Session session, bool sessionCreationEnabled, SecurityManager securityManager, AuthenticationResult authenticationResult, ShiroAuthenticationInfo authenticationInfo ) : base( principals, authenticated, host, session, sessionCreationEnabled, securityManager )
		 {
			  this._authenticationResult = authenticationResult;
			  this._authenticationInfo = authenticationInfo;
		 }

		 public virtual AuthenticationResult AuthenticationResult
		 {
			 get
			 {
				  return _authenticationResult;
			 }
			 set
			 {
				  this._authenticationResult = value;
			 }
		 }


		 public virtual ShiroAuthenticationInfo AuthenticationInfo
		 {
			 get
			 {
				  return _authenticationInfo;
			 }
		 }

		 public virtual void ClearAuthenticationInfo()
		 {
			  this._authenticationInfo = null;
		 }
	}

}