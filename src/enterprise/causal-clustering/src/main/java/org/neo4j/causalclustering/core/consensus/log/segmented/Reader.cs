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
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;

	public class Reader : System.IDisposable
	{
		 private readonly long _version;
		 private readonly StoreChannel _storeChannel;
		 private long _timeStamp;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Reader(Neo4Net.io.fs.FileSystemAbstraction fsa, java.io.File file, long version) throws java.io.IOException
		 internal Reader( FileSystemAbstraction fsa, File file, long version )
		 {
			  this._storeChannel = fsa.Open( file, OpenMode.READ );
			  this._version = version;
		 }

		 public virtual long Version()
		 {
			  return _version;
		 }

		 public virtual StoreChannel Channel()
		 {
			  return _storeChannel;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _storeChannel.close();
		 }

		 internal virtual long TimeStamp
		 {
			 set
			 {
				  this._timeStamp = value;
			 }
			 get
			 {
				  return _timeStamp;
			 }
		 }

	}

}