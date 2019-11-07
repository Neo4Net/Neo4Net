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
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using JUnit4 = org.junit.runners.JUnit4;
	using Suite = org.junit.runners.Suite;

	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using PropertyType = Neo4Net.Kernel.impl.store.PropertyType;
	using Neo4Net.Kernel.impl.store;
	using SchemaStore = Neo4Net.Kernel.impl.store.SchemaStore;
	using DynamicRecordFormat = Neo4Net.Kernel.impl.store.format.standard.DynamicRecordFormat;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.consistency.store.RecordAccessStub.SCHEMA_RECORD_TYPE;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Suite.class) @Suite.SuiteClasses({ DynamicRecordCheckTest.StringDynamicRecordCheckTest.class, DynamicRecordCheckTest.ArrayDynamicRecordCheckTest.class, DynamicRecordCheckTest.SchemaDynamicRecordCheckTest.class }) public abstract class DynamicRecordCheckTest extends RecordCheckTestBase<Neo4Net.kernel.impl.store.record.DynamicRecord,Neo4Net.consistency.report.ConsistencyReport_DynamicConsistencyReport,DynamicRecordCheck>
	public abstract class DynamicRecordCheckTest : RecordCheckTestBase<DynamicRecord, Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport, DynamicRecordCheck>
	{
		 private readonly int _blockSize;

		 private DynamicRecordCheckTest( DynamicRecordCheck check, int blockSize ) : base( check, typeof( Neo4Net.consistency.report.ConsistencyReport_DynamicConsistencyReport ), new int[0] )
		 {
			  this._blockSize = blockSize;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReportAnythingForRecordNotInUse()
		 public virtual void ShouldNotReportAnythingForRecordNotInUse()
		 {
			  // given
			  DynamicRecord property = NotInUse( Record( 42 ) );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport report = Check( property );

			  // then
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReportAnythingForRecordThatDoesNotReferenceOtherRecords()
		 public virtual void ShouldNotReportAnythingForRecordThatDoesNotReferenceOtherRecords()
		 {
			  // given
			  DynamicRecord property = InUse( Fill( Record( 42 ), _blockSize / 2 ) );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport report = Check( property );

			  // then
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReportAnythingForRecordWithConsistentReferences()
		 public virtual void ShouldNotReportAnythingForRecordWithConsistentReferences()
		 {
			  // given
			  DynamicRecord property = InUse( Fill( Record( 42 ) ) );
			  DynamicRecord next = Add( InUse( Fill( Record( 7 ), _blockSize / 2 ) ) );
			  property.NextBlock = next.Id;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport report = Check( property );

			  // then
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportNextRecordNotInUse()
		 public virtual void ShouldReportNextRecordNotInUse()
		 {
			  // given
			  DynamicRecord property = InUse( Fill( Record( 42 ) ) );
			  DynamicRecord next = Add( NotInUse( Record( 7 ) ) );
			  property.NextBlock = next.Id;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport report = Check( property );

			  // then
			  verify( report ).nextNotInUse( next );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportSelfReferentialNext()
		 public virtual void ShouldReportSelfReferentialNext()
		 {
			  // given
			  DynamicRecord property = Add( InUse( Fill( Record( 42 ) ) ) );
			  property.NextBlock = property.Id;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport report = Check( property );

			  // then
			  verify( report ).selfReferentialNext();
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportNonFullRecordWithNextReference()
		 public virtual void ShouldReportNonFullRecordWithNextReference()
		 {
			  // given
			  DynamicRecord property = InUse( Fill( Record( 42 ), _blockSize - 1 ) );
			  DynamicRecord next = Add( InUse( Fill( Record( 7 ), _blockSize / 2 ) ) );
			  property.NextBlock = next.Id;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport report = Check( property );

			  // then
			  verify( report ).recordNotFullReferencesNext();
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportInvalidDataLength()
		 public virtual void ShouldReportInvalidDataLength()
		 {
			  // given
			  DynamicRecord property = InUse( Record( 42 ) );
			  property.Length = -1;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport report = Check( property );

			  // then
			  verify( report ).invalidLength();
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportEmptyRecord()
		 public virtual void ShouldReportEmptyRecord()
		 {
			  // given
			  DynamicRecord property = InUse( Record( 42 ) );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport report = Check( property );

			  // then
			  verify( report ).emptyBlock();
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportRecordWithEmptyNext()
		 public virtual void ShouldReportRecordWithEmptyNext()
		 {
			  // given
			  DynamicRecord property = InUse( Fill( Record( 42 ) ) );
			  DynamicRecord next = Add( InUse( Record( 7 ) ) );
			  property.NextBlock = next.Id;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport report = Check( property );

			  // then
			  verify( report ).emptyNextBlock( next );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportCorrectTypeBasedOnProperBitsOnly()
		 public virtual void ShouldReportCorrectTypeBasedOnProperBitsOnly()
		 {
			  // given
			  DynamicRecord property = InUse( Record( 42 ) );
			  // Type is 9, which is string, but has an extra bit set at a higher up position
			  int type = PropertyType.STRING.intValue();
			  type = type | 0b10000000;

			  property.SetType( type );

			  // when
			  PropertyType reportedType = property.getType();

			  // then
			  // The type must be string
			  assertEquals( PropertyType.STRING, reportedType );
			  // but the type data must be preserved
			  assertEquals( type, property.TypeAsInt );
		 }

		 // utilities

		 internal virtual DynamicRecord Fill( DynamicRecord record )
		 {
			  return Fill( record, _blockSize );
		 }

		 internal abstract DynamicRecord Fill( DynamicRecord record, int size );

		 internal abstract DynamicRecord Record( long id );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(JUnit4.class) public static class StringDynamicRecordCheckTest extends DynamicRecordCheckTest
		 public class StringDynamicRecordCheckTest : DynamicRecordCheckTest
		 {
			  public StringDynamicRecordCheckTest() : base(new DynamicRecordCheck(ConfigureDynamicStore(66), DynamicStore.String), 66)
			  {
			  }

			  internal override DynamicRecord Record( long id )
			  {
					return String( new DynamicRecord( id ) );
			  }

			  internal override DynamicRecord Fill( DynamicRecord record, int size )
			  {
					record.Length = size;
					return record;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(JUnit4.class) public static class ArrayDynamicRecordCheckTest extends DynamicRecordCheckTest
		 public class ArrayDynamicRecordCheckTest : DynamicRecordCheckTest
		 {
			  public ArrayDynamicRecordCheckTest() : base(new DynamicRecordCheck(ConfigureDynamicStore(66), DynamicStore.Array), 66)
			  {
			  }

			  internal override DynamicRecord Record( long id )
			  {
					return Array( new DynamicRecord( id ) );
			  }

			  internal override DynamicRecord Fill( DynamicRecord record, int size )
			  {
					record.Length = size;
					return record;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(JUnit4.class) public static class SchemaDynamicRecordCheckTest extends DynamicRecordCheckTest
		 public class SchemaDynamicRecordCheckTest : DynamicRecordCheckTest
		 {
			  public SchemaDynamicRecordCheckTest() : base(new DynamicRecordCheck(ConfigureDynamicStore(SchemaStore.BLOCK_SIZE), DynamicStore.Schema), SchemaStore.BLOCK_SIZE)
			  {
			  }

			  internal override DynamicRecord Record( long id )
			  {
					DynamicRecord result = new DynamicRecord( id );
					result.SetType( SCHEMA_RECORD_TYPE );
					return result;
			  }

			  internal override DynamicRecord Fill( DynamicRecord record, int size )
			  {
					record.Length = size;
					return record;
			  }
		 }

		 public static RecordStore<DynamicRecord> ConfigureDynamicStore( int blockSize )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") Neo4Net.kernel.impl.store.RecordStore<Neo4Net.kernel.impl.store.record.DynamicRecord> mock = mock(Neo4Net.kernel.impl.store.RecordStore.class);
			  RecordStore<DynamicRecord> mock = mock( typeof( RecordStore ) );
			  when( mock.RecordSize ).thenReturn( blockSize + DynamicRecordFormat.RECORD_HEADER_SIZE );
			  when( mock.RecordDataSize ).thenReturn( blockSize );
			  return mock;
		 }
	}

}