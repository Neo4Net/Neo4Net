﻿using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Server.modules
{

	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using ServerSettings = Org.Neo4j.Server.configuration.ServerSettings;
	using SecurityFilter = Org.Neo4j.Server.rest.security.SecurityFilter;
	using SecurityRule = Org.Neo4j.Server.rest.security.SecurityRule;
	using WebServer = Org.Neo4j.Server.web.WebServer;

	public class SecurityRulesModule : ServerModule
	{
		 private readonly WebServer _webServer;
		 private readonly Config _config;
		 private readonly Log _log;

		 private SecurityFilter _mountedFilter;

		 public SecurityRulesModule( WebServer webServer, Config config, LogProvider logProvider )
		 {
			  this._webServer = webServer;
			  this._config = config;
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 public override void Start()
		 {
			  IEnumerable<SecurityRule> securityRules = SecurityRules;
			  if ( Iterables.count( securityRules ) > 0 )
			  {
					_mountedFilter = new SecurityFilter( securityRules );

					_webServer.addFilter( _mountedFilter, "/*" );

					foreach ( SecurityRule rule in securityRules )
					{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
						 _log.info( "Security rule [%s] installed on server", rule.GetType().FullName );
					}
			  }
		 }

		 public override void Stop()
		 {
			  if ( _mountedFilter != null )
			  {
					_mountedFilter.destroy();
			  }
		 }

		 private IEnumerable<SecurityRule> SecurityRules
		 {
			 get
			 {
				  List<SecurityRule> rules = new List<SecurityRule>();
   
				  foreach ( string classname in _config.get( ServerSettings.security_rules ) )
				  {
						try
						{
							 rules.Add( ( SecurityRule )System.Activator.CreateInstance( Type.GetType( classname ) ) );
						}
						catch ( Exception e )
						{
							 _log.error( "Could not load server security rule [%s], exception details: ", classname, e.Message );
							 Console.WriteLine( e.ToString() );
							 Console.Write( e.StackTrace );
						}
				  }
   
				  return rules;
			 }
		 }
	}

}