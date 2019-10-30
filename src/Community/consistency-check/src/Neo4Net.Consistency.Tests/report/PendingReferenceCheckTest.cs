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
namespace Neo4Net.Consistency.report
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;

	using Neo4Net.Consistency.checking;
	using Neo4Net.Consistency.report.ConsistencyReporter;
	using RecordAccess = Neo4Net.Consistency.Store.RecordAccess;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.consistency.report.ConsistencyReporter.NO_MONITOR;

	internal class PendingReferenceCheckTest
	{
		 private PendingReferenceCheck<PropertyRecord> _referenceCheck;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  RecordAccess records = mock( typeof( RecordAccess ) );
			  ReportHandler handler = new ReportHandler( mock( typeof( InconsistencyReport ) ), mock( typeof( ConsistencyReporter.ProxyFactory ) ), RecordType.PROPERTY, records, new PropertyRecord( 0 ), NO_MONITOR );
			  this._referenceCheck = new PendingReferenceCheck<PropertyRecord>( handler, mock( typeof( ComparativeRecordChecker ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAllowSkipAfterSkip()
		 internal virtual void ShouldAllowSkipAfterSkip()
		 {
			  // given
			  _referenceCheck.skip();
			  // when
			  _referenceCheck.skip();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAllowSkipAfterCheckReference()
		 internal virtual void ShouldAllowSkipAfterCheckReference()
		 {
			  // given
			  _referenceCheck.checkReference( new PropertyRecord( 0 ), null );
			  // when
			  _referenceCheck.skip();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAllowSkipAfterCheckDiffReference()
		 internal virtual void ShouldAllowSkipAfterCheckDiffReference()
		 {
			  // given
			  _referenceCheck.checkDiffReference( new PropertyRecord( 0 ), new PropertyRecord( 0 ), null );
			  // when
			  _referenceCheck.skip();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotAllowCheckReferenceAfterSkip()
		 internal virtual void ShouldNotAllowCheckReferenceAfterSkip()
		 {
			  // given
			  _referenceCheck.skip();

			  // when
			  System.InvalidOperationException exception = assertThrows( typeof( System.InvalidOperationException ), () => _referenceCheck.checkReference(new PropertyRecord(0), null) );
			  assertEquals( "Reference has already been checked.", exception.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotAllowCheckDiffReferenceAfterSkip()
		 internal virtual void ShouldNotAllowCheckDiffReferenceAfterSkip()
		 {
			  // given
			  _referenceCheck.skip();

			  // when
			  System.InvalidOperationException exception = assertThrows( typeof( System.InvalidOperationException ), () => _referenceCheck.checkDiffReference(new PropertyRecord(0), new PropertyRecord(0), null) );
			  assertEquals( "Reference has already been checked.", exception.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotAllowCheckReferenceAfterCheckReference()
		 internal virtual void ShouldNotAllowCheckReferenceAfterCheckReference()
		 {
			  // given
			  _referenceCheck.checkReference( new PropertyRecord( 0 ), null );

			  // when
			  System.InvalidOperationException exception = assertThrows( typeof( System.InvalidOperationException ), () => _referenceCheck.checkReference(new PropertyRecord(0), null) );
			  assertEquals( "Reference has already been checked.", exception.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotAllowCheckDiffReferenceAfterCheckReference()
		 internal virtual void ShouldNotAllowCheckDiffReferenceAfterCheckReference()
		 {
			  // given
			  _referenceCheck.checkReference( new PropertyRecord( 0 ), null );

			  // when
			  System.InvalidOperationException exception = assertThrows( typeof( System.InvalidOperationException ), () => _referenceCheck.checkDiffReference(new PropertyRecord(0), new PropertyRecord(0), null) );
			  assertEquals( "Reference has already been checked.", exception.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotAllowCheckReferenceAfterCheckDiffReference()
		 internal virtual void ShouldNotAllowCheckReferenceAfterCheckDiffReference()
		 {
			  // given
			  _referenceCheck.checkDiffReference( new PropertyRecord( 0 ), new PropertyRecord( 0 ), null );

			  // when
			  System.InvalidOperationException exception = assertThrows( typeof( System.InvalidOperationException ), () => _referenceCheck.checkReference(new PropertyRecord(0), null) );
			  assertEquals( "Reference has already been checked.", exception.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotAllowCheckDiffReferenceAfterCheckDiffReference()
		 internal virtual void ShouldNotAllowCheckDiffReferenceAfterCheckDiffReference()
		 {
			  // given
			  _referenceCheck.checkDiffReference( new PropertyRecord( 0 ), new PropertyRecord( 0 ), null );

			  // when
			  System.InvalidOperationException exception = assertThrows( typeof( System.InvalidOperationException ), () => _referenceCheck.checkDiffReference(new PropertyRecord(0), new PropertyRecord(0), null) );
			  assertEquals( "Reference has already been checked.", exception.Message );
		 }
	}

}