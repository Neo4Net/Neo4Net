using System;
using System.Collections.Generic;

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
namespace Neo4Net.Io.pagecache.randomharness
{
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using LongSets = org.eclipse.collections.impl.factory.primitive.LongSets;


	using MuninnPageCache = Neo4Net.Io.pagecache.impl.muninn.MuninnPageCache;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.isOneOf;

	internal class CommandPrimer
	{
		 private readonly Random _rng;
		 private readonly MuninnPageCache _cache;
		 private readonly File[] _files;
		 private readonly IDictionary<File, PagedFile> _fileMap;
		 private readonly IDictionary<File, IList<int>> _recordsWrittenTo;
		 private readonly IList<File> _mappedFiles;
		 private readonly ISet<File> _filesTouched;
		 private readonly int _filePageCount;
		 private readonly int _filePageSize;
		 private readonly RecordFormat _recordFormat;
		 private readonly int _maxRecordCount;
		 private readonly int _recordsPerPage;
		 // Entity-locks that protect the individual records, since page write locks are not exclusive.
		 private readonly TinyLockManager _recordLocks;

		 internal CommandPrimer( Random rng, MuninnPageCache cache, File[] files, IDictionary<File, PagedFile> fileMap, int filePageCount, int filePageSize, RecordFormat recordFormat )
		 {
			  this._rng = rng;
			  this._cache = cache;
			  this._files = files;
			  this._fileMap = fileMap;
			  this._filePageCount = filePageCount;
			  this._filePageSize = filePageSize;
			  this._recordFormat = recordFormat;
			  _mappedFiles = new List<File>();
			  ( ( IList<File> )_mappedFiles ).AddRange( fileMap.Keys );
			  _filesTouched = new HashSet<File>();
			  _filesTouched.addAll( _mappedFiles );
			  _recordsWrittenTo = new Dictionary<File, IList<int>>();
			  _recordsPerPage = cache.PageSize() / recordFormat.RecordSize;
			  _maxRecordCount = filePageCount * _recordsPerPage;
			  _recordLocks = new TinyLockManager();

			  foreach ( File file in files )
			  {
					_recordsWrittenTo[file] = new List<int>();
			  }
		 }

		 public virtual IList<File> MappedFiles
		 {
			 get
			 {
				  return _mappedFiles;
			 }
		 }

		 public virtual ISet<File> FilesTouched
		 {
			 get
			 {
				  return _filesTouched;
			 }
		 }

		 public virtual Action Prime( Command command )
		 {
			  switch ( command.innerEnumValue )
			  {
			  case Neo4Net.Io.pagecache.randomharness.Command.InnerEnum.FlushCache:
				  return FlushCache();
			  case Neo4Net.Io.pagecache.randomharness.Command.InnerEnum.FlushFile:
				  return FlushFile();
			  case Neo4Net.Io.pagecache.randomharness.Command.InnerEnum.MapFile:
				  return MapFile();
			  case Neo4Net.Io.pagecache.randomharness.Command.InnerEnum.UnmapFile:
				  return UnmapFile();
			  case Neo4Net.Io.pagecache.randomharness.Command.InnerEnum.ReadRecord:
				  return ReadRecord();
			  case Neo4Net.Io.pagecache.randomharness.Command.InnerEnum.WriteRecord:
				  return WriteRecord();
			  case Neo4Net.Io.pagecache.randomharness.Command.InnerEnum.ReadMulti:
				  return ReadMulti();
			  case Neo4Net.Io.pagecache.randomharness.Command.InnerEnum.WriteMulti:
				  return WriteMulti();
			  default:
				  throw new System.ArgumentException( "Unknown command: " + command );
			  }
		 }

		 private Action FlushCache()
		 {
			  return new ActionAnonymousInnerClass( this );
		 }

		 private class ActionAnonymousInnerClass : Action
		 {
			 private readonly CommandPrimer _outerInstance;

			 public ActionAnonymousInnerClass( CommandPrimer outerInstance ) : base( Command.FlushCache, "" )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void perform() throws Exception
			 public override void perform()
			 {
				  _outerInstance.cache.flushAndForce();
			 }
		 }

		 private Action FlushFile()
		 {
			  if ( _mappedFiles.Count > 0 )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File file = mappedFiles.get(rng.nextInt(mappedFiles.size()));
					File file = _mappedFiles[_rng.Next( _mappedFiles.Count )];
					return new ActionAnonymousInnerClass2( this, file.Name, file );
			  }
			  return new ActionAnonymousInnerClass3( this );
		 }

