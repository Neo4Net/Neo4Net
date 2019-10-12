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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{
	using Org.Neo4j.Index.@internal.gbptree;
	using Value = Org.Neo4j.Values.Storable.Value;

	/// <summary>
	/// Value in a <seealso cref="GBPTree"/> handling numbers suitable for schema indexing.
	/// 
	/// NOTE:  For the time being no data exists in <seealso cref="NativeIndexValue"/>, but since the layout is under development
	/// it's very convenient to have this class still exist so that it's very easy to try out different types
	/// of layouts without changing the entire stack of arguments. In the end it may just be that this class
	/// will be deleted, but for now it sticks around.
	/// </summary>
	internal class NativeIndexValue
	{
		 internal const int SIZE = 0;

		 internal static readonly NativeIndexValue Instance = new NativeIndexValue();

		 internal virtual void From( params Value[] values )
		 {
			  // not needed a.t.m.
		 }

		 public override string ToString()
		 {
			  return "[no value]";
		 }
	}

}