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
	using NeoStoreRecord = Neo4Net.Kernel.Impl.Store.Records.NeoStoreRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;

	internal class MetaDataStoreCheckTest : RecordCheckTestBase<NeoStoreRecord, Neo4Net.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport, NeoStoreCheck>
	{
		 internal MetaDataStoreCheckTest() : base(new NeoStoreCheck(new PropertyChain<>(from -> null)), typeof(Neo4Net.consistency.report.ConsistencyReport_NeoStoreConsistencyReport), new int[0])
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotReportAnythingForRecordWithNoPropertyReference()
		 internal virtual void ShouldNotReportAnythingForRecordWithNoPropertyReference()
		 {
			  // given
			  NeoStoreRecord record = new NeoStoreRecord();

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport report = Check( record );

			  // then
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotReportAnythingForRecordWithConsistentReferenceToProperty()
		 internal virtual void ShouldNotReportAnythingForRecordWithConsistentReferenceToProperty()
		 {
			  // given
			  NeoStoreRecord record = new NeoStoreRecord();
			  record.NextProp = Add( InUse( new PropertyRecord( 7 ) ) ).Id;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport report = Check( record );

			  // then
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportPropertyNotInUse()
		 internal virtual void ShouldReportPropertyNotInUse()
		 {
			  // given
			  NeoStoreRecord record = new NeoStoreRecord();
			  PropertyRecord property = Add( NotInUse( new PropertyRecord( 7 ) ) );
			  record.NextProp = property.Id;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport report = Check( record );

			  // then
			  verify( report ).propertyNotInUse( property );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportPropertyNotFirstInChain()
		 internal virtual void ShouldReportPropertyNotFirstInChain()
		 {
			  // given
			  NeoStoreRecord record = new NeoStoreRecord();
			  PropertyRecord property = Add( InUse( new PropertyRecord( 7 ) ) );
			  property.PrevProp = 6;
			  record.NextProp = property.Id;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport report = Check( record );

			  // then
			  verify( report ).propertyNotFirstInChain( property );
			  verifyNoMoreInteractions( report );
		 }
	}

}