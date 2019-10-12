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
namespace Neo4Net.Bolt.v1.messaging.example
{
	using AnyValue = Neo4Net.Values.AnyValue;
	using Values = Neo4Net.Values.Storable.Values;
	using NodeValue = Neo4Net.Values.@virtual.NodeValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.nodeValue;

	public class Nodes
	{
		 public static readonly NodeValue Alice = nodeValue( 1001L, stringArray( "Person", "Employee" ), VirtualValues.map( new string[]{ "name", "age" }, new AnyValue[]{ stringValue( "Alice" ), Values.longValue( 33L ) } ) );
		 public static readonly NodeValue Bob = nodeValue( 1002L, stringArray( "Person", "Employee" ), VirtualValues.map( new string[]{ "name", "age" }, new AnyValue[]{ stringValue( "Bob" ), Values.longValue( 44L ) } ) );
		 public static readonly NodeValue Carol = nodeValue( 1003L, stringArray( "Person" ), VirtualValues.map( new string[]{ "name" }, new AnyValue[]{ stringValue( "Carol" ) } ) );
		 public static readonly NodeValue Dave = nodeValue( 1004L, stringArray(), VirtualValues.map(new string[]{ "name" }, new AnyValue[]{ stringValue("Dave") }) );

		 private Nodes()
		 {
		 }
	}

}