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
namespace Org.Neo4j.Server.enterprise.jmx
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;

	using GraphDatabaseDependencies = Org.Neo4j.Graphdb.facade.GraphDatabaseDependencies;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using EnterpriseServerBuilder = Org.Neo4j.Server.enterprise.helpers.EnterpriseServerBuilder;
	using CleanupRule = Org.Neo4j.Test.rule.CleanupRule;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;

	public class ServerManagementIT
	{
		private bool InstanceFieldsInitialized = false;

		public ServerManagementIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _suppressOutput ).around( _baseDir ).around( _cleanup );
		}

		 private readonly CleanupRule _cleanup = new CleanupRule();
		 private readonly TestDirectory _baseDir = TestDirectory.testDirectory();
		 private readonly SuppressOutput _suppressOutput = SuppressOutput.suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(suppressOutput).around(baseDir).around(cleanup);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRestartServer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRestartServer()
		 {
			  // Given
			  string dataDirectory1 = _baseDir.directory( "data1" ).AbsolutePath;
			  string dataDirectory2 = _baseDir.directory( "data2" ).AbsolutePath;

			  Config config = Config.fromFile( EnterpriseServerBuilder.serverOnRandomPorts().withDefaultDatabaseTuning().usingDataDir(dataDirectory1).createConfigFiles() ).withHome(_baseDir.directory()).withSetting(GraphDatabaseSettings.logs_directory, _baseDir.directory("logs").Path).build();

			  // When
			  NeoServer server = _cleanup.add( new OpenEnterpriseNeoServer( config, GraphDbDependencies() ) );
			  server.Start();

			  assertNotNull( server.Database.Graph );
			  assertEquals( config.Get( GraphDatabaseSettings.database_path ).AbsolutePath, server.Database.Location.AbsolutePath );

			  // Change the database location
			  config.Augment( GraphDatabaseSettings.data_directory, dataDirectory2 );
			  ServerManagement bean = new ServerManagement( server );
			  bean.RestartServer();

			  // Then
			  assertNotNull( server.Database.Graph );
			  assertEquals( config.Get( GraphDatabaseSettings.database_path ).AbsolutePath, server.Database.Location.AbsolutePath );
		 }

		 private static GraphDatabaseDependencies GraphDbDependencies()
		 {
			  return GraphDatabaseDependencies.newDependencies().userLogProvider(NullLogProvider.Instance);
		 }
	}

}