using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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
namespace Neo4Net.Values.@virtual
{

	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using Neo4Net.Helpers.Collections;
	using Neo4Net.Values;
	using Neo4Net.Values;
	using ArrayValue = Neo4Net.Values.Storable.ArrayValue;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.SequenceValue_IterationPreference.RANDOM_ACCESS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.NO_VALUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.ArrayHelpers.containsNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_LIST;

	public abstract class ListValue : VirtualValue, SequenceValue, IEnumerable<AnyValue>
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
		public abstract SequenceValue_IterationPreference IterationPreference();
		 public abstract int Size();

		 public override abstract AnyValue Value( int offset );

		 public override string TypeName
		 {
			 get
			 {
				  return "List";
			 }
		 }

		 internal sealed class ArrayValueListValue : ListValue
		 {
			  internal readonly ArrayValue Array;

			  internal ArrayValueListValue( ArrayValue array )
			  {
					this.Array = array;
			  }

			  public override Neo4Net.Values.SequenceValue_IterationPreference IterationPreference()
			  {
					return RANDOM_ACCESS;
			  }

			  public override bool Storable()
			  {
					return true;
			  }

			  public override ArrayValue ToStorableArray()
			  {
					return Array;
			  }

			  public override int Size()
			  {
					return Array.length();
			  }

			  public override AnyValue Value( int offset )
			  {
					return Array.value( offset );
			  }

			  public override int ComputeHash()
			  {
					return Array.GetHashCode();
			  }
		 }

		 internal sealed class ArrayListValue : ListValue
		 {
			  internal readonly AnyValue[] Values;

			  internal ArrayListValue( AnyValue[] values )
			  {
					Debug.Assert( values != null );
					Debug.Assert( !containsNull( values ) );

					this.Values = values;
			  }

			  public override Neo4Net.Values.SequenceValue_IterationPreference IterationPreference()
			  {
					return RANDOM_ACCESS;
			  }

			  public override int Size()
			  {
					return Values.Length;
			  }

			  public override AnyValue Value( int offset )
			  {
					return Values[offset];
			  }

			  public override AnyValue[] AsArray()
			  {
					return Values;
			  }

			  public override int ComputeHash()
			  {
					return Arrays.GetHashCode( Values );
			  }
		 }

		 internal sealed class JavaListListValue : ListValue
		 {
			  internal readonly IList<AnyValue> Values;

			  internal JavaListListValue( IList<AnyValue> values )
			  {
					Debug.Assert( values != null );
					Debug.Assert( !containsNull( values ) );

					this.Values = values;
			  }

			  public override Neo4Net.Values.SequenceValue_IterationPreference IterationPreference()
			  {
					return Neo4Net.Values.SequenceValue_IterationPreference.Iteration;
			  }

			  public override int Size()
			  {
					return Values.Count;
			  }

			  public override AnyValue Value( int offset )
			  {
					return Values[offset];
			  }

			  public override AnyValue[] AsArray()
			  {
					return Values.ToArray();
			  }

			  public override int ComputeHash()
			  {
					return Values.GetHashCode();
			  }

			  public override IEnumerator<AnyValue> Iterator()
			  {
					return Values.GetEnumerator();
			  }
		 }

		 internal sealed class ListSlice : ListValue
		 {
			  internal readonly ListValue Inner;
			  internal readonly int From;
			  internal readonly int To;

			  internal ListSlice( ListValue inner, int from, int to )
			  {
					Debug.Assert( from >= 0 );
					Debug.Assert( to <= inner.Size() );
					Debug.Assert( from <= to );
					this.Inner = inner;
					this.From = from;
					this.To = to;
			  }

			  public override Neo4Net.Values.SequenceValue_IterationPreference IterationPreference()
			  {
					return Inner.iterationPreference();
			  }

			  public override int Size()
			  {
					return To - From;
			  }

			  public override AnyValue Value( int offset )
			  {
					return Inner.value( offset + From );
			  }

			  public override IEnumerator<AnyValue> Iterator()
			  {
					switch ( Inner.iterationPreference() )
					{
					case RANDOM_ACCESS:
						 return base.GetEnumerator();
					case ITERATION:
						 return new PrefetchingIteratorAnonymousInnerClass( this );

					default:
						 throw new System.InvalidOperationException( "unknown iteration preference" );
					}
			  }

