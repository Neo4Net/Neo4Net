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
namespace Org.Neo4j.Cypher.@internal.codegen
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using EnterpriseDatabaseRule = Org.Neo4j.Test.rule.EnterpriseDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;

	public class CompiledRuntimeEchoIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.EnterpriseDatabaseRule db = new org.neo4j.test.rule.EnterpriseDatabaseRule();
		 public readonly EnterpriseDatabaseRule Db = new EnterpriseDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToEchoMaps()
		 public virtual void ShouldBeAbleToEchoMaps()
		 {
			  Echo( map( "foo", "bar" ) );
			  Echo( map( "foo", 42L ) );
			  Echo( map( "foo", map( "bar", map( "baz", 1337L ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToEchoLists()
		 public virtual void ShouldBeAbleToEchoLists()
		 {
			  Echo( asList( 1L, 2L, 3L ) );
			  Echo( asList( "a", 1L, 17L ) );
			  Echo( map( "foo", asList( asList( 1L, 2L, 3L ), "foo" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToEchoListsOfMaps()
		 public virtual void ShouldBeAbleToEchoListsOfMaps()
		 {
			  Echo( singletonList( map( "foo", "bar" ) ) );
			  Echo( asList( "a", 1L, 17L, map( "foo", asList( 1L, 2L, 3L ) ) ) );
			  Echo( asList( "foo", asList( map( "bar", 42L ), "foo" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToEchoMapsOfLists()
		 public virtual void ShouldBeAbleToEchoMapsOfLists()
		 {
			  Echo( map( "foo", singletonList( "bar" ) ) );
			  Echo( map( "foo", singletonList( map( "bar", map( "baz", 1337L ) ) ) ) );
		 }

		 private void Echo( object value )
		 {
			  object result = Db.execute( "CYPHER runtime=compiled RETURN {p} AS p", map( "p", value ) ).next().get("p");
			  assertThat( result, equalTo( value ) );
		 }
	}

}