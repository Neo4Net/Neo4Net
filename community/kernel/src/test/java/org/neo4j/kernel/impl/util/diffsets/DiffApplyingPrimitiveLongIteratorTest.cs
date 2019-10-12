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
namespace Org.Neo4j.Kernel.impl.util.diffsets
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using LongSets = org.eclipse.collections.impl.factory.primitive.LongSets;
	using ImmutableEmptyLongIterator = org.eclipse.collections.impl.iterator.ImmutableEmptyLongIterator;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;

	using PrimitiveLongResourceIterator = Org.Neo4j.Collection.PrimitiveLongResourceIterator;
	using Resource = Org.Neo4j.Graphdb.Resource;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.arrayContainingInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.arrayWithSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.asArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.iterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.resourceIterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.toSet;

	public class DiffApplyingPrimitiveLongIteratorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void iterateOnlyOverAddedElementsWhenSourceIsEmpty()
		 public virtual void IterateOnlyOverAddedElementsWhenSourceIsEmpty()
		 {
			  LongIterator emptySource = ImmutableEmptyLongIterator.INSTANCE;
			  LongSet added = LongHashSet.newSetWith( 1L, 2L );
			  LongSet removed = LongHashSet.newSetWith( 3L );

			  LongIterator iterator = DiffApplyingPrimitiveLongIterator.Augment( emptySource, added, removed );
			  ISet<long> resultSet = toSet( iterator );
			  assertThat( resultSet, containsInAnyOrder( 1L, 2L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void appendSourceElementsDuringIteration()
		 public virtual void AppendSourceElementsDuringIteration()
		 {
			  LongIterator source = iterator( 4L, 5L );
			  LongSet added = LongHashSet.newSetWith( 1L, 2L );
			  LongSet removed = LongHashSet.newSetWith( 3L );

			  LongIterator iterator = DiffApplyingPrimitiveLongIterator.Augment( source, added, removed );
			  ISet<long> resultSet = toSet( iterator );
			  assertThat( resultSet, containsInAnyOrder( 1L, 2L, 4L, 5L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doNotIterateTwiceOverSameElementsWhenItsPartOfSourceAndAdded()
		 public virtual void DoNotIterateTwiceOverSameElementsWhenItsPartOfSourceAndAdded()
		 {
			  LongIterator source = iterator( 4L, 5L );
			  LongSet added = LongHashSet.newSetWith( 1L, 4L );
			  LongSet removed = LongHashSet.newSetWith( 3L );

			  LongIterator iterator = DiffApplyingPrimitiveLongIterator.Augment( source, added, removed );
			  long?[] values = ArrayUtils.toObject( asArray( iterator ) );
			  assertThat( values, arrayContainingInAnyOrder( 1L, 4L, 5L ) );
			  assertThat( values, arrayWithSize( 3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doNotIterateOverDeletedElement()
		 public virtual void DoNotIterateOverDeletedElement()
		 {
			  LongIterator source = iterator( 3L, 5L );
			  LongSet added = LongHashSet.newSetWith( 1L );
			  LongSet removed = LongHashSet.newSetWith( 3L );

			  LongIterator iterator = DiffApplyingPrimitiveLongIterator.Augment( source, added, removed );
			  ISet<long> resultSet = toSet( iterator );
			  assertThat( resultSet, containsInAnyOrder( 1L, 5L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeResource()
		 public virtual void CloseResource()
		 {
			  Resource resource = Mockito.mock( typeof( Resource ) );
			  PrimitiveLongResourceIterator source = resourceIterator( ImmutableEmptyLongIterator.INSTANCE, resource );

			  PrimitiveLongResourceIterator iterator = DiffApplyingPrimitiveLongIterator.Augment( source, LongSets.immutable.empty(), LongSets.immutable.empty() );

			  iterator.Close();

			  Mockito.verify( resource ).close();
		 }
	}

}