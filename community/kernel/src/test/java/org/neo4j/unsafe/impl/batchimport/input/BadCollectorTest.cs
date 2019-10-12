using System;
using System.Threading;

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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.input
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using Org.Neo4j.Test;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.NullOutputStream.NULL_OUTPUT_STREAM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.OtherThreadExecutor.command;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.BadCollector.COLLECT_ALL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.BadCollector.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.BadCollector.UNLIMITED_TOLERANCE;

	public class BadCollectorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.EphemeralFileSystemRule fs = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule Fs = new EphemeralFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCollectBadRelationshipsEvenIfThresholdNeverReached() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCollectBadRelationshipsEvenIfThresholdNeverReached()
		 {
			  // given
			  int tolerance = 5;

			  using ( BadCollector badCollector = new BadCollector( BadOutputFile(), tolerance, BadCollector.COLLECT_ALL ) )
			  {
					// when
					badCollector.CollectBadRelationship( "1", "a", "T", "2", "b", "1" );

					// then
					assertEquals( 1, badCollector.BadEntries() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowExceptionIfDuplicateNodeTipsUsOverTheToleranceEdge() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowExceptionIfDuplicateNodeTipsUsOverTheToleranceEdge()
		 {
			  // given
			  int tolerance = 1;

			  using ( BadCollector badCollector = new BadCollector( BadOutputFile(), tolerance, BadCollector.COLLECT_ALL ) )
			  {
					// when
					CollectBadRelationship( badCollector );
					try
					{
						 badCollector.CollectDuplicateNode( 1, 1, "group" );
						 fail( "Should have thrown an InputException" );
					}
					catch ( InputException )
					{
						 // then expect to end up here
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowExceptionIfBadRelationshipsTipsUsOverTheToleranceEdge() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowExceptionIfBadRelationshipsTipsUsOverTheToleranceEdge()
		 {
			  // given
			  int tolerance = 1;

			  using ( BadCollector badCollector = new BadCollector( BadOutputFile(), tolerance, BadCollector.COLLECT_ALL ) )
			  {
					// when
					badCollector.CollectDuplicateNode( 1, 1, "group" );
					try
					{
						 CollectBadRelationship( badCollector );
						 fail( "Should have thrown an InputException" );
					}
					catch ( InputException )
					{
						 // then expect to end up here
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCollectBadRelationshipsIfWeShouldOnlyBeCollectingNodes() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCollectBadRelationshipsIfWeShouldOnlyBeCollectingNodes()
		 {
			  // given
			  int tolerance = 1;

			  using ( BadCollector badCollector = new BadCollector( BadOutputFile(), tolerance, BadCollector.DUPLICATE_NODES ) )
			  {
					// when
					badCollector.CollectDuplicateNode( 1, 1, "group" );
					try
					{
						 CollectBadRelationship( badCollector );
					}
					catch ( InputException )
					{
						 // then expect to end up here
						 assertEquals( 1, badCollector.BadEntries() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCollectBadNodesIfWeShouldOnlyBeCollectingRelationships() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCollectBadNodesIfWeShouldOnlyBeCollectingRelationships()
		 {
			  // given
			  int tolerance = 1;

			  using ( BadCollector badCollector = new BadCollector( BadOutputFile(), tolerance, BadCollector.BAD_RELATIONSHIPS ) )
			  {
					// when
					CollectBadRelationship( badCollector );
					try
					{
						 badCollector.CollectDuplicateNode( 1, 1, "group" );
					}
					catch ( InputException )
					{
						 // then expect to end up here
						 assertEquals( 1, badCollector.BadEntries() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCollectUnlimitedNumberOfBadEntriesIfToldTo()
		 public virtual void ShouldCollectUnlimitedNumberOfBadEntriesIfToldTo()
		 {
			  // GIVEN
			  using ( BadCollector collector = new BadCollector( NULL_OUTPUT_STREAM, UNLIMITED_TOLERANCE, COLLECT_ALL ) )
			  {
					// WHEN
					int count = 10_000;
					for ( int i = 0; i < count; i++ )
					{
						 collector.CollectDuplicateNode( i, i, "group" );
					}

					// THEN
					assertEquals( count, collector.BadEntries() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void skipBadEntriesLogging()
		 public virtual void SkipBadEntriesLogging()
		 {
			  MemoryStream outputStream = new MemoryStream();
			  using ( BadCollector badCollector = new BadCollector( outputStream, 100, COLLECT_ALL, 10, true, NO_MONITOR ) )
			  {
					CollectBadRelationship( badCollector );
					for ( int i = 0; i < 2; i++ )
					{
						 badCollector.CollectDuplicateNode( i, i, "group" );
					}
					CollectBadRelationship( badCollector );
					badCollector.CollectExtraColumns( "a,b,c", 1, "a" );
					assertEquals( "Output stream should not have any reported entries", 0, outputStream.size() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyBackPressure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyBackPressure()
		 {
			  // given
			  int backPressureThreshold = 10;
			  BlockableMonitor monitor = new BlockableMonitor();
			  using ( OtherThreadExecutor<Void> t2 = new OtherThreadExecutor<Void>( "T2", null ), BadCollector badCollector = new BadCollector( NULL_OUTPUT_STREAM, UNLIMITED_TOLERANCE, COLLECT_ALL, backPressureThreshold, false, monitor ) )
			  {
					for ( int i = 0; i < backPressureThreshold; i++ )
					{
						 badCollector.CollectDuplicateNode( i, i, "group" );
					}

					// when
					Future<object> enqueue = t2.ExecuteDontWait( command( () => badCollector.collectDuplicateNode(999, 999, "group") ) );
					t2.WaitUntilWaiting( waitDetails => waitDetails.isAt( typeof( BadCollector ), "collect" ) );
					monitor.Unblock();

					// then
					enqueue.get();
			  }
		 }

		 private void CollectBadRelationship( Collector collector )
		 {
			  collector.CollectBadRelationship( "A", Group_Fields.Global.name(), "TYPE", "B", Group_Fields.Global.name(), "A" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.OutputStream badOutputFile() throws java.io.IOException
		 private Stream BadOutputFile()
		 {
			  File badDataPath = ( new File( "/tmp/foo2" ) ).AbsoluteFile;
			  FileSystemAbstraction fileSystem = Fs.get();
			  File badDataFile = badDataFile( fileSystem, badDataPath );
			  return fileSystem.OpenAsOutputStream( badDataFile, true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File badDataFile(org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.io.File badDataPath) throws java.io.IOException
		 private File BadDataFile( FileSystemAbstraction fileSystem, File badDataPath )
		 {
			  fileSystem.Mkdir( badDataPath.ParentFile );
			  fileSystem.Create( badDataPath );
			  return badDataPath;
		 }

		 private class BlockableMonitor : BadCollector.Monitor
		 {
			  internal readonly System.Threading.CountdownEvent Latch = new System.Threading.CountdownEvent( 1 );

			  public override void BeforeProcessEvent()
			  {
					try
					{
						 Latch.await();
					}
					catch ( InterruptedException e )
					{
						 Thread.CurrentThread.Interrupt();
						 throw new Exception( e );
					}
			  }

			  internal virtual void Unblock()
			  {
					Latch.Signal();
			  }
		 }
	}

}