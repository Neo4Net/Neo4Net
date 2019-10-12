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
namespace Org.Neo4j.causalclustering.core.consensus.log.segmented
{

	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;

	public class Reader : System.IDisposable
	{
		 private readonly long _version;
		 private readonly StoreChannel _storeChannel;
		 private long _timeStamp;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Reader(org.neo4j.io.fs.FileSystemAbstraction fsa, java.io.File file, long version) throws java.io.IOException
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