using System.Collections.Generic;

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
namespace Neo4Net.Bolt
{
	using RandomStringUtils = org.apache.commons.lang3.RandomStringUtils;
	using AfterClass = org.junit.AfterClass;
	using BeforeClass = org.junit.BeforeClass;
	using ClassRule = org.junit.ClassRule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Driver = Neo4Net.driver.v1.Driver;
	using GraphDatabase = Neo4Net.driver.v1.GraphDatabase;
	using Record = Neo4Net.driver.v1.Record;
	using Session = Neo4Net.driver.v1.Session;
	using StatementResult = Neo4Net.driver.v1.StatementResult;
	using Neo4NetRule = Neo4Net.Harness.junit.Neo4NetRule;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.runners.Parameterized.Parameter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.runners.Parameterized.Parameters;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.driver.v1.Values.parameters;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class BoltDriverLargePropertiesIT
	public class BoltDriverLargePropertiesIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.Neo4Net.harness.junit.Neo4NetRule db = new org.Neo4Net.harness.junit.Neo4NetRule().withConfig(org.Neo4Net.kernel.impl.enterprise.configuration.OnlineBackupSettings.online_backup_enabled, org.Neo4Net.kernel.configuration.Settings.FALSE);
		 public static readonly Neo4NetRule Db = new Neo4NetRule().withConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE);

		 private static Driver _driver;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameter public int size;
		 public int Size;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void SetUp()
		 {
			  _driver = GraphDatabase.driver( Db.boltURI() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void TearDown()
		 {
			  if ( _driver != null )
			  {
					_driver.close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters(name = "{0}") public static java.util.List<Object> arraySizes()
		 public static IList<object> ArraySizes()
		 {
			  return Arrays.asList( 1, 2, 3, 10, 999, 4_295, 10_001, 55_155, 100_000 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendAndReceiveString()
		 public virtual void ShouldSendAndReceiveString()
		 {
			  string originalValue = RandomStringUtils.randomAlphanumeric( Size );
			  object receivedValue = SendAndReceive( originalValue );
			  assertEquals( originalValue, receivedValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @SuppressWarnings("unchecked") public void shouldSendAndReceiveLongArray()
		 public virtual void ShouldSendAndReceiveLongArray()
		 {
			  IList<long> originalLongs = ThreadLocalRandom.current().longs(Size).boxed().collect(toList());
			  IList<long> receivedLongs = ( IList<long> ) SendAndReceive( originalLongs );
			  assertEquals( originalLongs, receivedLongs );
		 }

		 private static object SendAndReceive( object value )
		 {
			  using ( Session session = _driver.session() )
			  {
					StatementResult result = session.run( "RETURN $value", parameters( "value", value ) );
					Record record = result.single();
					return record.get( 0 ).asObject();
			  }
		 }
	}

}