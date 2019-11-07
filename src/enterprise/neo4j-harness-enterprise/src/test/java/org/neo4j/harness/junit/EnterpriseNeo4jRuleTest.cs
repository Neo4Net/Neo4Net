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
namespace Neo4Net.Harness.junit
{
	using HttpStatus = org.eclipse.jetty.http.HttpStatus;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using MyEnterpriseUnmanagedExtension = Neo4Net.Harness.extensionpackage.MyEnterpriseUnmanagedExtension;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using LegacySslPolicyConfig = Neo4Net.Kernel.configuration.ssl.LegacySslPolicyConfig;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsEqual.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.ServerTestUtils.getRelativePath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.ServerTestUtils.getSharedTestTemporaryFolder;

	public class EnterpriseNeo4NetRuleTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4NetRule Neo4Net = new EnterpriseNeo4NetRule().withConfig(Neo4Net.kernel.configuration.ssl.LegacySslPolicyConfig.certificates_directory.name(), getRelativePath(getSharedTestTemporaryFolder(), Neo4Net.kernel.configuration.ssl.LegacySslPolicyConfig.certificates_directory)).withExtension("/test", Neo4Net.harness.extensionpackage.MyEnterpriseUnmanagedExtension.class).withConfig(Neo4Net.kernel.impl.enterprise.configuration.OnlineBackupSettings.online_backup_enabled, Neo4Net.kernel.configuration.Settings.FALSE);
		 public Neo4NetRule Neo4Net = new EnterpriseNeo4NetRule().withConfig(LegacySslPolicyConfig.certificates_directory.name(), getRelativePath(SharedTestTemporaryFolder, LegacySslPolicyConfig.certificates_directory)).withExtension("/test", typeof(MyEnterpriseUnmanagedExtension)).withConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.SuppressOutput suppressOutput = Neo4Net.test.rule.SuppressOutput.suppressAll();
		 public SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExtensionWork()
		 public virtual void ShouldExtensionWork()
		 {
			  // Given running enterprise server
			  string doSomethingUri = Neo4Net.httpURI().resolve("test/myExtension/doSomething").ToString();

			  // When I run this test

			  // Then
			  HTTP.Response response = HTTP.GET( doSomethingUri );
			  assertThat( response.Status(), equalTo(234) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testPropertyExistenceConstraintCanBeCreated()
		 public virtual void TestPropertyExistenceConstraintCanBeCreated()
		 {
			  // Given running enterprise server
			  string createConstraintUri = Neo4Net.httpURI().resolve("test/myExtension/createConstraint").ToString();

			  // When I run this server

			  // Then constraint should be created
			  HTTP.Response response = HTTP.GET( createConstraintUri );
			  assertThat( response.Status(), equalTo(HttpStatus.CREATED_201) );
		 }
	}

}