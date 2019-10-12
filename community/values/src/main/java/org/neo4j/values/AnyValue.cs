using System;

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
namespace Org.Neo4j.Values
{
	public abstract class AnyValue
	{
		 private int _hash;

		 // this should be final, but Mockito barfs if it is,
		 // so we need to just manually ensure it isn't overridden
		 public override bool Equals( object other )
		 {
			  return this == other || other != null && Eq( other );
		 }

		 public override sealed int GetHashCode()
		 {
			  //We will always recompute hashcode for values
			  //where `hashCode == 0`, e.g. empty strings and empty lists
			  //however that shouldn't be shouldn't be too costly
			  if ( _hash == 0 )
			  {
					_hash = ComputeHash();
			  }
			  return _hash;
		 }

		 protected internal abstract bool Eq( object other );

		 protected internal abstract int ComputeHash();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract <E extends Exception> void writeTo(AnyValueWriter<E> writer) throws E;
		 public abstract void writeTo<E>( AnyValueWriter<E> writer ) where E : Exception;

		 public virtual bool SequenceValue
		 {
			 get
			 {
				  return false; // per default Values are no SequenceValues
			 }
		 }

		 public abstract bool? TernaryEquals( AnyValue other );

		 public abstract T map<T>( ValueMapper<T> mapper );

		 public abstract string TypeName { get; }
	}

}