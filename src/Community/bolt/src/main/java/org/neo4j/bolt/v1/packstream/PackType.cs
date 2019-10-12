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
namespace Neo4Net.Bolt.v1.packstream
{
	/// <summary>
	/// These are the primitive types that PackStream can represent. They map to the non-graph primitives of the Neo4j
	/// type system. Graph primitives and rich composite types are represented as <seealso cref="STRUCT"/>.
	/// </summary>
	public enum PackType
	{
		 /// <summary>
		 /// The absence of a value </summary>
		 Null,
		 /// <summary>
		 /// You know what this is </summary>
		 Boolean,
		 /// <summary>
		 /// 64-bit signed integer </summary>
		 Integer,
		 /// <summary>
		 /// 64-bit floating point number </summary>
		 Float,
		 /// <summary>
		 /// Binary data </summary>
		 Bytes,
		 /// <summary>
		 /// Unicode string </summary>
		 String,
		 /// <summary>
		 /// Sequence of zero or more values </summary>
		 List,
		 /// <summary>
		 /// Sequence of zero or more key/value pairs, keys are unique </summary>
		 Map,
		 /// <summary>
		 /// A composite data structure, made up of zero or more packstream values and a type signature. </summary>
		 Struct,
		 /// <summary>
		 /// A marker that denotes the end of a streamed value </summary>
		 EndOfStream,
		 /// <summary>
		 /// Undefined type, reserved for future use </summary>
		 Reserved
	}

}