using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Kernel.Api.Impl.Index
{
	using Document = org.apache.lucene.document.Document;
	using Field = org.apache.lucene.document.Field;
	using LongField = org.apache.lucene.document.LongField;
	using TextField = org.apache.lucene.document.TextField;
	using IndexFileNames = Org.Apache.Lucene.Index.IndexFileNames;
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using Directory = org.apache.lucene.store.Directory;
	using FSDirectory = org.apache.lucene.store.FSDirectory;
	using IOContext = org.apache.lucene.store.IOContext;
	using IndexInput = org.apache.lucene.store.IndexInput;
	using IndexOutput = org.apache.lucene.store.IndexOutput;
	using Lock = org.apache.lucene.store.Lock;
	using AfterAll = org.junit.jupiter.api.AfterAll;
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeAll = org.junit.jupiter.api.BeforeAll;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using RepeatedTest = org.junit.jupiter.api.RepeatedTest;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using AbstractIndexPartition = Neo4Net.Kernel.Api.Impl.Index.partition.AbstractIndexPartition;
	using IndexPartitionFactory = Neo4Net.Kernel.Api.Impl.Index.partition.IndexPartitionFactory;
	using WritableIndexPartitionFactory = Neo4Net.Kernel.Api.Impl.Index.partition.WritableIndexPartitionFactory;
	using DirectoryFactory = Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using PartitionedIndexStorage = Neo4Net.Kernel.Api.Impl.Index.storage.PartitionedIndexStorage;
	using AbstractIndexReader = Neo4Net.Kernel.Api.StorageEngine.schema.AbstractIndexReader;
	using DefaultFileSystemExtension = Neo4Net.Test.extension.DefaultFileSystemExtension;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTimeout;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({DefaultFileSystemExtension.class, TestDirectoryExtension.class}) class DatabaseIndexIntegrationTest
	internal class DatabaseIndexIntegrationTest
	{
		 private const int THREAD_NUMBER = 5;
		 private static ExecutorService _workers;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.io.fs.DefaultFileSystemAbstraction fileSystem;
		 private DefaultFileSystemAbstraction _fileSystem;

		 private readonly System.Threading.CountdownEvent _raceSignal = new System.Threading.CountdownEvent( 1 );
		 private SyncNotifierDirectoryFactory _directoryFactory;
		 private WritableTestDatabaseIndex _luceneIndex;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeAll static void initExecutors()
		 internal static void InitExecutors()
		 {
			  _workers = Executors.newFixedThreadPool( THREAD_NUMBER );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterAll static void shutDownExecutor()
		 internal static void ShutDownExecutor()
		 {
			  _workers.shutdownNow();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SetUp()
		 {
			  _directoryFactory = new SyncNotifierDirectoryFactory( _raceSignal );
			  _luceneIndex = CreateTestLuceneIndex( _directoryFactory, _testDirectory.directory() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void tearDown()
		 internal virtual void TearDown()
		 {
			  _directoryFactory.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RepeatedTest(2) void testSaveCallCommitAndCloseFromMultipleThreads()
		 internal virtual void TestSaveCallCommitAndCloseFromMultipleThreads()
		 {
			  assertTimeout(ofSeconds(60), () =>
			  {
				GenerateInitialData();
				Supplier<ThreadStart> closeTaskSupplier = () => CreateConcurrentCloseTask(_raceSignal);
				IList<Future<object>> closeFutures = SubmitTasks( closeTaskSupplier );

				foreach ( Future<object> closeFuture in closeFutures )
				{
					 closeFuture.get();
				}

				assertFalse( _luceneIndex.Open );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RepeatedTest(2) void saveCallCloseAndDropFromMultipleThreads()
		 internal virtual void SaveCallCloseAndDropFromMultipleThreads()
		 {
			  assertTimeout(ofSeconds(60), () =>
			  {
				GenerateInitialData();
				Supplier<ThreadStart> dropTaskSupplier = () => CreateConcurrentDropTask(_raceSignal);
				IList<Future<object>> futures = SubmitTasks( dropTaskSupplier );

				foreach ( Future<object> future in futures )
				{
					 future.get();
				}

				assertFalse( _luceneIndex.Open );
			  });
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private WritableTestDatabaseIndex createTestLuceneIndex(org.Neo4Net.kernel.api.impl.index.storage.DirectoryFactory dirFactory, java.io.File folder) throws java.io.IOException
		 private WritableTestDatabaseIndex CreateTestLuceneIndex( DirectoryFactory dirFactory, File folder )
		 {
			  PartitionedIndexStorage indexStorage = new PartitionedIndexStorage( dirFactory, _fileSystem, folder );
			  WritableTestDatabaseIndex index = new WritableTestDatabaseIndex( indexStorage );
			  index.Create();
			  index.open();
			  return index;
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.List<java.util.concurrent.Future<?>> submitTasks(System.Func<Runnable> taskSupplier)
		 private IList<Future<object>> SubmitTasks( System.Func<ThreadStart> taskSupplier )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<java.util.concurrent.Future<?>> futures = new java.util.ArrayList<>(THREAD_NUMBER);
			  IList<Future<object>> futures = new List<Future<object>>( THREAD_NUMBER );
			  futures.Add( _workers.submit( CreateMainCloseTask() ) );
			  for ( int i = 0; i < THREAD_NUMBER - 1; i++ )
			  {
					futures.Add( _workers.submit( taskSupplier() ) );
			  }
			  return futures;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void generateInitialData() throws java.io.IOException
		 private void GenerateInitialData()
		 {
			  IndexWriter indexWriter = FirstPartitionWriter();
			  for ( int i = 0; i < 10; i++ )
			  {
					indexWriter.addDocument( CreateTestDocument() );
			  }
		 }

		 private ThreadStart CreateConcurrentDropTask( System.Threading.CountdownEvent dropRaceSignal )
		 {
			  return () =>
			  {
				try
				{
					 dropRaceSignal.await();
					 Thread.yield();
					 _luceneIndex.drop();
				}
				catch ( Exception e )
				{
					 throw new Exception( e );
				}
			  };
		 }

		 private ThreadStart CreateConcurrentCloseTask( System.Threading.CountdownEvent closeRaceSignal )
		 {
			  return () =>
			  {
				try
				{
					 closeRaceSignal.await();
					 Thread.yield();
					 _luceneIndex.close();
				}
				catch ( Exception e )
				{
					 throw new Exception( e );
				}
			  };
		 }

		 private ThreadStart CreateMainCloseTask()
		 {
			  return () =>
			  {
				try
				{
					 _luceneIndex.close();
				}
				catch ( Exception e )
				{
					 throw new Exception( e );
				}
			  };
		 }

		 private static Document CreateTestDocument()
		 {
			  Document document = new Document();
			  document.add( new TextField( "text", "textValue", Field.Store.YES ) );
			  document.add( new LongField( "long", 1, Field.Store.YES ) );
			  return document;
		 }

		 private IndexWriter FirstPartitionWriter()
		 {
			  IList<AbstractIndexPartition> partitions = _luceneIndex.Partitions;
			  assertEquals( 1, partitions.Count );
			  AbstractIndexPartition partition = partitions[0];
			  return partition.IndexWriter;
		 }

		 private class WritableTestDatabaseIndex : WritableAbstractDatabaseIndex<TestLuceneIndex, AbstractIndexReader>
		 {
			  internal WritableTestDatabaseIndex( PartitionedIndexStorage indexStorage ) : base( new TestLuceneIndex( indexStorage, new WritableIndexPartitionFactory( IndexWriterConfigs.standard ) ) )
			  {
			  }
		 }

		 private class TestLuceneIndex : AbstractLuceneIndex<AbstractIndexReader>
		 {

			  internal TestLuceneIndex( PartitionedIndexStorage indexStorage, IndexPartitionFactory partitionFactory ) : base( indexStorage, partitionFactory, null )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.Neo4Net.Kernel.Api.StorageEngine.schema.AbstractIndexReader createSimpleReader(java.util.List<org.Neo4Net.kernel.api.impl.index.partition.AbstractIndexPartition> partitions) throws java.io.IOException
			  protected internal override AbstractIndexReader CreateSimpleReader( IList<AbstractIndexPartition> partitions )
			  {
					return null;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.Neo4Net.Kernel.Api.StorageEngine.schema.AbstractIndexReader createPartitionedReader(java.util.List<org.Neo4Net.kernel.api.impl.index.partition.AbstractIndexPartition> partitions) throws java.io.IOException
			  protected internal override AbstractIndexReader CreatePartitionedReader( IList<AbstractIndexPartition> partitions )
			  {
					return null;
			  }
		 }

		 private class SyncNotifierDirectoryFactory : DirectoryFactory
		 {
			  internal readonly System.Threading.CountdownEvent Signal;

			  internal SyncNotifierDirectoryFactory( System.Threading.CountdownEvent signal )
			  {
					this.Signal = signal;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.apache.lucene.store.Directory open(java.io.File dir, java.util.concurrent.CountDownLatch signal) throws java.io.IOException
			  public virtual Directory Open( File dir, System.Threading.CountdownEvent signal )
			  {
					Directory directory = Open( dir );
					return new SyncNotifierDirectory( directory, signal );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.apache.lucene.store.Directory open(java.io.File dir) throws java.io.IOException
			  public override Directory Open( File dir )
			  {
					dir.mkdirs();
					FSDirectory fsDir = FSDirectory.open( dir.toPath() );
					return new SyncNotifierDirectory( fsDir, Signal );
			  }

			  public override void Close()
			  {
			  }

			  private class SyncNotifierDirectory : Directory
			  {
					internal readonly Directory Delegate;
					internal readonly System.Threading.CountdownEvent Signal;

					internal SyncNotifierDirectory( Directory @delegate, System.Threading.CountdownEvent signal )
					{
						 this.Delegate = @delegate;
						 this.Signal = signal;
					}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String[] listAll() throws java.io.IOException
					public override string[] ListAll()
					{
						 return Delegate.listAll();
					}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void deleteFile(String name) throws java.io.IOException
					public override void DeleteFile( string name )
					{
						 Delegate.deleteFile( name );
					}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long fileLength(String name) throws java.io.IOException
					public override long FileLength( string name )
					{
						 return Delegate.fileLength( name );
					}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.apache.lucene.store.IndexOutput createOutput(String name, org.apache.lucene.store.IOContext context) throws java.io.IOException
					public override IndexOutput CreateOutput( string name, IOContext context )
					{
						 return Delegate.createOutput( name, context );
					}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void sync(java.util.Collection<String> names) throws java.io.IOException
					public override void Sync( ICollection<string> names )
					{
						 // where are waiting for a specific sync during index commit process inside lucene
						 // as soon as we will reach it - we will fail into sleep to give chance for concurrent close calls
						 if ( names.noneMatch( name => name.StartsWith( IndexFileNames.PENDING_SEGMENTS ) ) )
						 {
							  try
							  {
									Signal.Signal();
									Thread.Sleep( 500 );
							  }
							  catch ( Exception e )
							  {
									throw new Exception( e );
							  }
						 }

						 Delegate.sync( names );
					}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void renameFile(String source, String dest) throws java.io.IOException
					public override void RenameFile( string source, string dest )
					{
						 Delegate.renameFile( source, dest );
					}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.apache.lucene.store.IndexInput openInput(String name, org.apache.lucene.store.IOContext context) throws java.io.IOException
					public override IndexInput OpenInput( string name, IOContext context )
					{
						 return Delegate.openInput( name, context );
					}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.apache.lucene.store.Lock obtainLock(String name) throws java.io.IOException
					public override Lock ObtainLock( string name )
					{
						 return Delegate.obtainLock( name );
					}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
					public override void Close()
					{
						 Delegate.close();
					}
			  }
		 }
	}

}