		 private class ActionAnonymousInnerClass2 : Action
		 {
			 private readonly CommandPrimer _outerInstance;

			 private File _file;

			 public ActionAnonymousInnerClass2( CommandPrimer outerInstance, UnknownType getName, File file ) : base( Command.FlushFile, "[file=%s]", getName )
			 {
				 this.outerInstance = outerInstance;
				 this._file = file;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void perform() throws Exception
			 public override void perform()
			 {
				  PagedFile pagedFile = _outerInstance.fileMap[_file];
				  if ( pagedFile != null )
				  {
						pagedFile.FlushAndForce();
				  }
			 }
		 }

		 private class ActionAnonymousInnerClass3 : Action
		 {
			 private readonly CommandPrimer _outerInstance;

			 public ActionAnonymousInnerClass3( CommandPrimer outerInstance ) : base( Command.FlushFile, "[no files mapped to flush]" )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void perform()
			 {
			 }
		 }

		 private Action MapFile()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File file = files[rng.nextInt(files.length)];
			  File file = _files[_rng.Next( _files.Length )];
			  _mappedFiles.Add( file );
			  _filesTouched.Add( file );
			  return new ActionAnonymousInnerClass4( this, file );
		 }

		 private class ActionAnonymousInnerClass4 : Action
		 {
			 private readonly CommandPrimer _outerInstance;

			 private File _file;

			 public ActionAnonymousInnerClass4( CommandPrimer outerInstance, File file ) : base( Command.MapFile, "[file=%s]", file )
			 {
				 this.outerInstance = outerInstance;
				 this._file = file;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void perform() throws Exception
			 public override void perform()
			 {
				  _outerInstance.fileMap[_file] = _outerInstance.cache.map( _file, _outerInstance.filePageSize );
			 }
		 }

		 private Action UnmapFile()
		 {
			  if ( _mappedFiles.Count > 0 )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File file = mappedFiles.remove(rng.nextInt(mappedFiles.size()));
					File file = _mappedFiles.Remove( _rng.Next( _mappedFiles.Count ) );
					return new ActionAnonymousInnerClass5( this, file );
			  }
			  return null;
		 }

		 private class ActionAnonymousInnerClass5 : Action
		 {
			 private readonly CommandPrimer _outerInstance;

			 private File _file;

			 public ActionAnonymousInnerClass5( CommandPrimer outerInstance, File file ) : base( Command.UnmapFile, "[file=%s]", file )
			 {
				 this.outerInstance = outerInstance;
				 this._file = file;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void perform() throws Exception
			 public override void perform()
			 {
				  _outerInstance.fileMap[_file].close();
			 }
		 }

		 private Action ReadRecord()
		 {
			  return BuildReadRecord( null );
		 }

		 private Action WriteRecord()
		 {
			  return BuildWriteAction( null, LongSets.immutable.empty() );
		 }

		 private Action ReadMulti()
		 {
			  int count = _rng.Next( 5 ) + 1;
			  Action action = null;
			  for ( int i = 0; i < count; i++ )
			  {
					action = BuildReadRecord( action );
			  }
			  return action;
		 }

		 private Action WriteMulti()
		 {
			  int count = _rng.Next( 5 ) + 1;
			  Action action = null;
			  for ( int i = 0; i < count; i++ )
			  {
					action = BuildWriteAction( action, LongSets.immutable.empty() );
			  }
			  return action;
		 }

