/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Server
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseDependencies = Neo4Net.Graphdb.facade.GraphDatabaseDependencies;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using Config = Neo4Net.Kernel.configuration.Config;
	using CommunityGraphFactory = Neo4Net.Server.database.CommunityGraphFactory;
	using GraphFactory = Neo4Net.Server.database.GraphFactory;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.database_path;

	public class ServerBootstrapperTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.SuppressOutput suppress = org.neo4j.test.rule.SuppressOutput.suppress(org.neo4j.test.rule.SuppressOutput.System.out);
		 public readonly SuppressOutput Suppress = SuppressOutput.suppress( SuppressOutput.System.out );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory homeDir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory HomeDir = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotThrowNullPointerExceptionIfConfigurationValidationFails() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotThrowNullPointerExceptionIfConfigurationValidationFails()
		 {
			  // given
			  ServerBootstrapper serverBootstrapper = new ServerBootstrapperAnonymousInnerClass( this );

			  File dir = Files.createTempDirectory( "test-server-bootstrapper" ).toFile();
			  dir.deleteOnExit();

			  // when
			  Neo4Net.Server.ServerBootstrapper.Start( dir, null, MapUtil.stringMap( database_path.name(), HomeDir.absolutePath().AbsolutePath ) );

			  // then no exceptions are thrown and
			  assertThat( Suppress.OutputVoice.lines(), not(empty()) );
		 }

		 private class ServerBootstrapperAnonymousInnerClass : ServerBootstrapper
		 {
			 private readonly ServerBootstrapperTest _outerInstance;

			 public ServerBootstrapperAnonymousInnerClass( ServerBootstrapperTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override GraphFactory createGraphFactory( Config config )
			 {
				  return new CommunityGraphFactory();
			 }

			 protected internal override NeoServer createNeoServer( GraphFactory graphFactory, Config config, GraphDatabaseDependencies dependencies )
			 {
				  return mock( typeof( NeoServer ) );
			 }
		 }
	}

}