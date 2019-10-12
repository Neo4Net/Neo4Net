/*
 * Copyright (c) 2018-2019 "GraphFoundation" [https://graphfoundation.org]
 *
 * The included source code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html)
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 */
namespace Org.Neo4j.Server.enterprise
{
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using EnterpriseEditionModule = Org.Neo4j.Kernel.impl.enterprise.EnterpriseEditionModule;

	public class OpenEnterpriseEditionModule : EnterpriseEditionModule
	{
		 public OpenEnterpriseEditionModule( PlatformModule platformModule ) : base( platformModule )
		 {
		 }
	}

}