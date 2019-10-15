﻿/*
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
namespace Neo4Net.Server.enterprise.modules
{

	using AuthManager = Neo4Net.Kernel.api.security.AuthManager;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using AuthorizationModule = Neo4Net.Server.modules.AuthorizationModule;
	using AuthorizationDisabledFilter = Neo4Net.Server.rest.dbms.AuthorizationDisabledFilter;
	using EnterpriseAuthorizationDisabledFilter = Neo4Net.Server.rest.dbms.EnterpriseAuthorizationDisabledFilter;
	using WebServer = Neo4Net.Server.web.WebServer;

	public class EnterpriseAuthorizationModule : AuthorizationModule
	{
		 public EnterpriseAuthorizationModule( WebServer webServer, System.Func<AuthManager> authManager, LogProvider logProvider, Config config, Pattern[] uriWhitelist ) : base( webServer, authManager, logProvider, config, uriWhitelist )
		 {
		 }

		 protected internal override AuthorizationDisabledFilter CreateAuthorizationDisabledFilter()
		 {
			  return new EnterpriseAuthorizationDisabledFilter();
		 }
	}

}