			  private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<AnyValue>
			  {
				  private readonly ListSlice _outerInstance;

				  public PrefetchingIteratorAnonymousInnerClass( ListSlice outerInstance )
				  {
					  this.outerInstance = outerInstance;
					  innerIterator = outerInstance.Inner.GetEnumerator();
				  }

				  private int count;
				  private IEnumerator<AnyValue> innerIterator;

				  protected internal override AnyValue fetchNextOrNull()
				  {
						//make sure we are at least at first element
						while ( count < _outerInstance.from && innerIterator.hasNext() )
						{
							 innerIterator.next();
							 count++;
						}
						//check if we are done
						if ( count < _outerInstance.from || count >= _outerInstance.to || !innerIterator.hasNext() )
						{
							 return null;
						}
						//take the next step
						count++;
						return innerIterator.next();
				  }
			  }
		 }

		 internal sealed class ReversedList : ListValue
		 {
			  internal readonly ListValue Inner;

			  internal ReversedList( ListValue inner )
			  {
					this.Inner = inner;
			  }

			  public override Neo4Net.Values.SequenceValue_IterationPreference IterationPreference()
			  {
					return Inner.iterationPreference();
			  }

			  public override int Size()
			  {
					return Inner.size();
			  }

			  public override AnyValue Value( int offset )
			  {
					return Inner.value( Size() - 1 - offset );
			  }
		 }

		 internal sealed class DropNoValuesListValue : ListValue
		 {
			  internal readonly ListValue Inner;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int SizeConflict = -1;

			  internal DropNoValuesListValue( ListValue inner )
			  {
					this.Inner = inner;
			  }

			  public override int Size()
			  {
					if ( SizeConflict < 0 )
					{
						 int s = 0;
						 for ( int i = 0; i < Inner.size(); i++ )
						 {
							  if ( Inner.value( i ) != NO_VALUE )
							  {
									s++;
							  }
						 }
						 SizeConflict = s;
					}

					return SizeConflict;
			  }

			  public override AnyValue Value( int offset )
			  {
					int actualOffset = 0;
					int size = Inner.size();
					for ( int i = 0; i < size; i++ )
					{
						 AnyValue value = Inner.value( i );
						 if ( value != NO_VALUE )
						 {
							  if ( actualOffset == offset )
							  {
									return value;
							  }
							  actualOffset++;
						 }
					}

					throw new System.IndexOutOfRangeException();
			  }

			  public override IEnumerator<AnyValue> Iterator()
			  {
					return new FilteredIterator( this );
			  }

			  public override Neo4Net.Values.SequenceValue_IterationPreference IterationPreference()
			  {
					return Neo4Net.Values.SequenceValue_IterationPreference.Iteration;
			  }

			  private class FilteredIterator : IEnumerator<AnyValue>
			  {
				  private readonly ListValue.DropNoValuesListValue _outerInstance;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
					internal AnyValue NextConflict;
					internal int Index;

					internal FilteredIterator( ListValue.DropNoValuesListValue outerInstance )
					{
						this._outerInstance = outerInstance;
						 ComputeNext();
					}

					public override bool HasNext()
					{
						 return NextConflict != null;
					}

					public override AnyValue Next()
					{
						 if ( !HasNext() )
						 {
							  throw new NoSuchElementException();
						 }

						 AnyValue current = NextConflict;
						 ComputeNext();
						 return current;
					}

					internal virtual void ComputeNext()
					{
						 if ( Index >= outerInstance.Inner.size() )
						 {
							  NextConflict = null;
						 }
						 else
						 {
							  while ( true )
							  {
									if ( Index >= outerInstance.Inner.size() )
									{
										 NextConflict = null;
										 return;
									}
									AnyValue candidate = outerInstance.Inner.value( Index++ );
									if ( candidate != NO_VALUE )
									{
										 NextConflict = candidate;
										 return;
									}
							  }
						 }
					}
			  }
		 }

		 internal sealed class IntegralRangeListValue : ListValue
		 {
			  internal readonly long Start;
			  internal readonly long End;
			  internal readonly long Step;
			  internal int Length = -1;

			  internal IntegralRangeListValue( long start, long end, long step )
			  {
					this.Start = start;
					this.End = end;
					this.Step = step;
			  }

			  public override Neo4Net.Values.SequenceValue_IterationPreference IterationPreference()
			  {
					return RANDOM_ACCESS;
			  }

