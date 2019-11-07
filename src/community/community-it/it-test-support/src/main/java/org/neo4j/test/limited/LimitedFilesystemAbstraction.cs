using System.IO;

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
namespace Neo4Net.Test.limited
{

	using DelegatingFileSystemAbstraction = Neo4Net.GraphDb.mockfs.DelegatingFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using ChannelInputStream = Neo4Net.Test.impl.ChannelInputStream;
	using ChannelOutputStream = Neo4Net.Test.impl.ChannelOutputStream;

	public class LimitedFilesystemAbstraction : DelegatingFileSystemAbstraction
	{
		 private volatile bool _outOfSpace;

		 public LimitedFilesystemAbstraction( FileSystemAbstraction @delegate ) : base( @delegate )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.io.fs.StoreChannel open(java.io.File fileName, Neo4Net.io.fs.OpenMode openMode) throws java.io.IOException
		 public override StoreChannel Open( File fileName, OpenMode openMode )
		 {
			  return new LimitedFileChannel( base.Open( fileName, openMode ), this );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.OutputStream openAsOutputStream(java.io.File fileName, boolean append) throws java.io.IOException
		 public override Stream OpenAsOutputStream( File fileName, bool append )
		 {
			  return new ChannelOutputStream( Open( fileName, OpenMode.READ_WRITE ), append );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.InputStream openAsInputStream(java.io.File fileName) throws java.io.IOException
		 public override Stream OpenAsInputStream( File fileName )
		 {
			  return new ChannelInputStream( Open( fileName, OpenMode.READ ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.Reader openAsReader(java.io.File fileName, java.nio.charset.Charset charset) throws java.io.IOException
		 public override Reader OpenAsReader( File fileName, Charset charset )
		 {
			  return new StreamReader( OpenAsInputStream( fileName ), charset );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.Writer openAsWriter(java.io.File fileName, java.nio.charset.Charset charset, boolean append) throws java.io.IOException
		 public override Writer OpenAsWriter( File fileName, Charset charset, bool append )
		 {
			  return new StreamWriter( OpenAsOutputStream( fileName, append ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.io.fs.StoreChannel create(java.io.File fileName) throws java.io.IOException
		 public override StoreChannel Create( File fileName )
		 {
			  EnsureHasSpace();
			  return new LimitedFileChannel( base.Create( fileName ), this );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void mkdirs(java.io.File fileName) throws java.io.IOException
		 public override void Mkdirs( File fileName )
		 {
			  EnsureHasSpace();
			  base.Mkdirs( fileName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void renameFile(java.io.File from, java.io.File to, java.nio.file.CopyOption... copyOptions) throws java.io.IOException
		 public override void RenameFile( File from, File to, params CopyOption[] copyOptions )
		 {
			  EnsureHasSpace();
			  base.RenameFile( from, to, copyOptions );
		 }

		 public virtual void RunOutOfDiskSpace( bool outOfSpace )
		 {
			  this._outOfSpace = outOfSpace;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void ensureHasSpace() throws java.io.IOException
		 public virtual void EnsureHasSpace()
		 {
			  if ( _outOfSpace )
			  {
					throw new IOException( "No space left on device" );
			  }
		 }
	}

}