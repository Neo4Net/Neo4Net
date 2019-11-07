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
namespace Neo4Net.Server
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Neo4NetRule = Neo4Net.Harness.junit.Neo4NetRule;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using LegacySslPolicyConfig = Neo4Net.Kernel.configuration.ssl.LegacySslPolicyConfig;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using ServerSettings = Neo4Net.Server.configuration.ServerSettings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.ServerTestUtils.getRelativePath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.ServerTestUtils.getSharedTestTemporaryFolder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.server.HTTP.RawPayload.quotedJson;
	using static Neo4Net.Test.server.HTTP.Response;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.server.HTTP.withBaseUri;

	public class BatchEndpointIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.harness.junit.Neo4NetRule Neo4Net = new Neo4Net.harness.junit.Neo4NetRule().withConfig(Neo4Net.kernel.configuration.ssl.LegacySslPolicyConfig.certificates_directory, getRelativePath(getSharedTestTemporaryFolder(), Neo4Net.kernel.configuration.ssl.LegacySslPolicyConfig.certificates_directory)).withConfig(Neo4Net.graphdb.factory.GraphDatabaseSettings.logs_directory, getRelativePath(getSharedTestTemporaryFolder(), Neo4Net.graphdb.factory.GraphDatabaseSettings.logs_directory)).withConfig(Neo4Net.server.configuration.ServerSettings.http_logging_enabled, "true").withConfig(Neo4Net.graphdb.factory.GraphDatabaseSettings.auth_enabled, "false").withConfig(Neo4Net.kernel.impl.enterprise.configuration.OnlineBackupSettings.online_backup_enabled, Neo4Net.kernel.configuration.Settings.FALSE);
		 public readonly Neo4NetRule Neo4Net = new Neo4NetRule().withConfig(LegacySslPolicyConfig.certificates_directory, getRelativePath(SharedTestTemporaryFolder, LegacySslPolicyConfig.certificates_directory)).withConfig(GraphDatabaseSettings.logs_directory, getRelativePath(SharedTestTemporaryFolder, GraphDatabaseSettings.logs_directory)).withConfig(ServerSettings.http_logging_enabled, "true").withConfig(GraphDatabaseSettings.auth_enabled, "false").withConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void requestsShouldNotFailWhenHttpLoggingIsOn()
		 public virtual void RequestsShouldNotFailWhenHttpLoggingIsOn()
		 {
			  // Given
			  string body = "[" +
								 "{'method': 'POST', 'to': '/node', 'body': {'age': 1}, 'id': 1} ]";

			  // When
			  Response response = withBaseUri( Neo4Net.httpURI() ).withHeaders("Content-Type", "application/json").POST("db/data/batch", quotedJson(body));

			  // Then
			  assertEquals( 200, response.status() );
		 }
	}

}