			  public override string ToString()
			  {
					return "Range(" + Start + "..." + End + ", step = " + Step + ")";
			  }

			  public override int Size()
			  {
					if ( Length != -1 )
					{
						 return Length;
					}
					else
					{
						 long l = ( ( End - Start ) / Step ) + 1;
						 if ( l > int.MaxValue )
						 {
							  throw new System.OutOfMemoryException( "Cannot index an collection of size " + l );
						 }
						 Length = ( int ) l;
						 return Length;
					}
			  }

			  public override AnyValue Value( int offset )
			  {
					if ( offset >= Size() )
					{
						 throw new System.IndexOutOfRangeException();
					}
					else
					{
						 return Values.longValue( Start + offset * Step );
					}
			  }

			  public override int ComputeHash()
			  {
					int hashCode = 1;
					long current = Start;
					int size = size();
					for ( int i = 0; i < size; i++, current += Step )
					{
						 hashCode = 31 * hashCode + Long.GetHashCode( current );
					}
					return hashCode;
			  }

		 }

		 internal sealed class ConcatList : ListValue
		 {
			  internal readonly ListValue[] Lists;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int SizeConflict = -1;

			  internal ConcatList( ListValue[] lists )
			  {
					this.Lists = lists;
			  }

			  public override Neo4Net.Values.SequenceValue_IterationPreference IterationPreference()
			  {
					return Neo4Net.Values.SequenceValue_IterationPreference.Iteration;
			  }

			  public override int Size()
			  {
					if ( SizeConflict < 0 )
					{
						 int s = 0;
						 foreach ( ListValue list in Lists )
						 {
							  s += list.Size();
						 }
						 SizeConflict = s;
					}
					return SizeConflict;
			  }

			  public override AnyValue Value( int offset )
			  {
					foreach ( ListValue list in Lists )
					{
						 int size = list.Size();
						 if ( offset < size )
						 {
							  return list.Value( offset );
						 }
						 offset -= size;
					}
					throw new System.IndexOutOfRangeException();
			  }
		 }

		 internal sealed class AppendList : ListValue
		 {
			  internal readonly ListValue Base;
			  internal readonly AnyValue[] Appended;

			  internal AppendList( ListValue @base, AnyValue[] appended )
			  {
					this.Base = @base;
					this.Appended = appended;
			  }

			  public override Neo4Net.Values.SequenceValue_IterationPreference IterationPreference()
			  {
					return Base.iterationPreference();
			  }

			  public override int Size()
			  {
					return Base.size() + Appended.Length;
			  }

			  public override AnyValue Value( int offset )
			  {
					int size = Base.size();
					if ( offset < size )
					{
						 return Base.value( offset );
					}
					else if ( offset < size + Appended.Length )
					{
						 return Appended[offset - size];
					}
					else
					{
						 throw new System.IndexOutOfRangeException( offset + " is outside range " + size );
					}
			  }

			  public override IEnumerator<AnyValue> Iterator()
			  {
					switch ( Base.iterationPreference() )
					{
					case RANDOM_ACCESS:
						 return base.GetEnumerator();
					case ITERATION:
						 return Iterators.appendTo( Base.GetEnumerator(), Appended );
					default:
						 throw new System.InvalidOperationException( "unknown iteration preference" );
					}
			  }
		 }

		 internal sealed class PrependList : ListValue
		 {
			  internal readonly ListValue Base;
			  internal readonly AnyValue[] Prepended;

			  internal PrependList( ListValue @base, AnyValue[] prepended )
			  {
					this.Base = @base;
					this.Prepended = prepended;
			  }

			  public override Neo4Net.Values.SequenceValue_IterationPreference IterationPreference()
			  {
					return Base.iterationPreference();
			  }

			  public override int Size()
			  {
					return Prepended.Length + Base.size();
			  }

			  public override AnyValue Value( int offset )
			  {
					int size = Base.size();
					if ( offset < Prepended.Length )
					{
						 return Prepended[offset];
					}
					else if ( offset < size + Prepended.Length )
					{
						 return Base.value( offset - Prepended.Length );
					}
					else
					{
						 throw new System.IndexOutOfRangeException( offset + " is outside range " + size );
					}
			  }

