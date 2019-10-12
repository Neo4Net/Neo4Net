using System.Collections.Generic;

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
namespace Neo4Net.Values.Storable
{


	/// <summary>
	/// Array of one of the storable primitives
	/// </summary>
	public abstract class ArrayValue : Value, SequenceValue
	{
		public abstract bool? TernaryEquality( SequenceValue other );
		public abstract int CompareUsingIterators( SequenceValue a, SequenceValue b, IComparer<AnyValue> comparator );
		public abstract int CompareUsingRandomAccess( SequenceValue a, SequenceValue b, IComparer<AnyValue> comparator );
		public abstract int CompareToSequence( SequenceValue other, IComparer<AnyValue> comparator );
		public abstract bool? TernaryEqualsUsingIterators( SequenceValue a, SequenceValue b );
		public abstract bool EqualsUsingIterators( SequenceValue a, SequenceValue b );
		public abstract bool? TernaryEqualsUsingRandomAccess( SequenceValue a, SequenceValue b );
		public abstract bool EqualsUsingRandomAccess( SequenceValue a, SequenceValue b );
		public abstract boolean ( SequenceValue other );
		public abstract AnyValue Value( int offset );
		 public override abstract int Length();

		 public override Neo4Net.Values.SequenceValue_IterationPreference IterationPreference()
		 {
			  return Neo4Net.Values.SequenceValue_IterationPreference.RandomAccess;
		 }

		 public override IEnumerator<AnyValue> Iterator()
		 {
			  return new IteratorAnonymousInnerClass( this );
		 }

		 private class IteratorAnonymousInnerClass : IEnumerator<AnyValue>
		 {
			 private readonly ArrayValue _outerInstance;

			 public IteratorAnonymousInnerClass( ArrayValue outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 private int offset;

			 public bool hasNext()
			 {
				  return offset < outerInstance.Length();
			 }

			 public AnyValue next()
			 {
				  if ( !hasNext() )
				  {
						throw new NoSuchElementException();
				  }
				  return outerInstance.Value( offset++ );
			 }
		 }

		 public override bool Eq( object other )
		 {
			  if ( other == null )
			  {
					return false;
			  }

			  return other is SequenceValue && this.Equals( ( SequenceValue ) other );
		 }

		 public sealed override bool Equals( bool x )
		 {
			  return false;
		 }

		 public sealed override bool Equals( long x )
		 {
			  return false;
		 }

		 public sealed override bool Equals( double x )
		 {
			  return false;
		 }

		 public sealed override bool Equals( char x )
		 {
			  return false;
		 }

		 public sealed override bool Equals( string x )
		 {
			  return false;
		 }

		 public override bool SequenceValue
		 {
			 get
			 {
				  return true;
			 }
		 }
	}

}