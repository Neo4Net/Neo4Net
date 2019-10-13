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
namespace Neo4Net.Server.security.enterprise.auth.plugin
{

	using PredefinedRoles = Neo4Net.Server.security.enterprise.auth.plugin.api.PredefinedRoles;
	using AuthorizationInfo = Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthorizationInfo;
	using AuthorizationPlugin = Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthorizationPlugin;

	public class TestAuthorizationPlugin : Neo4Net.Server.security.enterprise.auth.plugin.spi.AuthorizationPlugin_Adapter
	{
		 public override string Name()
		 {
			  return this.GetType().Name;
		 }

		 public override AuthorizationInfo Authorize( ICollection<PrincipalAndProvider> principals )
		 {
			  if ( principals.Any( p => "neo4j".Equals( p.principal() ) ) )
			  {
					return ( AuthorizationInfo )() => Collections.singleton(PredefinedRoles.READER);
			  }
			  return null;
		 }
	}

}