			  public override IEnumerator<AnyValue> Iterator()
			  {
					switch ( Base.iterationPreference() )
					{
					case RANDOM_ACCESS:
						 return base.GetEnumerator();
					case ITERATION:
						 return Iterators.prependTo( Base.GetEnumerator(), Prepended );
					default:
						 throw new System.InvalidOperationException( "unknown iteration preference" );
					}
			  }
		 }

		 public virtual bool Empty
		 {
			 get
			 {
				  return Size() == 0;
			 }
		 }

		 public virtual bool NonEmpty()
		 {
			  return Size() != 0;
		 }

		 public virtual bool Storable()
		 {
			  return false;
		 }

		 public override string ToString()
		 {
			  StringBuilder sb = new StringBuilder( TypeName + "{" );
			  int i = 0;
			  for ( ; i < Size() - 1; i++ )
			  {
					sb.Append( Value( i ) );
					sb.Append( ", " );
			  }
			  if ( Size() > 0 )
			  {
					sb.Append( Value( i ) );
			  }
			  sb.Append( '}' );
			  return sb.ToString();
		 }

		 public virtual ArrayValue ToStorableArray()
		 {
			  throw new System.NotSupportedException( "List cannot be turned into a storable array" );
		 }

		 public override bool SequenceValue
		 {
			 get
			 {
				  return true;
			 }
		 }

		 public override T Map<T>( ValueMapper<T> mapper )
		 {
			  return mapper.MapSequence( this );
		 }

		 public override bool Equals( VirtualValue other )
		 {
			  return other != null && other.SequenceValue && Equals( ( SequenceValue ) other );
		 }

		 public virtual AnyValue Head()
		 {
			  int size = size();
			  if ( size == 0 )
			  {
					throw new NoSuchElementException( "head of empty list" );
			  }
			  return Value( 0 );
		 }

		 public virtual AnyValue Last()
		 {
			  int size = size();
			  if ( size == 0 )
			  {
					throw new NoSuchElementException( "last of empty list" );
			  }
			  return Value( size - 1 );
		 }

		 public override IEnumerator<AnyValue> Iterator()
		 {
			  return new IteratorAnonymousInnerClass( this );
		 }

		 private class IteratorAnonymousInnerClass : IEnumerator<AnyValue>
		 {
			 private readonly ListValue _outerInstance;

			 public IteratorAnonymousInnerClass( ListValue outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 private int count;

			 public bool hasNext()
			 {
				  return count < outerInstance.Size();
			 }

			 public AnyValue next()
			 {
				  if ( !hasNext() )
				  {
						throw new NoSuchElementException();
				  }
				  return outerInstance.Value( count++ );
			 }
		 }

		 public override VirtualValueGroup ValueGroup()
		 {
			  return VirtualValueGroup.List;
		 }

		 public override int Length()
		 {
			  return Size();
		 }

		 public override int CompareTo( VirtualValue other, IComparer<AnyValue> comparator )
		 {
			  if ( !( other is ListValue ) )
			  {
					throw new System.ArgumentException( "Cannot compare different virtual values" );
			  }

			  ListValue otherList = ( ListValue ) other;
			  if ( IterationPreference() == RANDOM_ACCESS && otherList.IterationPreference() == RANDOM_ACCESS )
			  {
					return RandomAccessCompareTo( comparator, otherList );
			  }
			  else
			  {
					return IteratorCompareTo( comparator, otherList );
			  }
		 }

		 public virtual AnyValue[] AsArray()
		 {
			  switch ( IterationPreference() )
			  {
			  case RANDOM_ACCESS:
					return RandomAccessAsArray();
			  case ITERATION:
					return IterationAsArray();
			  default:
					throw new System.InvalidOperationException( "not a valid iteration preference" );
			  }
		 }

