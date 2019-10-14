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
	using SecurityManager = org.apache.shiro.mgt.SecurityManager;
	using SubjectFactory = org.apache.shiro.mgt.SubjectFactory;
	using Session = org.apache.shiro.session.Session;
	using PrincipalCollection = org.apache.shiro.subject.PrincipalCollection;
	using Subject = org.apache.shiro.subject.Subject;
	using SubjectContext = org.apache.shiro.subject.SubjectContext;

	public class ShiroSubjectFactory : SubjectFactory
	{
		 public override Subject CreateSubject( SubjectContext context )
		 {
			  SecurityManager securityManager = context.resolveSecurityManager();
			  Session session = context.resolveSession();
			  bool sessionCreationEnabled = context.SessionCreationEnabled;
			  PrincipalCollection principals = context.resolvePrincipals();
			  bool authenticated = context.resolveAuthenticated();
			  string host = context.resolveHost();
			  ShiroAuthenticationInfo authcInfo = ( ShiroAuthenticationInfo ) context.AuthenticationInfo;

			  return new ShiroSubject( principals, authenticated, host, session, sessionCreationEnabled, securityManager, authcInfo.AuthenticationResult, authcInfo );
		 }
	}

}