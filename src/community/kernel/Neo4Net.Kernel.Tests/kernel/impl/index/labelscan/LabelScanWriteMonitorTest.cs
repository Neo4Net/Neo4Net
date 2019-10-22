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
namespace Neo4Net.Kernel.impl.index.labelscan
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;
	using Mockito = org.mockito.Mockito;


	using ByteUnit = Neo4Net.Io.ByteUnit;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.abs;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class LabelScanWriteMonitorTest
	{
		private bool InstanceFieldsInitialized = false;

		public LabelScanWriteMonitorTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Directory = TestDirectory.testDirectory( Fs );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.fs.DefaultFileSystemRule fs = new org.Neo4Net.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule Fs = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory directory = org.Neo4Net.test.rule.TestDirectory.testDirectory(fs);
		 public TestDirectory Directory;

		 private string _baseName;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _baseName = LabelScanWriteMonitor.WriteLogBaseFile( Directory.databaseLayout() ).Name;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRotateExistingFileOnOpen()
		 public virtual void ShouldRotateExistingFileOnOpen()
		 {
			  // given
			  LabelScanWriteMonitor writeMonitor = new LabelScanWriteMonitor( Fs, Directory.databaseLayout() );
			  writeMonitor.Close();

			  // when
			  LabelScanWriteMonitor secondWriteMonitor = new LabelScanWriteMonitor( Fs, Directory.databaseLayout() );
			  secondWriteMonitor.Close();

			  // then
			  assertEquals( 2, Directory.databaseDir().listFiles((dir, name) => name.StartsWith(_baseName)).length );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogAndDumpData() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogAndDumpData()
		 {
			  // given
			  DatabaseLayout databaseLayout = this.Directory.databaseLayout();
			  LabelScanWriteMonitor writeMonitor = new LabelScanWriteMonitor( Fs, databaseLayout );
			  LabelScanValue value = new LabelScanValue();
			  writeMonitor.Range( 3, 0 );
			  writeMonitor.PrepareAdd( 123, 4 );
			  writeMonitor.PrepareAdd( 123, 5 );
			  writeMonitor.MergeAdd( new LabelScanValue(), value.Set(4).set(5) );
			  writeMonitor.FlushPendingUpdates();
			  writeMonitor.PrepareRemove( 124, 5 );
			  writeMonitor.MergeRemove( value, ( new LabelScanValue() ).Set(5) );
			  writeMonitor.WriteSessionEnded();
			  writeMonitor.Range( 5, 1 );
			  writeMonitor.PrepareAdd( 125, 10 );
			  writeMonitor.MergeAdd( ( new LabelScanValue() ).Set(9), (new LabelScanValue()).Set(10) );
			  writeMonitor.FlushPendingUpdates();
			  writeMonitor.WriteSessionEnded();
			  writeMonitor.Close();

			  // when
			  LabelScanWriteMonitor.Dumper dumper = mock( typeof( LabelScanWriteMonitor.Dumper ) );
			  LabelScanWriteMonitor.Dump( Fs, databaseLayout, dumper, null );

			  // then
			  InOrder inOrder = Mockito.inOrder( dumper );
			  inOrder.verify( dumper ).prepare( true, 0, 0, 123, 64 * 3 + 4, 0 );
			  inOrder.verify( dumper ).prepare( true, 0, 0, 123, 64 * 3 + 5, 0 );
			  inOrder.verify( dumper ).merge( true, 0, 0, 3, 0, 0, 0b00000000_0000000_00000000_00000000__00000000_00000000_00000000_00110000 );
			  inOrder.verify( dumper ).prepare( false, 0, 1, 124, 64 * 3 + 5, 0 );
			  inOrder.verify( dumper ).merge( false, 0, 1, 3, 0, 0b00000000_0000000_00000000_00000000__00000000_00000000_00000000_00110000, 0b00000000_0000000_00000000_00000000__00000000_00000000_00000000_00100000 );
			  inOrder.verify( dumper ).prepare( true, 1, 0, 125, 64 * 5 + 10, 1 );
			  inOrder.verify( dumper ).merge( true, 1, 0, 5, 1, 0b00000000_0000000_00000000_00000000__00000000_00000000_00000010_00000000, 0b00000000_0000000_00000000_00000000__00000000_00000000_00000100_00000000 );
			  inOrder.verifyNoMoreInteractions();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseSimpleSingleTxFilter()
		 public virtual void ShouldParseSimpleSingleTxFilter()
		 {
			  // given
			  LabelScanWriteMonitor.TxFilter txFilter = LabelScanWriteMonitor.ParseTxFilter( "123" );

			  // when/then
			  assertFalse( txFilter.Contains( 122 ) );
			  assertTrue( txFilter.Contains( 123 ) );
			  assertFalse( txFilter.Contains( 124 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseRangedSingleTxFilter()
		 public virtual void ShouldParseRangedSingleTxFilter()
		 {
			  // given
			  LabelScanWriteMonitor.TxFilter txFilter = LabelScanWriteMonitor.ParseTxFilter( "123-126" );

			  // when/then
			  assertFalse( txFilter.Contains( 122 ) );
			  assertTrue( txFilter.Contains( 123 ) );
			  assertTrue( txFilter.Contains( 124 ) );
			  assertTrue( txFilter.Contains( 125 ) );
			  assertTrue( txFilter.Contains( 126 ) );
			  assertFalse( txFilter.Contains( 127 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseSimpleMultipleTxFilters()
		 public virtual void ShouldParseSimpleMultipleTxFilters()
		 {
			  // given
			  LabelScanWriteMonitor.TxFilter txFilter = LabelScanWriteMonitor.ParseTxFilter( "123,146,123456" );

			  // when/then
			  assertFalse( txFilter.Contains( 122 ) );
			  assertTrue( txFilter.Contains( 123 ) );
			  assertTrue( txFilter.Contains( 146 ) );
			  assertTrue( txFilter.Contains( 123456 ) );
			  assertFalse( txFilter.Contains( 147 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseRangedMultipleTxFilters()
		 public virtual void ShouldParseRangedMultipleTxFilters()
		 {
			  // given
			  LabelScanWriteMonitor.TxFilter txFilter = LabelScanWriteMonitor.ParseTxFilter( "123-125,345-567" );

			  // when/then
			  assertFalse( txFilter.Contains( 122 ) );
			  assertTrue( txFilter.Contains( 123 ) );
			  assertTrue( txFilter.Contains( 124 ) );
			  assertTrue( txFilter.Contains( 125 ) );
			  assertFalse( txFilter.Contains( 201 ) );
			  assertTrue( txFilter.Contains( 345 ) );
			  assertTrue( txFilter.Contains( 405 ) );
			  assertTrue( txFilter.Contains( 567 ) );
			  assertFalse( txFilter.Contains( 568 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRotateAtConfiguredThreshold()
		 public virtual void ShouldRotateAtConfiguredThreshold()
		 {
			  // given
			  File storeDir = this.Directory.databaseDir();
			  int rotationThreshold = 1_000;
			  LabelScanWriteMonitor writeMonitor = new LabelScanWriteMonitor( Fs, Directory.databaseLayout(), rotationThreshold, ByteUnit.Byte, 1, TimeUnit.DAYS );

			  // when
			  for ( int i = 0; storeDir.listFiles().length < 5; i++ )
			  {
					writeMonitor.Range( i, 1 );
					writeMonitor.PrepareAdd( i, 5 );
					writeMonitor.MergeAdd( new LabelScanValue(), (new LabelScanValue()).Set(5) );
					writeMonitor.WriteSessionEnded();
			  }

			  // then
			  writeMonitor.Close();
			  foreach ( File file in storeDir.listFiles( ( dir, name ) => !name.Equals( _baseName ) ) )
			  {
					long sizeDiff = abs( rotationThreshold - Fs.getFileSize( file ) );
					assertTrue( sizeDiff < rotationThreshold / 10D );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPruneAtConfiguredThreshold()
		 public virtual void ShouldPruneAtConfiguredThreshold()
		 {
			  // given
			  File storeDir = this.Directory.databaseDir();
			  int pruneThreshold = 200;
			  LabelScanWriteMonitor writeMonitor = new LabelScanWriteMonitor( Fs, Directory.databaseLayout(), 1_000, ByteUnit.Byte, pruneThreshold, TimeUnit.MILLISECONDS );

			  // when
			  long startTime = currentTimeMillis();
			  long endTime = startTime + TimeUnit.SECONDS.toMillis( 1 );
			  for ( int i = 0; currentTimeMillis() < endTime; i++ )
			  {
					writeMonitor.Range( i, 1 );
					writeMonitor.PrepareAdd( i, 5 );
					writeMonitor.MergeAdd( new LabelScanValue(), (new LabelScanValue()).Set(5) );
					writeMonitor.WriteSessionEnded();
			  }

			  // then
			  writeMonitor.Close();
			  foreach ( File file in storeDir.listFiles( ( dir, name ) => !name.Equals( _baseName ) ) )
			  {
					long timestamp = LabelScanWriteMonitor.MillisOf( file );
					long diff = endTime - timestamp;
					assertTrue( diff < pruneThreshold * 2 );
			  }
		 }
	}

}