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
namespace Org.Neo4j.Kernel.impl.store.counts.keys
{
	using CountsVisitor = Org.Neo4j.Kernel.Impl.Api.CountsVisitor;

	public sealed class IndexStatisticsKey : IndexKey
	{
		 internal IndexStatisticsKey( long indexId ) : base( indexId, CountsKeyType.IndexStatistics )
		 {
		 }

		 public override void Accept( CountsVisitor visitor, long updates, long size )
		 {
			  visitor.VisitIndexStatistics( IndexId(), updates, size );
		 }

		 public override int CompareTo( CountsKey other )
		 {
			  if ( other is IndexStatisticsKey )
			  {
					return base.CompareTo( other );
			  }
			  return RecordType().compareTo(other.RecordType());
		 }
	}

}