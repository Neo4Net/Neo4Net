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
namespace Neo4Net.Server.enterprise.jmx
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;

	using GraphDatabaseDependencies = Neo4Net.GraphDb.facade.GraphDatabaseDependencies;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Config = Neo4Net.Kernel.configuration.Config;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using EnterpriseServerBuilder = Neo4Net.Server.enterprise.helpers.EnterpriseServerBuilder;
	using CleanupRule = Neo4Net.Test.rule.CleanupRule;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

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