		 private Action BuildReadRecord( Action innerAction )
		 {
			  int mappedFilesCount = _mappedFiles.Count;
			  if ( mappedFilesCount == 0 )
			  {
					return innerAction;
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File file = mappedFiles.get(rng.nextInt(mappedFilesCount));
			  File file = _mappedFiles[_rng.Next( mappedFilesCount )];
			  IList<int> recordsWritten = _recordsWrittenTo[file];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int recordId = recordsWritten.isEmpty() ? rng.nextInt(maxRecordCount) : recordsWritten.get(rng.nextInt(recordsWritten.size()));
			  int recordId = recordsWritten.Count == 0 ? _rng.Next( _maxRecordCount ) : recordsWritten[_rng.Next( recordsWritten.Count )];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int pageId = recordId / recordsPerPage;
			  int pageId = recordId / _recordsPerPage;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int pageOffset = (recordId % recordsPerPage) * recordFormat.getRecordSize();
			  int pageOffset = ( recordId % _recordsPerPage ) * _recordFormat.RecordSize;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Record expectedRecord = recordFormat.createRecord(file, recordId);
			  Record expectedRecord = _recordFormat.createRecord( file, recordId );
			  return new ReadAction( this, file, recordId, pageId, pageOffset, expectedRecord, innerAction );
		 }

		 private Action BuildWriteAction( Action innerAction, LongSet forbiddenRecordIds )
		 {
			  int mappedFilesCount = _mappedFiles.Count;
			  if ( mappedFilesCount == 0 )
			  {
					return innerAction;
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File file = mappedFiles.get(rng.nextInt(mappedFilesCount));
			  File file = _mappedFiles[_rng.Next( mappedFilesCount )];
			  _filesTouched.Add( file );
			  int recordId;
			  do
			  {
					recordId = _rng.Next( _filePageCount * _recordsPerPage );
			  } while ( forbiddenRecordIds.contains( recordId ) );
			  _recordsWrittenTo[file].Add( recordId );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int pageId = recordId / recordsPerPage;
			  int pageId = recordId / _recordsPerPage;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int pageOffset = (recordId % recordsPerPage) * recordFormat.getRecordSize();
			  int pageOffset = ( recordId % _recordsPerPage ) * _recordFormat.RecordSize;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Record record = recordFormat.createRecord(file, recordId);
			  Record record = _recordFormat.createRecord( file, recordId );
			  return new WriteAction( this, file, recordId, pageId, pageOffset, record, innerAction );
		 }

		 private class ReadAction : Action
		 {
			 private readonly CommandPrimer _outerInstance;

			  internal readonly File File;
			  internal readonly int PageId;
			  internal readonly int PageOffset;
			  internal readonly Record ExpectedRecord;

			  internal ReadAction( CommandPrimer outerInstance, File file, int recordId, int pageId, int pageOffset, Record expectedRecord, Action innerAction ) : base( Command.ReadRecord, innerAction, "[file=%s, recordId=%s, pageId=%s, pageOffset=%s, expectedRecord=%s]", file, recordId, pageId, pageOffset, expectedRecord )
			  {
				  this._outerInstance = outerInstance;
					this.File = file;
					this.PageId = pageId;
					this.PageOffset = pageOffset;
					this.ExpectedRecord = expectedRecord;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void perform() throws Exception
			  public override void Perform()
			  {
					PagedFile pagedFile = outerInstance.fileMap[File];
					if ( pagedFile != null )
					{
						 using ( PageCursor cursor = pagedFile.Io( PageId, Neo4Net.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK ) )
						 {
							  if ( cursor.Next() )
							  {
									cursor.Offset = PageOffset;
									Record actualRecord = outerInstance.recordFormat.ReadRecord( cursor );
									assertThat( ToString(), actualRecord, isOneOf(ExpectedRecord, outerInstance.recordFormat.ZeroRecord()) );
									PerformInnerAction();
							  }
						 }
					}
			  }
		 }

		 private class WriteAction : Action
		 {
			 private readonly CommandPrimer _outerInstance;

			  internal readonly File File;
			  internal readonly int RecordId;
			  internal readonly int PageId;
			  internal readonly int PageOffset;
			  internal readonly Record Record;

			  internal WriteAction( CommandPrimer outerInstance, File file, int recordId, int pageId, int pageOffset, Record record, Action innerAction ) : base( Command.WriteRecord, innerAction, "[file=%s, recordId=%s, pageId=%s, pageOffset=%s, record=%s]", file, recordId, pageId, pageOffset, record )
			  {
				  this._outerInstance = outerInstance;
					this.File = file;
					this.RecordId = recordId;
					this.PageId = pageId;
					this.PageOffset = pageOffset;
					this.Record = record;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void perform() throws Exception
			  public override void Perform()
			  {
					PagedFile pagedFile = outerInstance.fileMap[File];
					if ( pagedFile != null )
					{
						 // We use tryLock to avoid any deadlock scenarios.
						 if ( outerInstance.recordLocks.TryLock( RecordId ) )
						 {
							  try
							  {
									using ( PageCursor cursor = pagedFile.Io( PageId, Neo4Net.Io.pagecache.PagedFile_Fields.PfSharedWriteLock ) )
									{
										 if ( cursor.Next() )
										 {
											  cursor.Offset = PageOffset;
											  outerInstance.recordFormat.Write( Record, cursor );
											  PerformInnerAction();
										 }
									}
							  }
							  finally
							  {
									outerInstance.recordLocks.Unlock( RecordId );
							  }
						 }
					}
					else
					{
						 PerformInnerAction();
					}
			  }
		 }
	}

}