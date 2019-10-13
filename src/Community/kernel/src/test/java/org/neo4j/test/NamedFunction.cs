﻿using System;

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
namespace Neo4Net.Test
{
	using Neo4Net.Function;

	public abstract class NamedFunction<FROM, TO> : ThrowingFunction<FROM, TO, Exception>
	{
		public abstract void ThrowIfPresent( Optional<E> exception );
		public abstract System.Func<T, R> CatchThrown( Type clazz, ThrowingFunction<T, R, E> throwing );
		public abstract R Apply( T t );
		 private readonly string _name;

		 public NamedFunction( string name )
		 {
			  this._name = name;
		 }

		 public override string ToString()
		 {
			  return _name;
		 }
	}

}