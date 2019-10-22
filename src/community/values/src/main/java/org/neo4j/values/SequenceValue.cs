using System.Collections.Generic;

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
namespace Neo4Net.Values
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.SequenceValue_IterationPreference.RANDOM_ACCESS;

	/// <summary>
	/// Values that represent sequences of values (such as Lists or Arrays) need to implement this interface.
	/// Thus we can get an equality check that is based on the values (e.g. List.equals(ArrayValue) )
	/// Values that implement this interface also need to overwrite isSequence() to return true!
	/// 
	/// Note that even though SequenceValue extends Iterable iterating over the sequence using iterator() might not be the
	/// most performant method. Branch using iterationPreference() in performance critical code paths.
	/// </summary>
	public interface SequenceValue : IEnumerable<AnyValue>
	{
		 /// <summary>
		 /// The preferred way to iterate this sequence. Preferred in this case means the method which is expected to be
		 /// the most performant.
		 /// </summary>

		 int Length();

		 AnyValue Value( int offset );

		 IEnumerator<AnyValue> Iterator();

		 SequenceValue_IterationPreference IterationPreference();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default boolean equals(SequenceValue other)
	//	 {
	//		  if (other == null)
	//		  {
	//				return false;
	//		  }
	//
	//		  IterationPreference pref = iterationPreference();
	//		  IterationPreference otherPref = other.iterationPreference();
	//		  if (pref == RANDOM_ACCESS && otherPref == RANDOM_ACCESS)
	//		  {
	//				return equalsUsingRandomAccess(this, other);
	//		  }
	//		  else
	//		  {
	//				return equalsUsingIterators(this, other);
	//		  }
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static boolean equalsUsingRandomAccess(SequenceValue a, SequenceValue b)
	//	 {
	//		  int i = 0;
	//		  boolean areEqual = a.length() == b.length();
	//
	//		  while (areEqual && i < a.length())
	//		  {
	//				areEqual = a.value(i).equals(b.value(i));
	//				i++;
	//		  }
	//		  return areEqual;
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static System.Nullable<bool> ternaryEqualsUsingRandomAccess(SequenceValue a, SequenceValue b)
	//	 {
	//		  if (a.length() != b.length())
	//		  {
	//				return false;
	//		  }
	//
	//		  int i = 0;
	//		  System.Nullable<bool> equivalenceResult = true;
	//
	//		  while (i < a.length())
	//		  {
	//				System.Nullable<bool> areEqual = a.value(i).ternaryEquals(b.value(i));
	//				if (areEqual == null)
	//				{
	//					 equivalenceResult = null;
	//				}
	//				else if (!areEqual)
	//				{
	//					 return false;
	//				}
	//				i++;
	//		  }
	//
	//		  return equivalenceResult;
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static boolean equalsUsingIterators(SequenceValue a, SequenceValue b)
	//	 {
	//		  boolean areEqual = true;
	//		  Iterator<AnyValue> aIterator = a.iterator();
	//		  Iterator<AnyValue> bIterator = b.iterator();
	//
	//		  while (areEqual && aIterator.hasNext() && bIterator.hasNext())
	//		  {
	//				areEqual = aIterator.next().equals(bIterator.next());
	//		  }
	//
	//		  return areEqual && aIterator.hasNext() == bIterator.hasNext();
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static System.Nullable<bool> ternaryEqualsUsingIterators(SequenceValue a, SequenceValue b)
	//	 {
	//		  System.Nullable<bool> equivalenceResult = true;
	//		  Iterator<AnyValue> aIterator = a.iterator();
	//		  Iterator<AnyValue> bIterator = b.iterator();
	//
	//		  while (aIterator.hasNext() && bIterator.hasNext())
	//		  {
	//				System.Nullable<bool> areEqual = aIterator.next().ternaryEquals(bIterator.next());
	//				if (areEqual == null)
	//				{
	//					 equivalenceResult = null;
	//				}
	//				else if (!areEqual)
	//				{
	//					 return false;
	//				}
	//		  }
	//
	//		  return aIterator.hasNext() == bIterator.hasNext() ? equivalenceResult : false;
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default int compareToSequence(SequenceValue other, java.util.Comparator<AnyValue> comparator)
	//	 {
	//		  IterationPreference pref = iterationPreference();
	//		  IterationPreference otherPref = other.iterationPreference();
	//		  if (pref == RANDOM_ACCESS && otherPref == RANDOM_ACCESS)
	//		  {
	//				return compareUsingRandomAccess(this, other, comparator);
	//		  }
	//		  else
	//		  {
	//				return compareUsingIterators(this, other, comparator);
	//		  }
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static int compareUsingRandomAccess(SequenceValue a, SequenceValue b, java.util.Comparator<AnyValue> comparator)
	//	 {
	//		  int i = 0;
	//		  int x = 0;
	//		  int length = Math.min(a.length(), b.length());
	//
	//		  while (x == 0 && i < length)
	//		  {
	//				x = comparator.compare(a.value(i), b.value(i));
	//				i++;
	//		  }
	//
	//		  if (x == 0)
	//		  {
	//				x = a.length() - b.length();
	//		  }
	//
	//		  return x;
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static int compareUsingIterators(SequenceValue a, SequenceValue b, java.util.Comparator<AnyValue> comparator)
	//	 {
	//		  int x = 0;
	//		  Iterator<AnyValue> aIterator = a.iterator();
	//		  Iterator<AnyValue> bIterator = b.iterator();
	//
	//		  while (aIterator.hasNext() && bIterator.hasNext())
	//		  {
	//				x = comparator.compare(aIterator.next(), bIterator.next());
	//		  }
	//
	//		  if (x == 0)
	//		  {
	//				x = Boolean.compare(aIterator.hasNext(), bIterator.hasNext());
	//		  }
	//
	//		  return x;
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default System.Nullable<bool> ternaryEquality(SequenceValue other)
	//	 {
	//		  IterationPreference pref = iterationPreference();
	//		  IterationPreference otherPref = other.iterationPreference();
	//		  if (pref == RANDOM_ACCESS && otherPref == RANDOM_ACCESS)
	//		  {
	//				return ternaryEqualsUsingRandomAccess(this, other);
	//		  }
	//		  else
	//		  {
	//				return ternaryEqualsUsingIterators(this, other);
	//		  }
	//	 }
	}

	 public enum SequenceValue_IterationPreference
	 {
		  RandomAccess,
		  Iteration
	 }

}