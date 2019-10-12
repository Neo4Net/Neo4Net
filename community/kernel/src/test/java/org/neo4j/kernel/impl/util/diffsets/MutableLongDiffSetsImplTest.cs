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
namespace Org.Neo4j.Kernel.impl.util.diffsets
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;
	using Test = org.junit.Test;

	using CollectionsFactory = Org.Neo4j.Kernel.impl.util.collection.CollectionsFactory;
	using OnHeapCollectionsFactory = Org.Neo4j.Kernel.impl.util.collection.OnHeapCollectionsFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.eclipse.collections.impl.set.mutable.primitive.LongHashSet.newSetWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doReturn;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.iterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.toSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	public class MutableLongDiffSetsImplTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void newDiffSetIsEmpty()
		 public virtual void NewDiffSetIsEmpty()
		 {
			  assertTrue( CreateDiffSet().Empty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addElementsToDiffSets()
		 public virtual void AddElementsToDiffSets()
		 {
			  MutableLongDiffSetsImpl diffSets = CreateDiffSet();

			  diffSets.Add( 1L );
			  diffSets.Add( 2L );

			  assertEquals( asSet( 1L, 2L ), toSet( diffSets.Added ) );
			  assertTrue( diffSets.Removed.Empty );
			  assertFalse( diffSets.Empty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removeElementsInDiffSets()
		 public virtual void RemoveElementsInDiffSets()
		 {
			  MutableLongDiffSetsImpl diffSets = CreateDiffSet();

			  diffSets.Remove( 1L );
			  diffSets.Remove( 2L );

			  assertFalse( diffSets.Empty );
			  assertEquals( asSet( 1L, 2L ), toSet( diffSets.Removed ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removeAndAddElementsToDiffSets()
		 public virtual void RemoveAndAddElementsToDiffSets()
		 {
			  MutableLongDiffSetsImpl diffSets = CreateDiffSet();

			  diffSets.Remove( 1L );
			  diffSets.Remove( 2L );
			  diffSets.Add( 1L );
			  diffSets.Add( 2L );
			  diffSets.Add( 3L );
			  diffSets.Remove( 4L );

			  assertFalse( diffSets.Empty );
			  assertEquals( asSet( 4L ), toSet( diffSets.Removed ) );
			  assertEquals( asSet( 3L ), toSet( diffSets.Added ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void checkIsElementsAddedOrRemoved()
		 public virtual void CheckIsElementsAddedOrRemoved()
		 {
			  MutableLongDiffSetsImpl diffSet = CreateDiffSet();

			  diffSet.Add( 1L );

			  assertTrue( diffSet.IsAdded( 1L ) );
			  assertFalse( diffSet.IsRemoved( 1L ) );

			  diffSet.Remove( 2L );

			  assertFalse( diffSet.IsAdded( 2L ) );
			  assertTrue( diffSet.IsRemoved( 2L ) );

			  assertFalse( diffSet.IsAdded( 3L ) );
			  assertFalse( diffSet.IsRemoved( 3L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addAllElements()
		 public virtual void AddAllElements()
		 {
			  MutableLongDiffSetsImpl diffSet = CreateDiffSet();

			  diffSet.AddAll( newSetWith( 7L, 8L ) );
			  diffSet.AddAll( newSetWith( 9L, 10L ) );

			  assertEquals( asSet( 7L, 8L, 9L, 10L ), toSet( diffSet.Added ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removeAllElements()
		 public virtual void RemoveAllElements()
		 {
			  MutableLongDiffSetsImpl diffSet = CreateDiffSet();

			  diffSet.RemoveAll( newSetWith( 7L, 8L ) );
			  diffSet.RemoveAll( newSetWith( 9L, 10L ) );

			  assertEquals( asSet( 7L, 8L, 9L, 10L ), toSet( diffSet.Removed ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addedAndRemovedElementsDelta()
		 public virtual void AddedAndRemovedElementsDelta()
		 {
			  MutableLongDiffSetsImpl diffSet = CreateDiffSet();
			  assertEquals( 0, diffSet.Delta() );

			  diffSet.AddAll( newSetWith( 7L, 8L ) );
			  diffSet.AddAll( newSetWith( 9L, 10L ) );
			  assertEquals( 4, diffSet.Delta() );

			  diffSet.RemoveAll( newSetWith( 8L, 9L ) );
			  assertEquals( 2, diffSet.Delta() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void augmentDiffSetWithExternalElements()
		 public virtual void AugmentDiffSetWithExternalElements()
		 {
			  MutableLongDiffSets diffSet = CreateDiffSet();
			  diffSet.AddAll( newSetWith( 9L, 10L, 11L ) );
			  diffSet.RemoveAll( newSetWith( 1L, 2L ) );

			  LongIterator augmentedIterator = diffSet.Augment( iterator( 5L, 6L ) );
			  assertEquals( asSet( 5L, 6L, 9L, 10L, 11L ), toSet( augmentedIterator ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useCollectionsFactory()
		 public virtual void UseCollectionsFactory()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.MutableLongSet set1 = new org.eclipse.collections.impl.set.mutable.primitive.LongHashSet();
			  MutableLongSet set1 = new LongHashSet();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.MutableLongSet set2 = new org.eclipse.collections.impl.set.mutable.primitive.LongHashSet();
			  MutableLongSet set2 = new LongHashSet();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.collection.CollectionsFactory collectionsFactory = mock(org.neo4j.kernel.impl.util.collection.CollectionsFactory.class);
			  CollectionsFactory collectionsFactory = mock( typeof( CollectionsFactory ) );
			  doReturn( set1, set2 ).when( collectionsFactory ).newLongSet();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final MutableLongDiffSetsImpl diffSets = new MutableLongDiffSetsImpl(collectionsFactory);
			  MutableLongDiffSetsImpl diffSets = new MutableLongDiffSetsImpl( collectionsFactory );
			  diffSets.Add( 1L );
			  diffSets.Remove( 2L );

			  assertSame( set1, diffSets.Added );
			  assertSame( set2, diffSets.Removed );
			  verify( collectionsFactory, times( 2 ) ).newLongSet();
			  verifyNoMoreInteractions( collectionsFactory );
		 }

		 private static MutableLongDiffSetsImpl CreateDiffSet()
		 {
			  return new MutableLongDiffSetsImpl( OnHeapCollectionsFactory.INSTANCE );
		 }
	}

}