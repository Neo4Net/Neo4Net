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

	using ConsistencyReport_RelationshipTypeConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_RelationshipTypeConsistencyReport;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;

	internal class RelationshipTypeTokenRecordCheckTest : RecordCheckTestBase<RelationshipTypeTokenRecord, ConsistencyReport_RelationshipTypeConsistencyReport, RelationshipTypeTokenRecordCheck>
	{
		 internal RelationshipTypeTokenRecordCheckTest() : base(new RelationshipTypeTokenRecordCheck(), typeof(ConsistencyReport_RelationshipTypeConsistencyReport), new int[0])
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotReportAnythingForRecordNotInUse()
		 internal virtual void ShouldNotReportAnythingForRecordNotInUse()
		 {
			  // given
			  RelationshipTypeTokenRecord label = NotInUse( new RelationshipTypeTokenRecord( 42 ) );

			  // when
			  ConsistencyReport_RelationshipTypeConsistencyReport report = Check( label );

			  // then
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotReportAnythingForRecordThatDoesNotReferenceADynamicBlock()
		 internal virtual void ShouldNotReportAnythingForRecordThatDoesNotReferenceADynamicBlock()
		 {
			  // given
			  RelationshipTypeTokenRecord label = InUse( new RelationshipTypeTokenRecord( 42 ) );

			  // when
			  ConsistencyReport_RelationshipTypeConsistencyReport report = Check( label );

			  // then
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportDynamicBlockNotInUse()
		 internal virtual void ShouldReportDynamicBlockNotInUse()
		 {
			  // given
			  RelationshipTypeTokenRecord label = InUse( new RelationshipTypeTokenRecord( 42 ) );
			  DynamicRecord name = AddRelationshipTypeName( NotInUse( new DynamicRecord( 6 ) ) );
			  label.NameId = ( int ) name.Id;

			  // when
			  ConsistencyReport_RelationshipTypeConsistencyReport report = Check( label );

			  // then
			  verify( report ).nameBlockNotInUse( name );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportEmptyName()
		 internal virtual void ShouldReportEmptyName()
		 {
			  // given
			  RelationshipTypeTokenRecord label = InUse( new RelationshipTypeTokenRecord( 42 ) );
			  DynamicRecord name = AddRelationshipTypeName( InUse( new DynamicRecord( 6 ) ) );
			  label.NameId = ( int ) name.Id;

			  // when
			  ConsistencyReport_RelationshipTypeConsistencyReport report = Check( label );

			  // then
			  verify( report ).emptyName( name );
			  verifyNoMoreInteractions( report );
		 }
	}

}