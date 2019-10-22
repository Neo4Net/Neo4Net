using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.tools.txlog
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;


	using Neo4Net.Functions;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using PropertyType = Neo4Net.Kernel.impl.store.PropertyType;
	using NeoStoreRecord = Neo4Net.Kernel.Impl.Store.Records.NeoStoreRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyBlock = Neo4Net.Kernel.Impl.Store.Records.PropertyBlock;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using LogEntryCursor = Neo4Net.Kernel.impl.transaction.log.LogEntryCursor;
	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;
	using LogVersionedStoreChannel = Neo4Net.Kernel.impl.transaction.log.LogVersionedStoreChannel;
	using PhysicalFlushableChannel = Neo4Net.Kernel.impl.transaction.log.PhysicalFlushableChannel;
	using PhysicalLogVersionedStoreChannel = Neo4Net.Kernel.impl.transaction.log.PhysicalLogVersionedStoreChannel;
	using PhysicalTransactionRepresentation = Neo4Net.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using TransactionLogWriter = Neo4Net.Kernel.impl.transaction.log.TransactionLogWriter;
	using LogEntryWriter = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryWriter;
	using LogHeaderWriter = Neo4Net.Kernel.impl.transaction.log.entry.LogHeaderWriter;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using TransactionLogFiles = Neo4Net.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.entry.LogHeader.LOG_HEADER_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.tools.txlog.checktypes.CheckTypes.CHECK_TYPES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.tools.txlog.checktypes.CheckTypes.NEO_STORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.tools.txlog.checktypes.CheckTypes.NODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.tools.txlog.checktypes.CheckTypes.PROPERTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.tools.txlog.checktypes.CheckTypes.RELATIONSHIP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.tools.txlog.checktypes.CheckTypes.RELATIONSHIP_GROUP;

	public class CheckTxLogsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.SuppressOutput mute = org.Neo4Net.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput Mute = SuppressOutput.suppressAll();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.fs.EphemeralFileSystemRule fsRule = new org.Neo4Net.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule FsRule = new EphemeralFileSystemRule();
		 private readonly File _storeDirectory = new File( "db" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportNoInconsistenciesFromValidLog() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportNoInconsistenciesFromValidLog()
		 {
			  // Given
			  File log = LogFile( 1 );

			  writeTxContent(log, 1, new Command.NodeCommand(new NodeRecord(42, false, false, -1, -1, 1), new NodeRecord(42, true, false, 42, -1, 1)
						), new Command.PropertyCommand( PropertyRecord( 5, false, -1, -1 ), PropertyRecord( 5, true, -1, -1, 777 ) ), new Command.NodeCommand(new NodeRecord(1, true, true, 2, -1, 1), new NodeRecord(1, true, false, -1, -1, 1)
						));

			  writeTxContent(log, 2, new Command.NodeCommand(new NodeRecord(2, false, false, -1, -1, 1), new NodeRecord(2, true, false, -1, -1, 1)
						), new Command.NodeCommand(new NodeRecord(42, true, false, 42, -1, 1), new NodeRecord(42, true, false, 24, 5, 1)
						));
			  CapturingInconsistenciesHandler handler = new CapturingInconsistenciesHandler();
			  CheckTxLogs checker = new CheckTxLogs( System.out, FsRule.get() );

			  // When
			  bool success = checker.Scan( LogFiles, handler, NODE );

			  // Then
			  assertTrue( success );

			  assertEquals( 0, handler.RecordInconsistencies.Count );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.kernel.impl.transaction.log.files.LogFiles getLogFiles() throws java.io.IOException
		 private LogFiles LogFiles
		 {
			 get
			 {
				  return LogFilesBuilder.logFilesBasedOnlyBuilder( _storeDirectory, FsRule.get() ).build();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportNodeInconsistenciesFromSingleLog() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportNodeInconsistenciesFromSingleLog()
		 {
			  // Given
			  File log = LogFile( 1 );

			  writeTxContent(log, 0, new Command.NodeCommand(new NodeRecord(42, false, false, -1, -1, 1), new NodeRecord(42, true, false, 42, -1, 1)
						), new Command.PropertyCommand( PropertyRecord( 5, false, -1, -1 ), PropertyRecord( 5, true, -1, -1, 777 ) ), new Command.NodeCommand(new NodeRecord(1, true, true, 2, -1, 1), new NodeRecord(1, true, false, -1, -1, 1)
						));

			  writeTxContent(log, 0, new Command.NodeCommand(new NodeRecord(2, false, false, -1, -1, 1), new NodeRecord(2, true, false, -1, -1, 1)
						), new Command.NodeCommand(new NodeRecord(42, true, false, 24, -1, 1), new NodeRecord(42, true, false, 24, 5, 1)
						));

			  CapturingInconsistenciesHandler handler = new CapturingInconsistenciesHandler();
			  CheckTxLogs checker = new CheckTxLogs( System.out, FsRule.get() );

			  // When
			  bool success = checker.Scan( LogFiles, handler, NODE );

			  // Then
			  assertFalse( success );
			  assertEquals( 1, handler.RecordInconsistencies.Count );

			  NodeRecord seenRecord = ( NodeRecord ) handler.RecordInconsistencies[0].committed.record();
			  NodeRecord currentRecord = ( NodeRecord ) handler.RecordInconsistencies[0].current.record();

			  assertEquals( 42, seenRecord.Id );
			  assertEquals( 42, seenRecord.NextRel );
			  assertEquals( 42, currentRecord.Id );
			  assertEquals( 24, currentRecord.NextRel );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportTransactionIdAndInconsistencyCount() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportTransactionIdAndInconsistencyCount()
		 {
			  // Given
			  File log = LogFile( 1 );

			  writeTxContent(log, 0, new Command.NodeCommand(new NodeRecord(42, false, false, -1, -1, 1), new NodeRecord(42, true, false, 42, -1, 1)
						), new Command.PropertyCommand( PropertyRecord( 5, false, -1, -1 ), PropertyRecord( 5, true, -1, -1, 777 ) ), new Command.NodeCommand(new NodeRecord(1, true, true, 2, -1, 1), new NodeRecord(1, true, false, -1, -1, 1)
						), new Command.NodeCommand(new NodeRecord(5, true, true, 2, -1, 1), new NodeRecord(5, true, false, -1, -1, 1)
						));

			  writeTxContent(log, 1, new Command.NodeCommand(new NodeRecord(2, false, false, -1, -1, 1), new NodeRecord(2, true, false, -1, -1, 1)
						), new Command.NodeCommand(new NodeRecord(5, true, true, 2, -1, 1), new NodeRecord(5, true, false, -1, -1, 1)
						), new Command.NodeCommand(new NodeRecord(1, true, false, -1, -1, 1), new NodeRecord(1, true, true, 2, 1, 1)
						), new Command.NodeCommand(new NodeRecord(42, true, false, 24, -1, 1), new NodeRecord(42, true, false, 24, 5, 1)
						));

			  CapturingInconsistenciesHandler handler = new CapturingInconsistenciesHandler();
			  CheckTxLogs checker = new CheckTxLogs( System.out, FsRule.get() );

			  // When
			  checker.Scan( LogFiles, handler, NODE );

			  // Then
			  assertEquals( 2, handler.RecordInconsistencies.Count );

			  assertEquals( 0, handler.RecordInconsistencies[0].committed.txId() );
			  assertEquals( 1, handler.RecordInconsistencies[0].current.txId() );

			  assertEquals( 0, handler.RecordInconsistencies[1].committed.txId() );
			  assertEquals( 1, handler.RecordInconsistencies[1].current.txId() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportNodeInconsistenciesFromDifferentLogs() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportNodeInconsistenciesFromDifferentLogs()
		 {
			  // Given
			  File log1 = LogFile( 1 );
			  File log2 = LogFile( 2 );
			  File log3 = LogFile( 3 );

			  writeTxContent(log1, 0, new Command.NodeCommand(new NodeRecord(42, false, false, -1, -1, 1), new NodeRecord(42, true, false, 42, -1, 1)
						), new Command.PropertyCommand( PropertyRecord( 5, true, -1, -1, 777 ), PropertyRecord( 5, true, -1, -1, 777, 888 ) ), new Command.NodeCommand(new NodeRecord(1, true, true, 2, -1, 1), new NodeRecord(1, true, false, -1, -1, 1)
						));

			  writeTxContent(log2, 0, new Command.NodeCommand(new NodeRecord(2, false, false, -1, -1, 1), new NodeRecord(2, true, false, -1, -1, 1)
						));

			  writeTxContent(log3, 0, new Command.NodeCommand(new NodeRecord(42, true, true, 42, -1, 1), new NodeRecord(42, true, true, 42, 10, 1)
						), new Command.NodeCommand(new NodeRecord(2, true, false, -1, -1, 5), new NodeRecord(2, false, false, -1, -1, 5)
						));

			  CapturingInconsistenciesHandler handler = new CapturingInconsistenciesHandler();
			  CheckTxLogs checker = new CheckTxLogs( System.out, FsRule.get() );

			  // When
			  bool success = checker.Scan( LogFiles, handler, NODE );

			  // Then
			  assertFalse( success );
			  assertEquals( 2, handler.RecordInconsistencies.Count );

			  NodeRecord seenRecord1 = ( NodeRecord ) handler.RecordInconsistencies[0].committed.record();
			  NodeRecord currentRecord1 = ( NodeRecord ) handler.RecordInconsistencies[0].current.record();

			  assertEquals( 42, seenRecord1.Id );
			  assertFalse( seenRecord1.Dense );
			  assertEquals( 42, currentRecord1.Id );
			  assertTrue( currentRecord1.Dense );

			  NodeRecord seenRecord2 = ( NodeRecord ) handler.RecordInconsistencies[1].committed.record();
			  NodeRecord currentRecord2 = ( NodeRecord ) handler.RecordInconsistencies[1].current.record();

			  assertEquals( 2, seenRecord2.Id );
			  assertEquals( 1, seenRecord2.LabelField );
			  assertEquals( 2, currentRecord2.Id );
			  assertEquals( 5, currentRecord2.LabelField );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportPropertyInconsistenciesFromSingleLog() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportPropertyInconsistenciesFromSingleLog()
		 {
			  // Given
			  File log = LogFile( 1 );

			  writeTxContent(log, 0, new Command.PropertyCommand(PropertyRecord(42, false, -1, -1), PropertyRecord(42, true, -1, -1, 10)), new Command.PropertyCommand(PropertyRecord(42, true, -1, -1, 10), PropertyRecord(42, true, 24, -1, 10))
			 );

			  writeTxContent(log, 0, new Command.NodeCommand(new NodeRecord(2, false, false, -1, -1, 1), new NodeRecord(2, true, false, -1, -1, 1)
						), new Command.PropertyCommand( PropertyRecord( 42, true, -1, -1, 10 ), PropertyRecord( 42, true, -1, -1, 10, 20 ) ));

			  CapturingInconsistenciesHandler handler = new CapturingInconsistenciesHandler();
			  CheckTxLogs checker = new CheckTxLogs( System.out, FsRule.get() );

			  // When
			  bool success = checker.Scan( LogFiles, handler, PROPERTY );

			  // Then
			  assertFalse( success );
			  assertEquals( 1, handler.RecordInconsistencies.Count );

			  PropertyRecord seenRecord = ( PropertyRecord ) handler.RecordInconsistencies[0].committed.record();
			  PropertyRecord currentRecord = ( PropertyRecord ) handler.RecordInconsistencies[0].current.record();

			  assertEquals( 42, seenRecord.Id );
			  assertEquals( 24, seenRecord.PrevProp );
			  assertEquals( 42, currentRecord.Id );
			  assertEquals( -1, currentRecord.PrevProp );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportPropertyInconsistenciesFromDifferentLogs() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportPropertyInconsistenciesFromDifferentLogs()
		 {
			  // Given
			  File log1 = LogFile( 1 );
			  File log2 = LogFile( 2 );
			  File log3 = LogFile( 3 );

			  writeTxContent(log1, 0, new Command.NodeCommand(new NodeRecord(42, false, false, -1, -1, 1), new NodeRecord(42, true, false, 42, -1, 1)
						), new Command.PropertyCommand( PropertyRecord( 5, true, -1, -1, 777 ), PropertyRecord( 5, true, -1, -1, 777 ) ), new Command.NodeCommand(new NodeRecord(1, true, true, 2, -1, 1), new NodeRecord(1, true, false, -1, -1, 1)
						));

			  writeTxContent(log2, 0, new Command.PropertyCommand(PropertyRecord(24, false, -1, -1), PropertyRecord(24, true, -1, -1, 777))
			 );

			  writeTxContent(log3, 0, new Command.PropertyCommand(PropertyRecord(24, false, -1, -1), PropertyRecord(24, true, -1, -1, 777)), new Command.NodeCommand(new NodeRecord(42, true, true, 42, -1, 1), new NodeRecord(42, true, true, 42, 10, 1)
						), new Command.PropertyCommand( PropertyRecord( 5, true, -1, -1, 777, 888 ), PropertyRecord( 5, true, -1, 9, 777, 888, 999 ) ));

			  CapturingInconsistenciesHandler handler = new CapturingInconsistenciesHandler();
			  CheckTxLogs checker = new CheckTxLogs( System.out, FsRule.get() );

			  // When
			  bool success = checker.Scan( LogFiles, handler, PROPERTY );

			  // Then
			  assertFalse( success );
			  assertEquals( 2, handler.RecordInconsistencies.Count );

			  RecordInconsistency inconsistency1 = handler.RecordInconsistencies[0];
			  PropertyRecord seenRecord1 = ( PropertyRecord ) inconsistency1.Committed.record();
			  PropertyRecord currentRecord1 = ( PropertyRecord ) inconsistency1.Current.record();

			  assertEquals( 24, seenRecord1.Id );
			  assertTrue( seenRecord1.InUse() );
			  assertEquals( 24, currentRecord1.Id );
			  assertFalse( currentRecord1.InUse() );
			  assertEquals( 2, inconsistency1.Committed.logVersion() );
			  assertEquals( 3, inconsistency1.Current.logVersion() );

			  RecordInconsistency inconsistency2 = handler.RecordInconsistencies[1];
			  PropertyRecord seenRecord2 = ( PropertyRecord ) inconsistency2.Committed.record();
			  PropertyRecord currentRecord2 = ( PropertyRecord ) inconsistency2.Current.record();

			  assertEquals( 5, seenRecord2.Id );
			  assertEquals( 777, seenRecord2.GetPropertyBlock( 0 ).SingleValueInt );
			  assertEquals( 5, currentRecord2.Id );
			  assertEquals( 777, currentRecord2.GetPropertyBlock( 0 ).SingleValueInt );
			  assertEquals( 888, currentRecord2.GetPropertyBlock( 1 ).SingleValueInt );
			  assertEquals( 1, inconsistency2.Committed.logVersion() );
			  assertEquals( 3, inconsistency2.Current.logVersion() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportRelationshipInconsistenciesFromSingleLog() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportRelationshipInconsistenciesFromSingleLog()
		 {
			  // Given
			  File log = LogFile( 1 );

			  writeTxContent(log, 0, new Command.RelationshipCommand(new RelationshipRecord(42, false, -1, -1, -1, -1, -1, -1, -1, false, false), new RelationshipRecord(42, true, 1, 2, 3, 4, 5, 6, 7, true, true)
						), new Command.PropertyCommand( PropertyRecord( 5, false, -1, -1 ), PropertyRecord( 5, true, -1, -1, 777 ) ), new Command.RelationshipCommand(new RelationshipRecord(21, true, 1, 2, 3, 4, 5, 6, 7, true, true), new RelationshipRecord(21, false, -1, -1, -1, -1, -1, -1, -1, false, false)
						));

			  writeTxContent(log, 0, new Command.RelationshipCommand(new RelationshipRecord(53, true, 1, 2, 3, 4, 5, 6, 7, true, true), new RelationshipRecord(53, true, 1, 2, 30, 4, 14, 6, 7, true, true)
						), new Command.RelationshipCommand(new RelationshipRecord(42, true, 1, 2, 3, 9, 5, 6, 7, true, true), new RelationshipRecord(42, true, 1, 2, 3, 4, 5, 6, 7, true, true)
						));

			  CapturingInconsistenciesHandler handler = new CapturingInconsistenciesHandler();
			  CheckTxLogs checker = new CheckTxLogs( System.out, FsRule.get() );

			  // When
			  checker.Scan( LogFiles, handler, RELATIONSHIP );

			  // Then
			  assertEquals( 1, handler.RecordInconsistencies.Count );

			  RelationshipRecord seenRecord = ( RelationshipRecord ) handler.RecordInconsistencies[0].committed.record();
			  RelationshipRecord currentRecord = ( RelationshipRecord ) handler.RecordInconsistencies[0].current.record();

			  assertEquals( 42, seenRecord.Id );
			  assertEquals( 4, seenRecord.FirstPrevRel );
			  assertEquals( 42, currentRecord.Id );
			  assertEquals( 9, currentRecord.FirstPrevRel );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportRelationshipInconsistenciesFromDifferentLogs() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportRelationshipInconsistenciesFromDifferentLogs()
		 {
			  // Given
			  File log1 = LogFile( 1 );
			  File log2 = LogFile( 2 );
			  File log3 = LogFile( 3 );

			  writeTxContent(log1, 0, new Command.RelationshipCommand(new RelationshipRecord(42, false, -1, -1, -1, -1, -1, -1, -1, false, false), new RelationshipRecord(42, true, 1, 2, 3, 4, 5, 6, 7, true, true)
						), new Command.PropertyCommand( PropertyRecord( 5, false, -1, -1 ), PropertyRecord( 5, true, -1, -1, 777 ) ), new Command.RelationshipCommand(new RelationshipRecord(21, true, 1, 2, 3, 4, 5, 6, 7, true, true), new RelationshipRecord(21, false, -1, -1, -1, -1, -1, -1, -1, false, false)
						));

			  writeTxContent(log2, 0, new Command.RelationshipCommand(new RelationshipRecord(42, true, 1, 2, 3, 9, 5, 6, 7, true, true), new RelationshipRecord(42, true, 1, 2, 3, 4, 5, 6, 7, true, true)
						));

			  writeTxContent(log3, 0, new Command.RelationshipCommand(new RelationshipRecord(53, true, 1, 2, 3, 4, 5, 6, 7, true, true), new RelationshipRecord(53, true, 1, 2, 30, 4, 14, 6, 7, true, true)
						), new Command.RelationshipCommand(new RelationshipRecord(42, true, 1, 2, 3, 4, 5, 6, 7, false, true), new RelationshipRecord(42, true, 1, 2, 3, 4, 5, 6, 7, false, true)
						));

			  CapturingInconsistenciesHandler handler = new CapturingInconsistenciesHandler();
			  CheckTxLogs checker = new CheckTxLogs( System.out, FsRule.get() );

			  // When
			  checker.Scan( LogFiles, handler, RELATIONSHIP );

			  // Then
			  assertEquals( 2, handler.RecordInconsistencies.Count );

			  RelationshipRecord seenRecord1 = ( RelationshipRecord ) handler.RecordInconsistencies[0].committed.record();
			  RelationshipRecord currentRecord1 = ( RelationshipRecord ) handler.RecordInconsistencies[0].current.record();

			  assertEquals( 42, seenRecord1.Id );
			  assertEquals( 4, seenRecord1.FirstPrevRel );
			  assertEquals( 42, currentRecord1.Id );
			  assertEquals( 9, currentRecord1.FirstPrevRel );

			  RelationshipRecord seenRecord2 = ( RelationshipRecord ) handler.RecordInconsistencies[1].committed.record();
			  RelationshipRecord currentRecord2 = ( RelationshipRecord ) handler.RecordInconsistencies[1].current.record();

			  assertEquals( 42, seenRecord2.Id );
			  assertTrue( seenRecord2.FirstInFirstChain );
			  assertEquals( 42, currentRecord2.Id );
			  assertFalse( currentRecord2.FirstInFirstChain );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportRelationshipGroupInconsistenciesFromSingleLog() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportRelationshipGroupInconsistenciesFromSingleLog()
		 {
			  // Given
			  File log = LogFile( 1 );

			  writeTxContent(log, 0, new Command.RelationshipGroupCommand(new RelationshipGroupRecord(42, -1, -1, -1, -1, -1, -1, false), new RelationshipGroupRecord(42, 1, 2, 3, 4, 5, 6, true)
						), new Command.PropertyCommand( PropertyRecord( 5, false, -1, -1 ), PropertyRecord( 5, true, -1, -1, 777 ) ), new Command.RelationshipGroupCommand(new RelationshipGroupRecord(21, 1, 2, 3, 4, 5, 7, true), new RelationshipGroupRecord(21, -1, -1, -1, -1, -1, -1, false)
						));

			  writeTxContent(log, 0, new Command.RelationshipGroupCommand(new RelationshipGroupRecord(53, 1, 2, 3, 4, 5, 6, true), new RelationshipGroupRecord(53, 1, 2, 30, 4, 14, 6, true)
						), new Command.RelationshipGroupCommand(new RelationshipGroupRecord(42, 1, 2, 3, 9, 5, 6, true), new RelationshipGroupRecord(42, 1, 2, 3, 4, 5, 6, true)
						));

			  CapturingInconsistenciesHandler handler = new CapturingInconsistenciesHandler();
			  CheckTxLogs checker = new CheckTxLogs( System.out, FsRule.get() );

			  // When
			  checker.Scan( LogFiles, handler, RELATIONSHIP_GROUP );

			  // Then
			  assertEquals( 1, handler.RecordInconsistencies.Count );

			  RelationshipGroupRecord seenRecord = ( RelationshipGroupRecord ) handler.RecordInconsistencies[0].committed.record();
			  RelationshipGroupRecord currentRecord = ( RelationshipGroupRecord ) handler.RecordInconsistencies[0].current.record();

			  assertEquals( 42, seenRecord.Id );
			  assertEquals( 4, seenRecord.FirstLoop );
			  assertEquals( 42, currentRecord.Id );
			  assertEquals( 9, currentRecord.FirstLoop );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportRelationshipGroupInconsistenciesFromDifferentLogs() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportRelationshipGroupInconsistenciesFromDifferentLogs()
		 {
			  // Given
			  File log1 = LogFile( 1 );
			  File log2 = LogFile( 2 );
			  File log3 = LogFile( 3 );

			  writeTxContent(log1, 0, new Command.RelationshipGroupCommand(new RelationshipGroupRecord(42, -1, -1, -1, -1, -1, -1, false), new RelationshipGroupRecord(42, 1, 2, 3, 4, 5, 6, true)
						), new Command.PropertyCommand( PropertyRecord( 5, false, -1, -1 ), PropertyRecord( 5, true, -1, -1, 777 ) ), new Command.RelationshipGroupCommand(new RelationshipGroupRecord(21, 1, 2, 3, 4, 5, 6, true), new RelationshipGroupRecord(21, -1, -1, -1, -1, -1, -1, false)
						));

			  writeTxContent(log2, 0, new Command.RelationshipGroupCommand(new RelationshipGroupRecord(42, 1, 2, 3, 9, 5, 6, true), new RelationshipGroupRecord(42, 1, 2, 3, 4, 5, 6, true)
						));

			  writeTxContent(log3, 0, new Command.RelationshipGroupCommand(new RelationshipGroupRecord(53, 1, 2, 3, 4, 5, 6, true), new RelationshipGroupRecord(53, 1, 2, 30, 4, 14, 6, true)
						), new Command.RelationshipGroupCommand(new RelationshipGroupRecord(42, 1, 2, 3, 4, 5, 6, false), new RelationshipGroupRecord(42, 1, 2, 3, 4, 5, 6, false)
						));

			  CapturingInconsistenciesHandler handler = new CapturingInconsistenciesHandler();
			  CheckTxLogs checker = new CheckTxLogs( System.out, FsRule.get() );

			  // When
			  checker.Scan( LogFiles, handler, RELATIONSHIP_GROUP );

			  // Then
			  assertEquals( 2, handler.RecordInconsistencies.Count );

			  RelationshipGroupRecord seenRecord1 = ( RelationshipGroupRecord ) handler.RecordInconsistencies[0].committed.record();
			  RelationshipGroupRecord currentRecord1 = ( RelationshipGroupRecord ) handler.RecordInconsistencies[0].current.record();

			  assertEquals( 42, seenRecord1.Id );
			  assertEquals( 4, seenRecord1.FirstLoop );
			  assertEquals( 42, currentRecord1.Id );
			  assertEquals( 9, currentRecord1.FirstLoop );

			  RelationshipGroupRecord seenRecord2 = ( RelationshipGroupRecord ) handler.RecordInconsistencies[1].committed.record();
			  RelationshipGroupRecord currentRecord2 = ( RelationshipGroupRecord ) handler.RecordInconsistencies[1].current.record();

			  assertEquals( 42, seenRecord2.Id );
			  assertTrue( seenRecord2.InUse() );
			  assertEquals( 42, currentRecord2.Id );
			  assertFalse( currentRecord2.InUse() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportNeoStoreInconsistenciesFromSingleLog() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportNeoStoreInconsistenciesFromSingleLog()
		 {
			  // Given
			  File log = LogFile( 1 );

			  writeTxContent(log, 0, new Command.NeoStoreCommand(new NeoStoreRecord(), CreateNeoStoreRecord(42)), new Command.PropertyCommand(PropertyRecord(5, false, -1, -1), PropertyRecord(5, true, -1, -1, 777)), new Command.NeoStoreCommand(CreateNeoStoreRecord(42), CreateNeoStoreRecord(21))
			 );

			  writeTxContent(log, 0, new Command.NeoStoreCommand(CreateNeoStoreRecord(42), CreateNeoStoreRecord(33))
			 );

			  CapturingInconsistenciesHandler handler = new CapturingInconsistenciesHandler();
			  CheckTxLogs checker = new CheckTxLogs( System.out, FsRule.get() );

			  // When
			  checker.Scan( LogFiles, handler, NEO_STORE );

			  // Then
			  assertEquals( 1, handler.RecordInconsistencies.Count );

			  NeoStoreRecord seenRecord = ( NeoStoreRecord ) handler.RecordInconsistencies[0].committed.record();
			  NeoStoreRecord currentRecord = ( NeoStoreRecord ) handler.RecordInconsistencies[0].current.record();

			  assertEquals( 21, seenRecord.NextProp );
			  assertEquals( 42, currentRecord.NextProp );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportNeoStoreInconsistenciesFromDifferentLogs() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportNeoStoreInconsistenciesFromDifferentLogs()
		 {
			  // Given
			  File log1 = LogFile( 1 );
			  File log2 = LogFile( 2 );
			  File log3 = LogFile( 3 );

			  writeTxContent(log1, 0, new Command.NeoStoreCommand(new NeoStoreRecord(), CreateNeoStoreRecord(42)), new Command.PropertyCommand(PropertyRecord(5, false, -1, -1), PropertyRecord(5, true, -1, -1, 777)), new Command.NeoStoreCommand(CreateNeoStoreRecord(42), CreateNeoStoreRecord(21))
			 );

			  writeTxContent(log2, 0, new Command.NeoStoreCommand(CreateNeoStoreRecord(12), CreateNeoStoreRecord(21))
			 );

			  writeTxContent(log3, 0, new Command.NeoStoreCommand(CreateNeoStoreRecord(13), CreateNeoStoreRecord(21))
			 );

			  CapturingInconsistenciesHandler handler = new CapturingInconsistenciesHandler();
			  CheckTxLogs checker = new CheckTxLogs( System.out, FsRule.get() );

			  // When
			  checker.Scan( LogFiles, handler, NEO_STORE );

			  // Then
			  assertEquals( 2, handler.RecordInconsistencies.Count );

			  NeoStoreRecord seenRecord1 = ( NeoStoreRecord ) handler.RecordInconsistencies[0].committed.record();
			  NeoStoreRecord currentRecord1 = ( NeoStoreRecord ) handler.RecordInconsistencies[0].current.record();

			  assertEquals( 21, seenRecord1.NextProp );
			  assertEquals( 12, currentRecord1.NextProp );

			  NeoStoreRecord seenRecord2 = ( NeoStoreRecord ) handler.RecordInconsistencies[1].committed.record();
			  NeoStoreRecord currentRecord2 = ( NeoStoreRecord ) handler.RecordInconsistencies[1].current.record();

			  assertEquals( 21, seenRecord2.NextProp );
			  assertEquals( 13, currentRecord2.NextProp );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectAnInconsistentCheckPointPointingToALogFileGreaterThanMaxLogVersion() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectAnInconsistentCheckPointPointingToALogFileGreaterThanMaxLogVersion()
		 {
			  // given
			  File log = LogFile( 1 );
			  WriteCheckPoint( log, 2, LOG_HEADER_SIZE );

			  CapturingInconsistenciesHandler handler = new CapturingInconsistenciesHandler();
			  CheckTxLogs checker = new CheckTxLogs( System.out, FsRule.get() );

			  // when
			  checker.ValidateCheckPoints( LogFiles, handler );

			  // then
			  assertEquals( 1, handler.CheckPointInconsistencies.Count );

			  assertEquals( 1, handler.CheckPointInconsistencies[0].logVersion );
			  assertEquals( new LogPosition( 2, LOG_HEADER_SIZE ), handler.CheckPointInconsistencies[0].logPosition );
			  assertThat( handler.CheckPointInconsistencies[0].size, lessThan( 0L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectAnInconsistentCheckPointPointingToAByteOffsetNotInTheFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectAnInconsistentCheckPointPointingToAByteOffsetNotInTheFile()
		 {
			  // given
			  EnsureLogExists( LogFile( 1 ) );
			  WriteCheckPoint( LogFile( 2 ), 1, 42 );

			  CapturingInconsistenciesHandler handler = new CapturingInconsistenciesHandler();
			  CheckTxLogs checker = new CheckTxLogs( System.out, FsRule.get() );

			  // when
			  checker.ValidateCheckPoints( LogFiles, handler );

			  // then
			  assertEquals( 1, handler.CheckPointInconsistencies.Count );

			  assertEquals( 2, handler.CheckPointInconsistencies[0].logVersion );
			  assertEquals( new LogPosition( 1, 42 ), handler.CheckPointInconsistencies[0].logPosition );
			  assertEquals( LOG_HEADER_SIZE, handler.CheckPointInconsistencies[0].size );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReportInconsistencyIfTheCheckPointAreValidOrTheyReferToPrunedLogs() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotReportInconsistencyIfTheCheckPointAreValidOrTheyReferToPrunedLogs()
		 {
			  // given
			  WriteCheckPoint( LogFile( 1 ), 0, LOG_HEADER_SIZE );
			  WriteCheckPoint( LogFile( 2 ), 1, LOG_HEADER_SIZE );
			  WriteCheckPoint( LogFile( 3 ), 3, LOG_HEADER_SIZE );

			  CapturingInconsistenciesHandler handler = new CapturingInconsistenciesHandler();
			  CheckTxLogs checker = new CheckTxLogs( System.out, FsRule.get() );

			  // when
			  checker.ValidateCheckPoints( LogFiles, handler );

			  // then
			  assertTrue( handler.CheckPointInconsistencies.Count == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportAnInconsistencyIfTxIdSequenceIsNotStrictlyIncreasing() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportAnInconsistencyIfTxIdSequenceIsNotStrictlyIncreasing()
		 {
			  // given
			  System.Func<long, Command.NodeCommand> newNodeCommandFunction = i => new Command.NodeCommand( new NodeRecord( i, false, false, -1, -1, -1 ), new NodeRecord( i, true, false, -1, -1, -1 ) );
			  WriteTxContent( LogFile( 1 ), 40L, newNodeCommandFunction( 1L ) );
			  WriteTxContent( LogFile( 1 ), 41L, newNodeCommandFunction( 2L ) );
			  WriteTxContent( LogFile( 1 ), 42L, newNodeCommandFunction( 3L ) );
			  WriteTxContent( LogFile( 2 ), 42L, newNodeCommandFunction( 4L ) );
			  WriteTxContent( LogFile( 2 ), 43L, newNodeCommandFunction( 5L ) );

			  CapturingInconsistenciesHandler handler = new CapturingInconsistenciesHandler();
			  CheckTxLogs checker = new CheckTxLogs( System.out, FsRule.get() );

			  // when
			  checker.Scan( LogFiles, handler, CHECK_TYPES );

			  // then
			  assertEquals( 1, handler.TxIdSequenceInconsistencies.Count );
			  assertEquals( 42, handler.TxIdSequenceInconsistencies[0].lastSeenTxId );
			  assertEquals( 42, handler.TxIdSequenceInconsistencies[0].currentTxId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportAnInconsistencyIfTxIdSequenceHasGaps() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportAnInconsistencyIfTxIdSequenceHasGaps()
		 {
			  // given
			  System.Func<long, Command.NodeCommand> newNodeCommandFunction = i => new Command.NodeCommand( new NodeRecord( i, false, false, -1, -1, -1 ), new NodeRecord( i, true, false, -1, -1, -1 ) );
			  WriteTxContent( LogFile( 1 ), 40L, newNodeCommandFunction( 1L ) );
			  WriteTxContent( LogFile( 1 ), 41L, newNodeCommandFunction( 2L ) );
			  WriteTxContent( LogFile( 1 ), 42L, newNodeCommandFunction( 3L ) );
			  WriteTxContent( LogFile( 2 ), 44L, newNodeCommandFunction( 4L ) );
			  WriteTxContent( LogFile( 2 ), 45L, newNodeCommandFunction( 5L ) );

			  CapturingInconsistenciesHandler handler = new CapturingInconsistenciesHandler();
			  CheckTxLogs checker = new CheckTxLogs( System.out, FsRule.get() );

			  // when
			  checker.Scan( LogFiles, handler, CHECK_TYPES );

			  // then
			  assertEquals( 1, handler.TxIdSequenceInconsistencies.Count );
			  assertEquals( 42, handler.TxIdSequenceInconsistencies[0].lastSeenTxId );
			  assertEquals( 44, handler.TxIdSequenceInconsistencies[0].currentTxId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportNoInconsistenciesIfTxIdSequenceIsStrictlyIncreasingAndHasNoGaps() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportNoInconsistenciesIfTxIdSequenceIsStrictlyIncreasingAndHasNoGaps()
		 {
			  // given

			  System.Func<long, Command.NodeCommand> newNodeCommandFunction = i => new Command.NodeCommand( new NodeRecord( i, false, false, -1, -1, -1 ), new NodeRecord( i, true, false, -1, -1, -1 ) );
			  WriteTxContent( LogFile( 1 ), 40L, newNodeCommandFunction( 1L ) );
			  WriteTxContent( LogFile( 1 ), 41L, newNodeCommandFunction( 2L ) );
			  WriteTxContent( LogFile( 1 ), 42L, newNodeCommandFunction( 3L ) );
			  WriteTxContent( LogFile( 2 ), 43L, newNodeCommandFunction( 4L ) );
			  WriteTxContent( LogFile( 2 ), 44L, newNodeCommandFunction( 5L ) );
			  WriteTxContent( LogFile( 2 ), 45L, newNodeCommandFunction( 6L ) );

			  CapturingInconsistenciesHandler handler = new CapturingInconsistenciesHandler();
			  CheckTxLogs checker = new CheckTxLogs( System.out, FsRule.get() );

			  // when
			  checker.Scan( LogFiles, handler, CHECK_TYPES );

			  // then
			  assertTrue( handler.TxIdSequenceInconsistencies.Count == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeLogEntryCursorAfterValidation() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseLogEntryCursorAfterValidation()
		 {
			  EnsureLogExists( LogFile( 1 ) );
			  WriteCheckPoint( LogFile( 2 ), 1, 42 );
			  LogEntryCursor entryCursor = Mockito.mock( typeof( LogEntryCursor ) );

			  CheckTxLogsWithCustomLogEntryCursor checkTxLogs = new CheckTxLogsWithCustomLogEntryCursor( this, System.out, FsRule.get(), entryCursor );
			  CapturingInconsistenciesHandler handler = new CapturingInconsistenciesHandler();
			  LogFiles logFiles = LogFiles;
			  bool logsValidity = checkTxLogs.ValidateCheckPoints( logFiles, handler );

			  assertTrue( "empty logs should be valid", logsValidity );
			  verify( entryCursor ).close();
		 }

		 private File LogFile( long version )
		 {
			  FsRule.get().mkdirs(_storeDirectory);
			  return new File( _storeDirectory, TransactionLogFiles.DEFAULT_NAME + "." + version );
		 }

		 private static PropertyRecord PropertyRecord( long id, bool inUse, long prevProp, long nextProp, params long[] blocks )
		 {
			  PropertyRecord record = new PropertyRecord( id );
			  record.InUse = inUse;
			  record.PrevProp = prevProp;
			  record.NextProp = nextProp;
			  for ( int i = 0; i < blocks.Length; i++ )
			  {
					long blockValue = blocks[i];
					PropertyBlock block = new PropertyBlock();
					long value = PropertyStore.singleBlockLongValue( i, PropertyType.INT, blockValue );
					block.SingleBlock = value;
					record.AddPropertyBlock( block );
			  }
			  return record;
		 }

		 private NeoStoreRecord CreateNeoStoreRecord( int nextProp )
		 {
			  NeoStoreRecord neoStoreRecord = new NeoStoreRecord();
			  neoStoreRecord.NextProp = nextProp;
			  return neoStoreRecord;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeTxContent(java.io.File log, long txId, org.Neo4Net.kernel.impl.transaction.command.Command... commands) throws java.io.IOException
		 private void WriteTxContent( File log, long txId, params Command[] commands )
		 {
			  PhysicalTransactionRepresentation tx = new PhysicalTransactionRepresentation( Arrays.asList( commands ) );
			  tx.SetHeader( new sbyte[0], 0, 0, 0, 0, 0, 0 );
			  WriteContent( log, txWriter => txWriter.append( tx, txId ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeCheckPoint(java.io.File log, long logVersion, long byteOffset) throws java.io.IOException
		 private void WriteCheckPoint( File log, long logVersion, long byteOffset )
		 {
			  LogPosition logPosition = new LogPosition( logVersion, byteOffset );
			  WriteContent( log, txWriter => txWriter.checkPoint( logPosition ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeContent(java.io.File log, org.Neo4Net.function.ThrowingConsumer<org.Neo4Net.kernel.impl.transaction.log.TransactionLogWriter,java.io.IOException> consumer) throws java.io.IOException
		 private void WriteContent( File log, ThrowingConsumer<TransactionLogWriter, IOException> consumer )
		 {
			  EnsureLogExists( log );
			  using ( StoreChannel channel = FsRule.open( log, OpenMode.READ_WRITE ), LogVersionedStoreChannel versionedChannel = new PhysicalLogVersionedStoreChannel( channel, 0, ( sbyte ) 0 ), PhysicalFlushableChannel writableLogChannel = new PhysicalFlushableChannel( versionedChannel ) )
			  {
					long offset = channel.size();
					channel.Position( offset );

					consumer.Accept( new TransactionLogWriter( new LogEntryWriter( writableLogChannel ) ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File ensureLogExists(java.io.File logFile) throws java.io.IOException
		 private File EnsureLogExists( File logFile )
		 {
			  FileSystemAbstraction fs = FsRule.get();
			  if ( !fs.FileExists( logFile ) )
			  {
					LogHeaderWriter.writeLogHeader( fs, logFile, LogFiles.getLogVersion( logFile ), 0 );
			  }
			  return logFile;
		 }

		 private class CheckTxLogsWithCustomLogEntryCursor : CheckTxLogs
		 {
			 private readonly CheckTxLogsTest _outerInstance;


			  internal readonly LogEntryCursor LogEntryCursor;

			  internal CheckTxLogsWithCustomLogEntryCursor( CheckTxLogsTest outerInstance, PrintStream @out, FileSystemAbstraction fs, LogEntryCursor logEntryCursor ) : base( @out, fs )
			  {
				  this._outerInstance = outerInstance;
					this.LogEntryCursor = logEntryCursor;
			  }

			  internal override LogEntryCursor OpenLogEntryCursor( LogFiles logFiles )
			  {
					return LogEntryCursor;
			  }
		 }

		 private class CapturingInconsistenciesHandler : InconsistenciesHandler
		 {
			  internal IList<TxIdSequenceInconsistency> TxIdSequenceInconsistencies = new List<TxIdSequenceInconsistency>();
			  internal IList<CheckPointInconsistency> CheckPointInconsistencies = new List<CheckPointInconsistency>();
			  internal IList<RecordInconsistency> RecordInconsistencies = new List<RecordInconsistency>();

			  public override void ReportInconsistentCheckPoint( long logVersion, LogPosition logPosition, long size )
			  {
					CheckPointInconsistencies.Add( new CheckPointInconsistency( logVersion, logPosition, size ) );
			  }

			  public override void ReportInconsistentCommand<T1, T2>( RecordInfo<T1> committed, RecordInfo<T2> current )
			  {
					RecordInconsistencies.Add( new RecordInconsistency( committed, current ) );
			  }

			  public override void ReportInconsistentTxIdSequence( long lastSeenTxId, long currentTxId )
			  {
					TxIdSequenceInconsistencies.Add( new TxIdSequenceInconsistency( lastSeenTxId, currentTxId ) );
			  }
		 }

		 private class TxIdSequenceInconsistency
		 {
			  internal readonly long LastSeenTxId;
			  internal readonly long CurrentTxId;

			  internal TxIdSequenceInconsistency( long lastSeenTxId, long currentTxId )
			  {
					this.LastSeenTxId = lastSeenTxId;
					this.CurrentTxId = currentTxId;
			  }

			  public override string ToString()
			  {
					return "TxIdSequenceInconsistency{" + "lastSeenTxId=" + LastSeenTxId + ", currentTxId=" + CurrentTxId + '}';
			  }
		 }

		 private class CheckPointInconsistency
		 {
			  internal readonly long LogVersion;
			  internal readonly LogPosition LogPosition;
			  internal readonly long Size;

			  internal CheckPointInconsistency( long logVersion, LogPosition logPosition, long? size )
			  {
					this.LogVersion = logVersion;
					this.LogPosition = logPosition;
					this.Size = size.Value;
			  }
		 }

		 private class RecordInconsistency
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: final RecordInfo<?> committed;
			  internal readonly RecordInfo<object> Committed;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: final RecordInfo<?> current;
			  internal readonly RecordInfo<object> Current;

			  internal RecordInconsistency<T1, T2>( RecordInfo<T1> committed, RecordInfo<T2> current )
			  {
					this.Committed = committed;
					this.Current = current;
			  }
		 }
	}

}