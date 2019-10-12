using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo Technology,"
 * Network Engine for Objects in Lund AB [http://neotechnology.com]
 *
 * Modifications Copyright (c) 2018-2019 "GraphFoundation" [https://graphfoundation.org]
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

	using CausalClusterConfigurationValidator = Org.Neo4j.causalclustering.core.CausalClusterConfigurationValidator;
	using HaConfigurationValidator = Org.Neo4j.Configuration.HaConfigurationValidator;
	using GraphDatabaseDependencies = Org.Neo4j.Graphdb.facade.GraphDatabaseDependencies;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConfigurationValidator = Org.Neo4j.Kernel.configuration.ConfigurationValidator;
	using GraphFactory = Org.Neo4j.Server.database.GraphFactory;

	/// <summary>
	/// See https://github.com/neo4j/neo4j/blob/3.2/enterprise/server-enterprise/src/main/java/org/neo4j/server/enterprise/EnterpriseBootstrapper.java
	/// </summary>
	public class EnterpriseBootstrapper : CommunityBootstrapper
	{
		 /// 
		 /// <param name="config">
		 /// @return </param>
		 protected internal virtual GraphFactory CreateGraphFactory( Config config )
		 {
			  return new OpenEnterpriseGraphFactory();
		 }

		 /// 
		 /// <param name="graphFactory"> </param>
		 /// <param name="config"> </param>
		 /// <param name="dependencies">
		 /// @return </param>
		 protected internal virtual NeoServer CreateNeoServer( GraphFactory graphFactory, Config config, GraphDatabaseDependencies dependencies )
		 {
			  return new EnterpriseNeoServer( config, graphFactory, dependencies );
		 }

		 /// <summary>
		 /// @return
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull protected java.util.Collection<org.neo4j.kernel.configuration.ConfigurationValidator> configurationValidators()
		 protected internal virtual ICollection<ConfigurationValidator> ConfigurationValidators()
		 {
			  IList<ConfigurationValidator> validators = new List<object>( base.ConfigurationValidators() );
			  // validators.addAll( super.configurationValidators() );
			  validators.Add( new HaConfigurationValidator() );
			  validators.Add( new CausalClusterConfigurationValidator() );
			  return validators;
		 }
	}

}