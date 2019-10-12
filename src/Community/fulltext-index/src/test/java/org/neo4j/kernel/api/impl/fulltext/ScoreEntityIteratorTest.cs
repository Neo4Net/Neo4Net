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
namespace Neo4Net.Kernel.Api.Impl.Fulltext
{
	using Test = org.junit.Test;


	using ScoreEntry = Neo4Net.Kernel.Api.Impl.Fulltext.ScoreEntityIterator.ScoreEntry;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class ScoreEntityIteratorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mergeShouldReturnOrderedResults()
		 public virtual void MergeShouldReturnOrderedResults()
		 {
			  ScoreEntityIterator one = IteratorOf( new ScoreEntry[]{ Entry( 3, 10 ), Entry( 10, 3 ), Entry( 12, 1 ) } );
			  ScoreEntityIterator two = IteratorOf( new ScoreEntry[]{ Entry( 1, 12 ), Entry( 5, 8 ), Entry( 7, 6 ), Entry( 8, 5 ), Entry( 11, 2 ) } );
			  ScoreEntityIterator three = IteratorOf( new ScoreEntry[]{ Entry( 2, 11 ), Entry( 4, 9 ), Entry( 6, 7 ), Entry( 9, 4 ) } );

			  ScoreEntityIterator concat = ScoreEntityIterator.MergeIterators( Arrays.asList( one, two, three ) );

			  for ( int i = 1; i <= 12; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( concat.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					ScoreEntry entry = concat.Next();
					assertEquals( i, entry.EntityId() );
					assertEquals( 13 - i, entry.Score(), 0.001 );
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( concat.HasNext() );
		 }

		 private static ScoreEntry Entry( long id, float s )
		 {
			  return new ScoreEntry( id, s );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mergeShouldHandleEmptyIterators()
		 public virtual void MergeShouldHandleEmptyIterators()
		 {
			  ScoreEntityIterator one = IteratorOf( EmptyEntries() );
			  ScoreEntityIterator two = IteratorOf( new ScoreEntry[]{ Entry( 1, 5 ), Entry( 2, 4 ), Entry( 3, 3 ), Entry( 4, 2 ), Entry( 5, 1 ) } );
			  ScoreEntityIterator three = IteratorOf( EmptyEntries() );

			  ScoreEntityIterator concat = ScoreEntityIterator.MergeIterators( Arrays.asList( one, two, three ) );

			  for ( int i = 1; i <= 5; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( concat.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					ScoreEntry entry = concat.Next();
					assertEquals( i, entry.EntityId() );
					assertEquals( 6 - i, entry.Score(), 0.001 );
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( concat.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mergeShouldHandleAllEmptyIterators()
		 public virtual void MergeShouldHandleAllEmptyIterators()
		 {
			  ScoreEntityIterator one = IteratorOf( EmptyEntries() );
			  ScoreEntityIterator two = IteratorOf( EmptyEntries() );
			  ScoreEntityIterator three = IteratorOf( EmptyEntries() );

			  ScoreEntityIterator concat = ScoreEntityIterator.MergeIterators( Arrays.asList( one, two, three ) );

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( concat.HasNext() );
		 }

		 private static ScoreEntry[] EmptyEntries()
		 {
			  return new ScoreEntry[]{};
		 }

		 private static ScoreEntityIterator IteratorOf( ScoreEntry[] input )
		 {
			  return new ScoreEntityIteratorAnonymousInnerClass( input );
		 }

		 private class ScoreEntityIteratorAnonymousInnerClass : ScoreEntityIterator
		 {
			 private ScoreEntry[] _input;

			 public ScoreEntityIteratorAnonymousInnerClass( ScoreEntry[] input ) : base( null )
			 {
				 this._input = input;
			 }

			 internal IEnumerator<ScoreEntry> entries = Arrays.asList( _input ).GetEnumerator();

			 public override bool hasNext()
			 {
				  return entries.hasNext();
			 }

			 public override ScoreEntry next()
			 {
				  return entries.next();
			 }
		 }
	}

}