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
namespace Neo4Net.Kernel.impl.proc
{
	using Test = org.junit.Test;

	using Config = Neo4Net.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.procedure_unrestricted;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.procedure_whitelist;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.proc.ProcedureConfig.PROC_ALLOWED_SETTING_DEFAULT_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.proc.ProcedureConfig.PROC_ALLOWED_SETTING_ROLES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.security.enterprise.configuration.SecuritySettings.default_allowed;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.security.enterprise.configuration.SecuritySettings.procedure_roles;

	public class ProcedureConfigTest
	{
		 private static readonly string[] _empty = new string[]{};

		 private static string[] ArrayOf( params string[] values )
		 {
			  return values;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveEmptyDefaultConfigs()
		 public virtual void ShouldHaveEmptyDefaultConfigs()
		 {
			  Config config = Config.defaults();
			  ProcedureConfig procConfig = new ProcedureConfig( config );
			  assertThat( procConfig.RolesFor( "x" ), equalTo( _empty ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveConfigsWithDefaultProcedureAllowed()
		 public virtual void ShouldHaveConfigsWithDefaultProcedureAllowed()
		 {
			  Config config = Config.defaults( default_allowed, "role1" );
			  ProcedureConfig procConfig = new ProcedureConfig( config );
			  assertThat( procConfig.RolesFor( "x" ), equalTo( ArrayOf( "role1" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveConfigsWithExactMatchProcedureAllowed()
		 public virtual void ShouldHaveConfigsWithExactMatchProcedureAllowed()
		 {
			  Config config = Config.defaults( stringMap( PROC_ALLOWED_SETTING_DEFAULT_NAME, "role1", PROC_ALLOWED_SETTING_ROLES, "xyz:anotherRole" ) );
			  ProcedureConfig procConfig = new ProcedureConfig( config );
			  assertThat( procConfig.RolesFor( "xyz" ), equalTo( ArrayOf( "anotherRole" ) ) );
			  assertThat( procConfig.RolesFor( "abc" ), equalTo( ArrayOf( "role1" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFailOnEmptyStringDefaultName()
		 public virtual void ShouldNotFailOnEmptyStringDefaultName()
		 {
			  Config config = Config.defaults( default_allowed, "" );
			  new ProcedureConfig( config );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFailOnEmptyStringRoles()
		 public virtual void ShouldNotFailOnEmptyStringRoles()
		 {
			  Config config = Config.defaults( procedure_roles, "" );
			  new ProcedureConfig( config );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFailOnBadStringRoles()
		 public virtual void ShouldNotFailOnBadStringRoles()
		 {
			  Config config = Config.defaults( procedure_roles, "matrix" );
			  new ProcedureConfig( config );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFailOnEmptyStringBoth()
		 public virtual void ShouldNotFailOnEmptyStringBoth()
		 {
			  Config config = Config.defaults( stringMap( PROC_ALLOWED_SETTING_DEFAULT_NAME, "", PROC_ALLOWED_SETTING_ROLES, "" ) );
			  new ProcedureConfig( config );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveConfigsWithWildcardProcedureAllowed()
		 public virtual void ShouldHaveConfigsWithWildcardProcedureAllowed()
		 {
			  Config config = Config.defaults( stringMap( PROC_ALLOWED_SETTING_DEFAULT_NAME, "role1", PROC_ALLOWED_SETTING_ROLES, "xyz*:anotherRole" ) );
			  ProcedureConfig procConfig = new ProcedureConfig( config );
			  assertThat( procConfig.RolesFor( "xyzabc" ), equalTo( ArrayOf( "anotherRole" ) ) );
			  assertThat( procConfig.RolesFor( "abcxyz" ), equalTo( ArrayOf( "role1" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveConfigsWithWildcardProcedureAllowedAndNoDefault()
		 public virtual void ShouldHaveConfigsWithWildcardProcedureAllowedAndNoDefault()
		 {
			  Config config = Config.defaults( procedure_roles, "xyz*:anotherRole" );
			  ProcedureConfig procConfig = new ProcedureConfig( config );
			  assertThat( procConfig.RolesFor( "xyzabc" ), equalTo( ArrayOf( "anotherRole" ) ) );
			  assertThat( procConfig.RolesFor( "abcxyz" ), equalTo( _empty ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveConfigsWithMultipleWildcardProcedureAllowedAndNoDefault()
		 public virtual void ShouldHaveConfigsWithMultipleWildcardProcedureAllowedAndNoDefault()
		 {
			  Config config = Config.defaults( procedure_roles, "apoc.convert.*:apoc_reader;apoc.load.json:apoc_writer;apoc.trigger.add:TriggerHappy" );
			  ProcedureConfig procConfig = new ProcedureConfig( config );
			  assertThat( procConfig.RolesFor( "xyz" ), equalTo( _empty ) );
			  assertThat( procConfig.RolesFor( "apoc.convert.xml" ), equalTo( ArrayOf( "apoc_reader" ) ) );
			  assertThat( procConfig.RolesFor( "apoc.convert.json" ), equalTo( ArrayOf( "apoc_reader" ) ) );
			  assertThat( procConfig.RolesFor( "apoc.load.xml" ), equalTo( _empty ) );
			  assertThat( procConfig.RolesFor( "apoc.load.json" ), equalTo( ArrayOf( "apoc_writer" ) ) );
			  assertThat( procConfig.RolesFor( "apoc.trigger.add" ), equalTo( ArrayOf( "TriggerHappy" ) ) );
			  assertThat( procConfig.RolesFor( "apoc.convert-json" ), equalTo( _empty ) );
			  assertThat( procConfig.RolesFor( "apoc.load-xml" ), equalTo( _empty ) );
			  assertThat( procConfig.RolesFor( "apoc.load-json" ), equalTo( _empty ) );
			  assertThat( procConfig.RolesFor( "apoc.trigger-add" ), equalTo( _empty ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveConfigsWithOverlappingMatchingWildcards()
		 public virtual void ShouldHaveConfigsWithOverlappingMatchingWildcards()
		 {
			  Config config = Config.defaults( stringMap( PROC_ALLOWED_SETTING_DEFAULT_NAME, "default", PROC_ALLOWED_SETTING_ROLES, "apoc.*:apoc;apoc.load.*:loader;apoc.trigger.*:trigger;apoc.trigger.add:TriggerHappy" ) );
			  ProcedureConfig procConfig = new ProcedureConfig( config );
			  assertThat( procConfig.RolesFor( "xyz" ), equalTo( ArrayOf( "default" ) ) );
			  assertThat( procConfig.RolesFor( "apoc.convert.xml" ), equalTo( ArrayOf( "apoc" ) ) );
			  assertThat( procConfig.RolesFor( "apoc.load.xml" ), equalTo( ArrayOf( "apoc", "loader" ) ) );
			  assertThat( procConfig.RolesFor( "apoc.trigger.add" ), equalTo( ArrayOf( "apoc", "trigger", "TriggerHappy" ) ) );
			  assertThat( procConfig.RolesFor( "apoc.trigger.remove" ), equalTo( ArrayOf( "apoc", "trigger" ) ) );
			  assertThat( procConfig.RolesFor( "apoc.load-xml" ), equalTo( ArrayOf( "apoc" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportSeveralRolesPerPattern()
		 public virtual void ShouldSupportSeveralRolesPerPattern()
		 {
			  Config config = Config.defaults( procedure_roles, "xyz*:role1,role2,  role3  ,    role4   ;    abc:  role3   ,role1" );
			  ProcedureConfig procConfig = new ProcedureConfig( config );
			  assertThat( procConfig.RolesFor( "xyzabc" ), equalTo( ArrayOf( "role1", "role2", "role3", "role4" ) ) );
			  assertThat( procConfig.RolesFor( "abc" ), equalTo( ArrayOf( "role3", "role1" ) ) );
			  assertThat( procConfig.RolesFor( "abcxyz" ), equalTo( _empty ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowFullAccessDefault()
		 public virtual void ShouldNotAllowFullAccessDefault()
		 {
			  Config config = Config.defaults();
			  ProcedureConfig procConfig = new ProcedureConfig( config );
			  assertThat( procConfig.FullAccessFor( "x" ), equalTo( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowFullAccessForProcedures()
		 public virtual void ShouldAllowFullAccessForProcedures()
		 {
			  Config config = Config.defaults( procedure_unrestricted, "test.procedure.name" );
			  ProcedureConfig procConfig = new ProcedureConfig( config );

			  assertThat( procConfig.FullAccessFor( "xyzabc" ), equalTo( false ) );
			  assertThat( procConfig.FullAccessFor( "test.procedure.name" ), equalTo( true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowFullAccessForSeveralProcedures()
		 public virtual void ShouldAllowFullAccessForSeveralProcedures()
		 {
			  Config config = Config.defaults( procedure_unrestricted, "test.procedure.name, test.procedure.otherName" );
			  ProcedureConfig procConfig = new ProcedureConfig( config );

			  assertThat( procConfig.FullAccessFor( "xyzabc" ), equalTo( false ) );
			  assertThat( procConfig.FullAccessFor( "test.procedure.name" ), equalTo( true ) );
			  assertThat( procConfig.FullAccessFor( "test.procedure.otherName" ), equalTo( true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowFullAccessForSeveralProceduresOddNames()
		 public virtual void ShouldAllowFullAccessForSeveralProceduresOddNames()
		 {
			  Config config = Config.defaults( procedure_unrestricted, "test\\.procedure.name, test*rocedure.otherName" );
			  ProcedureConfig procConfig = new ProcedureConfig( config );

			  assertThat( procConfig.FullAccessFor( "xyzabc" ), equalTo( false ) );
			  assertThat( procConfig.FullAccessFor( "test\\.procedure.name" ), equalTo( true ) );
			  assertThat( procConfig.FullAccessFor( "test*procedure.otherName" ), equalTo( true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowFullAccessWildcardProceduresNames()
		 public virtual void ShouldAllowFullAccessWildcardProceduresNames()
		 {
			  Config config = Config.defaults( procedure_unrestricted, " test.procedure.*  ,     test.*.otherName" );
			  ProcedureConfig procConfig = new ProcedureConfig( config );

			  assertThat( procConfig.FullAccessFor( "xyzabc" ), equalTo( false ) );
			  assertThat( procConfig.FullAccessFor( "test.procedure.name" ), equalTo( true ) );
			  assertThat( procConfig.FullAccessFor( "test.procedure.otherName" ), equalTo( true ) );
			  assertThat( procConfig.FullAccessFor( "test.other.otherName" ), equalTo( true ) );
			  assertThat( procConfig.FullAccessFor( "test.other.cool.otherName" ), equalTo( true ) );
			  assertThat( procConfig.FullAccessFor( "test.other.name" ), equalTo( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBlockWithWhiteListingForProcedures()
		 public virtual void ShouldBlockWithWhiteListingForProcedures()
		 {
			  Config config = Config.defaults( stringMap( procedure_unrestricted.name(), "test.procedure.name, test.procedure.name2", procedure_whitelist.name(), "test.procedure.name" ) );
			  ProcedureConfig procConfig = new ProcedureConfig( config );

			  assertThat( procConfig.IsWhitelisted( "xyzabc" ), equalTo( false ) );
			  assertThat( procConfig.IsWhitelisted( "test.procedure.name" ), equalTo( true ) );
			  assertThat( procConfig.IsWhitelisted( "test.procedure.name2" ), equalTo( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowWhiteListsWildcardProceduresNames()
		 public virtual void ShouldAllowWhiteListsWildcardProceduresNames()
		 {
			  Config config = Config.defaults( procedure_whitelist, " test.procedure.* ,  test.*.otherName" );
			  ProcedureConfig procConfig = new ProcedureConfig( config );

			  assertThat( procConfig.IsWhitelisted( "xyzabc" ), equalTo( false ) );
			  assertThat( procConfig.IsWhitelisted( "test.procedure.name" ), equalTo( true ) );
			  assertThat( procConfig.IsWhitelisted( "test.procedure.otherName" ), equalTo( true ) );
			  assertThat( procConfig.IsWhitelisted( "test.other.otherName" ), equalTo( true ) );
			  assertThat( procConfig.IsWhitelisted( "test.other.cool.otherName" ), equalTo( true ) );
			  assertThat( procConfig.IsWhitelisted( "test.other.name" ), equalTo( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreOddRegex()
		 public virtual void ShouldIgnoreOddRegex()
		 {
			  Config config = Config.defaults( procedure_whitelist, "[\\db^a]*" );
			  ProcedureConfig procConfig = new ProcedureConfig( config );
			  assertThat( procConfig.IsWhitelisted( "123" ), equalTo( false ) );
			  assertThat( procConfig.IsWhitelisted( "b" ), equalTo( false ) );
			  assertThat( procConfig.IsWhitelisted( "a" ), equalTo( false ) );

			  config = Config.defaults( procedure_whitelist, "(abc)" );
			  procConfig = new ProcedureConfig( config );
			  assertThat( procConfig.IsWhitelisted( "(abc)" ), equalTo( true ) );

			  config = Config.defaults( procedure_whitelist, "^$" );
			  procConfig = new ProcedureConfig( config );
			  assertThat( procConfig.IsWhitelisted( "^$" ), equalTo( true ) );

			  config = Config.defaults( procedure_whitelist, "\\" );
			  procConfig = new ProcedureConfig( config );
			  assertThat( procConfig.IsWhitelisted( "\\" ), equalTo( true ) );

			  config = Config.defaults( procedure_whitelist, "&&" );
			  procConfig = new ProcedureConfig( config );
			  assertThat( procConfig.IsWhitelisted( "&&" ), equalTo( true ) );

			  config = Config.defaults( procedure_whitelist, "\\p{Lower}" );
			  procConfig = new ProcedureConfig( config );
			  assertThat( procConfig.IsWhitelisted( "a" ), equalTo( false ) );
			  assertThat( procConfig.IsWhitelisted( "\\p{Lower}" ), equalTo( true ) );

			  config = Config.defaults( procedure_whitelist, "a+" );
			  procConfig = new ProcedureConfig( config );
			  assertThat( procConfig.IsWhitelisted( "aaaaaa" ), equalTo( false ) );
			  assertThat( procConfig.IsWhitelisted( "a+" ), equalTo( true ) );

			  config = Config.defaults( procedure_whitelist, "a|b" );
			  procConfig = new ProcedureConfig( config );
			  assertThat( procConfig.IsWhitelisted( "a" ), equalTo( false ) );
			  assertThat( procConfig.IsWhitelisted( "b" ), equalTo( false ) );
			  assertThat( procConfig.IsWhitelisted( "|" ), equalTo( false ) );
			  assertThat( procConfig.IsWhitelisted( "a|b" ), equalTo( true ) );

			  config = Config.defaults( procedure_whitelist, "[a-c]" );
			  procConfig = new ProcedureConfig( config );
			  assertThat( procConfig.IsWhitelisted( "a" ), equalTo( false ) );
			  assertThat( procConfig.IsWhitelisted( "b" ), equalTo( false ) );
			  assertThat( procConfig.IsWhitelisted( "c" ), equalTo( false ) );
			  assertThat( procConfig.IsWhitelisted( "-" ), equalTo( false ) );
			  assertThat( procConfig.IsWhitelisted( "[a-c]" ), equalTo( true ) );

			  config = Config.defaults( procedure_whitelist, "a\tb" );
			  procConfig = new ProcedureConfig( config );
			  assertThat( procConfig.IsWhitelisted( "a    b" ), equalTo( false ) );
			  assertThat( procConfig.IsWhitelisted( "a\tb" ), equalTo( true ) );
		 }
	}

}