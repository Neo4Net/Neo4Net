/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Kernel.impl.core
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using Node = Org.Neo4j.Graphdb.Node;
	using DateTimeValue = Org.Neo4j.Values.Storable.DateTimeValue;
	using DateValue = Org.Neo4j.Values.Storable.DateValue;
	using LocalTimeValue = Org.Neo4j.Values.Storable.LocalTimeValue;
	using Value = Org.Neo4j.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class TestCombinedPropertyTypes : AbstractNeo4jTestCase
	{
		 private Node _node1;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void createInitialNode()
		 public virtual void CreateInitialNode()
		 {
			  _node1 = GraphDb.createNode();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void deleteInitialNode()
		 public virtual void DeleteInitialNode()
		 {
			  _node1.delete();
		 }

		 protected internal override bool RestartGraphDbBetweenTests()
		 {
			  return true;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDateTypeOrdinalDayWithPrecedingInLinedLong()
		 public virtual void TestDateTypeOrdinalDayWithPrecedingInLinedLong()
		 {
			  TestDateTypeWithPrecedingInLinedLong( DateValue.ordinalDate( 4800, 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDateTypeOrdinalDayWithPrecedingNotInLinedLong()
		 public virtual void TestDateTypeOrdinalDayWithPrecedingNotInLinedLong()
		 {
			  TestDateTypeWithPrecedingNotInLinedLong( DateValue.ordinalDate( 4800, 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLocalTimeWithPrecedingInLinedLong()
		 public virtual void TestLocalTimeWithPrecedingInLinedLong()
		 {
			  TestDateTypeWithPrecedingInLinedLong( LocalTimeValue.parse( "13:45:02" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLocalTimeWithPrecedingNotInLinedLong()
		 public virtual void TestLocalTimeWithPrecedingNotInLinedLong()
		 {
			  TestDateTypeWithPrecedingNotInLinedLong( LocalTimeValue.parse( "13:45:02" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDateTimeWithPrecedingInLinedLong()
		 public virtual void TestDateTimeWithPrecedingInLinedLong()
		 {
			  TestDateTypeWithPrecedingInLinedLong( DateTimeValue.datetime( DateValue.parse( "2018-04-01" ), LocalTimeValue.parse( "01:02:03" ), ZoneId.of( "Europe/Stockholm" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDateTimeWithPrecedingNotInLinedLong()
		 public virtual void TestDateTimeWithPrecedingNotInLinedLong()
		 {
			  TestDateTypeWithPrecedingNotInLinedLong( DateTimeValue.datetime( DateValue.parse( "2018-04-01" ), LocalTimeValue.parse( "01:02:03" ), ZoneId.of( "Europe/Stockholm" ) ) );
		 }

		 private void TestDateTypeWithPrecedingInLinedLong( Value value )
		 {
			  _node1.setProperty( "l1", 255 ); // Setting these low bits was triggering a bug in some date types decision on formatting
			  string key = "dt";
			  _node1.setProperty( key, value );
			  NewTransaction();

			  object property = _node1.getProperty( key );
			  assertEquals( value.AsObjectCopy(), property );
		 }

		 private void TestDateTypeWithPrecedingNotInLinedLong( Value value )
		 {
			  _node1.setProperty( "l1", long.MaxValue );
			  string key = "dt";
			  _node1.setProperty( key, value );
			  NewTransaction();

			  object property = _node1.getProperty( key );
			  assertEquals( value.AsObjectCopy(), property );
		 }
	}

}