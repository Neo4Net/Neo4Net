/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.catchup.storecopy
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;

	internal class StoreResource
	{
		 private readonly File _file;
		 private readonly string _path;
		 private readonly int _recordSize;
		 private readonly FileSystemAbstraction _fs;

		 internal StoreResource( File file, string relativePath, int recordSize, FileSystemAbstraction fs )
		 {
			  this._file = file;
			  this._path = relativePath;
			  this._recordSize = recordSize;
			  this._fs = fs;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.nio.channels.ReadableByteChannel open() throws java.io.IOException
		 internal virtual ReadableByteChannel Open()
		 {
			  return _fs.open( _file, OpenMode.READ );
		 }

		 public virtual string Path()
		 {
			  return _path;
		 }

		 internal virtual int RecordSize()
		 {
			  return _recordSize;
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
			  StoreResource that = ( StoreResource ) o;
			  return _recordSize == that._recordSize && Objects.Equals( _file, that._file ) && Objects.Equals( _path, that._path );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _file, _path, _recordSize );
		 }

		 public override string ToString()
		 {
			  return "StoreResource{" + "path='" + _path + '\'' + ", recordSize=" + _recordSize + '}';
		 }
	}

}