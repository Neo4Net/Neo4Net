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
namespace Neo4Net.Consistency.checking
{
	using Test = org.junit.jupiter.api.Test;


	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using GeometryType = Neo4Net.Kernel.impl.store.GeometryType;
	using PropertyType = Neo4Net.Kernel.impl.store.PropertyType;
	using TemporalType = Neo4Net.Kernel.impl.store.TemporalType;
	using StandardFormatSettings = Neo4Net.Kernel.impl.store.format.standard.StandardFormatSettings;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using PropertyBlock = Neo4Net.Kernel.Impl.Store.Records.PropertyBlock;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;

	internal class PropertyRecordCheckTest : RecordCheckTestBase<PropertyRecord, Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport, PropertyRecordCheck>
	{
		 internal PropertyRecordCheckTest() : base(new PropertyRecordCheck(), typeof(org.Neo4Net.consistency.report.ConsistencyReport_PropertyConsistencyReport), new int[0])
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotReportAnythingForPropertyRecordNotInUse()
		 internal virtual void ShouldNotReportAnythingForPropertyRecordNotInUse()
		 {
			  // given
			  PropertyRecord property = NotInUse( new PropertyRecord( 42 ) );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport report = Check( property );

			  // then
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotReportAnythingForPropertyWithoutBlocksThatDoesNotReferenceAnyOtherRecords()
		 internal virtual void ShouldNotReportAnythingForPropertyWithoutBlocksThatDoesNotReferenceAnyOtherRecords()
		 {
			  // given
			  PropertyRecord property = InUse( new PropertyRecord( 42 ) );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport report = Check( property );

			  // then
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportPropertyKeyNotInUse()
		 internal virtual void ShouldReportPropertyKeyNotInUse()
		 {
			  // given
			  PropertyRecord property = InUse( new PropertyRecord( 42 ) );
			  PropertyKeyTokenRecord key = Add( NotInUse( new PropertyKeyTokenRecord( 0 ) ) );
			  PropertyBlock block = PropertyBlock( key, PropertyType.INT, 0 );
			  property.AddPropertyBlock( block );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport report = Check( property );

			  // then
			  verify( report ).keyNotInUse( block, key );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportPreviousPropertyNotInUse()
		 internal virtual void ShouldReportPreviousPropertyNotInUse()
		 {
			  // given
			  PropertyRecord property = InUse( new PropertyRecord( 42 ) );
			  PropertyRecord prev = Add( NotInUse( new PropertyRecord( 51 ) ) );
			  property.PrevProp = prev.Id;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport report = Check( property );

			  // then
			  verify( report ).prevNotInUse( prev );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportNextPropertyNotInUse()
		 internal virtual void ShouldReportNextPropertyNotInUse()
		 {
			  // given
			  PropertyRecord property = InUse( new PropertyRecord( 42 ) );
			  PropertyRecord next = Add( NotInUse( new PropertyRecord( 51 ) ) );
			  property.NextProp = next.Id;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport report = Check( property );

			  // then
			  verify( report ).nextNotInUse( next );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportPreviousPropertyNotReferringBack()
		 internal virtual void ShouldReportPreviousPropertyNotReferringBack()
		 {
			  // given
			  PropertyRecord property = InUse( new PropertyRecord( 42 ) );
			  PropertyRecord prev = Add( InUse( new PropertyRecord( 51 ) ) );
			  property.PrevProp = prev.Id;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport report = Check( property );

			  // then
			  verify( report ).previousDoesNotReferenceBack( prev );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportNextPropertyNotReferringBack()
		 internal virtual void ShouldReportNextPropertyNotReferringBack()
		 {
			  // given
			  PropertyRecord property = InUse( new PropertyRecord( 42 ) );
			  PropertyRecord next = Add( InUse( new PropertyRecord( 51 ) ) );
			  property.NextProp = next.Id;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport report = Check( property );

			  // then
			  verify( report ).nextDoesNotReferenceBack( next );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportStringRecordNotInUse()
		 internal virtual void ShouldReportStringRecordNotInUse()
		 {
			  // given
			  PropertyRecord property = InUse( new PropertyRecord( 42 ) );
			  PropertyKeyTokenRecord key = Add( InUse( new PropertyKeyTokenRecord( 6 ) ) );
			  DynamicRecord value = Add( NotInUse( String( new DynamicRecord( 1001 ) ) ) );
			  PropertyBlock block = PropertyBlock( key, value );
			  property.AddPropertyBlock( block );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport report = Check( property );
			  // then
			  verify( report ).stringNotInUse( block, value );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportArrayRecordNotInUse()
		 internal virtual void ShouldReportArrayRecordNotInUse()
		 {
			  // given
			  PropertyRecord property = InUse( new PropertyRecord( 42 ) );
			  PropertyKeyTokenRecord key = Add( InUse( new PropertyKeyTokenRecord( 6 ) ) );
			  DynamicRecord value = Add( NotInUse( Array( new DynamicRecord( 1001 ) ) ) );
			  PropertyBlock block = PropertyBlock( key, value );
			  property.AddPropertyBlock( block );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport report = Check( property );

			  // then
			  verify( report ).arrayNotInUse( block, value );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportEmptyStringRecord()
		 internal virtual void ShouldReportEmptyStringRecord()
		 {
			  // given
			  PropertyRecord property = InUse( new PropertyRecord( 42 ) );
			  PropertyKeyTokenRecord key = Add( InUse( new PropertyKeyTokenRecord( 6 ) ) );
			  DynamicRecord value = Add( InUse( String( new DynamicRecord( 1001 ) ) ) );
			  PropertyBlock block = PropertyBlock( key, value );
			  property.AddPropertyBlock( block );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport report = Check( property );

			  // then
			  verify( report ).stringEmpty( block, value );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportUnknownGTypeGeometryRecord()
		 internal virtual void ShouldReportUnknownGTypeGeometryRecord()
		 {
			  // given
			  PropertyRecord property = InUse( new PropertyRecord( 42 ) );
			  const int keyId = 6;
			  Add( InUse( new PropertyKeyTokenRecord( keyId ) ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long[] longs = org.Neo4Net.kernel.impl.store.GeometryType.encodePoint(keyId, org.Neo4Net.values.storable.CoordinateReferenceSystem.WGS84, new double[] { 1.0, 2.0 });
			  long[] longs = GeometryType.encodePoint( keyId, CoordinateReferenceSystem.WGS84, new double[] { 1.0, 2.0 } );
			  // corrupt array
			  long gtypeBits = 0xFL << StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS + 4;
			  longs[0] |= gtypeBits;

			  ExpectInvalidPropertyValue( property, longs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReport15DimensionalPointRecord()
		 internal virtual void ShouldReport15DimensionalPointRecord()
		 {
			  // given
			  PropertyRecord property = InUse( new PropertyRecord( 42 ) );
			  const int keyId = 6;
			  Add( InUse( new PropertyKeyTokenRecord( keyId ) ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long[] longs = org.Neo4Net.kernel.impl.store.GeometryType.encodePoint(keyId, org.Neo4Net.values.storable.CoordinateReferenceSystem.WGS84, new double[] { 1.0, 2.0 });
			  long[] longs = GeometryType.encodePoint( keyId, CoordinateReferenceSystem.WGS84, new double[] { 1.0, 2.0 } );
			  // corrupt array
			  long dimensionBits = 0xFL << StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS + 8;
			  longs[0] |= dimensionBits;

			  ExpectInvalidPropertyValue( property, longs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportUnknownCRSPointRecord()
		 internal virtual void ShouldReportUnknownCRSPointRecord()
		 {
			  // given
			  PropertyRecord property = InUse( new PropertyRecord( 42 ) );
			  const int keyId = 6;
			  Add( InUse( new PropertyKeyTokenRecord( keyId ) ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long[] longs = org.Neo4Net.kernel.impl.store.GeometryType.encodePoint(keyId, org.Neo4Net.values.storable.CoordinateReferenceSystem.WGS84, new double[] { 1.0, 2.0 });
			  long[] longs = GeometryType.encodePoint( keyId, CoordinateReferenceSystem.WGS84, new double[] { 1.0, 2.0 } );
			  // corrupt array
			  long crsTableIdAndCodeBits = 0xFFFFL << StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS + 12;
			  longs[0] |= crsTableIdAndCodeBits;

			  ExpectInvalidPropertyValue( property, longs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTooHighDateRecord()
		 internal virtual void ShouldReportTooHighDateRecord()
		 {
			  // given
			  PropertyRecord property = InUse( new PropertyRecord( 42 ) );
			  const int keyId = 6;
			  Add( InUse( new PropertyKeyTokenRecord( keyId ) ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long[] longs = org.Neo4Net.kernel.impl.store.TemporalType.encodeDate(keyId, java.time.LocalDate.MAX.toEpochDay() + 1);
			  long[] longs = TemporalType.encodeDate( keyId, LocalDate.MAX.toEpochDay() + 1 );

			  ExpectInvalidPropertyValue( property, longs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTooHighLocalTimeRecord()
		 internal virtual void ShouldReportTooHighLocalTimeRecord()
		 {
			  // given
			  PropertyRecord property = InUse( new PropertyRecord( 42 ) );
			  const int keyId = 6;
			  Add( InUse( new PropertyKeyTokenRecord( keyId ) ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long[] longs = org.Neo4Net.kernel.impl.store.TemporalType.encodeLocalTime(keyId, java.time.LocalTime.MAX.toNanoOfDay() + 1);
			  long[] longs = TemporalType.encodeLocalTime( keyId, LocalTime.MAX.toNanoOfDay() + 1 );

			  ExpectInvalidPropertyValue( property, longs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTooHighNanoLocalDateTimeRecord()
		 internal virtual void ShouldReportTooHighNanoLocalDateTimeRecord()
		 {
			  // given
			  PropertyRecord property = InUse( new PropertyRecord( 42 ) );
			  const int keyId = 6;
			  Add( InUse( new PropertyKeyTokenRecord( keyId ) ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long[] longs = org.Neo4Net.kernel.impl.store.TemporalType.encodeLocalDateTime(keyId, 1, 1_000_000_000);
			  long[] longs = TemporalType.encodeLocalDateTime( keyId, 1, 1_000_000_000 );

			  ExpectInvalidPropertyValue( property, longs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTooHighEpochSecondLocalDateTimeRecord()
		 internal virtual void ShouldReportTooHighEpochSecondLocalDateTimeRecord()
		 {
			  // given
			  PropertyRecord property = InUse( new PropertyRecord( 42 ) );
			  const int keyId = 6;
			  Add( InUse( new PropertyKeyTokenRecord( keyId ) ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long[] longs = org.Neo4Net.kernel.impl.store.TemporalType.encodeLocalDateTime(keyId, java.time.Instant.MAX.getEpochSecond() + 1,1);
			  long[] longs = TemporalType.encodeLocalDateTime( keyId, Instant.MAX.EpochSecond + 1,1 );

			  ExpectInvalidPropertyValue( property, longs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTooHighNanoDateTimeRecord()
		 internal virtual void ShouldReportTooHighNanoDateTimeRecord()
		 {
			  // given
			  PropertyRecord property = InUse( new PropertyRecord( 42 ) );
			  const int keyId = 6;
			  Add( InUse( new PropertyKeyTokenRecord( keyId ) ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long[] longs = org.Neo4Net.kernel.impl.store.TemporalType.encodeDateTime(keyId, 1, 1_000_000_000, 0);
			  long[] longs = TemporalType.encodeDateTime( keyId, 1, 1_000_000_000, 0 );

			  ExpectInvalidPropertyValue( property, longs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTooHighEpochSecondDateTimeRecord()
		 internal virtual void ShouldReportTooHighEpochSecondDateTimeRecord()
		 {
			  // given
			  PropertyRecord property = InUse( new PropertyRecord( 42 ) );
			  const int keyId = 6;
			  Add( InUse( new PropertyKeyTokenRecord( keyId ) ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long[] longs = org.Neo4Net.kernel.impl.store.TemporalType.encodeDateTime(keyId, java.time.Instant.MAX.getEpochSecond() + 1,1, 0);
			  long[] longs = TemporalType.encodeDateTime( keyId, Instant.MAX.EpochSecond + 1,1, 0 );

			  ExpectInvalidPropertyValue( property, longs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTooHighNanoDateTimeRecordWithNamedTZ()
		 internal virtual void ShouldReportTooHighNanoDateTimeRecordWithNamedTZ()
		 {
			  // given
			  PropertyRecord property = InUse( new PropertyRecord( 42 ) );
			  const int keyId = 6;
			  Add( InUse( new PropertyKeyTokenRecord( keyId ) ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long[] longs = org.Neo4Net.kernel.impl.store.TemporalType.encodeDateTime(keyId, 1, 1_000_000_000, "Europe/London");
			  long[] longs = TemporalType.encodeDateTime( keyId, 1, 1_000_000_000, "Europe/London" );

			  ExpectInvalidPropertyValue( property, longs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTooHighEpochSecondDateTimeRecordWithNamedTZ()
		 internal virtual void ShouldReportTooHighEpochSecondDateTimeRecordWithNamedTZ()
		 {
			  // given
			  PropertyRecord property = InUse( new PropertyRecord( 42 ) );
			  const int keyId = 6;
			  Add( InUse( new PropertyKeyTokenRecord( keyId ) ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long[] longs = org.Neo4Net.kernel.impl.store.TemporalType.encodeDateTime(keyId, java.time.Instant.MAX.getEpochSecond() + 1,1, "Europe/London");
			  long[] longs = TemporalType.encodeDateTime( keyId, Instant.MAX.EpochSecond + 1,1, "Europe/London" );

			  ExpectInvalidPropertyValue( property, longs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTooHighOffsetSecondDateTimeRecord()
		 internal virtual void ShouldReportTooHighOffsetSecondDateTimeRecord()
		 {
			  // given
			  PropertyRecord property = InUse( new PropertyRecord( 42 ) );
			  const int keyId = 6;
			  Add( InUse( new PropertyKeyTokenRecord( keyId ) ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long[] longs = org.Neo4Net.kernel.impl.store.TemporalType.encodeDateTime(keyId, 1,1, java.time.ZoneOffset.MAX.getTotalSeconds() + 1);
			  long[] longs = TemporalType.encodeDateTime( keyId, 1,1, ZoneOffset.MAX.TotalSeconds + 1 );

			  ExpectInvalidPropertyValue( property, longs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTooHighNanoTimeRecord()
		 internal virtual void ShouldReportTooHighNanoTimeRecord()
		 {
			  // given
			  PropertyRecord property = InUse( new PropertyRecord( 42 ) );
			  const int keyId = 6;
			  Add( InUse( new PropertyKeyTokenRecord( keyId ) ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long[] longs = org.Neo4Net.kernel.impl.store.TemporalType.encodeTime(keyId, java.time.LocalTime.MAX.toNanoOfDay() + 1, 0);
			  long[] longs = TemporalType.encodeTime( keyId, LocalTime.MAX.toNanoOfDay() + 1, 0 );

			  ExpectInvalidPropertyValue( property, longs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTooHighOffsetSecondTimeRecord()
		 internal virtual void ShouldReportTooHighOffsetSecondTimeRecord()
		 {
			  // given
			  PropertyRecord property = InUse( new PropertyRecord( 42 ) );
			  const int keyId = 6;
			  Add( InUse( new PropertyKeyTokenRecord( keyId ) ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long[] longs = org.Neo4Net.kernel.impl.store.TemporalType.encodeTime(keyId, 1, java.time.ZoneOffset.MAX.getTotalSeconds() + 1);
			  long[] longs = TemporalType.encodeTime( keyId, 1, ZoneOffset.MAX.TotalSeconds + 1 );

			  ExpectInvalidPropertyValue( property, longs );
		 }

		 private void ExpectInvalidPropertyValue( PropertyRecord property, long[] longs )
		 {
			  PropertyBlock block = new PropertyBlock();
			  block.ValueBlocks = longs;
			  property.AddPropertyBlock( block );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport report = Check( property );

			  // then
			  verify( report ).invalidPropertyValue( block );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportEmptyArrayRecord()
		 internal virtual void ShouldReportEmptyArrayRecord()
		 {
			  // given
			  PropertyRecord property = InUse( new PropertyRecord( 42 ) );
			  PropertyKeyTokenRecord key = Add( InUse( new PropertyKeyTokenRecord( 6 ) ) );
			  DynamicRecord value = Add( InUse( Array( new DynamicRecord( 1001 ) ) ) );
			  PropertyBlock block = PropertyBlock( key, value );
			  property.AddPropertyBlock( block );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport report = Check( property );

			  // then
			  verify( report ).arrayEmpty( block, value );
			  verifyNoMoreInteractions( report );
		 }
	}

}