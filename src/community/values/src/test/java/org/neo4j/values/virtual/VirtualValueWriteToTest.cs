using System.Collections.Generic;

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
namespace Neo4Net.Values.@virtual
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;

	using Specials = Neo4Net.Values.Storable.BufferValueWriter.Specials;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.BufferAnyValueWriter.Specials.beginList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.BufferAnyValueWriter.Specials.beginMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.BufferAnyValueWriter.Specials.endList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.BufferAnyValueWriter.Specials.endMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.BufferAnyValueWriter.Specials.writeRelationship;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.BufferAnyValueWriter.Specials.writeRelationshipReference;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.BufferAnyValueWriter.Specials.writeNode;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.BufferAnyValueWriter.Specials.writeNodeReference;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.BufferAnyValueWriter.Specials.writePath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.booleanValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.byteArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.charValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.intValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.stringArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.relationship;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.relationshipValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.emptyMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.nodeValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(value = Parameterized.class) public class VirtualValueWriteToTest
	public class VirtualValueWriteToTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static Iterable<WriteTest> data()
		 public static IEnumerable<WriteTest> Data()
		 {
			  return Arrays.asList( ShouldWrite( VirtualValues.List( booleanValue( false ), byteArray( new sbyte[]{ 3, 4, 5 } ), stringValue( "yo" ) ), beginList( 3 ), false, Specials.byteArray( new sbyte[]{ 3, 4, 5 } ), "yo", endList() ), ShouldWrite(VirtualValues.Map(new string[]{ "foo", "bar" }, new AnyValue[]{ intValue(100), charValue('c') }), beginMap(2), "bar", 'c', "foo", 100, endMap()), ShouldWrite(VirtualValues.Node(1L), writeNodeReference(1L)), ShouldWrite(relationship(2L), writeRelationshipReference(2L)), ShouldWrite(VirtualValues.Path(new NodeValue[]{ nodeValue(20L, stringArray("L"), emptyMap()), nodeValue(40L, stringArray("L"), emptyMap()) }, new RelationshipValue[]{ relationshipValue(100L, nodeValue(40L, stringArray("L"), emptyMap()), nodeValue(20L, stringArray("L"), emptyMap()), stringValue("T"), emptyMap()) }), writePath(new NodeValue[]{ nodeValue(20L, stringArray("L"), emptyMap()), nodeValue(40L, stringArray("L"), emptyMap()) }, new RelationshipValue[]{ relationshipValue(100L, nodeValue(40L, stringArray("L"), emptyMap()), nodeValue(20L, stringArray("L"), emptyMap()), stringValue("T"), emptyMap()) })), ShouldWrite(VirtualValues.Map(new string[]{ "foo" }, new AnyValue[]{ VirtualValues.List(VirtualValues.Map(new string[]{ "bar" }, new AnyValue[]{ VirtualValues.List() })) }), beginMap(1), "foo", beginList(1), beginMap(1), "bar", beginList(0), endList(), endMap(), endList(), endMap()), ShouldWrite(nodeValue(1337L, stringArray("L1", "L2"), map(new string[]{ "foo" }, new AnyValue[]{ stringValue("foo") })), writeNode(1337L, stringArray("L1", "L2"), map(new string[]{ "foo" }, new AnyValue[]{ stringValue("foo") }))), ShouldWrite(relationshipValue(1337L, nodeValue(42L, stringArray("L"), emptyMap()), nodeValue(43L, stringArray("L"), emptyMap()), stringValue("T"), map(new string[]{ "foo" }, new AnyValue[]{ stringValue("foo") })), writeRelationship(1337L, 42L, 43L, stringValue("T"), map(new string[]{ "foo" }, new AnyValue[]{ stringValue("foo") }))) );
		 }

		 private WriteTest _currentTest;

		 public VirtualValueWriteToTest( WriteTest currentTest )
		 {
			  this._currentTest = currentTest;
		 }

		 private static WriteTest ShouldWrite( AnyValue value, params object[] expected )
		 {
			  return new WriteTest( value, expected );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void runTest()
		 public virtual void RunTest()
		 {
			  _currentTest.verifyWriteTo();
		 }

		 private class WriteTest
		 {
			  internal readonly AnyValue Value;
			  internal readonly object[] Expected;

			  internal WriteTest( AnyValue value, params object[] expected )
			  {
					this.Value = value;
					this.Expected = expected;
			  }

			  public override string ToString()
			  {
					return string.Format( "{0} should write {1}", Value, Arrays.ToString( Expected ) );
			  }

			  internal virtual void VerifyWriteTo()
			  {
					BufferAnyValueWriter writer = new BufferAnyValueWriter();
					Value.writeTo( writer );
					writer.AssertBuffer( Expected );
			  }
		 }
	}

}