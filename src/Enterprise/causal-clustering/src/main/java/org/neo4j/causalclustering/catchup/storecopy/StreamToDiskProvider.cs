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
namespace Neo4Net.causalclustering.catchup.storecopy
{

	using FileCopyMonitor = Neo4Net.causalclustering.catchup.tx.FileCopyMonitor;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;

	public class StreamToDiskProvider : StoreFileStreamProvider
	{
		 private readonly File _storeDir;
		 private readonly FileSystemAbstraction _fs;
		 private readonly FileCopyMonitor _fileCopyMonitor;

		 internal StreamToDiskProvider( File storeDir, FileSystemAbstraction fs, Monitors monitors )
		 {
			  this._storeDir = storeDir;
			  this._fs = fs;
			  this._fileCopyMonitor = monitors.NewMonitor( typeof( FileCopyMonitor ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public StoreFileStream acquire(String destination, int requiredAlignment) throws java.io.IOException
		 public override StoreFileStream Acquire( string destination, int requiredAlignment )
		 {
			  File fileName = new File( _storeDir, destination );
			  _fs.mkdirs( fileName.ParentFile );
			  _fileCopyMonitor.copyFile( fileName );
			  return StreamToDisk.FromFile( _fs, fileName );
		 }
	}

}