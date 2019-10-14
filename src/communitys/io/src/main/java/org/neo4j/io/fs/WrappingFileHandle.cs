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
namespace Neo4Net.Io.fs
{

	internal class WrappingFileHandle : FileHandle
	{
		 private readonly File _file;
		 private readonly File _baseDirectory;
		 private readonly FileSystemAbstraction _fs;

		 internal WrappingFileHandle( File file, File baseDirectory, FileSystemAbstraction fs )
		 {
			  this._file = file;
			  this._baseDirectory = baseDirectory;
			  this._fs = fs;
		 }

		 public virtual File File
		 {
			 get
			 {
				  return _file;
			 }
		 }

		 public virtual File RelativeFile
		 {
			 get
			 {
				  int baseLength = _baseDirectory.Path.length();
				  if ( _baseDirectory.Parent != null )
				  {
						baseLength++;
				  }
				  return new File( _file.Path.substring( baseLength ) );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void rename(java.io.File to, java.nio.file.CopyOption... options) throws java.io.IOException
		 public override void Rename( File to, params CopyOption[] options )
		 {
			  File parentFile = _file.ParentFile;
			  File canonicalTarget = to.CanonicalFile;
			  _fs.mkdirs( canonicalTarget.ParentFile );
			  _fs.renameFile( _file, canonicalTarget, options );
			  RemoveEmptyParent( parentFile );
		 }

		 private void RemoveEmptyParent( File parentFile )
		 {
			  // delete up to and including the base directory, but not above.
			  // Note that this may be 'null' if 'baseDirectory' is the top directory.
			  // Fortunately, 'File.equals(other)' handles 'null' and returns 'false' when 'other' is 'null'.
			  File end = _baseDirectory.ParentFile;
			  while ( parentFile != null && !parentFile.Equals( end ) )
			  {
					File[] files = _fs.listFiles( parentFile );
					if ( files == null || Files.Length > 0 )
					{
						 return;
					}
					_fs.deleteFile( parentFile );
					parentFile = parentFile.ParentFile;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void delete() throws java.io.IOException
		 public override void Delete()
		 {
			  File parentFile = _file.ParentFile;
			  _fs.deleteFileOrThrow( _file );
			  RemoveEmptyParent( parentFile );
		 }
	}

}