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
namespace Org.Neo4j.Helpers.Collection
{
	using Org.Neo4j.Collection;
	using Org.Neo4j.Function;

	internal class RawMapIterator<FROM, TO, EX> : RawIterator<TO, EX> where EX : Exception
	{
		 private readonly RawIterator<FROM, EX> _fromIterator;
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private final org.neo4j.function.ThrowingFunction<? super FROM,? extends TO,EX> function;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private readonly ThrowingFunction<object, ? extends TO, EX> _function;

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: RawMapIterator(org.neo4j.collection.RawIterator<FROM,EX> fromIterator, org.neo4j.function.ThrowingFunction<? super FROM,? extends TO,EX> function)
		 internal RawMapIterator<T1>( RawIterator<FROM, EX> fromIterator, ThrowingFunction<T1> function ) where T1 : TO
		 {
			  this._fromIterator = fromIterator;
			  this._function = function;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean hasNext() throws EX
		 public override bool HasNext()
		 {
			  return _fromIterator.hasNext();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TO next() throws EX
		 public override TO Next()
		 {
			  FROM from = _fromIterator.next();
			  return _function.apply( from );
		 }

		 public override void Remove()
		 {
			  _fromIterator.remove();
		 }
	}

}