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
namespace Neo4Net.@unsafe.Impl.Batchimport.input
{
	using Test = org.junit.Test;

	using Race = Neo4Net.Test.Race;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.Groups.LOWEST_NONGLOBAL_ID;

	public class GroupsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleConcurrentGetOrCreate() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleConcurrentGetOrCreate()
		 {
			  // GIVEN
			  Groups groups = new Groups();
			  Race race = new Race();
			  string name = "MyGroup";
			  for ( int i = 0; i < Runtime.Runtime.availableProcessors(); i++ )
			  {
					race.AddContestant(() =>
					{
					 Group group = groups.GetOrCreate( name );
					 assertEquals( LOWEST_NONGLOBAL_ID, group.Id() );
					});
			  }

			  // WHEN
			  race.Go();

			  // THEN
			  Group otherGroup = groups.GetOrCreate( "MyOtherGroup" );
			  assertEquals( LOWEST_NONGLOBAL_ID + 1, otherGroup.Id() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportMixedGroupModeInGetOrCreate()
		 public virtual void ShouldSupportMixedGroupModeInGetOrCreate()
		 {
			  // given
			  Groups groups = new Groups();
			  assertEquals( Group_Fields.Global, groups.GetOrCreate( null ) );

			  // when
			  assertNotEquals( Group_Fields.Global, groups.GetOrCreate( "Something" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportMixedGroupModeInGetOrCreate2()
		 public virtual void ShouldSupportMixedGroupModeInGetOrCreate2()
		 {
			  // given
			  Groups groups = new Groups();
			  assertNotEquals( Group_Fields.Global, groups.GetOrCreate( "Something" ) );

			  // when
			  assertEquals( Group_Fields.Global, groups.GetOrCreate( null ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetCreatedGroup()
		 public virtual void ShouldGetCreatedGroup()
		 {
			  // given
			  Groups groups = new Groups();
			  string name = "Something";
			  Group createdGroup = groups.GetOrCreate( name );

			  // when
			  Group gottenGroup = groups.Get( name );

			  // then
			  assertSame( createdGroup, gottenGroup );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetGlobalGroup()
		 public virtual void ShouldGetGlobalGroup()
		 {
			  // given
			  Groups groups = new Groups();
			  groups.GetOrCreate( null );

			  // when
			  Group group = groups.Get( null );

			  // then
			  assertSame( Group_Fields.Global, group );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportMixedGroupModeInGet()
		 public virtual void ShouldSupportMixedGroupModeInGet()
		 {
			  // given
			  Groups groups = new Groups();
			  groups.GetOrCreate( "Something" );

			  // when
			  assertEquals( Group_Fields.Global, groups.Get( null ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = HeaderException.class) public void shouldFailOnGettingNonExistentGroup()
		 public virtual void ShouldFailOnGettingNonExistentGroup()
		 {
			  // given
			  Groups groups = new Groups();

			  // when
			  groups.Get( "Something" );
		 }
	}

}