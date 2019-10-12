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
namespace Org.Neo4j.Server.enterprise
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using GraphDatabaseDependencies = Org.Neo4j.Graphdb.facade.GraphDatabaseDependencies;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using BoltConnector = Org.Neo4j.Kernel.configuration.BoltConnector;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Mode = Org.Neo4j.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings.Mode;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.enterprise.configuration.EnterpriseEditionSettings.mode;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class OpenEnterpriseNeoServerTest
	internal class OpenEnterpriseNeoServerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkExpectedDatabaseDirectory()
		 internal virtual void CheckExpectedDatabaseDirectory()
		 {
			  Config config = Config.builder().withServerDefaults().withSetting(mode, Mode.SINGLE.name()).withSetting(GraphDatabaseSettings.neo4j_home, _testDirectory.storeDir().AbsolutePath).withSetting((new BoltConnector("bolt")).listen_address.name(), "localhost:0").withSetting((new BoltConnector("http")).listen_address.name(), "localhost:0").withSetting((new BoltConnector("https")).listen_address.name(), "localhost:0").build();
			  GraphDatabaseDependencies dependencies = GraphDatabaseDependencies.newDependencies().userLogProvider(NullLogProvider.Instance);
			  OpenEnterpriseNeoServer server = new OpenEnterpriseNeoServer( config, dependencies );

			  server.Start();
			  try
			  {
					Path expectedPath = Paths.get( _testDirectory.storeDir().Path, "data", "databases", "graph.db" );
					GraphDatabaseFacade graph = server.Database.Graph;
					assertEquals( expectedPath, graph.DatabaseLayout().databaseDirectory().toPath() );
			  }
			  finally
			  {
					server.Stop();
			  }
		 }
	}

}