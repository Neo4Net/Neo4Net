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
namespace Org.Neo4j.Server.rest
{
	using AfterClass = org.junit.AfterClass;
	using Before = org.junit.Before;
	using BeforeClass = org.junit.BeforeClass;
	using ClassRule = org.junit.ClassRule;
	using TemporaryFolder = org.junit.rules.TemporaryFolder;

	using EnterpriseServerBuilder = Org.Neo4j.Server.enterprise.helpers.EnterpriseServerBuilder;
	using FunctionalTestHelper = Org.Neo4j.Server.helpers.FunctionalTestHelper;
	using ExclusiveServerTestBase = Org.Neo4j.Test.server.ExclusiveServerTestBase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.SuppressOutput.suppressAll;

	public abstract class EnterpriseVersionIT : ExclusiveServerTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static org.junit.rules.TemporaryFolder staticFolder = new org.junit.rules.TemporaryFolder();
		 public static TemporaryFolder StaticFolder = new TemporaryFolder();
		 protected internal static NeoServer Server;
		 internal static FunctionalTestHelper FunctionalTestHelper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setupServer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void SetupServer()
		 {
			  Server = EnterpriseServerBuilder.serverOnRandomPorts().usingDataDir(StaticFolder.Root.AbsolutePath).build();

			  suppressAll().call((Callable<Void>)() =>
			  {
				Server.start();
				return null;
			  });
			  FunctionalTestHelper = new FunctionalTestHelper( Server );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void stopServer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void StopServer()
		 {
			  suppressAll().call((Callable<Void>)() =>
			  {
				Server.stop();
				return null;
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupTheDatabase()
		 public virtual void SetupTheDatabase()
		 {
			  // do nothing, we don't care about the database contents here
		 }
	}

}