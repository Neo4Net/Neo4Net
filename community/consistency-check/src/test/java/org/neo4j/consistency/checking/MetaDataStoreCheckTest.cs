﻿/*
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
namespace Org.Neo4j.Consistency.checking
{
	using Test = org.junit.jupiter.api.Test;

	using ConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport;
	using NeoStoreRecord = Org.Neo4j.Kernel.impl.store.record.NeoStoreRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;

	internal class MetaDataStoreCheckTest : RecordCheckTestBase<NeoStoreRecord, Org.Neo4j.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport, NeoStoreCheck>
	{
		 internal MetaDataStoreCheckTest() : base(new NeoStoreCheck(new PropertyChain<>(from -> null)), typeof(org.neo4j.consistency.report.ConsistencyReport_NeoStoreConsistencyReport), new int[0])
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotReportAnythingForRecordWithNoPropertyReference()
		 internal virtual void ShouldNotReportAnythingForRecordWithNoPropertyReference()
		 {
			  // given
			  NeoStoreRecord record = new NeoStoreRecord();

			  // when
			  Org.Neo4j.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport report = Check( record );

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
			  Org.Neo4j.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport report = Check( record );

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
			  Org.Neo4j.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport report = Check( record );

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
			  Org.Neo4j.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport report = Check( record );

			  // then
			  verify( report ).propertyNotFirstInChain( property );
			  verifyNoMoreInteractions( report );
		 }
	}

}