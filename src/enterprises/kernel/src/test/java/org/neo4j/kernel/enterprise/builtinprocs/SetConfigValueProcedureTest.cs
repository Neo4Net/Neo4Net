/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Kernel.enterprise.builtinprocs
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using InvalidSettingException = Neo4Net.Graphdb.config.InvalidSettingException;
	using Config = Neo4Net.Kernel.configuration.Config;
	using NestedThrowableMatcher = Neo4Net.Test.matchers.NestedThrowableMatcher;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentEnterpriseDatabaseRule = Neo4Net.Test.rule.ImpermanentEnterpriseDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.log_queries;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.plugin_dir;

	public class SetConfigValueProcedureTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.ImpermanentEnterpriseDatabaseRule();
		 public readonly DatabaseRule Db = new ImpermanentEnterpriseDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.ExpectedException expect = org.junit.rules.ExpectedException.none();
		 public readonly ExpectedException Expect = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void configShouldBeAffected()
		 public virtual void ConfigShouldBeAffected()
		 {
			  Config config = Db.resolveDependency( typeof( Config ) );

			  Db.execute( "CALL dbms.setConfigValue('" + log_queries.name() + "', 'false')" );
			  assertFalse( config.Get( log_queries ) );

			  Db.execute( "CALL dbms.setConfigValue('" + log_queries.name() + "', 'true')" );
			  assertTrue( config.Get( log_queries ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failIfUnknownSetting()
		 public virtual void FailIfUnknownSetting()
		 {
			  Expect.expect( new NestedThrowableMatcher( typeof( System.ArgumentException ) ) );
			  Expect.expectMessage( "Unknown setting: unknown.setting.indeed" );

			  Db.execute( "CALL dbms.setConfigValue('unknown.setting.indeed', 'foo')" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failIfStaticSetting()
		 public virtual void FailIfStaticSetting()
		 {
			  Expect.expect( new NestedThrowableMatcher( typeof( System.ArgumentException ) ) );
			  Expect.expectMessage( "Setting is not dynamic and can not be changed at runtime" );

			  // Static setting, at least for now
			  Db.execute( "CALL dbms.setConfigValue('" + plugin_dir.name() + "', 'path/to/dir')" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failIfInvalidValue()
		 public virtual void FailIfInvalidValue()
		 {
			  Expect.expect( new NestedThrowableMatcher( typeof( InvalidSettingException ) ) );
			  Expect.expectMessage( "Bad value 'invalid' for setting 'dbms.logs.query.enabled': must be 'true' or 'false'" );

			  Db.execute( "CALL dbms.setConfigValue('" + log_queries.name() + "', 'invalid')" );
		 }
	}

}