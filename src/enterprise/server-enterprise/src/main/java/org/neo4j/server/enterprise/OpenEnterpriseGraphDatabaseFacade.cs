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

namespace Neo4Net.Server.enterprise
{

	using GraphDatabaseFacadeFactory = Neo4Net.GraphDb.facade.GraphDatabaseFacadeFactory;
	using Dependencies = Neo4Net.GraphDb.facade.GraphDatabaseFacadeFactory.Dependencies;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;

	public class OpenEnterpriseGraphDatabaseFacade : GraphDatabaseFacade
	{
		 public OpenEnterpriseGraphDatabaseFacade( File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  ( new GraphDatabaseFacadeFactory( DatabaseInfo.ENTERPRISE, OpenEnterpriseEditionModule::new ) ).initFacade( storeDir, config, dependencies, this );
		 }
	}

}