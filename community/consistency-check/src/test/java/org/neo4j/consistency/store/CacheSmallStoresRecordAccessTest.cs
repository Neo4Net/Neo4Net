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
namespace Org.Neo4j.Consistency.store
{
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;
	using Test = org.junit.jupiter.api.Test;

	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;

	internal class CacheSmallStoresRecordAccessTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDelegateLookupForMostStores()
		 internal virtual void ShouldDelegateLookupForMostStores()
		 {
			  // given
			  RecordAccess @delegate = mock( typeof( RecordAccess ) );
			  CacheSmallStoresRecordAccess recordAccess = new CacheSmallStoresRecordAccess( @delegate, null, null, null );

			  // when
			  recordAccess.Node( 42 );
			  recordAccess.Relationship( 2001 );
			  recordAccess.Property( 2468 );
			  recordAccess.String( 666 );
			  recordAccess.Array( 11 );

			  // then
			  verify( @delegate ).node( 42 );
			  verify( @delegate ).relationship( 2001 );
			  verify( @delegate ).property( 2468 );
			  verify( @delegate ).@string( 666 );
			  verify( @delegate ).array( 11 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldServePropertyKeysAndRelationshipLabelsFromSuppliedArrayCaches()
		 internal virtual void ShouldServePropertyKeysAndRelationshipLabelsFromSuppliedArrayCaches()
		 {
			  // given
			  RecordAccess @delegate = mock( typeof( RecordAccess ) );
			  PropertyKeyTokenRecord propertyKey0 = new PropertyKeyTokenRecord( 0 );
			  PropertyKeyTokenRecord propertyKey2 = new PropertyKeyTokenRecord( 2 );
			  PropertyKeyTokenRecord propertyKey1 = new PropertyKeyTokenRecord( 1 );
			  RelationshipTypeTokenRecord relationshipType0 = new RelationshipTypeTokenRecord( 0 );
			  RelationshipTypeTokenRecord relationshipType1 = new RelationshipTypeTokenRecord( 1 );
			  RelationshipTypeTokenRecord relationshipType2 = new RelationshipTypeTokenRecord( 2 );
			  LabelTokenRecord label0 = new LabelTokenRecord( 0 );
			  LabelTokenRecord label1 = new LabelTokenRecord( 1 );
			  LabelTokenRecord label2 = new LabelTokenRecord( 2 );

			  CacheSmallStoresRecordAccess recordAccess = new CacheSmallStoresRecordAccess( @delegate, new PropertyKeyTokenRecord[]{ propertyKey0, propertyKey1, propertyKey2 }, new RelationshipTypeTokenRecord[]{ relationshipType0, relationshipType1, relationshipType2 }, new LabelTokenRecord[]{ label0, label1, label2 } );

			  // when
			  assertThat( recordAccess.PropertyKey( 0 ), IsDirectReferenceTo( propertyKey0 ) );
			  assertThat( recordAccess.PropertyKey( 1 ), IsDirectReferenceTo( propertyKey1 ) );
			  assertThat( recordAccess.PropertyKey( 2 ), IsDirectReferenceTo( propertyKey2 ) );
			  assertThat( recordAccess.RelationshipType( 0 ), IsDirectReferenceTo( relationshipType0 ) );
			  assertThat( recordAccess.RelationshipType( 1 ), IsDirectReferenceTo( relationshipType1 ) );
			  assertThat( recordAccess.RelationshipType( 2 ), IsDirectReferenceTo( relationshipType2 ) );
			  assertThat( recordAccess.Label( 0 ), IsDirectReferenceTo( label0 ) );
			  assertThat( recordAccess.Label( 1 ), IsDirectReferenceTo( label1 ) );
			  assertThat( recordAccess.Label( 2 ), IsDirectReferenceTo( label2 ) );

			  // then
			  verifyZeroInteractions( @delegate );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static <T extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord> org.hamcrest.Matcher<RecordReference<T>> isDirectReferenceTo(T record)
		 private static Matcher<RecordReference<T>> IsDirectReferenceTo<T>( T record ) where T : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  return ( Matcher ) new DirectReferenceMatcher<RecordReference<T>>( record );
		 }

		 private class DirectReferenceMatcher<T> : TypeSafeMatcher<DirectRecordReference<T>> where T : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  internal readonly T Record;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") DirectReferenceMatcher(T record)
			  internal DirectReferenceMatcher( T record ) : base( typeof( DirectRecordReference ) )
			  {
					this.Record = record;
			  }

			  public override bool MatchesSafely( DirectRecordReference<T> reference )
			  {
					return Record == reference.Record();
			  }

			  public override void DescribeTo( Description description )
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					description.appendText( typeof( DirectRecordReference ).FullName ).appendText( "( " ).appendValue( Record ).appendText( " )" );
			  }
		 }
	}

}