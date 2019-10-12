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
namespace Neo4Net.Harness.@internal
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Settings = Neo4Net.Kernel.configuration.Settings;
	using LegacySslPolicyConfig = Neo4Net.Kernel.configuration.ssl.LegacySslPolicyConfig;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using ServerTestUtils = Neo4Net.Server.ServerTestUtils;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class EnterpriseInProcessServerBuilderIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDir = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.SuppressOutput suppressOutput = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLaunchAServerInSpecifiedDirectory()
		 public virtual void ShouldLaunchAServerInSpecifiedDirectory()
		 {
			  // Given
			  File workDir = new File( TestDir.directory(), "specific" );
			  workDir.mkdir();

			  // When
			  using ( ServerControls server = GetTestServerBuilder( workDir ).newServer() )
			  {
					// Then
					assertThat( HTTP.GET( server.HttpURI().ToString() ).status(), equalTo(200) );
					assertThat( workDir.list().length, equalTo(1) );
			  }

			  // And after it's been closed, it should've cleaned up after itself.
			  assertThat( Arrays.ToString( workDir.list() ), workDir.list().length, equalTo(0) );
		 }

		 private TestServerBuilder GetTestServerBuilder( File workDir )
		 {
			  string certificatesDirectoryKey = LegacySslPolicyConfig.certificates_directory.name();
			  string certificatesDirectoryValue = ServerTestUtils.getRelativePath( TestDir.directory(), LegacySslPolicyConfig.certificates_directory );

			  return EnterpriseTestServerBuilders.newInProcessBuilder( workDir ).withConfig( certificatesDirectoryKey, certificatesDirectoryValue ).withConfig( OnlineBackupSettings.online_backup_enabled, Settings.FALSE );
		 }
	}

}