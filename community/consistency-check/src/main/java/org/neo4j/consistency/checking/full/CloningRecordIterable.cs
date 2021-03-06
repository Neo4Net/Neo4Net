﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Consistency.checking.full
{

	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;

	public class CloningRecordIterable<R> : IEnumerable<R> where R : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
	{
		 private readonly IEnumerable<R> _actual;

		 public CloningRecordIterable( IEnumerable<R> actual )
		 {
			  this._actual = actual;
		 }

		 public override IEnumerator<R> Iterator()
		 {
			  return new CloningRecordIterator<R>( _actual.GetEnumerator() );
		 }

		 public static IEnumerable<R> Cloned<R>( IEnumerable<R> actual ) where R : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  return new CloningRecordIterable<R>( actual );
		 }
	}

}