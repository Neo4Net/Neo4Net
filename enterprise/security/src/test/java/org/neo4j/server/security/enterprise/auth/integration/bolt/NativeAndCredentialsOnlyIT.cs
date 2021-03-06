﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Server.security.enterprise.auth.integration.bolt
{
	using Test = org.junit.Test;


	using Org.Neo4j.Graphdb.config;
	using SecuritySettings = Org.Neo4j.Server.security.enterprise.configuration.SecuritySettings;

	public class NativeAndCredentialsOnlyIT : EnterpriseAuthenticationTestBase
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: protected java.util.Map<org.neo4j.graphdb.config.Setting<?>, String> getSettings()
		 protected internal override IDictionary<Setting<object>, string> Settings
		 {
			 get
			 {
				  return Collections.singletonMap( SecuritySettings.auth_providers, "native,plugin-TestCredentialsOnlyPlugin" );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAuthenticateWithCredentialsOnlyPlugin()
		 public virtual void ShouldAuthenticateWithCredentialsOnlyPlugin()
		 {
			  AssertAuth( "", "BASE64-ENC-PASSWORD", "plugin-TestCredentialsOnlyPlugin" );
		 }
	}

}