		 public override int ComputeHash()
		 {
			  switch ( IterationPreference() )
			  {
			  case RANDOM_ACCESS:
					return RandomAccessComputeHash();
			  case ITERATION:
					return IterationComputeHash();
			  default:
					throw new System.InvalidOperationException( "not a valid iteration preference" );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void writeTo(org.neo4j.values.AnyValueWriter<E> writer) throws E
		 public override void WriteTo<E>( AnyValueWriter<E> writer ) where E : Exception
		 {
			  switch ( IterationPreference() )
			  {
			  case RANDOM_ACCESS:
					RandomAccessWriteTo( writer );
					break;
			  case ITERATION:
					IterationWriteTo( writer );
					break;
			  default:
					throw new System.InvalidOperationException( "not a valid iteration preference" );
			  }
		 }

		 public virtual ListValue DropNoValues()
		 {
			  return new DropNoValuesListValue( this );
		 }

		 public virtual ListValue Slice( int from, int to )
		 {
			  int f = Math.Max( from, 0 );
			  int t = Math.Min( to, Size() );
			  if ( f > t )
			  {
					return EMPTY_LIST;
			  }
			  else
			  {
					return new ListSlice( this, f, t );
			  }
		 }

		 public virtual ListValue Tail()
		 {
			  return Slice( 1, Size() );
		 }

		 public virtual ListValue Drop( int n )
		 {
			  int size = size();
			  int start = Math.Max( 0, Math.Min( n, size ) );
			  return new ListSlice( this, start, size );
		 }

		 public virtual ListValue Take( int n )
		 {
			  int end = Math.Max( 0, Math.Min( n, Size() ) );
			  return new ListSlice( this, 0, end );
		 }

		 public virtual ListValue Reverse()
		 {
			  return new ReversedList( this );
		 }

		 public virtual ListValue Append( params AnyValue[] values )
		 {
			  if ( values.Length == 0 )
			  {
					return this;
			  }
			  return new AppendList( this, values );
		 }

		 public virtual ListValue Prepend( params AnyValue[] values )
		 {
			  if ( values.Length == 0 )
			  {
					return this;
			  }
			  return new PrependList( this, values );
		 }

		 private AnyValue[] IterationAsArray()
		 {
			  List<AnyValue> values = new List<AnyValue>();
			  int size = 0;
			  foreach ( AnyValue value in this )
			  {
					values.Add( value );
					size++;
			  }
			  return values.ToArray();
		 }

		 private AnyValue[] RandomAccessAsArray()
		 {
			  int size = size();
			  AnyValue[] values = new AnyValue[size];
			  for ( int i = 0; i < values.Length; i++ )
			  {
					values[i] = Value( i );
			  }
			  return values;
		 }

		 private int RandomAccessComputeHash()
		 {
			  int hashCode = 1;
			  int size = size();
			  for ( int i = 0; i < size; i++ )
			  {
					hashCode = 31 * hashCode + Value( i ).GetHashCode();
			  }
			  return hashCode;
		 }

		 private int IterationComputeHash()
		 {
			  int hashCode = 1;
			  foreach ( AnyValue value in this )
			  {
					hashCode = 31 * hashCode + value.GetHashCode();
			  }
			  return hashCode;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <E extends Exception> void randomAccessWriteTo(org.neo4j.values.AnyValueWriter<E> writer) throws E
		 private void RandomAccessWriteTo<E>( AnyValueWriter<E> writer ) where E : Exception
		 {
			  writer.BeginList( Size() );
			  for ( int i = 0; i < Size(); i++ )
			  {
					Value( i ).writeTo( writer );
			  }
			  writer.EndList();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private <E extends Exception> void iterationWriteTo(org.neo4j.values.AnyValueWriter<E> writer) throws E
		 private void IterationWriteTo<E>( AnyValueWriter<E> writer ) where E : Exception
		 {
			  writer.BeginList( Size() );
			  foreach ( AnyValue value in this )
			  {
					value.WriteTo( writer );
			  }
			  writer.EndList();
		 }

		 private int RandomAccessCompareTo( IComparer<AnyValue> comparator, ListValue otherList )
		 {
			  int x = Integer.compare( this.Length(), otherList.Length() );

			  if ( x == 0 )
			  {
					for ( int i = 0; i < Length(); i++ )
					{
						 x = comparator.Compare( this.Value( i ), otherList.Value( i ) );
						 if ( x != 0 )
						 {
							  return x;
						 }
					}
			  }

			  return x;
		 }

		 private int IteratorCompareTo( IComparer<AnyValue> comparator, ListValue otherList )
		 {
			  IEnumerator<AnyValue> thisIterator = Iterator();
			  IEnumerator<AnyValue> thatIterator = otherList.GetEnumerator();
			  while ( thisIterator.MoveNext() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( !thatIterator.hasNext() )
					{
						 return 1;
					}
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					int compare = comparator.Compare( thisIterator.Current, thatIterator.next() );
					if ( compare != 0 )
					{
						 return compare;
					}
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( thatIterator.hasNext() )
			  {
					return -1;
			  }
			  else
			  {
					return 0;
			  }
		 }
	}

}