using System;

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
namespace Neo4Net.Values.Storable
{
	using HashFunction = Neo4Net.Hashing.HashFunction;
	using Neo4Net.Values;

	/// <summary>
	/// Not a value.
	/// 
	/// The NULL object of the Value world. Is implemented as a singleton, to allow direct reference equality checks (==),
	/// and avoid unnecessary object creation.
	/// </summary>
	public sealed class NoValue : Value
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public static final NoValue NO_VALUE = new NoValue();
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static readonly NoValue NoValueConflict = new NoValue();

		 private NoValue()
		 {
		 }

		 public override bool Eq( object other )
		 {
			  return this == other;
		 }

		 public override bool? TernaryEquals( AnyValue other )
		 {
			  return null;
		 }

		 public override T Map<T>( ValueMapper<T> mapper )
		 {
			  return mapper.MapNoValue();
		 }

		 public override long UpdateHash( HashFunction hashFunction, long hash )
		 {
			  return hashFunction.Update( hash, GetHashCode() );
		 }

		 public override int ComputeHash()
		 {
			  return System.identityHashCode( this );
		 }

		 public override bool Equals( Value other )
		 {
			  return this == other;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void writeTo(ValueWriter<E> writer) throws E
		 public override void WriteTo<E>( ValueWriter<E> writer ) where E : Exception
		 {
			  writer.WriteNull();
		 }

		 public override object AsObjectCopy()
		 {
			  return null;
		 }

		 public override string ToString()
		 {
			  return PrettyPrint();
		 }

		 public override string PrettyPrint()
		 {
			  return TypeName;
		 }

		 public override string TypeName
		 {
			 get
			 {
				  return "NO_VALUE";
			 }
		 }

		 public override ValueGroup ValueGroup()
		 {
			  return ValueGroup.NoValue;
		 }

		 public override NumberType NumberType()
		 {
			  return NumberType.NoNumber;
		 }

		 internal override int UnsafeCompareTo( Value other )
		 {
			  return 0;
		 }
	}

}