﻿/*
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
namespace Org.Neo4j.@unsafe.Impl.Batchimport
{
	using GatheringMemoryStatsVisitor = Org.Neo4j.@unsafe.Impl.Batchimport.cache.GatheringMemoryStatsVisitor;
	using MemoryStatsVisitor = Org.Neo4j.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor;
	using DetailLevel = Org.Neo4j.@unsafe.Impl.Batchimport.stats.DetailLevel;
	using GenericStatsProvider = Org.Neo4j.@unsafe.Impl.Batchimport.stats.GenericStatsProvider;
	using Keys = Org.Neo4j.@unsafe.Impl.Batchimport.stats.Keys;
	using Stat = Org.Neo4j.@unsafe.Impl.Batchimport.stats.Stat;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Format.bytes;

	/// <summary>
	/// Provides <seealso cref="Stat statistics"/> about memory usage, as the key <seealso cref="Keys.memory_usage"/>
	/// </summary>
	public class MemoryUsageStatsProvider : GenericStatsProvider, Stat
	{
		 private readonly Org.Neo4j.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor_Visitable[] _users;

		 public MemoryUsageStatsProvider( params Org.Neo4j.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor_Visitable[] users )
		 {
			  this._users = users;
			  Add( Keys.memory_usage, this );
		 }

		 public override DetailLevel DetailLevel()
		 {
			  return DetailLevel.IMPORTANT;
		 }

		 public override long AsLong()
		 {
			  GatheringMemoryStatsVisitor visitor = new GatheringMemoryStatsVisitor();
			  foreach ( Org.Neo4j.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor_Visitable user in _users )
			  {
					user.AcceptMemoryStatsVisitor( visitor );
			  }
			  return visitor.HeapUsage + visitor.OffHeapUsage;
		 }

		 public override string ToString()
		 {
			  return bytes( AsLong() );
		 }
	}

}