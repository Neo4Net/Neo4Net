using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.core.consensus.log
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public abstract class ConcurrentStressIT<T> where T : RaftLog, Neo4Net.Kernel.Lifecycle.Lifecycle
	{
		 private const int MAX_CONTENT_SIZE = 2048;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory dir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Dir = TestDirectory.testDirectory();

		 protected internal abstract T CreateRaftLog( FileSystemAbstraction fsa, File dir );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readAndWrite() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadAndWrite()
		 {
			  ReadAndWrite( 5, 2, SECONDS );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void readAndWrite(int nReaders, int time, java.util.concurrent.TimeUnit unit) throws Throwable
		 private void ReadAndWrite( int nReaders, int time, TimeUnit unit )
		 {
			  using ( DefaultFileSystemAbstraction fsa = new DefaultFileSystemAbstraction() )
			  {
					LifeSupport lifeSupport = new LifeSupport();
					T raftLog = CreateRaftLog( fsa, Dir.directory() );
					lifeSupport.Add( raftLog );
					lifeSupport.Start();

					try
					{
						 ExecutorService es = Executors.newCachedThreadPool();

						 ICollection<Future<long>> futures = new List<Future<long>>();
						 futures.Add( es.submit( new TimedTask( this, () => write(raftLog), time, unit ) ) );

						 for ( int i = 0; i < nReaders; i++ )
						 {
							  futures.Add( es.submit( new TimedTask( this, () => read(raftLog), time, unit ) ) );
						 }

						 foreach ( Future<long> f in futures )
						 {
							  long iterations = f.get();
						 }

						 es.shutdown();
					}
					finally
					{
						 lifeSupport.Shutdown();
					}
			  }
		 }

		 private class TimedTask : Callable<long>
		 {
			 private readonly ConcurrentStressIT<T> _outerInstance;

			  internal ThreadStart Task;
			  internal readonly long RunTimeMillis;

			  internal TimedTask( ConcurrentStressIT<T> outerInstance, ThreadStart task, int time, TimeUnit unit )
			  {
				  this._outerInstance = outerInstance;
					this.Task = task;
					this.RunTimeMillis = unit.toMillis( time );
			  }

			  public override long? Call()
			  {
					long endTime = DateTimeHelper.CurrentUnixTimeMillis() + RunTimeMillis;
					long count = 0;
					while ( endTime > DateTimeHelper.CurrentUnixTimeMillis() )
					{
						 Task.run();
						 count++;
					}
					return count;
			  }
		 }

		 private void Read( RaftLog raftLog )
		 {
			  try
			  {
					  using ( RaftLogCursor cursor = raftLog.GetEntryCursor( 0 ) )
					  {
						while ( cursor.Next() )
						{
							 RaftLogEntry entry = cursor.get();
							 ReplicatedString content = ( ReplicatedString ) entry.Content();
							 assertEquals( StringForIndex( cursor.Index() ), content.Value() );
						}
					  }
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 private void Write( RaftLog raftLog )
		 {
			  long index = raftLog.AppendIndex();
			  long term = ( index + 1 ) * 3;
			  try
			  {
					string data = StringForIndex( index + 1 );
					raftLog.Append( new RaftLogEntry( term, new ReplicatedString( data ) ) );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 private const CharSequence CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

		 private string StringForIndex( long index )
		 {
			  int len = ( ( int ) index ) % MAX_CONTENT_SIZE + 1;
			  StringBuilder str = new StringBuilder( len );

			  while ( len-- > 0 )
			  {
					str.Append( CHARS.charAt( len % CHARS.length() ) );
			  }

			  return str.ToString();
		 }
	}

}