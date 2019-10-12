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
namespace Org.Neo4j.Consistency.checking.full
{
	using Test = org.junit.jupiter.api.Test;

	using Org.Neo4j.Consistency.checking;
	using Org.Neo4j.Consistency.checking;
	using ConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport;
	using RecordAccessStub = Org.Neo4j.Consistency.store.RecordAccessStub;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PropertyBlock = Org.Neo4j.Kernel.impl.store.record.PropertyBlock;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.RecordCheckTestBase.inUse;

	internal class DuplicatePropertyTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportDuplicatePropertyIndexesInPropertyRecordForNode()
		 internal virtual void ShouldReportDuplicatePropertyIndexesInPropertyRecordForNode()
		 {
			  // given
			  ChainCheck check = new ChainCheck();

			  RecordAccessStub records = new RecordAccessStub();

			  NodeRecord master = records.Add( inUse( new NodeRecord( 1, false, -1, 1 ) ) );

			  PropertyRecord propertyRecord = inUse( new PropertyRecord( 1 ) );
			  PropertyBlock firstBlock = new PropertyBlock();
			  firstBlock.SingleBlock = 1;
			  firstBlock.KeyIndexId = 1;

			  PropertyBlock secondBlock = new PropertyBlock();
			  secondBlock.SingleBlock = 1;
			  secondBlock.KeyIndexId = 2;

			  PropertyBlock thirdBlock = new PropertyBlock();
			  thirdBlock.SingleBlock = 1;
			  thirdBlock.KeyIndexId = 1;

			  propertyRecord.AddPropertyBlock( firstBlock );
			  propertyRecord.AddPropertyBlock( secondBlock );
			  propertyRecord.AddPropertyBlock( thirdBlock );

			  records.Add( propertyRecord );

			  // when
			  Org.Neo4j.Consistency.report.ConsistencyReport_NodeConsistencyReport report = mock( typeof( Org.Neo4j.Consistency.report.ConsistencyReport_NodeConsistencyReport ) );
			  CheckerEngine<NodeRecord, Org.Neo4j.Consistency.report.ConsistencyReport_NodeConsistencyReport> checkEngine = records.Engine( master, report );
			  check.checkReference( master, propertyRecord, checkEngine, records );

			  // then
			  verify( report ).propertyKeyNotUniqueInChain();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportDuplicatePropertyIndexesAcrossRecordsInPropertyChainForNode()
		 internal virtual void ShouldReportDuplicatePropertyIndexesAcrossRecordsInPropertyChainForNode()
		 {
			  // given
			  ChainCheck check = new ChainCheck();

			  RecordAccessStub records = new RecordAccessStub();

			  RelationshipRecord master = records.Add( inUse( new RelationshipRecord( 1, 2, 3, 4 ) ) );
			  master.NextProp = 1;

			  PropertyRecord firstRecord = inUse( new PropertyRecord( 1 ) );
			  firstRecord.NextProp = 12;

			  PropertyBlock firstBlock = new PropertyBlock();
			  firstBlock.SingleBlock = 1;
			  firstBlock.KeyIndexId = 1;

			  PropertyBlock secondBlock = new PropertyBlock();
			  secondBlock.SingleBlock = 1;
			  secondBlock.KeyIndexId = 2;

			  PropertyRecord secondRecord = inUse( new PropertyRecord( 12 ) );
			  secondRecord.PrevProp = 1;

			  PropertyBlock thirdBlock = new PropertyBlock();
			  thirdBlock.SingleBlock = 1;
			  thirdBlock.KeyIndexId = 4;

			  PropertyBlock fourthBlock = new PropertyBlock();
			  fourthBlock.SingleBlock = 1;
			  fourthBlock.KeyIndexId = 1;

			  firstRecord.AddPropertyBlock( firstBlock );
			  firstRecord.AddPropertyBlock( secondBlock );
			  secondRecord.AddPropertyBlock( thirdBlock );
			  secondRecord.AddPropertyBlock( fourthBlock );

			  records.Add( firstRecord );
			  records.Add( secondRecord );

			  // when
			  Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipConsistencyReport report = mock( typeof( Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipConsistencyReport ) );
			  CheckerEngine<RelationshipRecord, Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipConsistencyReport> checkEngine = records.Engine( master, report );
			  check.checkReference( master, firstRecord, checkEngine, records );
			  records.CheckDeferred();

			  // then
			  verify( report ).propertyKeyNotUniqueInChain();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotReportAnythingForConsistentChains()
		 internal virtual void ShouldNotReportAnythingForConsistentChains()
		 {
			  // given
			  ChainCheck check = new ChainCheck();

			  RecordAccessStub records = new RecordAccessStub();

			  RelationshipRecord master = records.Add( inUse( new RelationshipRecord( 1, 2, 3, 4 ) ) );
			  master.NextProp = 1;

			  PropertyRecord firstRecord = inUse( new PropertyRecord( 1 ) );
			  firstRecord.NextProp = 12;

			  PropertyBlock firstBlock = new PropertyBlock();
			  firstBlock.SingleBlock = 1;
			  firstBlock.KeyIndexId = 1;

			  PropertyBlock secondBlock = new PropertyBlock();
			  secondBlock.SingleBlock = 1;
			  secondBlock.KeyIndexId = 2;

			  PropertyRecord secondRecord = inUse( new PropertyRecord( 12 ) );
			  secondRecord.PrevProp = 1;

			  PropertyBlock thirdBlock = new PropertyBlock();
			  thirdBlock.SingleBlock = 1;
			  thirdBlock.KeyIndexId = 4;

			  PropertyBlock fourthBlock = new PropertyBlock();
			  fourthBlock.SingleBlock = 11;
			  fourthBlock.KeyIndexId = 11;

			  firstRecord.AddPropertyBlock( firstBlock );
			  firstRecord.AddPropertyBlock( secondBlock );
			  secondRecord.AddPropertyBlock( thirdBlock );
			  secondRecord.AddPropertyBlock( fourthBlock );

			  records.Add( firstRecord );
			  records.Add( secondRecord );

			  // when
			  Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipConsistencyReport report = mock( typeof( Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipConsistencyReport ) );
			  CheckerEngine<RelationshipRecord, Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipConsistencyReport> checkEngine = records.Engine( master, report );
			  check.checkReference( master, firstRecord, checkEngine, records );
			  records.CheckDeferred();

			  // then
			  verifyZeroInteractions( report );
		 }
	}

}