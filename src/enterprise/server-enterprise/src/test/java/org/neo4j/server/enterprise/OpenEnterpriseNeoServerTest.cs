/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Server.enterprise
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using GraphDatabaseDependencies = Neo4Net.GraphDb.facade.GraphDatabaseDependencies;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Mode = Neo4Net.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings.Mode;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.enterprise.configuration.EnterpriseEditionSettings.mode;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class OpenEnterpriseNeoServerTest
	internal class OpenEnterpriseNeoServerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkExpectedDatabaseDirectory()
		 internal virtual void CheckExpectedDatabaseDirectory()
		 {
			  Config config = Config.builder().withServerDefaults().withSetting(mode, Mode.SINGLE.name()).withSetting(GraphDatabaseSettings.Neo4Net_home, _testDirectory.storeDir().AbsolutePath).withSetting((new BoltConnector("bolt")).listen_address.name(), "localhost:0").withSetting((new BoltConnector("http")).listen_address.name(), "localhost:0").withSetting((new BoltConnector("https")).listen_address.name(), "localhost:0").build();
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