using System;
using System.Collections.Generic;
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
namespace Org.Neo4j.Logging
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using Adversary = Org.Neo4j.Adversaries.Adversary;
	using RandomAdversary = Org.Neo4j.Adversaries.RandomAdversary;
	using AdversarialFileSystemAbstraction = Org.Neo4j.Adversaries.fs.AdversarialFileSystemAbstraction;
	using AdversarialOutputStream = Org.Neo4j.Adversaries.fs.AdversarialOutputStream;
	using Suppliers = Org.Neo4j.Function.Suppliers;
	using DelegatingFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.DelegatingFileSystemAbstraction;
	using EphemeralFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using RotationListener = Org.Neo4j.Logging.RotatingFileOutputStreamSupplier.RotationListener;
	using EphemeralFileSystemExtension = Org.Neo4j.Test.extension.EphemeralFileSystemExtension;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using SuppressOutputExtension = Org.Neo4j.Test.extension.SuppressOutputExtension;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.sameInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTimeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.FormattedLog.OUTPUT_STREAM_CONVERTER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.RotatingFileOutputStreamSupplier.getAllArchives;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({SuppressOutputExtension.class, EphemeralFileSystemExtension.class, TestDirectoryExtension.class}) class RotatingFileOutputStreamSupplierTest
	internal class RotatingFileOutputStreamSupplierTest
	{
		 private const long TEST_TIMEOUT_MILLIS = 10_000;
		 private static readonly Executor _directExecutor = ThreadStart.run;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.graphdb.mockfs.EphemeralFileSystemAbstraction fileSystem;
		 private EphemeralFileSystemAbstraction _fileSystem;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.SuppressOutput suppressOutput;
		 private SuppressOutput _suppressOutput;

		 private File _logFile;
		 private File _archiveLogFile1;
		 private File _archiveLogFile2;
		 private File _archiveLogFile3;
		 private File _archiveLogFile4;
		 private File _archiveLogFile5;
		 private File _archiveLogFile6;
		 private File _archiveLogFile7;
		 private File _archiveLogFile8;
		 private File _archiveLogFile9;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setup()
		 internal virtual void Setup()
		 {
			  File logDir = _testDirectory.directory();
			  _logFile = new File( logDir, "logfile.log" );
			  _archiveLogFile1 = new File( logDir, "logfile.log.1" );
			  _archiveLogFile2 = new File( logDir, "logfile.log.2" );
			  _archiveLogFile3 = new File( logDir, "logfile.log.3" );
			  _archiveLogFile4 = new File( logDir, "logfile.log.4" );
			  _archiveLogFile5 = new File( logDir, "logfile.log.5" );
			  _archiveLogFile6 = new File( logDir, "logfile.log.6" );
			  _archiveLogFile7 = new File( logDir, "logfile.log.7" );
			  _archiveLogFile8 = new File( logDir, "logfile.log.8" );
			  _archiveLogFile9 = new File( logDir, "logfile.log.9" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void createsLogOnConstruction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CreatesLogOnConstruction()
		 {
			  new RotatingFileOutputStreamSupplier( _fileSystem, _logFile, 250000, 0, 10, _directExecutor );
			  assertThat( _fileSystem.fileExists( _logFile ), @is( true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void rotatesLogWhenSizeExceeded() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void RotatesLogWhenSizeExceeded()
		 {
			  RotatingFileOutputStreamSupplier supplier = new RotatingFileOutputStreamSupplier( _fileSystem, _logFile, 10, 0, 10, _directExecutor );

			  Write( supplier, "A string longer than 10 bytes" );

			  assertThat( _fileSystem.fileExists( _logFile ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile1 ), @is( false ) );

			  Write( supplier, "A string longer than 10 bytes" );

			  assertThat( _fileSystem.fileExists( _logFile ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile1 ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile2 ), @is( false ) );

			  Write( supplier, "Short" );
			  Write( supplier, "A string longer than 10 bytes" );

			  assertThat( _fileSystem.fileExists( _logFile ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile1 ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile2 ), @is( true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void limitsNumberOfArchivedLogs() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void LimitsNumberOfArchivedLogs()
		 {
			  RotatingFileOutputStreamSupplier supplier = new RotatingFileOutputStreamSupplier( _fileSystem, _logFile, 10, 0, 2, _directExecutor );

			  Write( supplier, "A string longer than 10 bytes" );

			  assertThat( _fileSystem.fileExists( _logFile ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile1 ), @is( false ) );

			  Write( supplier, "A string longer than 10 bytes" );

			  assertThat( _fileSystem.fileExists( _logFile ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile1 ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile2 ), @is( false ) );

			  Write( supplier, "A string longer than 10 bytes" );

			  assertThat( _fileSystem.fileExists( _logFile ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile1 ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile2 ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile3 ), @is( false ) );

			  Write( supplier, "A string longer than 10 bytes" );

			  assertThat( _fileSystem.fileExists( _logFile ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile1 ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile2 ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile3 ), @is( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void rotationShouldNotDeadlockOnListener()
		 internal virtual void RotationShouldNotDeadlockOnListener()
		 {
			  assertTimeout(ofMillis(TEST_TIMEOUT_MILLIS), () =>
			  {
				string logContent = "Output file created";
				AtomicReference<Exception> listenerException = new AtomicReference<Exception>( null );
				System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );
				RotationListener listener = new RotationListenerAnonymousInnerClass( this, listenerException, latch );
				ExecutorService executor = Executors.newSingleThreadExecutor();
				DefaultFileSystemAbstraction defaultFileSystemAbstraction = new DefaultFileSystemAbstraction();
				RotatingFileOutputStreamSupplier supplier = new RotatingFileOutputStreamSupplier( defaultFileSystemAbstraction, _logFile, 0, 0, 10, executor, listener );

				Stream outputStream = supplier.Get();
				LockingPrintWriter lockingPrintWriter = new LockingPrintWriter( this, outputStream );
				lockingPrintWriter.WithLock(() =>
				{
					 supplier.Rotate();
					 latch.await();
					 return Void.TYPE;
				});

				ShutDownExecutor( executor );

				IList<string> strings = Files.readAllLines( _logFile.toPath() );
				string actual = string.join( "", strings );
				assertEquals( logContent, actual );
				assertNull( listenerException.get() );
			  });
		 }

		 private class RotationListenerAnonymousInnerClass : RotationListener
		 {
			 private readonly RotatingFileOutputStreamSupplierTest _outerInstance;

			 private AtomicReference<Exception> _listenerException;
			 private System.Threading.CountdownEvent _latch;

			 public RotationListenerAnonymousInnerClass( RotatingFileOutputStreamSupplierTest outerInstance, AtomicReference<Exception> listenerException, System.Threading.CountdownEvent latch )
			 {
				 this.outerInstance = outerInstance;
				 this._listenerException = listenerException;
				 this._latch = latch;
			 }

			 public override void outputFileCreated( Stream @out )
			 {
				  try
				  {
						Thread thread = new Thread(() =>
						{
									 try
									 {
										  @out.WriteByte( logContent.Bytes );
										  @out.Flush();
									 }
									 catch ( IOException e )
									 {
										  _listenerException.set( e );
									 }
						});
						thread.Start();
						thread.Join();
				  }
				  catch ( Exception e )
				  {
						_listenerException.set( e );
				  }
				  base.outputFileCreated( @out );
			 }

			 public override void rotationCompleted( Stream @out )
			 {
				  _latch.countDown();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shutDownExecutor(java.util.concurrent.ExecutorService executor) throws InterruptedException
		 private void ShutDownExecutor( ExecutorService executor )
		 {
			  executor.shutdown();
			  bool terminated = executor.awaitTermination( TEST_TIMEOUT_MILLIS, TimeUnit.MILLISECONDS );
			  if ( !terminated )
			  {
					throw new System.InvalidOperationException( "Rotation execution failed to complete within reasonable time." );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotRotateLogWhenSizeExceededButNotDelay() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNotRotateLogWhenSizeExceededButNotDelay()
		 {
			  UpdatableLongSupplier clock = new UpdatableLongSupplier( DateTimeHelper.CurrentUnixTimeMillis() );
			  RotatingFileOutputStreamSupplier supplier = new RotatingFileOutputStreamSupplier( clock, _fileSystem, _logFile, 10, SECONDS.toMillis( 60 ), 10, _directExecutor, new RotationListener() );

			  Write( supplier, "A string longer than 10 bytes" );

			  assertThat( _fileSystem.fileExists( _logFile ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile1 ), @is( false ) );

			  Write( supplier, "A string longer than 10 bytes" );

			  assertThat( _fileSystem.fileExists( _logFile ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile1 ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile2 ), @is( false ) );

			  Write( supplier, "A string longer than 10 bytes" );

			  clock.Value = clock.AsLong + SECONDS.toMillis( 59 );
			  Write( supplier, "A string longer than 10 bytes" );

			  clock.Value = clock.AsLong + SECONDS.toMillis( 1 );
			  Write( supplier, "A string longer than 10 bytes" );

			  assertThat( _fileSystem.fileExists( _logFile ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile1 ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile2 ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile3 ), @is( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFindAllArchives() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldFindAllArchives()
		 {
			  RotatingFileOutputStreamSupplier supplier = new RotatingFileOutputStreamSupplier( _fileSystem, _logFile, 10, 0, 2, _directExecutor );

			  Write( supplier, "A string longer than 10 bytes" );
			  Write( supplier, "A string longer than 10 bytes" );

			  assertThat( _fileSystem.fileExists( _logFile ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile1 ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile2 ), @is( false ) );

			  IList<File> allArchives = getAllArchives( _fileSystem, _logFile );
			  assertThat( allArchives.Count, @is( 1 ) );
			  assertThat( allArchives, hasItem( _archiveLogFile1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotifyListenerWhenNewLogIsCreated() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNotifyListenerWhenNewLogIsCreated()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch allowRotationComplete = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent allowRotationComplete = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch rotationComplete = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent rotationComplete = new System.Threading.CountdownEvent( 1 );
			  string outputFileCreatedMessage = "Output file created";
			  string rotationCompleteMessage = "Rotation complete";

			  RotationListener rotationListener = spy( new RotationListenerAnonymousInnerClass( this, allowRotationComplete, rotationComplete, outputFileCreatedMessage, rotationCompleteMessage ) );

			  ExecutorService rotationExecutor = Executors.newSingleThreadExecutor();
			  try
			  {
					RotatingFileOutputStreamSupplier supplier = new RotatingFileOutputStreamSupplier( _fileSystem, _logFile, 10, 0, 10, rotationExecutor, rotationListener );

					Write( supplier, "A string longer than 10 bytes" );
					Write( supplier, "A string longer than 10 bytes" );

					allowRotationComplete.Signal();
					rotationComplete.await( 1L, TimeUnit.SECONDS );

					verify( rotationListener ).outputFileCreated( any( typeof( Stream ) ) );
					verify( rotationListener ).rotationCompleted( any( typeof( Stream ) ) );
			  }
			  finally
			  {
					ShutDownExecutor( rotationExecutor );
			  }
		 }

		 private class RotationListenerAnonymousInnerClass : RotationListener
		 {
			 private readonly RotatingFileOutputStreamSupplierTest _outerInstance;

			 private System.Threading.CountdownEvent _allowRotationComplete;
			 private System.Threading.CountdownEvent _rotationComplete;
			 private string _outputFileCreatedMessage;
			 private string _rotationCompleteMessage;

			 public RotationListenerAnonymousInnerClass( RotatingFileOutputStreamSupplierTest outerInstance, System.Threading.CountdownEvent allowRotationComplete, System.Threading.CountdownEvent rotationComplete, string outputFileCreatedMessage, string rotationCompleteMessage )
			 {
				 this.outerInstance = outerInstance;
				 this._allowRotationComplete = allowRotationComplete;
				 this._rotationComplete = rotationComplete;
				 this._outputFileCreatedMessage = outputFileCreatedMessage;
				 this._rotationCompleteMessage = rotationCompleteMessage;
			 }

			 public override void outputFileCreated( Stream @out )
			 {
				  try
				  {
						_allowRotationComplete.await( 1L, TimeUnit.SECONDS );
						@out.WriteByte( _outputFileCreatedMessage.GetBytes() );
				  }
				  catch ( Exception e ) when ( e is InterruptedException || e is IOException )
				  {
						throw new Exception( e );
				  }
			 }

			 public override void rotationCompleted( Stream @out )
			 {
				  _rotationComplete.Signal();
				  try
				  {
						@out.WriteByte( _rotationCompleteMessage.GetBytes() );
				  }
				  catch ( IOException e )
				  {
						throw new Exception( e );
				  }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotifyListenerOnRotationErrorDuringJobExecution() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNotifyListenerOnRotationErrorDuringJobExecution()
		 {
			  RotationListener rotationListener = mock( typeof( RotationListener ) );
			  Executor executor = mock( typeof( Executor ) );
			  RotatingFileOutputStreamSupplier supplier = new RotatingFileOutputStreamSupplier( _fileSystem, _logFile, 10, 0, 10, executor, rotationListener );
			  Stream outputStream = supplier.Get();

			  RejectedExecutionException exception = new RejectedExecutionException( "text exception" );
			  doThrow( exception ).when( executor ).execute( any( typeof( ThreadStart ) ) );

			  Write( supplier, "A string longer than 10 bytes" );
			  assertThat( supplier.Get(), @is(outputStream) );

			  verify( rotationListener ).rotationError( exception, outputStream );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReattemptRotationAfterExceptionDuringJobExecution() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReattemptRotationAfterExceptionDuringJobExecution()
		 {
			  RotationListener rotationListener = mock( typeof( RotationListener ) );
			  Executor executor = mock( typeof( Executor ) );
			  RotatingFileOutputStreamSupplier supplier = new RotatingFileOutputStreamSupplier( _fileSystem, _logFile, 10, 0, 10, executor, rotationListener );
			  Stream outputStream = supplier.Get();

			  RejectedExecutionException exception = new RejectedExecutionException( "text exception" );
			  doThrow( exception ).when( executor ).execute( any( typeof( ThreadStart ) ) );

			  Write( supplier, "A string longer than 10 bytes" );
			  assertThat( supplier.Get(), @is(outputStream) );
			  assertThat( supplier.Get(), @is(outputStream) );

			  verify( rotationListener, times( 2 ) ).rotationError( exception, outputStream );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotifyListenerOnRotationErrorDuringRotationIO() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNotifyListenerOnRotationErrorDuringRotationIO()
		 {
			  RotationListener rotationListener = mock( typeof( RotationListener ) );
			  FileSystemAbstraction fs = spy( _fileSystem );
			  RotatingFileOutputStreamSupplier supplier = new RotatingFileOutputStreamSupplier( fs, _logFile, 10, 0, 10, _directExecutor, rotationListener );
			  Stream outputStream = supplier.Get();

			  IOException exception = new IOException( "text exception" );
			  doThrow( exception ).when( fs ).renameFile( any( typeof( File ) ), any( typeof( File ) ) );

			  Write( supplier, "A string longer than 10 bytes" );
			  assertThat( supplier.Get(), @is(outputStream) );

			  verify( rotationListener ).rotationError( eq( exception ), any( typeof( Stream ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotUpdateOutputStreamWhenClosedDuringRotation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNotUpdateOutputStreamWhenClosedDuringRotation()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch allowRotationComplete = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent allowRotationComplete = new System.Threading.CountdownEvent( 1 );

			  RotationListener rotationListener = spy( new RotationListenerAnonymousInnerClass2( this, allowRotationComplete ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<java.io.OutputStream> mockStreams = new java.util.ArrayList<>();
			  IList<Stream> mockStreams = new List<Stream>();
			  FileSystemAbstraction fs = new DelegatingFileSystemAbstractionAnonymousInnerClass( this, _fileSystem, mockStreams );

			  ExecutorService rotationExecutor = Executors.newSingleThreadExecutor();
			  try
			  {
					RotatingFileOutputStreamSupplier supplier = new RotatingFileOutputStreamSupplier( fs, _logFile, 10, 0, 10, rotationExecutor, rotationListener );
					Stream outputStream = supplier.Get();

					Write( supplier, "A string longer than 10 bytes" );
					assertThat( supplier.Get(), @is(outputStream) );

					allowRotationComplete.Signal();
					supplier.Dispose();
			  }
			  finally
			  {
					ShutDownExecutor( rotationExecutor );
			  }

			  AssertStreamClosed( mockStreams[0] );
		 }

		 private class RotationListenerAnonymousInnerClass2 : RotationListener
		 {
			 private readonly RotatingFileOutputStreamSupplierTest _outerInstance;

			 private System.Threading.CountdownEvent _allowRotationComplete;

			 public RotationListenerAnonymousInnerClass2( RotatingFileOutputStreamSupplierTest outerInstance, System.Threading.CountdownEvent allowRotationComplete )
			 {
				 this.outerInstance = outerInstance;
				 this._allowRotationComplete = allowRotationComplete;
			 }

			 public override void outputFileCreated( Stream @out )
			 {
				  try
				  {
						_allowRotationComplete.await();
				  }
				  catch ( InterruptedException e )
				  {
						throw new Exception( e );
				  }
			 }
		 }

		 private class DelegatingFileSystemAbstractionAnonymousInnerClass : DelegatingFileSystemAbstraction
		 {
			 private readonly RotatingFileOutputStreamSupplierTest _outerInstance;

			 private IList<Stream> _mockStreams;

			 public DelegatingFileSystemAbstractionAnonymousInnerClass( RotatingFileOutputStreamSupplierTest outerInstance, EphemeralFileSystemAbstraction fileSystem, IList<Stream> mockStreams ) : base( fileSystem )
			 {
				 this.outerInstance = outerInstance;
				 this._mockStreams = mockStreams;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.OutputStream openAsOutputStream(java.io.File fileName, boolean append) throws java.io.IOException
			 public override Stream openAsOutputStream( File fileName, bool append )
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.OutputStream stream = spy(super.openAsOutputStream(fileName, append));
				  Stream stream = spy( base.openAsOutputStream( fileName, append ) );
				  _mockStreams.Add( stream );
				  return stream;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCloseAllOutputStreams() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCloseAllOutputStreams()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<java.io.OutputStream> mockStreams = new java.util.ArrayList<>();
			  IList<Stream> mockStreams = new List<Stream>();
			  FileSystemAbstraction fs = new DelegatingFileSystemAbstractionAnonymousInnerClass2( this, _fileSystem, mockStreams );

			  RotatingFileOutputStreamSupplier supplier = new RotatingFileOutputStreamSupplier( fs, _logFile, 10, 0, 10, _directExecutor );

			  Write( supplier, "A string longer than 10 bytes" );

			  supplier.Dispose();

			  AssertStreamClosed( mockStreams[0] );
		 }

		 private class DelegatingFileSystemAbstractionAnonymousInnerClass2 : DelegatingFileSystemAbstraction
		 {
			 private readonly RotatingFileOutputStreamSupplierTest _outerInstance;

			 private IList<Stream> _mockStreams;

			 public DelegatingFileSystemAbstractionAnonymousInnerClass2( RotatingFileOutputStreamSupplierTest outerInstance, EphemeralFileSystemAbstraction fileSystem, IList<Stream> mockStreams ) : base( fileSystem )
			 {
				 this.outerInstance = outerInstance;
				 this._mockStreams = mockStreams;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.OutputStream openAsOutputStream(java.io.File fileName, boolean append) throws java.io.IOException
			 public override Stream openAsOutputStream( File fileName, bool append )
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.OutputStream stream = spy(super.openAsOutputStream(fileName, append));
				  Stream stream = spy( base.openAsOutputStream( fileName, append ) );
				  _mockStreams.Add( stream );
				  return stream;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCloseAllStreamsDespiteError() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCloseAllStreamsDespiteError()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<java.io.OutputStream> mockStreams = new java.util.ArrayList<>();
			  IList<Stream> mockStreams = new List<Stream>();
			  FileSystemAbstraction fs = new DelegatingFileSystemAbstractionAnonymousInnerClass3( this, _fileSystem, mockStreams );

			  RotatingFileOutputStreamSupplier supplier = new RotatingFileOutputStreamSupplier( fs, _logFile, 10, 0, 10, _directExecutor );

			  Write( supplier, "A string longer than 10 bytes" );
			  Write( supplier, "A string longer than 10 bytes" );

			  IOException exception = new IOException( "test exception" );
			  Stream mockStream = mockStreams[1];
			  doThrow( exception ).when( mockStream ).close();

			  IOException ioException = assertThrows( typeof( IOException ), supplier.close );
			  assertThat( ioException, sameInstance( exception ) );
			  verify( mockStream ).close();
		 }

		 private class DelegatingFileSystemAbstractionAnonymousInnerClass3 : DelegatingFileSystemAbstraction
		 {
			 private readonly RotatingFileOutputStreamSupplierTest _outerInstance;

			 private IList<Stream> _mockStreams;

			 public DelegatingFileSystemAbstractionAnonymousInnerClass3( RotatingFileOutputStreamSupplierTest outerInstance, EphemeralFileSystemAbstraction fileSystem, IList<Stream> mockStreams ) : base( fileSystem )
			 {
				 this.outerInstance = outerInstance;
				 this._mockStreams = mockStreams;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.OutputStream openAsOutputStream(java.io.File fileName, boolean append) throws java.io.IOException
			 public override Stream openAsOutputStream( File fileName, bool append )
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.OutputStream stream = spy(super.openAsOutputStream(fileName, append));
				  Stream stream = spy( base.openAsOutputStream( fileName, append ) );
				  _mockStreams.Add( stream );
				  return stream;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSurviveFilesystemErrors() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldSurviveFilesystemErrors()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.adversaries.RandomAdversary adversary = new org.neo4j.adversaries.RandomAdversary(0.1, 0.1, 0);
			  RandomAdversary adversary = new RandomAdversary( 0.1, 0.1, 0 );
			  adversary.ProbabilityFactor = 0;

			  AdversarialFileSystemAbstraction adversarialFileSystem = new SensibleAdversarialFileSystemAbstraction( adversary, _fileSystem );
			  RotatingFileOutputStreamSupplier supplier = new RotatingFileOutputStreamSupplier( adversarialFileSystem, _logFile, 1000, 0, 9, _directExecutor );

			  adversary.ProbabilityFactor = 1;
			  WriteLines( supplier, 10000 );

			  // run cleanly for a while, to allow it to fill any gaps left in log archive numbers
			  adversary.ProbabilityFactor = 0;
			  WriteLines( supplier, 10000 );

			  assertThat( _fileSystem.fileExists( _logFile ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile1 ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile2 ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile3 ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile4 ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile5 ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile6 ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile7 ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile8 ), @is( true ) );
			  assertThat( _fileSystem.fileExists( _archiveLogFile9 ), @is( true ) );
		 }

		 private void Write( RotatingFileOutputStreamSupplier supplier, string line )
		 {
			  PrintWriter writer = new PrintWriter( supplier.Get() );
			  writer.println( line );
			  writer.flush();
		 }

		 private void WriteLines( System.Func<Stream> outputStreamSupplier, int count )
		 {
			  System.Func<PrintWriter> printWriterSupplier = Suppliers.adapted( outputStreamSupplier, OUTPUT_STREAM_CONVERTER );
			  for ( ; count >= 0; --count )
			  {
					printWriterSupplier().println("We are what we repeatedly do. Excellence, then, is not an act, but a habit.");
					Thread.yield();
			  }
		 }

		 private void AssertStreamClosed( Stream stream )
		 {
			  assertThrows( typeof( ClosedChannelException ), () => stream.WriteByte(0) );
		 }

		 private class LockingPrintWriter : PrintWriter
		 {
			 private readonly RotatingFileOutputStreamSupplierTest _outerInstance;

			  internal LockingPrintWriter( RotatingFileOutputStreamSupplierTest outerInstance, Stream @out ) : base( @out )
			  {
				  this._outerInstance = outerInstance;
					@lock = new object();

			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void withLock(java.util.concurrent.Callable callable) throws Exception
			  internal virtual void WithLock( Callable callable )
			  {
					lock ( @lock )
					{
						 callable.call();
					}
			  }
		 }

		 private class UpdatableLongSupplier : System.Func<long>
		 {
			  internal readonly AtomicLong LongValue;

			  internal UpdatableLongSupplier( long value )
			  {
					this.LongValue = new AtomicLong( value );
			  }

			  internal virtual long setValue( long value )
			  {
					return LongValue.getAndSet( value );
			  }

			  public override long AsLong
			  {
				  get
				  {
						return LongValue.get();
				  }
			  }
		 }

		 private class SensibleAdversarialFileSystemAbstraction : AdversarialFileSystemAbstraction
		 {
			  internal readonly Adversary Adversary;
			  internal readonly FileSystemAbstraction Delegate;

			  internal SensibleAdversarialFileSystemAbstraction( Adversary adversary, FileSystemAbstraction @delegate ) : base( adversary, @delegate )
			  {
					this.Adversary = adversary;
					this.Delegate = @delegate;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.OutputStream openAsOutputStream(java.io.File fileName, boolean append) throws java.io.IOException
			  public override Stream OpenAsOutputStream( File fileName, bool append )
			  {
					// Default adversarial might throw a java.lang.SecurityException here, which is an exception
					// that should not be survived
					Adversary.injectFailure( typeof( FileNotFoundException ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.OutputStream outputStream = delegate.openAsOutputStream(fileName, append);
					Stream outputStream = Delegate.openAsOutputStream( fileName, append );
					return new AdversarialOutputStreamAnonymousInnerClass( this, outputStream, Adversary );
			  }

			  private class AdversarialOutputStreamAnonymousInnerClass : AdversarialOutputStream
			  {
				  private readonly SensibleAdversarialFileSystemAbstraction _outerInstance;

				  private Stream _outputStream;

				  public AdversarialOutputStreamAnonymousInnerClass( SensibleAdversarialFileSystemAbstraction outerInstance, Stream outputStream, Adversary adversary ) : base( outputStream, adversary )
				  {
					  this.outerInstance = outerInstance;
					  this._outputStream = outputStream;
				  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(byte[] b) throws java.io.IOException
				  public override void write( sbyte[] b )
				  {
						// Default adversarial might throw a NullPointerException.class or IndexOutOfBoundsException here,
						// which are exceptions that should not be survived
						_outerInstance.adversary.injectFailure( typeof( IOException ) );
						_outputStream.Write( b, 0, b.Length );
				  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(byte[] b, int off, int len) throws java.io.IOException
				  public override void write( sbyte[] b, int off, int len )
				  {
						// Default adversarial might throw a NullPointerException.class or IndexOutOfBoundsException here,
						// which are exceptions that should not be survived
						_outerInstance.adversary.injectFailure( typeof( IOException ) );
						_outputStream.Write( b, off, len );
				  }
			  }

			  public override bool FileExists( File file )
			  {
					// Default adversarial might throw a java.lang.SecurityException here, which is an exception
					// that should not be survived
					return Delegate.fileExists( file );
			  }

			  public override long GetFileSize( File fileName )
			  {
					// Default adversarial might throw a java.lang.SecurityException here, which is an exception
					// that should not be survived
					return Delegate.getFileSize( fileName );
			  }
		 }
	}

}