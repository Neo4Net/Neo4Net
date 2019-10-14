using System;

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
namespace Neo4Net.Kernel.impl.transaction.log
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using LogFile = Neo4Net.Kernel.impl.transaction.log.files.LogFile;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using LifeRule = Neo4Net.Kernel.Lifecycle.LifeRule;
	using ReadPastEndException = Neo4Net.Storageengine.Api.ReadPastEndException;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using Neo4Net.Test.rule.concurrent;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	/// <summary>
	/// Tests an issue where writer would append data and sometimes rotate the log to new file. When rotating the log
	/// there's an intricate relationship between <seealso cref="LogVersionRepository"/>, creating the file and writing
	/// the header. Concurrent readers which scans the log stream will use <seealso cref="LogVersionBridge"/> to seemlessly
	/// jump over to new files, where the highest file is dictated by <seealso cref="LogVersionRepository.getCurrentLogVersion()"/>.
	/// There was this race where the log version was incremented, the new log file created and reader would get
	/// to this new file and try to read the header and fail before the header had been written.
	/// 
	/// This test tries to reproduce this race. It will not produce false negatives, but sometimes false positives
	/// since it's non-deterministic.
	/// </summary>
	public class TransactionLogFileRotateAndReadRaceIT
	{
		private bool InstanceFieldsInitialized = false;

		public TransactionLogFileRotateAndReadRaceIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Rules = RuleChain.outerRule( _directory ).around( _life ).around( _t2 ).around( _fileSystemRule );
		}

		 private readonly TestDirectory _directory = TestDirectory.testDirectory();
		 private readonly LifeRule _life = new LifeRule( true );
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		 private readonly OtherThreadRule<Void> _t2 = new OtherThreadRule<Void>( this.GetType().FullName + "-T2" );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain rules = org.junit.rules.RuleChain.outerRule(directory).around(life).around(t2).around(fileSystemRule);
		 public RuleChain Rules;

		 // If any of these limits are reached the test ends, that or if there's a failure of course
		 private static readonly long _limitTime = SECONDS.toMillis( 5 );
		 private const int LIMIT_ROTATIONS = 500;
		 private const int LIMIT_READS = 1_000;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSeeEmptyLogFileWhenReadingTransactionStream() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSeeEmptyLogFileWhenReadingTransactionStream()
		 {
			  // GIVEN
			  LogVersionRepository logVersionRepository = new SimpleLogVersionRepository();
			  LogFiles logFiles = LogFilesBuilder.builder( _directory.databaseLayout(), _fileSystemRule.get() ).withLogVersionRepository(logVersionRepository).withTransactionIdStore(new SimpleTransactionIdStore()).build();
			  _life.add( logFiles );
			  LogFile logFile = logFiles.LogFile;
			  FlushablePositionAwareChannel writer = logFile.Writer;
			  LogPositionMarker startPosition = new LogPositionMarker();
			  writer.GetCurrentPosition( startPosition );

			  // WHEN
			  AtomicBoolean end = new AtomicBoolean();
			  sbyte[] dataChunk = new sbyte[100];
			  // one thread constantly writing to and rotating the channel
			  AtomicInteger rotations = new AtomicInteger();
			  System.Threading.CountdownEvent startSignal = new System.Threading.CountdownEvent( 1 );
			  Future<Void> writeFuture = _t2.execute(ignored =>
			  {
				ThreadLocalRandom random = ThreadLocalRandom.current();
				startSignal.Signal();
				while ( !end.get() )
				{
					 writer.Put( dataChunk, random.Next( 1, dataChunk.Length ) );
					 if ( logFile.RotationNeeded() )
					 {
						  logFile.Rotate();
						  // Let's just close the gap to the reader so that it gets closer to the "hot zone"
						  // where the rotation happens.
						  writer.GetCurrentPosition( startPosition );
						  rotations.incrementAndGet();
					 }
				}
				return null;
			  });
			  assertTrue( startSignal.await( 10, SECONDS ) );
			  // one thread reading through the channel
			  long maxEndTime = currentTimeMillis() + _limitTime;
			  int reads = 0;
			  try
			  {
					for ( ; currentTimeMillis() < maxEndTime && reads < LIMIT_READS && rotations.get() < LIMIT_ROTATIONS; reads++ )
					{
						 using ( ReadableLogChannel reader = logFile.GetReader( startPosition.NewPosition() ) )
						 {
							  Deplete( reader );
						 }
					}
			  }
			  finally
			  {
					end.set( true );
					writeFuture.get();
			  }

			  // THEN simply getting here means this was successful
		 }

		 private void Deplete( ReadableLogChannel reader )
		 {
			  sbyte[] dataChunk = new sbyte[100];
			  try
			  {
					while ( true )
					{
						 reader.Get( dataChunk, dataChunk.Length );
					}
			  }
			  catch ( ReadPastEndException )
			  {
					// This is OK, it means we've reached the end
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }
	}

}