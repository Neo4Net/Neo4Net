using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;


	internal class ReaderPool
	{
		 private List<Reader> _pool;
		 private readonly int _maxSize;
		 private readonly Log _log;
		 private readonly FileNames _fileNames;
		 private readonly FileSystemAbstraction _fsa;
		 private readonly Clock _clock;

		 internal ReaderPool( int maxSize, LogProvider logProvider, FileNames fileNames, FileSystemAbstraction fsa, Clock clock )
		 {
			  this._pool = new List<Reader>( maxSize );
			  this._maxSize = maxSize;
			  this._log = logProvider.getLog( this.GetType() );
			  this._fileNames = fileNames;
			  this._fsa = fsa;
			  this._clock = clock;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Reader acquire(long version, long byteOffset) throws java.io.IOException
		 internal virtual Reader Acquire( long version, long byteOffset )
		 {
			  Reader reader = GetFromPool( version );
			  if ( reader == null )
			  {
					reader = CreateFor( version );
			  }
			  reader.Channel().position(byteOffset);
			  return reader;
		 }

		 internal virtual void Release( Reader reader )
		 {
			  reader.TimeStamp = _clock.millis();
			  Optional<Reader> optionalOverflow = PutInPool( reader );
			  optionalOverflow.ifPresent( this.dispose );
		 }

		 private Reader GetFromPool( long version )
		 {
			 lock ( this )
			 {
				  IEnumerator<Reader> itr = _pool.GetEnumerator();
				  while ( itr.MoveNext() )
				  {
						Reader reader = itr.Current;
						if ( reader.Version() == version )
						{
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
							 itr.remove();
							 return reader;
						}
				  }
				  return null;
			 }
		 }

		 private Optional<Reader> PutInPool( Reader reader )
		 {
			 lock ( this )
			 {
				  _pool.Add( reader );
				  return _pool.Count > _maxSize ? of( _pool.RemoveAt( 0 ) ) : empty();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Reader createFor(long version) throws java.io.IOException
		 private Reader CreateFor( long version )
		 {
			  return new Reader( _fsa, _fileNames.getForVersion( version ), version );
		 }

		 internal virtual void Prune( long maxAge, TimeUnit unit )
		 {
			 lock ( this )
			 {
				  if ( _pool == null )
				  {
						return;
				  }
      
				  long endTimeMillis = _clock.millis() - unit.toMillis(maxAge);
      
				  IEnumerator<Reader> itr = _pool.GetEnumerator();
				  while ( itr.MoveNext() )
				  {
						Reader reader = itr.Current;
						if ( reader.TimeStamp < endTimeMillis )
						{
							 Dispose( reader );
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
							 itr.remove();
						}
				  }
			 }
		 }

		 private void Dispose( Reader reader )
		 {
			  try
			  {
					reader.Dispose();
			  }
			  catch ( IOException e )
			  {
					_log.error( "Failed to close reader", e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: synchronized void close() throws java.io.IOException
		 internal virtual void Close()
		 {
			 lock ( this )
			 {
				  foreach ( Reader reader in _pool )
				  {
						reader.Dispose();
				  }
				  _pool.Clear();
				  _pool = null;
			 }
		 }

		 public virtual void Prune( long version )
		 {
			 lock ( this )
			 {
				  if ( _pool == null )
				  {
						return;
				  }
      
				  IEnumerator<Reader> itr = _pool.GetEnumerator();
				  while ( itr.MoveNext() )
				  {
						Reader reader = itr.Current;
						if ( reader.Version() == version )
						{
							 Dispose( reader );
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
							 itr.remove();
						}
				  }
			 }
		 }
	}

}