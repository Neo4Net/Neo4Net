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
namespace Neo4Net.Dbms.archive
{

	using Resource = Neo4Net.Graphdb.Resource;
	using ByteUnit = Neo4Net.Io.ByteUnit;

	internal class ArchiveProgressPrinter
	{
		 private readonly AtomicBoolean _printUpdate;
		 private readonly PrintStream _output;
		 private readonly bool _interactive;
		 private long _currentBytes;
		 private long _currentFiles;
		 private bool _done;
		 internal long MaxBytes;
		 internal long MaxFiles;

		 internal ArchiveProgressPrinter( PrintStream output )
		 {
			  this._output = output;
			  _printUpdate = new AtomicBoolean();
			  _interactive = System.console() != null;
		 }

		 internal virtual Resource StartPrinting()
		 {
			  ScheduledExecutorService timer = Executors.newSingleThreadScheduledExecutor();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.ScheduledFuture<?> timerFuture = timer.scheduleAtFixedRate(this::printOnNextUpdate, 0, interactive ? 100 : 5_000, java.util.concurrent.TimeUnit.MILLISECONDS);
			  ScheduledFuture<object> timerFuture = timer.scheduleAtFixedRate( this.printOnNextUpdate, 0, _interactive ? 100 : 5_000, TimeUnit.MILLISECONDS );
			  return () =>
			  {
				timerFuture.cancel( false );
				timer.shutdown();
				try
				{
					 timer.awaitTermination( 10, TimeUnit.SECONDS );
				}
				catch ( InterruptedException )
				{
				}
				Done();
				PrintProgress();
			  };
		 }

		 internal virtual void PrintOnNextUpdate()
		 {
			  _printUpdate.set( true );
		 }

		 internal virtual void Reset()
		 {
			  MaxBytes = 0;
			  MaxFiles = 0;
			  _currentBytes = 0;
			  _currentFiles = 0;
		 }

		 internal virtual void BeginFile()
		 {
			  _currentFiles++;
		 }

		 internal virtual void AddBytes( long n )
		 {
			  _currentBytes += n;
			  if ( _printUpdate.get() )
			  {
					PrintProgress();
					_printUpdate.set( false );
			  }
		 }

		 internal virtual void EndFile()
		 {
			  PrintProgress();
		 }

		 internal virtual void Done()
		 {
			  _done = true;
		 }

		 internal virtual void PrintProgress()
		 {
			  if ( _output != null )
			  {
					char lineSep = _interactive ? '\r' : '\n';
					if ( _done )
					{
						 _output.println( lineSep + "Done: " + _currentFiles + " files, " + ByteUnit.bytesToString( _currentBytes ) + " processed." );
					}
					else if ( MaxFiles > 0 && MaxBytes > 0 )
					{
						 double progress = ( _currentBytes / ( double ) MaxBytes ) * 100;
						 _output.print( lineSep + "Files: " + _currentFiles + '/' + MaxFiles + ", data: " + string.Format( "{0,4:F1}%", progress ) );
					}
					else
					{
						 _output.print( lineSep + "Files: " + _currentFiles + "/?" + ", data: ??.?%" );
					}
			  }
		 }
	}

}