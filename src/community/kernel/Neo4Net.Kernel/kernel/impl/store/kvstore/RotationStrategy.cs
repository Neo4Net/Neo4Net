using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.store.kvstore
{

	using Neo4Net.Helpers.Collections;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;

	internal abstract class RotationStrategy
	{
		 protected internal readonly FileSystemAbstraction Fs;
		 protected internal readonly PageCache Pages;
		 private readonly ProgressiveFormat _format;
		 private readonly RotationMonitor _monitor;

		 internal RotationStrategy( FileSystemAbstraction fs, PageCache pages, ProgressiveFormat format, RotationMonitor monitor )
		 {
			  this.Fs = fs;
			  this.Pages = pages;
			  this._format = format;
			  this._monitor = monitor;
		 }

		 protected internal abstract File InitialFile();

		 protected internal abstract IEnumerable<File> CandidateFiles();

		 protected internal abstract File NextFile( File previous );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final org.Neo4Net.helpers.collection.Pair<java.io.File, KeyValueStoreFile> open() throws java.io.IOException
		 public Pair<File, KeyValueStoreFile> Open()
		 {
			  KeyValueStoreFile result = null;
			  File path = null;
			  foreach ( File candidatePath in CandidateFiles() )
			  {
					KeyValueStoreFile file;
					if ( Fs.fileExists( candidatePath ) )
					{
						 try
						 {
							  file = _format.openStore( Fs, Pages, candidatePath );
						 }
						 catch ( Exception e )
						 {
							  _monitor.failedToOpenStoreFile( candidatePath, e );
							  continue;
						 }
						 if ( result == null || _format.compareHeaders( result.Headers(), file.Headers() ) < 0 )
						 {
							  if ( result != null )
							  {
									result.Dispose();
							  }
							  result = file;
							  path = candidatePath;
						 }
						 else
						 {
							  file.Dispose();
						 }
					}
			  }
			  return result == null ? null : Pair.of( path, result );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final org.Neo4Net.helpers.collection.Pair<java.io.File, KeyValueStoreFile> create(DataProvider initialData, long version) throws java.io.IOException
		 public Pair<File, KeyValueStoreFile> Create( DataProvider initialData, long version )
		 {
			  File path = InitialFile();
			  return Pair.of( path, _format.createStore( Fs, Pages, path, _format.keySize(), _format.valueSize(), _format.initialHeaders(version), initialData ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final org.Neo4Net.helpers.collection.Pair<java.io.File, KeyValueStoreFile> next(java.io.File file, Headers headers, DataProvider data) throws java.io.IOException
		 public Pair<File, KeyValueStoreFile> Next( File file, Headers headers, DataProvider data )
		 {
			  File path = NextFile( file );
			  _monitor.beforeRotation( file, path, headers );
			  KeyValueStoreFile store;
			  try
			  {
					store = _format.createStore( Fs, Pages, path, _format.keySize(), _format.valueSize(), headers, data );
			  }
			  catch ( Exception e )
			  {
					_monitor.rotationFailed( file, path, headers, e );
					throw e;
			  }
			  _monitor.rotationSucceeded( file, path, headers );
			  return Pair.of( path, store );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: final KeyValueStoreFile openStoreFile(java.io.File path) throws java.io.IOException
		 internal KeyValueStoreFile OpenStoreFile( File path )
		 {
			  return _format.openStore( Fs, Pages, path );
		 }

		 internal class LeftRight : RotationStrategy
		 {
			  internal readonly File Left;
			  internal readonly File Right;

			  internal LeftRight( FileSystemAbstraction fs, PageCache pages, ProgressiveFormat format, RotationMonitor monitor, File left, File right ) : base( fs, pages, format, monitor )
			  {
					this.Left = left;
					this.Right = right;
			  }

			  protected internal override File InitialFile()
			  {
					return Left;
			  }

			  protected internal override IEnumerable<File> CandidateFiles()
			  {
					return Arrays.asList( Left, Right );
			  }

			  protected internal override File NextFile( File previous )
			  {
					if ( Left.Equals( previous ) )
					{
						 return Right;
					}
					else if ( Right.Equals( previous ) )
					{
						 return Left;
					}
					else
					{
						 throw new System.InvalidOperationException( "Invalid path: " + previous );
					}
			  }
		 }

		 internal class Incrementing : RotationStrategy, FilenameFilter
		 {
			  internal static readonly Pattern Suffix = Pattern.compile( "\\.[0-9]+" );
			  internal readonly DatabaseLayout DatabaseLayout;
			  internal readonly string BaseName;

			  internal Incrementing( FileSystemAbstraction fs, PageCache pages, ProgressiveFormat format, RotationMonitor monitor, DatabaseLayout databaseLayout ) : base( fs, pages, format, monitor )
			  {
					this.DatabaseLayout = databaseLayout;
					BaseName = databaseLayout.CountStoreA().Name;
			  }

			  protected internal override File InitialFile()
			  {
					return DatabaseLayout.file( BaseName + ".0" );
			  }

			  protected internal override IEnumerable<File> CandidateFiles()
			  {
					return Arrays.asList( Fs.listFiles( DatabaseLayout.databaseDirectory(), this ) );
			  }

			  protected internal override File NextFile( File previous )
			  {
					string name = previous.Name;
					int pos = name.LastIndexOf( '.' );
					int next;
					try
					{
						 int number = int.Parse( name.Substring( pos + 1 ) );
						 if ( !DatabaseLayout.databaseDirectory().Equals(previous.ParentFile) || !BaseName.Equals(name.Substring(0, pos)) )
						 {
							  throw new System.InvalidOperationException( "Invalid path: " + previous );
						 }
						 next = number + 1;
					}
					catch ( System.FormatException e )
					{
						 throw new System.InvalidOperationException( "Invalid path: " + previous, e );
					}
					return DatabaseLayout.file( BaseName + "." + next );
			  }

			  public override bool Accept( File dir, string name )
			  {
					return name.StartsWith( BaseName, StringComparison.Ordinal ) && Suffix.matcher( name.Substring( BaseName.Length ) ).matches();
			  }
		 }
	}

}